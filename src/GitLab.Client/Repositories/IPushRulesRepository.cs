using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the Push rules resource: builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" />
///     and calls <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this
///     layer should build a route or touch <see cref="IGitLabApiConnection" /> directly.
/// </summary>
[GenerateClientLayers(typeof(IPushRulesService), typeof(IPushRulesClient))]
internal interface IPushRulesRepository
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