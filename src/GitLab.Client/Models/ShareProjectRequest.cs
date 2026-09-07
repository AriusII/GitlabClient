namespace GitLab.Client.Models;

/// <summary>Request body for <c>POST /projects/:id/share</c>.</summary>
public sealed record ShareProjectRequest
{
    /// <summary>The ID of the group to share the project with.</summary>
    public required long GroupId { get; init; }

    /// <summary>
    ///     The access level the group's members get on the project: 10 (Guest), 15 (Planner), 20 (Reporter),
    ///     25 (Developer on some tiers), 30 (Developer), 40 (Maintainer) or 50 (Owner).
    /// </summary>
    public required int GroupAccess { get; init; }

    /// <summary>When the share expires. A calendar date, as GitLab declares it.</summary>
    public DateOnly? ExpiresAt { get; init; }

    /// <summary>The ID of a custom member role to assign to the group instead of a stock access level.</summary>
    public long? MemberRoleId { get; init; }
}