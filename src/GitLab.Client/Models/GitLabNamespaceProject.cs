namespace GitLab.Client.Models;

/// <summary>
///     One project inside <c>GET /internal/gitlab_subscriptions/namespaces/:id/projects</c> - the
///     namespace's projects (including every subgroup's) with license data folded in, so a caller does
///     not have to make one extra call per project to fetch it separately.
/// </summary>
public sealed record GitLabNamespaceProject
{
    public required long Id { get; init; }

    public string? PathWithNamespace { get; init; }

    public Uri? WebUrl { get; init; }

    /// <summary>
    ///     "public", "internal" or "private", as GitLab's schema documents it here - a bare string because
    ///     this response schema does not enumerate the value, unlike <see cref="Domain.GitLabVisibility" />
    ///     elsewhere in the library.
    /// </summary>
    public string? Visibility { get; init; }

    public bool? EmptyRepo { get; init; }

    public bool? WikiEnabled { get; init; }

    public GitLabNamespaceProjectLicense? License { get; init; }
}