using GitLab.Client.Configuration;

using Microsoft.Extensions.Options;

namespace GitLab.Client.Tests.DependencyInjection;

public sealed class GitLabClientOptionsValidatorTests
{
    private static GitLabClientOptions ValidOptions => new() { AccessToken = "glpat-test-token" };

    [Fact]
    public void Validate_AcceptsTheDefaultsPlusAToken()
    {
        Assert.True(Validate(ValidOptions).Succeeded);
    }

    [Fact]
    public void Validate_RequiresAnAccessToken()
    {
        GitLabClientOptions options = ValidOptions;
        options.AccessToken = "   ";

        AssertFailsWith(options, nameof(GitLabClientOptions.AccessToken));
    }

    [Theory]
    [InlineData("glpat-with\rcarriage-return")]
    [InlineData("glpat-with\nline-feed")]
    [InlineData("glpat-with\0null")]
    public void Validate_RejectsControlCharactersInTheAccessToken(string accessToken)
    {
        // The token goes out via TryAddWithoutValidation, which performs no checking at all, so a newline
        // picked up from a config file would be a request-smuggling vector.
        GitLabClientOptions options = ValidOptions;
        options.AccessToken = accessToken;

        AssertFailsWith(options, nameof(GitLabClientOptions.AccessToken));
    }

    [Fact]
    public void Validate_RejectsANullBaseAddress_WithoutThrowing()
    {
        GitLabClientOptions options = ValidOptions;
        options.BaseAddress = null!;

        AssertFailsWith(options, nameof(GitLabClientOptions.BaseAddress));
    }

    [Theory]
    [InlineData("https://gitlab.example/api/v4")]
    [InlineData("ftp://gitlab.example/api/v4/")]
    [InlineData("file:///c:/gitlab/")]
    [InlineData("https://token@gitlab.example/api/v4/")]
    [InlineData("https://gitlab.example/api/v4/?private_token=secret")]
    [InlineData("https://gitlab.example/api/v4/#fragment")]
    public void Validate_RejectsABaseAddressThatIsNotAnHttpRootEndingInASlash(string baseAddress)
    {
        GitLabClientOptions options = ValidOptions;
        options.BaseAddress = new Uri(baseAddress);

        AssertFailsWith(options, nameof(GitLabClientOptions.BaseAddress));
    }

    [Fact]
    public void Validate_RequiresAnExplicitOptInForPlaintextHttp()
    {
        GitLabClientOptions options = ValidOptions;
        options.BaseAddress = new Uri("http://gitlab.example/api/v4/");

        AssertFailsWith(options, nameof(GitLabClientOptions.BaseAddress));

        options.AllowInsecureHttp = true;
        Assert.True(Validate(options).Succeeded);
    }

    [Fact]
    public void Validate_RejectsAnUndefinedAuthenticationMode()
    {
        GitLabClientOptions options = ValidOptions;
        options.AuthenticationMode = (GitLabAuthenticationMode)99;

        AssertFailsWith(options, nameof(GitLabClientOptions.AuthenticationMode));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_RejectsANonPositiveTimeout(int seconds)
    {
        GitLabClientOptions options = ValidOptions;
        options.Timeout = TimeSpan.FromSeconds(seconds);

        AssertFailsWith(options, nameof(GitLabClientOptions.Timeout));
    }

    [Fact]
    public void Validate_AcceptsAnInfiniteTimeout()
    {
        GitLabClientOptions options = ValidOptions;
        options.Timeout = Timeout.InfiniteTimeSpan;

        Assert.True(Validate(options).Succeeded);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("MyApp/1.0 (unterminated comment")]
    [InlineData("MyApp/1.0\r\nX-Injected: yes")]
    public void Validate_RejectsAUserAgentTheHttpClientCannotParse(string userAgent)
    {
        // Otherwise ParseAdd throws FormatException from inside the AddHttpClient configure delegate, on the
        // first request, with a stack trace that names nothing the consumer wrote.
        GitLabClientOptions options = ValidOptions;
        options.UserAgent = userAgent;

        AssertFailsWith(options, nameof(GitLabClientOptions.UserAgent));
    }

    [Fact]
    public void Validate_ReportsEveryFailureAtOnce()
    {
        GitLabClientOptions options = new()
        {
            AccessToken = null,
            BaseAddress = new Uri("ftp://gitlab.example/api/v4/"),
            AuthenticationMode = (GitLabAuthenticationMode)99,
            Timeout = TimeSpan.Zero,
            UserAgent = string.Empty
        };

        ValidateOptionsResult result = Validate(options);

        Assert.False(result.Succeeded);
        Assert.Equal(5, result.Failures?.Count());
    }

    [Fact]
    public void Validate_SkipsNamedInstances()
    {
        // Named GitLab instances are not supported yet; validating someone else's options as if they were
        // ours would be worse than not validating them.
        Assert.True(Validate(new GitLabClientOptions(), "secondary").Skipped);
    }

    private static ValidateOptionsResult Validate(GitLabClientOptions options, string? name = "")
    {
        return new GitLabClientOptionsValidator().Validate(name, options);
    }

    private static void AssertFailsWith(GitLabClientOptions options, string expectedPropertyName)
    {
        ValidateOptionsResult result = Validate(options);

        Assert.False(result.Succeeded);
        Assert.Contains(result.Failures ?? [],
            failure => failure.Contains(expectedPropertyName, StringComparison.Ordinal));
    }
}