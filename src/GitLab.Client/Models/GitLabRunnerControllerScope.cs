namespace GitLab.Client.Models;

/// <summary>
///     One scoping entry on a runner controller - the set of runners whose jobs it evaluates.
///     <para>
///         GitLab has two scoping entities and this record models both, because a controller's scope
///         listing (<c>GET /runner_controllers/:id/scopes</c>) returns them side by side.
///         <see cref="RunnerId" /> is what separates them: it is set for a runner-level scope
///         (<c>APIEntitiesCiRunnerControllerRunnerLevelScoping</c>) and <see langword="null" /> for the
///         instance-level scope (<c>APIEntitiesCiRunnerControllerInstanceLevelScoping</c>), which covers
///         every runner on the instance.
///     </para>
///     <para>
///         The two are mutually exclusive: a controller holding the instance scope cannot take runner
///         scopes, and adding a second instance scope is answered with a
///         <see cref="Abstractions.Exceptions.GitLabConflictException" />.
///     </para>
/// </summary>
public sealed record GitLabRunnerControllerScope
{
    /// <summary>The scoped runner, or <see langword="null" /> when this is the instance-wide scope.</summary>
    public long? RunnerId { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }

    public DateTimeOffset? UpdatedAt { get; init; }
}