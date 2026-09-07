using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     Request body for <c>POST /projects/:id/invitations</c> and <c>POST /groups/:id/invitations</c>.
///     One call invites several people at once, by email address, by existing user ID, or both.
/// </summary>
public sealed record CreateInvitationRequest
{
    /// <summary>
    ///     The access level to grant: one of 5, 10, 15, 20, 25, 30, 40 or 50 (Minimal Access, Guest, ...,
    ///     Owner). Deliberately a plain <c>int</c> rather than an enum, because 5 is accepted here but not
    ///     by <see cref="UpdateInvitationRequest.AccessLevel" /> - the two endpoints do not share a value
    ///     set. Defaults to 30 (Developer) server-side.
    /// </summary>
    public required int AccessLevel { get; init; }

    /// <summary>
    ///     Email addresses to invite. GitLab names this parameter <c>email</c> even though it carries an
    ///     array, hence the explicit wire name - the snake_case default would send <c>emails</c>, which
    ///     GitLab silently ignores.
    /// </summary>
    [JsonPropertyName("email")]
    public IReadOnlyList<string>? Emails { get; init; }

    /// <summary>
    ///     IDs of existing users to invite, as strings (the spec types the array's items as <c>string</c>).
    ///     Named <c>user_id</c> on the wire despite being an array, for the same reason as
    ///     <see cref="Emails" />.
    /// </summary>
    [JsonPropertyName("user_id")]
    public IReadOnlyList<string>? UserIds { get; init; }

    /// <summary>When the invitation stops being redeemable. A full ISO-8601 timestamp, not a plain date.</summary>
    public DateTimeOffset? ExpiresAt { get; init; }

    /// <summary>Free-text label for whatever triggered the invitation; GitLab records it for analytics.</summary>
    public string? InviteSource { get; init; }

    /// <summary>The ID of a custom member role to attach to the invitation.</summary>
    public long? MemberRoleId { get; init; }
}