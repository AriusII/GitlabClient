namespace GitLab.Client.Models;

/// <summary>Request body for <c>POST /groups/:id/share</c>, which shares a group with another group.</summary>
public sealed record ShareGroupRequest
{
    /// <summary>The group to share with. Numeric only - this endpoint does not accept a path.</summary>
    public required long GroupId { get; init; }

    /// <summary>
    ///     The role the invited group's members receive, on GitLab's numeric ladder: 10 Guest, 15 Planner,
    ///     20 Reporter, 25 Admin, 30 Developer, 40 Maintainer, 50 Owner.
    /// </summary>
    public required int GroupAccess { get; init; }

    /// <summary>When the share lapses. A plain date, not a timestamp.</summary>
    public DateOnly? ExpiresAt { get; init; }

    /// <summary>A custom member role to grant instead of the stock one. Ultimate only.</summary>
    public long? MemberRoleId { get; init; }
}