using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;
using GitLab.Client.Query;

namespace GitLab.Client.Abstractions;

/// <summary>
///     The legacy group Epics REST API (<c>/groups/:id/epics</c>), including child and related epic
///     links, epic issues, legacy epic boards, and epic to-dos. New GitLab instances model epics as work
///     items, but these GitLab 19.x REST routes remain available for compatibility.
/// </summary>
public interface IEpicsClient
{
    /// <summary>Streams the group and hierarchy epics selected by <paramref name="options" />.</summary>
    IAsyncEnumerable<GitLabEpic> ListAsync(GroupId groupId, EpicListOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Retrieves one group epic by its group-scoped internal id.</summary>
    Task<GitLabEpic> GetAsync(GroupId groupId, long epicIid, CancellationToken cancellationToken = default);

    /// <summary>Creates a group epic.</summary>
    Task<GitLabEpic> CreateAsync(GroupId groupId, CreateEpicRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Applies a partial update to an epic.</summary>
    Task<GitLabEpic> UpdateAsync(GroupId groupId, long epicIid, UpdateEpicRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Deletes an epic permanently.</summary>
    Task DeleteAsync(GroupId groupId, long epicIid, CancellationToken cancellationToken = default);

    /// <summary>Streams the child epics directly related to an epic.</summary>
    IAsyncEnumerable<GitLabEpic> ListChildEpicsAsync(GroupId groupId, long epicIid,
        CancellationToken cancellationToken = default);

    /// <summary>Creates a child epic and relates it to the supplied parent in the same operation.</summary>
    Task<GitLabEpic> CreateChildEpicAsync(GroupId groupId, long epicIid, CreateChildEpicRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Relates an existing epic, identified by its database id, as a child of the parent epic.</summary>
    Task<GitLabEpic> RelateChildEpicAsync(GroupId groupId, long epicIid, long childEpicId,
        CancellationToken cancellationToken = default);

    /// <summary>Removes a direct parent-child relation without deleting either epic.</summary>
    Task<GitLabEpic> RemoveChildEpicAsync(GroupId groupId, long epicIid, long childEpicId,
        CancellationToken cancellationToken = default);

    /// <summary>Moves a child epic relative to its siblings.</summary>
    Task<GitLabEpic> ReorderChildEpicAsync(GroupId groupId, long epicIid, long childEpicId,
        ReorderEpicRequest request, CancellationToken cancellationToken = default);

    /// <summary>Streams the issues associated with an epic.</summary>
    IAsyncEnumerable<GitLabEpicIssue> ListIssuesAsync(GroupId groupId, long epicIid,
        EpicIssueListOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>Adds an existing issue, identified by its database id, to an epic.</summary>
    Task<GitLabEpicIssueLink> AddIssueAsync(GroupId groupId, long epicIid, long issueId,
        CancellationToken cancellationToken = default);

    /// <summary>Removes an epic-issue association without deleting its issue.</summary>
    Task<GitLabEpicIssueLink> RemoveIssueAsync(GroupId groupId, long epicIid, long epicIssueId,
        CancellationToken cancellationToken = default);

    /// <summary>Moves an epic-issue association relative to other associations.</summary>
    Task<GitLabEpicIssue> ReorderIssueAsync(GroupId groupId, long epicIid, long epicIssueId,
        ReorderEpicIssueRequest request, CancellationToken cancellationToken = default);

    /// <summary>Streams the group's legacy read-only epic boards.</summary>
    IAsyncEnumerable<GitLabEpicBoard> ListBoardsAsync(GroupId groupId,
        CancellationToken cancellationToken = default);

    /// <summary>Retrieves one legacy epic board.</summary>
    Task<GitLabEpicBoard> GetBoardAsync(GroupId groupId, long boardId,
        CancellationToken cancellationToken = default);

    /// <summary>Streams the lists on one legacy epic board.</summary>
    IAsyncEnumerable<GitLabEpicBoardList> ListBoardListsAsync(GroupId groupId, long boardId,
        CancellationToken cancellationToken = default);

    /// <summary>Retrieves one list from a legacy epic board.</summary>
    Task<GitLabEpicBoardList> GetBoardListAsync(GroupId groupId, long boardId, long listId,
        CancellationToken cancellationToken = default);

    /// <summary>Streams the two-way links attached to an epic.</summary>
    IAsyncEnumerable<GitLabRelatedEpic> ListRelatedAsync(GroupId groupId, long epicIid,
        CancellationToken cancellationToken = default);

    /// <summary>Creates a two-way link between this epic and another group's epic.</summary>
    Task<GitLabRelatedEpicLink> CreateRelatedLinkAsync(GroupId groupId, long epicIid,
        CreateRelatedEpicLinkRequest request, CancellationToken cancellationToken = default);

    /// <summary>Deletes a two-way epic link by its relationship id.</summary>
    Task<GitLabRelatedEpicLink> DeleteRelatedLinkAsync(GroupId groupId, long epicIid, long relatedEpicLinkId,
        CancellationToken cancellationToken = default);

    /// <summary>Creates a to-do for the authenticated user on an epic.</summary>
    Task<GitLabTodo> CreateTodoAsync(GroupId groupId, long epicIid, CancellationToken cancellationToken = default);

    /// <summary>Streams related epic links visible throughout a group's hierarchy.</summary>
    IAsyncEnumerable<GitLabRelatedEpic> ListRelatedForGroupAsync(GroupId groupId,
        RelatedEpicLinkListOptions? options = null, CancellationToken cancellationToken = default);
}