using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;

using GitLabJsonContext = GitLab.Client.Serialization.GitLabJsonContext;

namespace GitLab.Client.Endpoints;

internal sealed class DraftNotesClient(IGitLabApiConnection connection) : IDraftNotesClient
{
    public IAsyncEnumerable<GitLabDraftNote> ListAsync(ProjectId projectId, long mergeRequestIid,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("merge_requests").Segment(mergeRequestIid)
                .Literal("draft_notes").Build(),
            GitLabJsonContext.Default.GitLabDraftNoteArray,
            cancellationToken);
    }

    public Task<GitLabDraftNote> GetAsync(ProjectId projectId, long mergeRequestIid, long draftNoteId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("merge_requests").Segment(mergeRequestIid)
                .Literal("draft_notes").Segment(draftNoteId).Build(),
            GitLabJsonContext.Default.GitLabDraftNote,
            cancellationToken);
    }

    public Task<GitLabDraftNote> CreateAsync(ProjectId projectId, long mergeRequestIid, CreateDraftNoteRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("merge_requests").Segment(mergeRequestIid)
                .Literal("draft_notes").Build(),
            request,
            GitLabJsonContext.Default.CreateDraftNoteRequest,
            GitLabJsonContext.Default.GitLabDraftNote,
            cancellationToken);
    }

    public Task<GitLabDraftNote> UpdateAsync(ProjectId projectId, long mergeRequestIid, long draftNoteId,
        UpdateDraftNoteRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("merge_requests").Segment(mergeRequestIid)
                .Literal("draft_notes").Segment(draftNoteId).Build(),
            request,
            GitLabJsonContext.Default.UpdateDraftNoteRequest,
            GitLabJsonContext.Default.GitLabDraftNote,
            cancellationToken);
    }

    public Task DeleteAsync(ProjectId projectId, long mergeRequestIid, long draftNoteId,
        CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("merge_requests").Segment(mergeRequestIid)
                .Literal("draft_notes").Segment(draftNoteId).Build(),
            cancellationToken);
    }

    public Task PublishAsync(ProjectId projectId, long mergeRequestIid, long draftNoteId,
        CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("merge_requests").Segment(mergeRequestIid)
                .Literal("draft_notes").Segment(draftNoteId).Literal("publish").Build(),
            cancellationToken);
    }

    public Task PublishAllAsync(ProjectId projectId, long mergeRequestIid, PublishDraftNotesRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("merge_requests").Segment(mergeRequestIid)
                .Literal("draft_notes").Literal("bulk_publish").Build(),
            request,
            GitLabJsonContext.Default.PublishDraftNotesRequest,
            cancellationToken);
    }
}