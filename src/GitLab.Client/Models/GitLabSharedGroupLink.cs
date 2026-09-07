namespace GitLab.Client.Models;

/// <summary>
///     One entry of a group's <c>shared_with_groups</c>: another group this one has been shared with
///     through <c>POST /groups/:id/share</c>.
/// </summary>
public sealed record GitLabSharedGroupLink
{
    /// <summary>ID of the group the share was granted to.</summary>
    public long? GroupId { get; init; }

    public string? GroupName { get; init; }

    public string? GroupFullPath { get; init; }

    /// <summary>The role the share grants, on GitLab's numeric ladder (10 Guest ... 50 Owner).</summary>
    public int? GroupAccessLevel { get; init; }

    /// <summary>When the share lapses. A plain date, not a timestamp.</summary>
    public DateOnly? ExpiresAt { get; init; }
}