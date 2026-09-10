using GitLab.Client.Configuration;
using GitLab.Client.Infrastructure.GraphQL;

using Microsoft.Extensions.Options;

namespace GitLab.Client.Tests.Http.GraphQL;

public sealed class GitLabGraphQLEndpointTests
{
    [Theory]
    [InlineData("https://gitlab.example/api/v4/", "https://gitlab.example/api/graphql")]
    [InlineData("https://gitlab.example/gitlab/api/v4/", "https://gitlab.example/gitlab/api/graphql")]
    public void Resolve_DerivesTheVersionlessGraphQLEndpointFromAStandardRestRoot(
        string restBaseAddress,
        string expectedGraphQLEndpoint)
    {
        GitLabClientOptions options = CreateValidOptions(restBaseAddress);

        Uri endpoint = GitLabGraphQLEndpoint.Resolve(options);

        Assert.Equal(new Uri(expectedGraphQLEndpoint), endpoint);
    }

    [Fact]
    public void Resolve_UsesTheExplicitSameOriginOverrideWithoutDerivingFromTheRestPath()
    {
        GitLabClientOptions options = CreateValidOptions("https://gitlab.example/custom-rest/");
        options.GraphQLEndpoint = new Uri("https://gitlab.example/custom/graphql");

        Uri endpoint = GitLabGraphQLEndpoint.Resolve(options);

        Assert.Equal(options.GraphQLEndpoint, endpoint);
    }

    [Fact]
    public void Resolve_RejectsAnUnconventionalRestPathWithoutAnExplicitGraphQLOverride()
    {
        GitLabClientOptions options = CreateValidOptions("https://gitlab.example/custom-rest/");

        InvalidOperationException exception =
            Assert.Throws<InvalidOperationException>(() => GitLabGraphQLEndpoint.Resolve(options));

        Assert.Contains(nameof(GitLabClientOptions.GraphQLEndpoint), exception.Message,
            StringComparison.Ordinal);
    }

    [Fact]
    public void Validate_AcceptsAnUnconventionalRestRootWhenGraphQLIsNotConfigured()
    {
        GitLabClientOptions options = CreateValidOptions("https://gitlab.example/custom-rest/");

        ValidateOptionsResult result = Validate(options);

        Assert.True(result.Succeeded);
    }

    [Fact]
    public void Validate_AcceptsAnExplicitGraphQLEndpointOnTheRestOrigin()
    {
        GitLabClientOptions options = CreateValidOptions("https://gitlab.example/gitlab/api/v4/");
        options.GraphQLEndpoint = new Uri("https://gitlab.example/gitlab/api/graphql");

        ValidateOptionsResult result = Validate(options);

        Assert.True(result.Succeeded);
    }

    [Theory]
    [InlineData("https://other-gitlab.example/api/graphql")]
    [InlineData("https://gitlab.example:8443/api/graphql")]
    public void Validate_RejectsAGraphQLEndpointOnADifferentOrigin(string graphQLEndpoint)
    {
        GitLabClientOptions options = CreateValidOptions("https://gitlab.example/api/v4/");
        options.GraphQLEndpoint = new Uri(graphQLEndpoint);

        ValidateOptionsResult result = Validate(options);

        Assert.False(result.Succeeded);
        Assert.Contains(result.Failures ?? [],
            failure => failure.Contains("same scheme, host and port", StringComparison.Ordinal));
    }

    [Fact]
    public void Validate_RejectsAGraphQLEndpointUsingAnotherSchemeOnTheSameHost()
    {
        GitLabClientOptions options = CreateValidOptions("http://gitlab.example/api/v4/");
        options.AllowInsecureHttp = true;
        options.GraphQLEndpoint = new Uri("https://gitlab.example/api/graphql");

        ValidateOptionsResult result = Validate(options);

        Assert.False(result.Succeeded);
        Assert.Contains(result.Failures ?? [],
            failure => failure.Contains("same scheme, host and port", StringComparison.Ordinal));
    }

    [Fact]
    public void Validate_RequiresAnExplicitOptInForAPlaintextGraphQLEndpoint()
    {
        GitLabClientOptions options = CreateValidOptions("http://gitlab.example/api/v4/");
        options.GraphQLEndpoint = new Uri("http://gitlab.example/api/graphql");

        ValidateOptionsResult rejected = Validate(options);

        Assert.False(rejected.Succeeded);
        Assert.Contains(rejected.Failures ?? [],
            failure => failure.Contains(nameof(GitLabClientOptions.GraphQLEndpoint), StringComparison.Ordinal));

        options.AllowInsecureHttp = true;
        Assert.True(Validate(options).Succeeded);
    }

    [Theory]
    [InlineData("https://token@gitlab.example/api/graphql")]
    [InlineData("https://gitlab.example/api/graphql?private_token=secret")]
    [InlineData("https://gitlab.example/api/graphql#fragment")]
    public void Validate_RejectsAGraphQLEndpointWithAnUnsafeUriComponent(string graphQLEndpoint)
    {
        GitLabClientOptions options = CreateValidOptions("https://gitlab.example/api/v4/");
        options.GraphQLEndpoint = new Uri(graphQLEndpoint);

        ValidateOptionsResult result = Validate(options);

        Assert.False(result.Succeeded);
        Assert.Contains(result.Failures ?? [],
            failure => failure.Contains(nameof(GitLabClientOptions.GraphQLEndpoint), StringComparison.Ordinal));
    }

    private static GitLabClientOptions CreateValidOptions(string restBaseAddress)
    {
        return new GitLabClientOptions { AccessToken = "glpat-test-token", BaseAddress = new Uri(restBaseAddress) };
    }

    private static ValidateOptionsResult Validate(GitLabClientOptions options)
    {
        return new GitLabClientOptionsValidator().Validate(Options.DefaultName, options);
    }
}