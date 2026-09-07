namespace GitLab.Client.Models;

/// <summary>
///     A feature flag user list (<c>/projects/:id/feature_flags_user_lists</c>) - the set of external
///     user IDs a <c>gitlabUserList</c> strategy targets.
/// </summary>
public sealed record GitLabFeatureFlagUserList
{
    public required long Id { get; init; }

    /// <summary>
    ///     The list's internal ID. This, not <see cref="Id" />, is what every
    ///     <c>feature_flags_user_lists/:iid</c> route takes.
    /// </summary>
    public required long Iid { get; init; }

    public required string Name { get; init; }

    /// <summary>The external user IDs in the list, as GitLab stores them: one comma-separated string.</summary>
    public string? UserXids { get; init; }

    public long? ProjectId { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }

    public DateTimeOffset? UpdatedAt { get; init; }

    /// <summary>The list's path in the GitLab UI, relative to the instance root - not an absolute URL.</summary>
    public string? Path { get; init; }

    /// <summary>The path of the list's edit page in the GitLab UI, relative to the instance root.</summary>
    public string? EditPath { get; init; }
}