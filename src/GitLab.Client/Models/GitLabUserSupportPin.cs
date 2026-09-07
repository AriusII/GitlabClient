namespace GitLab.Client.Models;

/// <summary>
///     A short-lived PIN a user reads out to GitLab Support to prove they own the account
///     (<c>GET /users/:id/support_pin</c>, <c>/user/support_pin</c>).
/// </summary>
public sealed record GitLabUserSupportPin
{
    /// <summary>The PIN itself. Treat it as a credential: it authenticates the account to support staff.</summary>
    public required string Pin { get; init; }

    /// <summary>When the PIN stops being accepted.</summary>
    public DateTimeOffset? ExpiresAt { get; init; }
}