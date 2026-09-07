namespace GitLab.Client.Models;

/// <summary>
///     Request body for <c>PUT /groups/:id/members/:user_id</c>. Separate from
///     <see cref="UpdateMemberRequest" /> because the group route also accepts a custom member role.
/// </summary>
public sealed record UpdateGroupMemberRequest
{
    /// <summary>
    ///     The role to grant - 10 Guest, 20 Reporter, 30 Developer, 40 Maintainer, 50 Owner (and the newer
    ///     5, 15 and 25 rungs). Required by GitLab even when only <see cref="ExpiresAt" /> is changing.
    /// </summary>
    public required int AccessLevel { get; init; }

    /// <summary>When the membership lapses. A plain date (YYYY-MM-DD), not a timestamp.</summary>
    public DateOnly? ExpiresAt { get; init; }

    /// <summary>The ID of a custom member role to attach to the membership.</summary>
    public long? MemberRoleId { get; init; }
}