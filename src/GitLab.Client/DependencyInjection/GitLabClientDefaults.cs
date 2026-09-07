namespace GitLab.Client.DependencyInjection;

/// <summary>
///     Well-known names this library registers under, so consumers (and a future named-instance overload of
///     <c>AddGitLabClient</c>) can reach exactly the same wiring.
/// </summary>
public static class GitLabClientDefaults
{
    /// <summary>
    ///     The <see cref="IHttpClientFactory" /> logical name of the HTTP client every GitLab request is issued
    ///     through. A named client rather than a typed one is deliberate: the connection is a singleton, and a
    ///     typed client would be captured by it for the process lifetime.
    /// </summary>
    public const string HttpClientName = "GitLab.Client";
}