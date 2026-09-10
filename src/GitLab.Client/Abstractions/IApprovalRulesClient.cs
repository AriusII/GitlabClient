using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps the GitLab "Approval rules" API area (<c>/projects/:id/approval_rules</c> and
///     <c>/groups/:id/approval_rules</c>) - the policy layer that merge-request approval rules inherit from.
/// </summary>
/// <remarks>
///     The group scope deliberately offers only list, create and update: GitLab exposes no
///     <c>GET /groups/:id/approval_rules/:approval_rule_id</c> and no delete verb, and calling one would 404.
/// </remarks>
public interface IApprovalRulesClient
{
    /// <summary>
    ///     Streams a project's approval rules (<c>GET /projects/:id/approval_rules</c>), following pagination
    ///     as it goes.
    /// </summary>
    IAsyncEnumerable<GitLabApprovalRule> ListForProjectAsync(ProjectId projectId,
        CancellationToken cancellationToken = default);

    /// <summary>Retrieves one project approval rule (<c>GET /projects/:id/approval_rules/:approval_rule_id</c>).</summary>
    Task<GitLabApprovalRule> GetForProjectAsync(ProjectId projectId, long approvalRuleId,
        CancellationToken cancellationToken = default);

    /// <summary>Creates a project approval rule (<c>POST /projects/:id/approval_rules</c>).</summary>
    Task<GitLabApprovalRule> CreateForProjectAsync(ProjectId projectId, CreateApprovalRuleRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Updates a project approval rule (<c>PUT /projects/:id/approval_rules/:approval_rule_id</c>).
    ///     Approver lists sent here are authoritative - see <see cref="UpdateApprovalRuleRequest" />.
    /// </summary>
    Task<GitLabApprovalRule> UpdateForProjectAsync(ProjectId projectId, long approvalRuleId,
        UpdateApprovalRuleRequest request, CancellationToken cancellationToken = default);

    /// <summary>Deletes a project approval rule (<c>DELETE /projects/:id/approval_rules/:approval_rule_id</c>).</summary>
    Task DeleteForProjectAsync(ProjectId projectId, long approvalRuleId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams a group's approval rules (<c>GET /groups/:id/approval_rules</c>), following pagination as it
    ///     goes. Restricted to group administrators.
    /// </summary>
    IAsyncEnumerable<GitLabApprovalRule> ListForGroupAsync(GroupId groupId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Creates a group approval rule from the legacy project-shaped request. Its project-only values are
    ///     discarded before the request is sent. Prefer <see cref="CreateGroupAsync" /> for new code.
    /// </summary>
    Task<GitLabApprovalRule> CreateForGroupAsync(GroupId groupId, CreateApprovalRuleRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Creates a group approval rule (<c>POST /groups/:id/approval_rules</c>) with the exact group body.
    /// </summary>
    Task<GitLabApprovalRule> CreateGroupAsync(GroupId groupId, CreateGroupApprovalRuleRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        return CreateForGroupAsync(groupId,
            new CreateApprovalRuleRequest
            {
                Name = request.Name,
                ApprovalsRequired = request.ApprovalsRequired,
                RuleType = request.RuleType,
                UserIds = request.UserIds,
                GroupIds = request.GroupIds
            }, cancellationToken);
    }

    /// <summary>
    ///     Updates a group approval rule from the legacy project-shaped request. Its project-only values are
    ///     discarded before the request is sent. Prefer <see cref="UpdateGroupAsync" /> for new code.
    /// </summary>
    Task<GitLabApprovalRule> UpdateForGroupAsync(GroupId groupId, long approvalRuleId,
        UpdateApprovalRuleRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Updates a group approval rule (<c>PUT /groups/:id/approval_rules/:approval_rule_id</c>) with the exact
    ///     group body.
    /// </summary>
    Task<GitLabApprovalRule> UpdateGroupAsync(GroupId groupId, long approvalRuleId,
        UpdateGroupApprovalRuleRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.RuleType is not null)
        {
            throw new NotSupportedException(
                "An implementation of IApprovalRulesClient must implement UpdateGroupAsync to send rule_type.");
        }

        return UpdateForGroupAsync(groupId, approvalRuleId,
            new UpdateApprovalRuleRequest
            {
                Name = request.Name,
                ApprovalsRequired = request.ApprovalsRequired,
                UserIds = request.UserIds,
                GroupIds = request.GroupIds
            }, cancellationToken);
    }

    /// <summary>
    ///     Retrieves a project's approval settings - every applicable rule plus the fallback approval count
    ///     (<c>GET /projects/:id/approval_settings</c>). Pass <paramref name="targetBranch" /> to narrow the
    ///     result to the rules that apply to one branch.
    /// </summary>
    /// <remarks>
    ///     This and the three <c>/approval_settings/rules</c> operations below are marked "Private API subject
    ///     to change" in GitLab's own OpenAPI document. They are wrapped because they are the only way to read
    ///     the effective, branch-scoped rule set, but treat them as less stable than
    ///     <c>/projects/:id/approval_rules</c> and prefer those where they suffice.
    /// </remarks>
    Task<GitLabProjectApprovalSettings> GetSettingsForProjectAsync(ProjectId projectId, string? targetBranch = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Creates a rule through the approval-settings surface
    ///     (<c>POST /projects/:id/approval_settings/rules</c>). Takes the same body as
    ///     <see cref="CreateForProjectAsync" /> but answers with the settings-rule shape, which resolves the
    ///     approver list rather than listing eligible approvers.
    /// </summary>
    Task<GitLabProjectApprovalSettingRule> CreateSettingsRuleForProjectAsync(ProjectId projectId,
        CreateApprovalRuleRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Updates a rule through the approval-settings surface
    ///     (<c>PUT /projects/:id/approval_settings/rules/:approval_rule_id</c>). Approver lists sent here are
    ///     authoritative - see <see cref="UpdateApprovalRuleRequest" />.
    /// </summary>
    Task<GitLabProjectApprovalSettingRule> UpdateSettingsRuleForProjectAsync(ProjectId projectId,
        long approvalRuleId, UpdateApprovalRuleRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Deletes a rule through the approval-settings surface
    ///     (<c>DELETE /projects/:id/approval_settings/rules/:approval_rule_id</c>).
    /// </summary>
    Task DeleteSettingsRuleForProjectAsync(ProjectId projectId, long approvalRuleId,
        CancellationToken cancellationToken = default);
}