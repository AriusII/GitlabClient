namespace GitLab.Client.Models;

/// <summary>
///     Request body for <c>PUT /projects/:id/protected_environments/:name</c> and its group counterpart.
///     There is no <c>name</c> member: the environment (or deployment tier) is addressed by the route.
///     <para>
///         Entries in <see cref="DeployAccessLevels" /> and <see cref="ApprovalRules" /> without an
///         <c>id</c> are added; entries carrying an <c>id</c> are edited, or deleted when their
///         <c>_destroy</c> flag is set.
///     </para>
/// </summary>
public sealed record UpdateProtectedEnvironmentRequest
{
    /// <summary>
    ///     Legacy, environment-wide approval count. Prefer per-rule
    ///     <see cref="ProtectedEnvironmentApprovalRuleRequest.RequiredApprovals" /> in
    ///     <see cref="ApprovalRules" />.
    /// </summary>
    public int? RequiredApprovalCount { get; init; }

    /// <summary>Deploy-access entries to add, edit or delete.</summary>
    public IReadOnlyList<DeployAccessLevelRequest>? DeployAccessLevels { get; init; }

    /// <summary>Approval rules to add, edit or delete.</summary>
    public IReadOnlyList<ProtectedEnvironmentApprovalRuleRequest>? ApprovalRules { get; init; }
}