namespace GitLab.Client.Models;

/// <summary>
///     A protected environment, as returned by the GitLab Protected Environments API
///     (<c>/projects/:id/protected_environments</c>, <c>/groups/:id/protected_environments</c>) - who may
///     deploy to it, and who must approve the deployment.
/// </summary>
public sealed record GitLabProtectedEnvironment
{
    /// <summary>
    ///     The environment name on a project (<c>production</c>, <c>review/*</c>), or the deployment tier
    ///     on a group (<c>production</c>, <c>staging</c>, <c>testing</c>, <c>development</c>, <c>other</c>).
    /// </summary>
    public required string Name { get; init; }

    /// <summary>Users, groups, roles and deploy keys allowed to deploy to this environment.</summary>
    public IReadOnlyList<GitLabDeployAccessLevel>? DeployAccessLevels { get; init; }

    /// <summary>
    ///     Legacy, environment-wide approval count. GitLab marks it deprecated in favour of the per-rule
    ///     <see cref="GitLabProtectedEnvironmentApprovalRule.RequiredApprovals" /> in
    ///     <see cref="ApprovalRules" />; read that first.
    /// </summary>
    public int? RequiredApprovalCount { get; init; }

    /// <summary>Who must approve a deployment to this environment, and how many approvals each entry contributes.</summary>
    public IReadOnlyList<GitLabProtectedEnvironmentApprovalRule>? ApprovalRules { get; init; }
}