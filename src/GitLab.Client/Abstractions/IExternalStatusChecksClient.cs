using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;
using GitLab.Client.Models.Responses;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps the GitLab "External status checks" API area
///     (<c>/projects/:id/external_status_checks</c> and the merge-request status-check routes) - the mechanism
///     a third-party system uses to gate a merge.
/// </summary>
/// <remarks>
///     Three similarly named routes hang off a merge request and do different things:
///     <see cref="ListForMergeRequestAsync" /> reads <c>status_checks</c>, <see cref="SetStatusAsync" /> posts
///     to <c>status_check_responses</c>, and <see cref="RetryAsync" /> posts to
///     <c>status_checks/:id/retry</c>.
/// </remarks>
public interface IExternalStatusChecksClient
{
    /// <summary>
    ///     Streams the external status checks configured on a project
    ///     (<c>GET /projects/:id/external_status_checks</c>), following pagination as it goes.
    /// </summary>
    IAsyncEnumerable<GitLabExternalStatusCheck> ListAsync(ProjectId projectId,
        CancellationToken cancellationToken = default);

    /// <summary>Creates an external status check on a project (<c>POST /projects/:id/external_status_checks</c>).</summary>
    Task<GitLabExternalStatusCheck> CreateAsync(ProjectId projectId, CreateExternalStatusCheckRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Updates an external status check (<c>PUT /projects/:id/external_status_checks/:check_id</c>).
    /// </summary>
    Task<GitLabExternalStatusCheck> UpdateAsync(ProjectId projectId, long checkId,
        UpdateExternalStatusCheckRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Deletes an external status check (<c>DELETE /projects/:id/external_status_checks/:check_id</c>).
    /// </summary>
    Task DeleteAsync(ProjectId projectId, long checkId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams a merge request's status checks and their current results
    ///     (<c>GET /projects/:id/merge_requests/:iid/status_checks</c>).
    /// </summary>
    IAsyncEnumerable<GitLabMergeRequestStatusCheck> ListForMergeRequestAsync(ProjectId projectId,
        long mergeRequestIid, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Reports an external check's verdict for a merge request
    ///     (<c>POST /projects/:id/merge_requests/:iid/status_check_responses</c>). This is the callback an
    ///     external gate posts to.
    /// </summary>
    Task<GitLabStatusCheckResponse> SetStatusAsync(ProjectId projectId, long mergeRequestIid,
        SetStatusCheckStatusRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Asks GitLab to re-run a failed external status check
    ///     (<c>POST /projects/:id/merge_requests/:iid/status_checks/:external_status_check_id/retry</c>).
    ///     GitLab answers <c>202 Accepted</c> with no body, so there is nothing to return.
    /// </summary>
    Task RetryAsync(ProjectId projectId, long mergeRequestIid, long externalStatusCheckId,
        CancellationToken cancellationToken = default);
}