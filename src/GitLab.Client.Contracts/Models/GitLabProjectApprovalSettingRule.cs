namespace GitLab.Client.Models;

/// <summary>
///     One rule of a project's approval settings (<c>APIEntitiesProjectApprovalSettingRule</c>, returned by
///     <c>/projects/:id/approval_settings</c> and its <c>/rules</c> children).
/// </summary>
/// <remarks>
///     Deliberately a separate type from <see cref="GitLabApprovalRule" /> rather than a widening of it. The
///     two shapes disagree in ways a merged type would lose: this one projects the resolved approver list as
///     <c>approvers</c> where the approval-rules endpoints call it <c>eligible_approvers</c>, and it adds the
///     four security-report members below, which the approval-rules shape has no counterpart for.
/// </remarks>
public sealed record GitLabProjectApprovalSettingRule
{
    public required long Id { get; init; }

    public required string Name { get; init; }

    public GitLabApprovalRuleType? RuleType { get; init; }

    public int? ApprovalsRequired { get; init; }

    /// <summary>The users named on the rule.</summary>
    public IReadOnlyList<GitLabUser>? Users { get; init; }

    /// <summary>The groups named on the rule.</summary>
    public IReadOnlyList<GitLabGroup>? Groups { get; init; }

    /// <summary>
    ///     Everyone who may satisfy this rule, after group membership is expanded. The approval-rules endpoints
    ///     project the same idea as <c>eligible_approvers</c>.
    /// </summary>
    public IReadOnlyList<GitLabUser>? Approvers { get; init; }

    public bool? ContainsHiddenGroups { get; init; }

    /// <summary>Free-form; set for report-approver rules, for example <c>code_coverage</c>.</summary>
    public string? ReportType { get; init; }

    /// <summary>
    ///     The protected branches this rule is scoped to. GitLab's OpenAPI document declares this member as a
    ///     bare reference rather than an array, which is a Grape artifact - the wire value is an array.
    /// </summary>
    public IReadOnlyList<GitLabProtectedBranch>? ProtectedBranches { get; init; }

    public bool? AppliesToAllProtectedBranches { get; init; }

    /// <summary>A percentage (0-100); GitLab declares it as an unformatted JSON number.</summary>
    public double? CoverageMinimumThreshold { get; init; }

    /// <summary>The number of vulnerabilities tolerated before this rule demands approval.</summary>
    public int? VulnerabilitiesAllowed { get; init; }

    /// <summary>The security scanners this rule considers, for example <c>sast</c>.</summary>
    public IReadOnlyList<string>? Scanners { get; init; }

    /// <summary>The vulnerability severity levels this rule considers, for example <c>high</c>.</summary>
    public IReadOnlyList<string>? SeverityLevels { get; init; }

    /// <summary>The vulnerability states this rule considers, for example <c>detected</c>.</summary>
    public IReadOnlyList<string>? VulnerabilityStates { get; init; }
}