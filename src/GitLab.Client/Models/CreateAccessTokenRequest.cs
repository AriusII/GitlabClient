namespace GitLab.Client.Models;

/// <summary>
///     Request body for <c>POST /projects/:id/access_tokens</c> and <c>POST /groups/:id/access_tokens</c>.
///     GitLab declares the two bodies identically, so one type serves both.
/// </summary>
public sealed record CreateAccessTokenRequest
{
    public required string Name { get; init; }

    /// <summary>The permissions of the token - for example <c>api</c>, <c>read_repository</c>.</summary>
    public required IReadOnlyList<string> Scopes { get; init; }

    public string? Description { get; init; }

    /// <summary>
    ///     The expiry date. GitLab types this as a plain date here (unlike the date-time it returns on
    ///     <see cref="GitLabAccessToken.ExpiresAt" />). Required in practice when the instance enforces
    ///     "Require personal access token expiry".
    /// </summary>
    public DateOnly? ExpiresAt { get; init; }

    /// <summary>
    ///     The role the token acts with: 10 (Guest), 15 (Planner), 20 (Reporter), 25, 30 (Developer),
    ///     40 (Maintainer) or 50 (Owner). Cannot exceed the caller's own role.
    /// </summary>
    public int? AccessLevel { get; init; }
}