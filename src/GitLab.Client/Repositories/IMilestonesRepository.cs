using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the Milestones resource: builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" />
///     and calls <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this
///     layer should build a route or touch <see cref="IGitLabApiConnection" /> directly.
/// </summary>
[GenerateClientLayers(typeof(IMilestonesService), typeof(IMilestonesClient))]
internal interface IMilestonesRepository
{
    Task<GitLabMilestone> GetAsync(ProjectId projectId, long milestoneId,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabMilestone> ListAsync(ProjectId projectId, MilestoneListOptions? options = null,
        CancellationToken cancellationToken = default);

    Task<GitLabMilestone> CreateAsync(ProjectId projectId, CreateMilestoneRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabMilestone> UpdateAsync(ProjectId projectId, long milestoneId, UpdateMilestoneRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(ProjectId projectId, long milestoneId, CancellationToken cancellationToken = default);

    Task PromoteAsync(ProjectId projectId, long milestoneId, CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabIssue> ListIssuesAsync(ProjectId projectId, long milestoneId,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabMergeRequest> ListMergeRequestsAsync(ProjectId projectId, long milestoneId,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabBurndownEvent> ListBurndownEventsAsync(ProjectId projectId, long milestoneId,
        CancellationToken cancellationToken = default);

    Task<GitLabMilestone> GetForGroupAsync(GroupId groupId, long milestoneId,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabMilestone> ListForGroupAsync(GroupId groupId, GroupMilestoneListOptions? options = null,
        CancellationToken cancellationToken = default);

    Task<GitLabMilestone> CreateForGroupAsync(GroupId groupId, CreateMilestoneRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabMilestone> UpdateForGroupAsync(GroupId groupId, long milestoneId, UpdateMilestoneRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteForGroupAsync(GroupId groupId, long milestoneId, CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabIssue> ListIssuesForGroupAsync(GroupId groupId, long milestoneId,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabMergeRequest> ListMergeRequestsForGroupAsync(GroupId groupId, long milestoneId,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabBurndownEvent> ListBurndownEventsForGroupAsync(GroupId groupId, long milestoneId,
        CancellationToken cancellationToken = default);
}