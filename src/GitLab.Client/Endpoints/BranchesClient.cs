using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;
using GitLab.Client.Query;

using GitLabJsonContext = GitLab.Client.Serialization.GitLabJsonContext;

namespace GitLab.Client.Endpoints;

internal sealed class BranchesClient(IGitLabApiConnection connection) : IBranchesClient
{
    public Task<GitLabBranch> GetAsync(ProjectId projectId, string branchName,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            BranchRoute(projectId, branchName).Build(),
            GitLabJsonContext.Default.GitLabBranch,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabBranch> ListAsync(ProjectId projectId, BranchListOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            BranchesRoute(projectId)
                .QueryFrom(options)
                .Build(),
            GitLabJsonContext.Default.GitLabBranchArray,
            cancellationToken);
    }

    public Task<GitLabBranch> CreateAsync(ProjectId projectId, CreateBranchRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            BranchesRoute(projectId).Build(),
            request,
            GitLabJsonContext.Default.CreateBranchRequest,
            GitLabJsonContext.Default.GitLabBranch,
            cancellationToken);
    }

    public Task DeleteAsync(ProjectId projectId, string branchName, CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            BranchRoute(projectId, branchName).Build(),
            cancellationToken);
    }

    public Task DeleteMergedAsync(ProjectId projectId, CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("repository")
                .Literal("merged_branches")
                .Build(),
            cancellationToken);
    }

    public async Task<bool> ExistsAsync(ProjectId projectId, string branchName,
        CancellationToken cancellationToken = default)
    {
        GitLabHeadResponse response = await connection
            .HeadAsync(BranchRoute(projectId, branchName).Build(), cancellationToken)
            .ConfigureAwait(false);

        return response.Exists;
    }

    public Task<GitLabBranch> ProtectAsync(ProjectId projectId, string branchName,
        ProtectSingleBranchRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            BranchRoute(projectId, branchName)
                .Literal("protect")
                .Build(),
            request,
            GitLabJsonContext.Default.ProtectSingleBranchRequest,
            GitLabJsonContext.Default.GitLabBranch,
            cancellationToken);
    }

    public Task<GitLabBranch> UnprotectAsync(ProjectId projectId, string branchName,
        CancellationToken cancellationToken = default)
    {
        // The endpoint declares no parameters but does answer with the updated branch, and the transport
        // has no body-less PUT that deserializes a response. An empty JSON object leaves Grape with the
        // same empty parameter set a body-less PUT would, so send that rather than widen the shared
        // IGitLabApiConnection surface from here.
        return connection.PutAsync(
            BranchRoute(projectId, branchName)
                .Literal("unprotect")
                .Build(),
            UnprotectBranchRequest.Instance,
            GitLabJsonContext.Default.UnprotectBranchRequest,
            GitLabJsonContext.Default.GitLabBranch,
            cancellationToken);
    }

    private static GitLabRouteBuilder BranchesRoute(ProjectId projectId)
    {
        return GitLabRouteBuilder.Create("projects")
            .Segment(projectId)
            .Literal("repository")
            .Literal("branches");
    }

    /// <summary>
    ///     Branch names carry slashes (<c>feature/new-thing</c>), so the name is percent-encoded here
    ///     rather than appended verbatim: an unescaped one becomes a 404 on a route that does not exist,
    ///     which reads like a missing branch instead of a client bug.
    /// </summary>
    private static GitLabRouteBuilder BranchRoute(ProjectId projectId, string branchName)
    {
        return BranchesRoute(projectId).Escaped(branchName);
    }
}