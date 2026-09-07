namespace GitLab.Client.Models;

/// <summary>
///     A project's approval settings (<c>GET /projects/:id/approval_settings</c>): every rule that applies,
///     plus the number of approvals required when no rule matches.
/// </summary>
public sealed record GitLabProjectApprovalSettings
{
    /// <summary>
    ///     The rules in effect - scoped to a single branch when the request supplied a target branch,
    ///     otherwise every rule on the project.
    /// </summary>
    public IReadOnlyList<GitLabProjectApprovalSettingRule>? Rules { get; init; }

    /// <summary>How many approvals a merge request needs when no rule in <see cref="Rules" /> applies to it.</summary>
    public int? FallbackApprovalsRequired { get; init; }
}