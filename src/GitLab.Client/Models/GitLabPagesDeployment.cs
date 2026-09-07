namespace GitLab.Client.Models;

/// <summary>
///     One published Pages deployment of a project, as reported inside <see cref="GitLabPagesSettings" />.
///     A project has several at once when it publishes to path prefixes - one per parallel deployment.
/// </summary>
public sealed record GitLabPagesDeployment
{
    /// <summary>When the deployment was published.</summary>
    public DateTimeOffset? CreatedAt { get; init; }

    /// <summary>The URL this deployment is served at.</summary>
    public Uri? Url { get; init; }

    /// <summary>
    ///     The path prefix this deployment occupies under the site root, or an empty string for the root
    ///     deployment.
    /// </summary>
    public string? PathPrefix { get; init; }

    /// <summary>The directory inside the published artifact that is served as the site root.</summary>
    public string? RootDirectory { get; init; }
}