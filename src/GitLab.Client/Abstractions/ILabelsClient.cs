using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps the GitLab "Labels" API area (<c>/projects/:id/labels</c> and <c>/groups/:id/labels</c>).
///     <para>
///         Project labels and group labels are separate entities on the wire, so they are separate types
///         here too: a project label carries <see cref="GitLabLabel.Priority" /> and
///         <see cref="GitLabLabel.IsProjectLabel" />, a <see cref="GitLabGroupLabel" /> neither.
///     </para>
///     <para>
///         Every method that names a label addresses it through the <c>/labels/:name</c> route rather than
///         the <c>name</c> query parameter GitLab also accepts - the query-parameter forms are the ones
///         GitLab deprecated on projects. Label names are free text containing spaces and slashes
///         (<c>type::bug</c>, <c>needs review</c>), and the route builder percent-encodes them, so pass the
///         name raw. GitLab also accepts a numeric label id in place of the name.
///     </para>
///     <para>
///         <see cref="DeleteForGroupByQueryAsync" /> is the one deliberate exception: GitLab has not
///         deprecated the <c>name</c>-query-parameter delete for group labels the way it has for project
///         labels, and unlike <see cref="DeleteForGroupAsync" /> it is the only group-label delete that
///         hands back the removed label.
///     </para>
/// </summary>
public interface ILabelsClient
{
    /// <summary>Streams every label of a project, following GitLab's <c>Link</c> pagination.</summary>
    IAsyncEnumerable<GitLabLabel> ListAsync(ProjectId projectId, LabelListOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets one project label by name (or by numeric id).</summary>
    Task<GitLabLabel> GetAsync(ProjectId projectId, string name, LabelGetOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Creates a project label.</summary>
    Task<GitLabLabel> CreateAsync(ProjectId projectId, CreateLabelRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Updates a project label. Renaming goes through <see cref="UpdateLabelRequest.NewName" />; the
    ///     current name stays in the route.
    /// </summary>
    Task<GitLabLabel> UpdateAsync(ProjectId projectId, string name, UpdateLabelRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Deletes a project label. GitLab answers <c>200</c> echoing the deleted label; the body is not
    ///     surfaced, because the label no longer exists to be acted on.
    /// </summary>
    Task DeleteAsync(ProjectId projectId, string name, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Promotes a project label to a group label, moving every issue and merge request assignment with
    ///     it. Irreversible, and it fails on a project that has no parent group.
    ///     <para>
    ///         GitLab answers with the resulting group label, but this returns <see cref="Task" />: the
    ///         transport has no body-less <c>PUT</c> that also deserializes a response, and sending a body
    ///         to an endpoint that declares none is worse than dropping it. Re-read it with
    ///         <see cref="GetForGroupAsync" /> if you need the promoted label.
    ///     </para>
    /// </summary>
    Task PromoteAsync(ProjectId projectId, string name, CancellationToken cancellationToken = default);

    /// <summary>Streams every label of a group, following GitLab's <c>Link</c> pagination.</summary>
    IAsyncEnumerable<GitLabGroupLabel> ListForGroupAsync(GroupId groupId, GroupLabelListOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets one group label by name (or by numeric id).</summary>
    Task<GitLabGroupLabel> GetForGroupAsync(GroupId groupId, string name, GroupLabelGetOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Creates a group label.</summary>
    Task<GitLabGroupLabel> CreateForGroupAsync(GroupId groupId, CreateGroupLabelRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Updates a group label addressed by name in the route.</summary>
    Task<GitLabGroupLabel> UpdateForGroupAsync(GroupId groupId, string name, UpdateGroupLabelRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Updates a group label through the collection route <c>PUT /groups/:id/labels</c>, which identifies
    ///     the label in the body instead of the path. Prefer <see cref="UpdateForGroupAsync" />; this form
    ///     exists for callers holding a numeric <see cref="UpdateGroupLabelByIdRequest.LabelId" /> and no name.
    /// </summary>
    Task<GitLabGroupLabel> UpdateForGroupByLabelIdAsync(GroupId groupId, UpdateGroupLabelByIdRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Deletes a group label. As with <see cref="DeleteAsync" />, GitLab's echoed body is discarded.</summary>
    Task DeleteForGroupAsync(GroupId groupId, string name, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Deletes a group label through the collection route <c>DELETE /groups/:id/labels</c>, which
    ///     identifies the label with a <c>name</c> query parameter instead of a path segment, and returns
    ///     the label GitLab just removed. Prefer <see cref="DeleteForGroupAsync" /> unless you need that
    ///     returned body.
    /// </summary>
    Task<GitLabGroupLabel> DeleteForGroupByQueryAsync(GroupId groupId, string name,
        CancellationToken cancellationToken = default);
}