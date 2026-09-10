namespace GitLab.Client.Models;

/// <summary>
///     A GitLab organization - the newer top-level container GitLab is building above groups
///     (<c>POST /organizations</c>). An early, evolving surface behind a feature flag as of GitLab 19.4,
///     so every member past <see cref="Id" /> is nullable and <see cref="Visibility" /> stays a bare
///     string: GitLab's response schema does not enumerate it, unlike the create request's
///     <see cref="GitLabOrganizationVisibility" />.
/// </summary>
public sealed record GitLabOrganization
{
    public required long Id { get; init; }

    public string? Uuid { get; init; }

    public string? Name { get; init; }

    public string? Path { get; init; }

    public string? Description { get; init; }

    /// <summary>"private" or "public", as GitLab returns it. See the type summary for why this is a bare string.</summary>
    public string? Visibility { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }

    public DateTimeOffset? UpdatedAt { get; init; }

    public Uri? WebUrl { get; init; }

    public Uri? AvatarUrl { get; init; }
}