namespace GitLab.Client.Models;

/// <summary>
///     A project- or group-level approval rule (<c>/projects/:id/approval_rules</c>,
///     <c>/groups/:id/approval_rules</c>). These are the policy rules that merge-request-level rules
///     (<see cref="GitLabMergeRequestApprovalRule" />) are copied from.
///     <para>
///         One type covers both scopes: GitLab's <c>APIEntitiesGroupApprovalRule</c> and
///         <c>APIEntitiesProjectApprovalRule</c> are identical except that only the project shape carries
///         <see cref="CoverageMinimumThreshold" />, which therefore stays null on group results.
///     </para>
/// </summary>
public sealed record GitLabApprovalRule
{
    public required long Id { get; init; }

    public required string Name { get; init; }

    public GitLabApprovalRuleType? RuleType { get; init; }

    public IReadOnlyList<GitLabUser>? EligibleApprovers { get; init; }

    public int? ApprovalsRequired { get; init; }

    public IReadOnlyList<GitLabUser>? Users { get; init; }

    public IReadOnlyList<GitLabGroup>? Groups { get; init; }

    public bool? ContainsHiddenGroups { get; init; }

    /// <summary>Free-form; set for report-approver rules, for example <c>code_coverage</c>.</summary>
    public string? ReportType { get; init; }

    /// <summary>
    ///     The protected branches this rule is scoped to. GitLab's OpenAPI document declares this member as a
    ///     bare <c>$ref</c> rather than an array on both the group and project shapes, which is a Grape artifact
    ///     - the wire value is an array.
    /// </summary>
    public IReadOnlyList<GitLabProtectedBranch>? ProtectedBranches { get; init; }

    public bool? AppliesToAllProtectedBranches { get; init; }

    /// <summary>Project-scope only. A percentage (0-100); GitLab declares it as an unformatted JSON number.</summary>
    public double? CoverageMinimumThreshold { get; init; }
}