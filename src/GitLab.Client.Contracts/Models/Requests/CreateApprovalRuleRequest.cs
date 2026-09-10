namespace GitLab.Client.Models.Requests;

/// <summary>
///     Request body for <c>POST /projects/:id/approval_rules</c>.
///     <para>
///         <c>POST /projects/:id/approval_settings/rules</c> declares the same body apart from two deprecated
///         aliases (<c>users</c>, <c>groups</c>) that this type deliberately omits, so it is reused there
///         rather than duplicated.
///     </para>
/// </summary>
public sealed record CreateApprovalRuleRequest
{
    public required string Name { get; init; }

    /// <summary>Zero is legitimate - it is exactly what an <c>any_approver</c> rule uses.</summary>
    public required int ApprovalsRequired { get; init; }

    /// <summary>
    ///     Optional, and best left unset: GitLab's own documentation says not to send <c>rule_type</c> when
    ///     building approval rules through the API. It exists here for the report-approver flows that need it.
    /// </summary>
    public GitLabApprovalRuleType? RuleType { get; init; }

    public IReadOnlyList<long>? UserIds { get; init; }

    public IReadOnlyList<long>? GroupIds { get; init; }

    public IReadOnlyList<string>? Usernames { get; init; }

    /// <summary>Project scope only.</summary>
    public IReadOnlyList<long>? ProtectedBranchIds { get; init; }

    /// <summary>Project scope only.</summary>
    public bool? AppliesToAllProtectedBranches { get; init; }

    /// <summary>
    ///     Project scope only. Required when <see cref="RuleType" /> is <c>report_approver</c>; <c>code_coverage</c> is
    ///     the only accepted value.
    /// </summary>
    public string? ReportType { get; init; }

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