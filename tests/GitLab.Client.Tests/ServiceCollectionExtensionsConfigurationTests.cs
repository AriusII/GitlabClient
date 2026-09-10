using GitLab.Client.Abstractions;
using GitLab.Client.Configuration;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace GitLab.Client.Tests;

/// <summary>
///     Covers <c>AddGitLabClient(IServiceCollection, IConfiguration, string)</c> - binding
///     <see cref="GitLabClientOptions" /> from a configuration section rather than a delegate. The
///     delegate overload's own behaviour (handler pipeline order, singleton lifetimes, resource-client
///     registration, ThrowIfAlreadyRegistered, ...) is covered by <see cref="ServiceCollectionExtensionsTests" />
///     and is exercised identically here through the shared private helper, so it is not re-asserted.
/// </summary>
public sealed class ServiceCollectionExtensionsConfigurationTests
{
    private static readonly Dictionary<string, string?> FullSection = new()
    {
        ["GitLab:BaseAddress"] = "https://gitlab.example/api/v4/",
        ["GitLab:AccessToken"] = "glpat-config-token",
        ["GitLab:AuthenticationMode"] = nameof(GitLabAuthenticationMode.OAuthBearer),
        ["GitLab:UserAgent"] = "GitLab.Client.Tests/1.0",
        ["GitLab:Timeout"] = "00:00:42"
    };

    [Fact]
    public void AddGitLabClient_FromConfiguration_BindsEveryOption()
    {
        IConfiguration configuration = new ConfigurationBuilder().AddInMemoryCollection(FullSection).Build();

        ServiceCollection services = new();
        services.AddGitLabClient(configuration);

        using ServiceProvider provider = services.BuildServiceProvider();

        GitLabClientOptions options = provider.GetRequiredService<IOptions<GitLabClientOptions>>().Value;

        Assert.Equal(new Uri("https://gitlab.example/api/v4/"), options.BaseAddress);
        Assert.Equal("glpat-config-token", options.AccessToken);
        Assert.Equal(GitLabAuthenticationMode.OAuthBearer, options.AuthenticationMode);
        Assert.Equal("GitLab.Client.Tests/1.0", options.UserAgent);
        Assert.Equal(TimeSpan.FromSeconds(42), options.Timeout);
    }

    [Fact]
    public void AddGitLabClient_FromConfiguration_ResolvesTheGitLabClient()
    {
        IConfiguration configuration = new ConfigurationBuilder().AddInMemoryCollection(FullSection).Build();

        ServiceCollection services = new();
        services.AddGitLabClient(configuration);

        using ServiceProvider provider = services.BuildServiceProvider();

        IGitLabClient client = provider.GetRequiredService<IGitLabClient>();

        Assert.NotNull(client.Projects);
    }

    [Fact]
    public void AddGitLabClient_FromConfiguration_MissingBaseAddress_KeepsTheGitLabComDefault()
    {
        // BaseAddress is deliberately absent from this section - the whole point of binding onto an
        // options instance that already carries its own defaults is that an omitted key must not
        // overwrite GitLabClientOptions.BaseAddress with null/empty.
        IConfiguration configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["GitLab:AccessToken"] = "glpat-config-token"
        }).Build();

        ServiceCollection services = new();
        services.AddGitLabClient(configuration);

        using ServiceProvider provider = services.BuildServiceProvider();

        GitLabClientOptions options = provider.GetRequiredService<IOptions<GitLabClientOptions>>().Value;

        Assert.Equal(new Uri("https://gitlab.com/api/v4/"), options.BaseAddress);
        Assert.Equal("glpat-config-token", options.AccessToken);
    }

    [Fact]
    public void AddGitLabClient_FromConfiguration_UsesTheGivenSectionName()
    {
        IConfiguration configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["SelfManaged:BaseAddress"] = "https://gitlab.example/api/v4/",
            ["SelfManaged:AccessToken"] = "glpat-config-token"
        }).Build();

        ServiceCollection services = new();
        services.AddGitLabClient(configuration, "SelfManaged");

        using ServiceProvider provider = services.BuildServiceProvider();

        GitLabClientOptions options = provider.GetRequiredService<IOptions<GitLabClientOptions>>().Value;

        Assert.Equal(new Uri("https://gitlab.example/api/v4/"), options.BaseAddress);
        Assert.Equal("glpat-config-token", options.AccessToken);
    }

    [Fact]
    public void AddGitLabClient_FromConfiguration_WithoutAccessToken_FailsWhenTheGraphIsBuilt()
    {
        IConfiguration configuration = new ConfigurationBuilder().AddInMemoryCollection(
            new Dictionary<string, string?>()).Build();

        ServiceCollection services = new();
        services.AddGitLabClient(configuration);

        using ServiceProvider provider = services.BuildServiceProvider();

        OptionsValidationException exception =
            Assert.Throws<OptionsValidationException>(() => provider.GetRequiredService<IGitLabClient>());

        Assert.Contains(nameof(GitLabClientOptions.AccessToken), exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AddGitLabClient_FromConfiguration_CalledTwice_Throws()
    {
        IConfiguration configuration = new ConfigurationBuilder().AddInMemoryCollection(FullSection).Build();

        ServiceCollection services = new();
        services.AddGitLabClient(configuration);

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            services.AddGitLabClient(configuration));

        Assert.Contains("already been called", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AddGitLabClient_FromConfiguration_NullArguments_Throw()
    {
        ServiceCollection services = new();
        IConfiguration configuration = new ConfigurationBuilder().Build();

        Assert.Throws<ArgumentNullException>(() =>
            ServiceCollectionExtensionsConfigurationTestHelpers.AddGitLabClientWithNullServices(configuration));
        Assert.Throws<ArgumentNullException>(() => services.AddGitLabClient((IConfiguration)null!));
        Assert.Throws<ArgumentNullException>(() => services.AddGitLabClient(configuration, null!));
    }
}

file static class ServiceCollectionExtensionsConfigurationTestHelpers
{
    public static void AddGitLabClientWithNullServices(IConfiguration configuration)
    {
        IServiceCollection services = null!;
        services.AddGitLabClient(configuration);
    }
}