namespace GitLab.Client.DependencyInjection;

public sealed class GitLabClientOptions
{
    public const string DefaultHost = "gitlab.com";

    /// <summary>
    ///     The GitLab API root, e.g. <c>https://gitlab.com/api/v4/</c> or <c>https://gitlab.example.com/api/v4/</c>
    ///     for a self-managed instance. Must end with a trailing slash so relative request URIs combine correctly.
    /// </summary>
    public Uri BaseAddress { get; set; } = new($"https://{DefaultHost}/api/v4/");

    /// <summary>The personal access token, OAuth token, or CI job token, depending on <see cref="AuthenticationMode" />.</summary>
    public string? AccessToken { get; set; }

    public GitLabAuthenticationMode AuthenticationMode { get; set; } = GitLabAuthenticationMode.PersonalAccessToken;

    public string UserAgent { get; set; } = "GitLab.Client/1.0";

    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(100);
}