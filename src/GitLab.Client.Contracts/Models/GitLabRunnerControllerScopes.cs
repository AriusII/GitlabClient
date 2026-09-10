namespace GitLab.Client.Models;

/// <summary>
///     The scope sets assigned to a runner controller. GitLab returns instance-level and runner-level entries in
///     separate arrays from <c>GET /runner_controllers/:id/scopes</c>.
/// </summary>
public sealed record GitLabRunnerControllerScopes
{
    /// <summary>The optional instance-wide scoping entries.</summary>
    public IReadOnlyList<GitLabRunnerControllerScope>? InstanceLevelScopings { get; init; }

    /// <summary>The optional scoping entries for individual runners.</summary>
    public IReadOnlyList<GitLabRunnerControllerScope>? RunnerLevelScopings { get; init; }
}