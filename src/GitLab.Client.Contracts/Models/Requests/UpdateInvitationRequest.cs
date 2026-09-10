namespace GitLab.Client.Models.Requests;

/// <summary>
///     Request body for <c>PUT /projects/:id/invitations/:email</c> and its group counterpart. Every
///     member is optional; unset members are omitted rather than sent as null, so they keep their
///     current value.
/// </summary>
public sealed record UpdateInvitationRequest
{
    /// <summary>
    ///     The access level to grant: one of 10, 15, 20, 25, 30, 40 or 50 (Guest through Owner). Unlike
    ///     <see cref="CreateInvitationRequest.AccessLevel" />, 5 (Minimal Access) is not accepted here.
    /// </summary>
    public int? AccessLevel { get; init; }

    /// <summary>When the invitation stops being redeemable. A full ISO-8601 timestamp, not a plain date.</summary>
    public DateTimeOffset? ExpiresAt { get; init; }

    /// <summary>The ID of a custom member role to attach to the invitation.</summary>
    public long? MemberRoleId { get; init; }
}