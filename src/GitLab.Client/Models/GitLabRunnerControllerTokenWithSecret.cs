namespace GitLab.Client.Models;

/// <summary>
///     A runner controller token together with its secret, as returned by
///     <c>POST /runner_controllers/:id/tokens/:token_id/rotate</c>
///     (<c>APIEntitiesCiRunnerControllerTokenWithToken</c>).
///     <para>
///         <see cref="Token" /> is returned exactly once, by the rotation that minted it; no later read of
///         the token will surface it again, which is why <see cref="GitLabRunnerControllerToken" /> has no
///         token member. Persist it immediately, and never write it to a log or an exception message.
///     </para>
/// </summary>
public sealed record GitLabRunnerControllerTokenWithSecret
{
    public required long Id { get; init; }

    public long? RunnerControllerId { get; init; }

    public string? Description { get; init; }

    public DateTimeOffset? LastUsedAt { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }

    public DateTimeOffset? UpdatedAt { get; init; }

    /// <summary>The freshly minted secret. Store it now; it cannot be read back.</summary>
    public required string Token { get; init; }
}