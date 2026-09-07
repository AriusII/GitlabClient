namespace GitLab.Client.Models;

/// <summary>
///     The link created when a project is shared with a group - the response of
///     <c>POST /projects/:id/share</c>.
/// </summary>
public sealed record GitLabProjectGroupLink
{
    public required long Id { get; init; }

    public long? ProjectId { get; init; }

    public long? GroupId { get; init; }

    /// <summary>The access level the group's members get on the project.</summary>
    public int? GroupAccess { get; init; }

    /// <summary>When the share expires, if it was given an expiry.</summary>
    public DateOnly? ExpiresAt { get; init; }

    /// <summary>The custom member role assigned to the group, if any.</summary>
    public long? MemberRoleId { get; init; }
}