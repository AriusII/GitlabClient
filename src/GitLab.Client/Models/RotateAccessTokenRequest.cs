namespace GitLab.Client.Models;

/// <summary>
///     Request body for the token rotate operations (<c>POST .../access_tokens/:token_id/rotate</c> and
///     <c>POST /personal_access_tokens/:id/rotate</c>). Every member is optional: rotating with no body
///     lets GitLab pick the default expiry.
/// </summary>
public sealed record RotateAccessTokenRequest
{
    /// <summary>The expiry date of the replacement token. GitLab types this as a plain date, not a date-time.</summary>
    public DateOnly? ExpiresAt { get; init; }
}