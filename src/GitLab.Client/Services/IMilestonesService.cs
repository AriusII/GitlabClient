using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for Milestones, sitting between the public <c>IMilestonesClient</c>
///     controller and <c>IMilestonesRepository</c>'s raw GitLab access. Mirrors the repository's method
///     shapes 1:1 today (its implementation is generated); this is the seam where request validation,
///     caching, or cross-resource composition would go once the resource needs more than pass-through.
/// </summary>
internal interface IMilestonesService
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