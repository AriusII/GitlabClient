using System.Net;
using System.Net.Http.Headers;

using GitLab.Client.Configuration;
using GitLab.Client.Infrastructure.RateLimiting;
using GitLab.Client.Tests.TestSupport;

using Microsoft.Extensions.DependencyInjection;

namespace GitLab.Client.Tests.Infrastructure;

/// <summary>
///     Exercises the real DI-built handler chain with only the primary handler swapped out, so these assert
///     what a consumer actually gets rather than what a hand-assembled handler would do.
/// </summary>
public sealed class GitLabAuthenticationHandlerTests : IDisposable
{
    private const string TestToken = "glpat-test-token";

    private static readonly Uri GitLabBaseAddress = new("https://gitlab.example/api/v4/");

    private readonly List<ServiceProvider> _providers = [];

    public void Dispose()
    {
        foreach (ServiceProvider provider in _providers)
        {
            provider.Dispose();
        }
    }

    [Fact]
    public async Task SendAsync_AttachesPrivateToken_ForTheConfiguredInstance()
    {
        using RecordingHttpMessageHandler handler = new(static (_, _) => Ok());
        using HttpClient client = CreateClient(handler, static _ => { });

        using HttpResponseMessage response = await client.GetAsync(
            new Uri("projects/1", UriKind.Relative), TestContext.Current.CancellationToken);

        Assert.Equal(TestToken, Assert.Single(handler.Requests).Headers.GetValues("PRIVATE-TOKEN").Single());
    }

    [Fact]
    public async Task SendAsync_DoesNotAttachTheToken_WhenTheRequestTargetsAnotherHost()
    {
        // The shape a cross-host redirect target or a hostile pagination Link header produces. GitLab's
        // credential headers are custom headers, so nothing inside HttpClient would strip them for us.
        using RecordingHttpMessageHandler handler = new(static (_, _) => Ok());
        using HttpClient client = CreateClient(handler, static _ => { });

        using HttpResponseMessage response = await client.GetAsync(
            new Uri("https://evil.example/api/v4/projects/1"), TestContext.Current.CancellationToken);

        HttpRequestMessage sent = Assert.Single(handler.Requests);
        Assert.False(sent.Headers.Contains("PRIVATE-TOKEN"));
        Assert.False(sent.Headers.Contains("JOB-TOKEN"));
        Assert.Null(sent.Headers.Authorization);
    }

    [Fact]
    public async Task SendAsync_DoesNotAttachTheToken_WhenOnlyTheSchemeDiffers()
    {
        using RecordingHttpMessageHandler handler = new(static (_, _) => Ok());
        using HttpClient client = CreateClient(handler, static _ => { });

        using HttpResponseMessage response = await client.GetAsync(
            new Uri("http://gitlab.example/api/v4/projects/1"), TestContext.Current.CancellationToken);

        Assert.False(Assert.Single(handler.Requests).Headers.Contains("PRIVATE-TOKEN"));
    }

    [Fact]
    public async Task SendAsync_UsesBearerAuthorization_InOAuthMode()
    {
        using RecordingHttpMessageHandler handler = new(static (_, _) => Ok());
        using HttpClient client = CreateClient(handler,
            static options => options.AuthenticationMode = GitLabAuthenticationMode.OAuthBearer);

        using HttpResponseMessage response = await client.GetAsync(
            new Uri("projects/1", UriKind.Relative), TestContext.Current.CancellationToken);

        AuthenticationHeaderValue? authorization = Assert.Single(handler.Requests).Headers.Authorization;
        Assert.Equal("Bearer", authorization?.Scheme);
        Assert.Equal(TestToken, authorization?.Parameter);
    }

    [Fact]
    public async Task SendAsync_UsesJobTokenHeader_InJobTokenMode()
    {
        using RecordingHttpMessageHandler handler = new(static (_, _) => Ok());
        using HttpClient client = CreateClient(handler,
            static options => options.AuthenticationMode = GitLabAuthenticationMode.JobToken);

        using HttpResponseMessage response = await client.GetAsync(
            new Uri("projects/1", UriKind.Relative), TestContext.Current.CancellationToken);

        Assert.Equal(TestToken, Assert.Single(handler.Requests).Headers.GetValues("JOB-TOKEN").Single());
    }

    [Fact]
    public async Task SendAsync_RecordsTheRateLimitHeaders_OnTheTracker()
    {
        using RecordingHttpMessageHandler handler = new(static (_, _) =>
        {
            HttpResponseMessage response = Ok();
            response.Headers.TryAddWithoutValidation("RateLimit-Limit", "2000");
            response.Headers.TryAddWithoutValidation("RateLimit-Remaining", "1999");
            return response;
        });

        using HttpClient client = CreateClient(handler, static _ => { }, out ServiceProvider provider);

        using HttpResponseMessage response = await client.GetAsync(
            new Uri("projects/1", UriKind.Relative), TestContext.Current.CancellationToken);

        GitLabRateLimitSnapshot? snapshot = provider.GetRequiredService<IGitLabRateLimitTracker>().Current;

        Assert.Equal(2000, snapshot?.Limit);
        Assert.Equal(1999, snapshot?.Remaining);
    }

    private static HttpResponseMessage Ok()
    {
        return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{}") };
    }

    private HttpClient CreateClient(HttpMessageHandler primaryHandler, Action<GitLabClientOptions> configureOptions)
    {
        return CreateClient(primaryHandler, configureOptions, out _);
    }

    private HttpClient CreateClient(HttpMessageHandler primaryHandler, Action<GitLabClientOptions> configureOptions,
        out ServiceProvider provider)
    {
        ServiceCollection services = new();
        services.AddGitLabClient(options =>
            {
                options.AccessToken = TestToken;
                options.BaseAddress = GitLabBaseAddress;
                configureOptions(options);
            })
            .ConfigurePrimaryHttpMessageHandler(() => primaryHandler);

        provider = services.BuildServiceProvider();
        _providers.Add(provider);

        return provider.GetRequiredService<IHttpClientFactory>().CreateClient(GitLabClientDefaults.HttpClientName);
    }
}