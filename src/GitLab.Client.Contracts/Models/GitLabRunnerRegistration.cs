namespace GitLab.Client.Models;

/// <summary>
///     The result of creating a runner with <c>POST /user/runners</c>
///     (<c>APIEntitiesCiRunnerRegistrationDetails</c>) - the new runner's id plus the authentication token
///     its <c>gitlab-runner register</c> invocation needs.
///     <para>
///         <see cref="Token" /> is a <em>secret</em> returned exactly once, at creation. No later read of
///         the runner surfaces it again, which is why <see cref="GitLabRunner" /> has no token member.
///         Persist it immediately, and never write it to a log or an exception message.
///     </para>
/// </summary>
public sealed record GitLabRunnerRegistration
{
    /// <summary>The new runner's id, which the <c>/runners/:id</c> routes take.</summary>
    public required long Id { get; init; }

    /// <summary>The freshly minted authentication token. Store it now; it cannot be read back.</summary>
    public required string Token { get; init; }

    /// <summary>
    ///     When the token stops working, or <see langword="null" /> when the instance does not expire runner
    ///     tokens. Declared as a bare <c>string</c> in the spec, with no <c>format: date-time</c>, but it is
    ///     an ISO-8601 timestamp.
    /// </summary>
    public DateTimeOffset? TokenExpiresAt { get; init; }
}