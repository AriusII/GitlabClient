namespace GitLab.Client.Models;

/// <summary>
///     Body of <c>POST /token_exchange</c>. Issues a short-lived JWT scoped to
///     <see cref="Audience" />, carrying the requesting user's ID, organization ID and deployment realm
///     as claims - verified by the target modular service against the instance's JWKS.
/// </summary>
public sealed record TokenExchangeRequest
{
    public required GitLabTokenExchangeAudience Audience { get; init; }

    /// <summary>Requested token lifetime in seconds, 1-43200. GitLab defaults to 300 when omitted.</summary>
    public int? ExpiresIn { get; init; }
}