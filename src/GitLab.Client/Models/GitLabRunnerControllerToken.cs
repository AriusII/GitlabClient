namespace GitLab.Client.Models;

/// <summary>
///     A runner controller token - the credential a runner controller authenticates with
///     (<c>/runner_controllers/:id/tokens</c>).
///     <para>
///         This record carries no secret, by design: the token value itself is returned only by a rotation
///         and is modelled separately as <see cref="GitLabRunnerControllerTokenWithSecret" />, so a token
///         cannot be logged by accident just because a listing was dumped.
///     </para>
/// </summary>
public sealed record GitLabRunnerControllerToken
{
    public required long Id { get; init; }

    /// <summary>The controller this token authenticates.</summary>
    public long? RunnerControllerId { get; init; }

    public string? Description { get; init; }

    /// <summary>When the token was last presented, or <see langword="null" /> if it never has been.</summary>
    public DateTimeOffset? LastUsedAt { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }

    public DateTimeOffset? UpdatedAt { get; init; }
}