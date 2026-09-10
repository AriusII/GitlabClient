namespace GitLab.Client.Configuration;

public sealed class GitLabClientOptions
{
    public const string DefaultHost = "gitlab.com";

    /// <summary>
    ///     The GitLab API root, e.g. <c>https://gitlab.com/api/v4/</c> or <c>https://gitlab.example.com/api/v4/</c>
    ///     for a self-managed instance. Must end with a trailing slash so relative request URIs combine correctly.
    /// </summary>
    public Uri BaseAddress { get; set; } = new($"https://{DefaultHost}/api/v4/");

    /// <summary>
    ///     An optional explicit GitLab GraphQL endpoint. When omitted, the endpoint is derived from
    ///     <see cref="BaseAddress" />, for example <c>https://gitlab.example/gitlab/api/v4/</c> becomes
    ///     <c>https://gitlab.example/gitlab/api/graphql</c>.
    /// </summary>
    /// <remarks>
    ///     This URI is constrained to the same origin as <see cref="BaseAddress" />. GraphQL uses the same
    ///     GitLab credential and HTTP pipeline as REST, so permitting a different origin here could disclose
    ///     the configured access token to an unintended host.
    /// </remarks>
    public Uri? GraphQLEndpoint { get; set; }

    /// <summary>
    ///     Allows a plaintext <c>http://</c> endpoint for a deliberately isolated development or test
    ///     instance. Production callers should leave this <see langword="false" /> so access tokens can
    ///     never be sent over an unencrypted connection by configuration accident.
    /// </summary>
    public bool AllowInsecureHttp { get; set; }

    /// <summary>The personal access token, OAuth token, or CI job token, depending on <see cref="AuthenticationMode" />.</summary>
    public string? AccessToken { get; set; }

    public GitLabAuthenticationMode AuthenticationMode { get; set; } = GitLabAuthenticationMode.PersonalAccessToken;

    public string UserAgent { get; set; } = "GitLab.Client/1.0";

    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(100);
}