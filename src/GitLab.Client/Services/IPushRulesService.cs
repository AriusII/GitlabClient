using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for Push rules, sitting between the public
///     <c>IPushRulesClient</c> controller and <c>IPushRulesRepository</c>'s raw GitLab access. Mirrors
///     the repository's method shapes 1:1 today (its implementation is generated); this is the seam
///     where request validation, caching, or cross-resource composition would go once the resource
///     needs more than pass-through.
/// </summary>
internal interface IPushRulesService
{
    Task<GitLabProjectPushRule> GetForProjectAsync(ProjectId projectId, CancellationToken cancellationToken = default);

    Task<GitLabProjectPushRule> CreateForProjectAsync(ProjectId projectId, CreatePushRuleRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabProjectPushRule> UpdateForProjectAsync(ProjectId projectId, UpdatePushRuleRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteForProjectAsync(ProjectId projectId, CancellationToken cancellationToken = default);

    Task<GitLabGroupPushRule> GetForGroupAsync(GroupId groupId, CancellationToken cancellationToken = default);

    Task<GitLabGroupPushRule> CreateForGroupAsync(GroupId groupId, CreatePushRuleRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabGroupPushRule> UpdateForGroupAsync(GroupId groupId, UpdatePushRuleRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteForGroupAsync(GroupId groupId, CancellationToken cancellationToken = default);
}