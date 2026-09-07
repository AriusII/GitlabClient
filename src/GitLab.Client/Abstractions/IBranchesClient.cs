using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Abstractions;

/// <summary>Wraps the GitLab "Branches" API area (<c>/projects/:id/repository/branches</c>).</summary>
public interface IBranchesClient
{
    Task<GitLabBranch> GetAsync(ProjectId projectId, string branchName, CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabBranch> ListAsync(ProjectId projectId, BranchListOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Creates a branch (<c>POST /projects/:id/repository/branches</c>).</summary>
    Task<GitLabBranch> CreateAsync(ProjectId projectId, CreateBranchRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Deletes one branch (<c>DELETE /projects/:id/repository/branches/:branch</c>).</summary>
    Task DeleteAsync(ProjectId projectId, string branchName, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Deletes every branch already merged into the default branch
    ///     (<c>DELETE /projects/:id/repository/merged_branches</c>). GitLab answers <c>202 Accepted</c>: the
    ///     deletion is queued, so the branches are not necessarily gone when this returns.
    /// </summary>
    Task DeleteMergedAsync(ProjectId projectId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Checks whether a branch exists, without fetching it
    ///     (<c>HEAD /projects/:id/repository/branches/:branch</c>). GitLab answers this route with no body
    ///     at all, so a missing branch comes back as <see langword="false" /> rather than as a
    ///     <see cref="Exceptions.GitLabNotFoundException" />. Every other failure - an expired token, a
    ///     project the caller cannot see - still throws, so "false" always means "no such branch".
    /// </summary>
    /// <param name="projectId">The project that owns the branch.</param>
    /// <param name="branchName">The branch name. Slashes are URL-encoded for you.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <returns>True when the branch exists.</returns>
    Task<bool> ExistsAsync(ProjectId projectId, string branchName, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Protects a single branch (<c>PUT /projects/:id/repository/branches/:branch/protect</c>). This is
    ///     the coarse, branch-level toggle; <see cref="IProtectedBranchesClient" /> is the API to use when
    ///     access has to be expressed per role or per user.
    /// </summary>
    Task<GitLabBranch> ProtectAsync(ProjectId projectId, string branchName, ProtectSingleBranchRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Removes branch-level protection
    ///     (<c>PUT /projects/:id/repository/branches/:branch/unprotect</c>) and returns the updated branch.
    /// </summary>
    Task<GitLabBranch> UnprotectAsync(ProjectId projectId, string branchName,
        CancellationToken cancellationToken = default);
}