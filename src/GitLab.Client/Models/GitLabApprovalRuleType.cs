using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     The kind of an approval rule (<c>rule_type</c>), shared by project, group and merge-request rules.
/// </summary>
/// <remarks>
///     GitLab stores this as an integer-backed database enum, so the vocabulary is genuinely closed rather
///     than free-form: a value outside these four cannot be written by the API. Modelling it as an enum is
///     therefore safe, and it is what lets the same four names be reused on the request side without the
///     wire value and the C# name drifting apart.
/// </remarks>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabApprovalRuleType>))]
public enum GitLabApprovalRuleType
{
    /// <summary>An ordinary rule with an explicit approver list.</summary>
    [JsonStringEnumMemberName("regular")] Regular,

    /// <summary>A rule generated from a CODEOWNERS section.</summary>
    [JsonStringEnumMemberName("code_owner")]
    CodeOwner,

    /// <summary>A rule driven by a report - see <c>report_type</c>, for example <c>code_coverage</c>.</summary>
    [JsonStringEnumMemberName("report_approver")]
    ReportApprover,

    /// <summary>A rule satisfied by any eligible approver, with no named approvers of its own.</summary>
    [JsonStringEnumMemberName("any_approver")]
    AnyApprover
}