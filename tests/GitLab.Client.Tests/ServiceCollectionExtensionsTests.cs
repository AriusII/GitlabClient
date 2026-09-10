using System.Net;

using GitLab.Client.Abstractions;
using GitLab.Client.Configuration;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Infrastructure.RateLimiting;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace GitLab.Client.Tests;

public sealed class ServiceCollectionExtensionsTests
{
    private const string TestToken = "glpat-test-token";

    private static readonly Uri GitLabBaseAddress = new("https://gitlab.example/api/v4/");

    public static TheoryData<Type> ResourceClientTypes =>
    [
        typeof(IProjectsClient), typeof(IGroupsClient), typeof(IUsersClient), typeof(IBranchesClient),
        typeof(ICommitsClient), typeof(IIssuesClient), typeof(IMergeRequestsClient), typeof(ILabelsClient),
        typeof(IMilestonesClient), typeof(ITagsClient), typeof(IPipelinesClient), typeof(IJobsClient),
        typeof(IRepositoryFilesClient), typeof(IReleasesClient), typeof(IProtectedBranchesClient),
        typeof(IEnvironmentsClient), typeof(INotesClient), typeof(IMembersClient), typeof(IProjectHooksClient)
    ];

    [Fact]
    public void AddGitLabClient_RegistersResolvableGitLabClient()
    {
        ServiceCollection services = new();

        services.AddGitLabClient(options => options.AccessToken = TestToken);

        using ServiceProvider provider = services.BuildServiceProvider();

        IGitLabClient client = provider.GetRequiredService<IGitLabClient>();

        Assert.NotNull(client.Projects);
    }

    [Theory]
    [MemberData(nameof(ResourceClientTypes))]
    public void AddGitLabClient_RegistersEveryResourceClient(Type clientType)
    {
        ServiceCollection services = new();
        services.AddGitLabClient(options => options.AccessToken = TestToken);

        // ValidateOnBuild makes the container itself prove every registered service can be constructed, and
        // ValidateScopes proves no singleton captures a scoped service. One test with these flags turns a
        // whole class of composition-root bugs into a build failure.
        using ServiceProvider provider = services.BuildServiceProvider(
            new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true });

