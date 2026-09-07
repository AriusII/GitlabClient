namespace GitLab.Client.Models;

/// <summary>
///     A Support PIN, as returned by <c>GET /user/support_pin</c>, <c>POST /user/support_pin</c> and
///     <c>GET /users/:id/support_pin</c>. GitLab Support asks for it to verify the identity of whoever
///     opened a ticket.
/// </summary>
public sealed record GitLabSupportPin
{
    /// <summary>
    ///     The PIN itself. It is a shared secret with GitLab Support and with nobody else - never log it,
    ///     and never put it in a ticket body or an exception message.
    /// </summary>
    public required string Pin { get; init; }

    /// <summary>When the PIN stops being valid. GitLab issues them with a seven-day life.</summary>
    public DateTimeOffset? ExpiresAt { get; init; }
}