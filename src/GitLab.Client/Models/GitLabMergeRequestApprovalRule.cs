namespace GitLab.Client.Models;

/// <summary>
///     A merge-request-level approval rule (<c>/projects/:id/merge_requests/:iid/approval_rules</c>).
///     <para>
///         This is the union of GitLab's two shapes for the same concept:
///         <c>APIEntitiesMergeRequestApprovalRule</c>, returned by the approval-rule endpoints, and
///         <c>APIEntitiesMergeRequestApprovalStateRule</c>, returned inside
///         <see cref="GitLabMergeRequestApprovalState" />, which adds <see cref="CodeOwner" />,
///         <see cref="ApprovedBy" /> and <see cref="Approved" />. Those three are therefore null on results
///         from the rule endpoints.
///     </para>
/// </summary>
public sealed record GitLabMergeRequestApprovalRule
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

    /// <summary>The CODEOWNERS section this rule came from, for example <c>Backend</c>.</summary>
    public string? Section { get; init; }

    public GitLabApprovalSourceRule? SourceRule { get; init; }

    /// <summary>Whether this rule overrides the project-level rule it was created from.</summary>
    public bool? Overridden { get; init; }

    /// <summary>Only populated by the approval-state endpoint.</summary>
    public bool? CodeOwner { get; init; }

    /// <summary>Only populated by the approval-state endpoint.</summary>
    public IReadOnlyList<GitLabUser>? ApprovedBy { get; init; }

    /// <summary>Only populated by the approval-state endpoint.</summary>
    public bool? Approved { get; init; }
}