        Assert.NotNull(provider.GetRequiredService(clientType));
    }

    [Fact]
    public void AddGitLabClient_RegistersTheApiConnectionAsASingleton()
    {
        // Regression guard for the captive-dependency bug: the connection must not be a transient typed
        // HttpClient captured by the singleton resource graph.
        ServiceCollection services = new();
        services.AddGitLabClient(options => options.AccessToken = TestToken);

        using ServiceProvider provider = services.BuildServiceProvider(
            new ServiceProviderOptions { ValidateScopes = true });
        using IServiceScope first = provider.CreateScope();
        using IServiceScope second = provider.CreateScope();

        Assert.Same(
            first.ServiceProvider.GetRequiredService<IGitLabApiConnection>(),
            second.ServiceProvider.GetRequiredService<IGitLabApiConnection>());
    }

    [Fact]
    public void AddGitLabClient_WithoutAccessToken_FailsWhenTheGraphIsBuilt()
    {
        ServiceCollection services = new();
        services.AddGitLabClient(static _ => { });

        using ServiceProvider provider = services.BuildServiceProvider();

        OptionsValidationException exception =
            Assert.Throws<OptionsValidationException>(() => provider.GetRequiredService<IGitLabClient>());

        Assert.Contains(nameof(GitLabClientOptions.AccessToken), exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AddGitLabClient_ReportsEveryOptionsFailureAtOnce()
    {
        ServiceCollection services = new();
        services.AddGitLabClient(static options =>
        {
            options.BaseAddress = new Uri("ftp://gitlab.example/api/v4/");
            options.UserAgent = string.Empty;
        });

        using ServiceProvider provider = services.BuildServiceProvider();

        OptionsValidationException exception =
            Assert.Throws<OptionsValidationException>(() => provider.GetRequiredService<IGitLabClient>());

        Assert.Equal(3, exception.Failures.Count());
    }

    [Fact]
    public void AddGitLabClient_CalledTwice_Throws()
    {
        ServiceCollection services = new();
        services.AddGitLabClient(options => options.AccessToken = TestToken);

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            services.AddGitLabClient(static options => options.AccessToken = "glpat-a-different-token"));

        Assert.Contains("already been called", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AddGitLabClient_ConfiguresTheNamedHttpClientFromOptions()
    {
        ServiceCollection services = new();
        services.AddGitLabClient(static options =>
        {
            options.AccessToken = TestToken;
            options.BaseAddress = GitLabBaseAddress;
            options.Timeout = TimeSpan.FromSeconds(42);
        });

        using ServiceProvider provider = services.BuildServiceProvider();

        using HttpClient client = provider.GetRequiredService<IHttpClientFactory>()
            .CreateClient(GitLabClientDefaults.HttpClientName);

        Assert.Equal(GitLabBaseAddress, client.BaseAddress);
        Assert.Equal(TimeSpan.FromSeconds(42), client.Timeout);
        Assert.Equal(HttpVersion.Version20, client.DefaultRequestVersion);
        Assert.Equal(HttpVersionPolicy.RequestVersionOrLower, client.DefaultVersionPolicy);
    }

    [Fact]
    public void AddGitLabClient_EnablesResponseCompressionAndDisablesRedirectsAndCookies()
    {
        ServiceCollection services = new();
        services.AddGitLabClient(options => options.AccessToken = TestToken);

        using ServiceProvider provider = services.BuildServiceProvider();

        SocketsHttpHandler primaryHandler = Assert.IsType<SocketsHttpHandler>(GetPrimaryHandler(provider));

        Assert.Equal(DecompressionMethods.All, primaryHandler.AutomaticDecompression);

        // Redirects are followed below every DelegatingHandler, so the authentication handler cannot withhold
        // PRIVATE-TOKEN from a cross-host 3xx target. Not following them at all is the fix.
        Assert.False(primaryHandler.AllowAutoRedirect);
        Assert.False(primaryHandler.UseCookies);
    }

    [Fact]
    public void AddGitLabClient_OrdersTheHandlerChain_AuthenticationThenRateLimit()
    {
        ServiceCollection services = new();
        services.AddGitLabClient(options => options.AccessToken = TestToken);

        using ServiceProvider provider = services.BuildServiceProvider();

        List<Type> chain = [];
        HttpMessageHandler handler = provider.GetRequiredService<IHttpMessageHandlerFactory>()
            .CreateHandler(GitLabClientDefaults.HttpClientName);

        while (handler is DelegatingHandler delegating && delegating.InnerHandler is not null)
        {
            chain.Add(handler.GetType());
            handler = delegating.InnerHandler;
        }

        int authenticationIndex = chain.IndexOf(typeof(GitLabAuthenticationHandler));
        int rateLimitIndex = chain.IndexOf(typeof(GitLabRateLimitHandler));

        Assert.NotEqual(-1, authenticationIndex);
        Assert.True(authenticationIndex < rateLimitIndex,
            "Authentication must run outside the rate-limit observer so every attempt is authenticated.");
    }

    [Fact]
    public void AddGitLabClient_ExposesTheRateLimitTrackerAsReadOnlyToConsumers()
    {
        ServiceCollection services = new();
        services.AddGitLabClient(options => options.AccessToken = TestToken);

        using ServiceProvider provider = services.BuildServiceProvider();

        // The consumer-facing reader and the pipeline-owned tracker must be the same instance, or the
        // tracker would report state nobody ever wrote to it.
        Assert.Same(
            provider.GetRequiredService<IGitLabRateLimitTracker>(),
            provider.GetRequiredService<GitLabRateLimitTracker>());
    }

    private static HttpMessageHandler GetPrimaryHandler(IServiceProvider provider)
    {
        HttpMessageHandler handler = provider.GetRequiredService<IHttpMessageHandlerFactory>()
            .CreateHandler(GitLabClientDefaults.HttpClientName);

        while (handler is DelegatingHandler delegating && delegating.InnerHandler is not null)
        {
            handler = delegating.InnerHandler;
        }

        return handler;
    }
}