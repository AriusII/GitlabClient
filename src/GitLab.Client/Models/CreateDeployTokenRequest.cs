namespace GitLab.Client.Models;

/// <summary>
///     Request body for <c>POST /projects/:id/deploy_tokens</c> and
///     <c>POST /groups/:id/deploy_tokens</c>. GitLab declares the two bodies identically, so one type
///     serves both.
/// </summary>
public sealed record CreateDeployTokenRequest
{
    /// <summary>The name to give the token. Shown in the UI; it is not part of the credential.</summary>
    public required string Name { get; init; }

    /// <summary>
    ///     What the token may do. Must contain at least one value; see
    ///     <see cref="GitLabDeployTokenScopes" /> for the accepted set.
    /// </summary>
    public required IReadOnlyList<string> Scopes { get; init; }

    /// <summary>
    ///     When the token should stop working. <see langword="null" /> creates a token that never
    ///     expires. GitLab types this one as a full timestamp, not a plain date.
    /// </summary>
    public DateTimeOffset? ExpiresAt { get; init; }

    /// <summary>
    ///     The username half of the credential. Leave <see langword="null" /> to let GitLab assign
    ///     <c>gitlab+deploy-token-{n}</c>.
    /// </summary>
    public string? Username { get; init; }
}