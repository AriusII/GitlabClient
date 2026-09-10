namespace GitLab.Client.Models;

/// <summary>
///     The result of resetting a runner token - <c>POST /runners/reset_registration_token</c>,
///     <c>POST /projects/:id/runners/reset_registration_token</c>,
///     <c>POST /groups/:id/runners/reset_registration_token</c> and
///     <c>POST /runners/:id/reset_authentication_token</c>.
///     <para>
///         <see cref="Token" /> is a <em>secret</em>. GitLab returns it exactly once, at the moment of the
///         reset, and no later read of the runner will ever surface it again - which is why
///         <see cref="GitLabRunner" /> deliberately has no token member. Persist it immediately, and never
///         write it to a log or an exception message.
///     </para>
/// </summary>
public sealed record GitLabRunnerToken
{
    /// <summary>The freshly minted token. Store it now; it cannot be read back.</summary>
    public required string Token { get; init; }

    /// <summary>
    ///     When the token stops working, or <see langword="null" /> when the instance does not expire runner
    ///     tokens. Declared as a bare <c>string</c> in the spec, with no <c>format: date-time</c>, but it is an
    ///     ISO-8601 timestamp.
    /// </summary>
    public DateTimeOffset? TokenExpiresAt { get; init; }
}