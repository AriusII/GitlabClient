namespace GitLab.Client.Models;

/// <summary>
///     Request body for <c>POST /groups/:id/members</c>.
///     <para>
///         A separate type from <see cref="AddMemberRequest" /> - one request DTO per operation, per this
///         library's convention - even though the two shapes are now identical on the wire: GitLab's
///         project route accepts the same <see cref="Username" />/<see cref="InviteSource" />
///         alternative to <see cref="UserId" /> as the group route does. Exactly one of
///         <see cref="UserId" /> and <see cref="Username" /> must be set - GitLab rejects the call when
///         both or neither are present.
///     </para>
/// </summary>
public sealed record AddGroupMemberRequest
{
    /// <summary>
    ///     The role to grant: 10 Guest, 20 Reporter, 30 Developer, 40 Maintainer, 50 Owner, plus the newer
    ///     5 (Minimal Access), 15 (Planner) and 25 (Admin) rungs. An integer on the wire, not a name.
    /// </summary>
    public required int AccessLevel { get; init; }

    /// <summary>The numeric ID of the user to add. Mutually exclusive with <see cref="Username" />.</summary>
    public long? UserId { get; init; }

    /// <summary>
    ///     The username of the user to add, or several separated by commas. Mutually exclusive with
    ///     <see cref="UserId" />.
    /// </summary>
    public string? Username { get; init; }

    /// <summary>When the membership lapses. A plain date (YYYY-MM-DD), not a timestamp.</summary>
    public DateOnly? ExpiresAt { get; init; }

    /// <summary>Free-text label for whatever triggered the addition; GitLab records it for analytics.</summary>
    public string? InviteSource { get; init; }
}