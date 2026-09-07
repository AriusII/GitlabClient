using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps the GitLab "Milestones" API area (<c>/projects/:id/milestones</c> and
///     <c>/groups/:id/milestones</c>).
///     <para>
///         Both scopes return the same <see cref="GitLabMilestone" /> entity and take the same create and
///         update bodies, so those types are shared; only the list filters differ, because
///         <c>include_descendants</c> exists on groups alone.
///     </para>
///     <para>
///         Every milestone is addressed by its <c>id</c>, not its <c>iid</c> - the numeric route parameter
///         GitLab calls <c>milestone_id</c> is <see cref="GitLabMilestone.Id" />. Filter by
///         <see cref="GitLabMilestone.Iid" /> through the list options instead.
///     </para>
/// </summary>
public interface IMilestonesClient
{
    /// <summary>Gets one project milestone by id.</summary>
    Task<GitLabMilestone> GetAsync(ProjectId projectId, long milestoneId,
        CancellationToken cancellationToken = default);

    /// <summary>Streams every milestone of a project, following GitLab's <c>Link</c> pagination.</summary>
    IAsyncEnumerable<GitLabMilestone> ListAsync(ProjectId projectId, MilestoneListOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Creates a project milestone.</summary>
    Task<GitLabMilestone> CreateAsync(ProjectId projectId, CreateMilestoneRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Updates a project milestone. Closing or reopening one goes through
    ///     <see cref="UpdateMilestoneRequest.StateEvent" />, not through a <c>state</c> field.
    /// </summary>
    Task<GitLabMilestone> UpdateAsync(ProjectId projectId, long milestoneId, UpdateMilestoneRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Deletes a project milestone. Requires at least the Developer role.</summary>
    Task DeleteAsync(ProjectId projectId, long milestoneId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Promotes a project milestone to a group milestone, moving every issue and merge request
    ///     assignment with it. Irreversible, and GitLab answers <c>201</c> with no body.
    /// </summary>
    Task PromoteAsync(ProjectId projectId, long milestoneId, CancellationToken cancellationToken = default);

    /// <summary>Streams every issue assigned to a project milestone.</summary>
    IAsyncEnumerable<GitLabIssue> ListIssuesAsync(ProjectId projectId, long milestoneId,
        CancellationToken cancellationToken = default);

    /// <summary>Streams every merge request assigned to a project milestone.</summary>
    IAsyncEnumerable<GitLabMergeRequest> ListMergeRequestsAsync(ProjectId projectId, long milestoneId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams the burndown chart events of a project milestone. A Premium/Ultimate feature: on a plan
    ///     without it GitLab answers <c>403</c>.
    /// </summary>
    IAsyncEnumerable<GitLabBurndownEvent> ListBurndownEventsAsync(ProjectId projectId, long milestoneId,
        CancellationToken cancellationToken = default);

    /// <summary>Gets one group milestone by id.</summary>
    Task<GitLabMilestone> GetForGroupAsync(GroupId groupId, long milestoneId,
        CancellationToken cancellationToken = default);

    /// <summary>Streams every milestone of a group, following GitLab's <c>Link</c> pagination.</summary>
    IAsyncEnumerable<GitLabMilestone> ListForGroupAsync(GroupId groupId, GroupMilestoneListOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Creates a group milestone.</summary>
    Task<GitLabMilestone> CreateForGroupAsync(GroupId groupId, CreateMilestoneRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Updates a group milestone.</summary>
    Task<GitLabMilestone> UpdateForGroupAsync(GroupId groupId, long milestoneId, UpdateMilestoneRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Deletes a group milestone.</summary>
    Task DeleteForGroupAsync(GroupId groupId, long milestoneId, CancellationToken cancellationToken = default);

    /// <summary>Streams every issue assigned to a group milestone, across the group's projects.</summary>
    IAsyncEnumerable<GitLabIssue> ListIssuesForGroupAsync(GroupId groupId, long milestoneId,
        CancellationToken cancellationToken = default);

    /// <summary>Streams every merge request assigned to a group milestone, across the group's projects.</summary>
    IAsyncEnumerable<GitLabMergeRequest> ListMergeRequestsForGroupAsync(GroupId groupId, long milestoneId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams the burndown chart events of a group milestone. A Premium/Ultimate feature: on a plan
    ///     without it GitLab answers <c>403</c>.
    /// </summary>
    IAsyncEnumerable<GitLabBurndownEvent> ListBurndownEventsForGroupAsync(GroupId groupId, long milestoneId,
        CancellationToken cancellationToken = default);
}