namespace GitLab.Client.Models;

/// <summary>
///     Request body for <c>POST /users/:user_id/impersonation_tokens</c>. Administrators only.
///     <para>
///         The resulting token acts as the target user for both API calls and Git reads and writes, and
///         is not visible to that user on their profile page, so treat creating one as a privileged
///         operation.
///     </para>
/// </summary>
public sealed record CreateImpersonationTokenRequest
{
    public required string Name { get; init; }

    /// <summary>
    ///     The permissions of the token - see <see cref="GitLabTokenScopes" />. Mutually exclusive with
    ///     <see cref="GranularScopes" />.
    /// </summary>
    public IReadOnlyList<string>? Scopes { get; init; }

    /// <summary>
    ///     Granular permissions to assign instead of the coarse <see cref="Scopes" />. Mutually
    ///     exclusive with <see cref="Scopes" />.
    /// </summary>
    public IReadOnlyList<GitLabGranularScope>? GranularScopes { get; init; }

    public string? Description { get; init; }

    /// <summary>The expiry date. GitLab types this as a plain date, not a date-time.</summary>
    public DateOnly? ExpiresAt { get; init; }
}