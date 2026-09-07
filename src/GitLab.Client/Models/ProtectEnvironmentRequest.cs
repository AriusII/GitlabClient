namespace GitLab.Client.Models;

/// <summary>
///     Request body for <c>POST /projects/:id/protected_environments</c> and
///     <c>POST /groups/:id/protected_environments</c>.
/// </summary>
public sealed record ProtectEnvironmentRequest
{
    /// <summary>
    ///     The environment name on a project (<c>production</c>, <c>review/*</c>), or the deployment tier
    ///     on a group - one of <c>production</c>, <c>staging</c>, <c>testing</c>, <c>development</c> or
    ///     <c>other</c>.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>Who may deploy. GitLab requires at least one entry.</summary>
    public required IReadOnlyList<DeployAccessLevelRequest> DeployAccessLevels { get; init; }

    /// <summary>
    ///     Legacy, environment-wide approval count, kept because GitLab still accepts it. Prefer
    ///     <see cref="ApprovalRules" /> with a per-rule
    ///     <see cref="ProtectedEnvironmentApprovalRuleRequest.RequiredApprovals" />, which is what GitLab's
    ///     own documentation points at.
    /// </summary>
    public int? RequiredApprovalCount { get; init; }

    /// <summary>Who must approve a deployment, and how many approvals each entry contributes.</summary>
    public IReadOnlyList<ProtectedEnvironmentApprovalRuleRequest>? ApprovalRules { get; init; }
}