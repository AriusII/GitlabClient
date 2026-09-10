using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;
using GitLab.Client.Models.Responses;

using GitLabJsonContext = GitLab.Client.Serialization.GitLabJsonContext;

namespace GitLab.Client.Endpoints;

internal sealed class ExternalStatusChecksClient(IGitLabApiConnection connection)
    : IExternalStatusChecksClient
{
    public IAsyncEnumerable<GitLabExternalStatusCheck> ListAsync(ProjectId projectId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("external_status_checks").Build(),
            GitLabJsonContext.Default.GitLabExternalStatusCheckArray,
            cancellationToken);
    }

    public Task<GitLabExternalStatusCheck> CreateAsync(ProjectId projectId,
        CreateExternalStatusCheckRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("external_status_checks").Build(),
            request,
            GitLabJsonContext.Default.CreateExternalStatusCheckRequest,
            GitLabJsonContext.Default.GitLabExternalStatusCheck,
            cancellationToken);
    }

    public Task<GitLabExternalStatusCheck> UpdateAsync(ProjectId projectId, long checkId,
        UpdateExternalStatusCheckRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("external_status_checks")
                .Segment(checkId).Build(),
            request,
            GitLabJsonContext.Default.UpdateExternalStatusCheckRequest,
            GitLabJsonContext.Default.GitLabExternalStatusCheck,
            cancellationToken);
    }

    public Task DeleteAsync(ProjectId projectId, long checkId, CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("external_status_checks")
                .Segment(checkId).Build(),
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabMergeRequestStatusCheck> ListForMergeRequestAsync(ProjectId projectId,
        long mergeRequestIid, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            MergeRequestRoute(projectId, mergeRequestIid).Literal("status_checks").Build(),
            GitLabJsonContext.Default.GitLabMergeRequestStatusCheckArray,
            cancellationToken);
    }

    public Task<GitLabStatusCheckResponse> SetStatusAsync(ProjectId projectId, long mergeRequestIid,
        SetStatusCheckStatusRequest request, CancellationToken cancellationToken = default)
    {
        // "status_check_responses" - plural, and NOT the "status_checks" route the list above reads.
        return connection.PostAsync(
            MergeRequestRoute(projectId, mergeRequestIid).Literal("status_check_responses").Build(),
            request,
            GitLabJsonContext.Default.SetStatusCheckStatusRequest,
            GitLabJsonContext.Default.GitLabStatusCheckResponse,
            cancellationToken);
    }

    public Task RetryAsync(ProjectId projectId, long mergeRequestIid, long externalStatusCheckId,
        CancellationToken cancellationToken = default)
    {
        // A body-less POST answered with 202 Accepted and no content - hence the no-content PostAsync overload.
        return connection.PostAsync(
            MergeRequestRoute(projectId, mergeRequestIid)
                .Literal("status_checks")
                .Segment(externalStatusCheckId)
                .Literal("retry")
                .Build(),
            cancellationToken);
    }

    /// <summary>
    ///     The three merge-request-scoped routes share this prefix, addressed by the merge request's
    ///     <c>iid</c>. The builder is mutable and chained, so each call returns a fresh one.
    /// </summary>
    private static GitLabRouteBuilder MergeRequestRoute(ProjectId projectId, long mergeRequestIid)
    {
        return GitLabRouteBuilder.Create("projects")
            .Segment(projectId)
            .Literal("merge_requests")
            .Segment(mergeRequestIid);
    }
}