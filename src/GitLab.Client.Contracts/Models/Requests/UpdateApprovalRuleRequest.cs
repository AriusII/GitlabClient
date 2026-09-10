namespace GitLab.Client.Models.Requests;

/// <summary>
///     Request body for <c>PUT /projects/:id/approval_rules/:approval_rule_id</c>.
///     <para>
///         <b>The approver lists are destructive.</b> GitLab removes any approver or group not named in
///         <see cref="UserIds" />, <see cref="GroupIds" /> or <see cref="Usernames" />. Unset members are
///         omitted from the payload rather than sent as null, so leaving all three unset is how you keep the
///         existing approvers - but sending a shorter list than the rule currently has clears the difference.
///     </para>
///     <para>
///         <c>PUT /projects/:id/approval_settings/rules/:approval_rule_id</c> declares the same body apart
///         from two deprecated aliases (<c>users</c>, <c>groups</c>) that this type deliberately omits, so it
///         is reused there rather than duplicated.
///     </para>
/// </summary>
public sealed record UpdateApprovalRuleRequest
{
    public string? Name { get; init; }

    public int? ApprovalsRequired { get; init; }

    public IReadOnlyList<long>? UserIds { get; init; }

    public IReadOnlyList<long>? GroupIds { get; init; }

    public IReadOnlyList<string>? Usernames { get; init; }

    /// <summary>Project scope only.</summary>
    public IReadOnlyList<long>? ProtectedBranchIds { get; init; }

    /// <summary>Project scope only.</summary>
    public bool? AppliesToAllProtectedBranches { get; init; }

    /// <summary>Project scope only.</summary>
    public bool? RemoveHiddenGroups { get; init; }

    /// <summary>Project scope only. A percentage (0-100) below which approval becomes required.</summary>
    public double? CoverageMinimumThreshold { get; init; }

    /// <summary>Project scope only.</summary>
    public int? VulnerabilitiesAllowed { get; init; }

    /// <summary>Project scope only. The security scanners this rule considers.</summary>
    public IReadOnlyList<string>? Scanners { get; init; }

    /// <summary>Project scope only. The vulnerability severity levels this rule considers.</summary>
    public IReadOnlyList<string>? SeverityLevels { get; init; }

    /// <summary>Project scope only. The vulnerability states this rule considers.</summary>
    public IReadOnlyList<string>? VulnerabilityStates { get; init; }
}