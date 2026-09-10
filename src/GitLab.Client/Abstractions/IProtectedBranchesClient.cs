using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;
using GitLab.Client.Query;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps the GitLab "Protected Branches" API area (<c>/projects/:id/protected_branches</c> and
///     <c>/groups/:id/protected_branches</c>).
///     <para>
///         The group-scoped half carries a <c>...ForGroupAsync</c> suffix rather than overloading the
///         project methods, because <see cref="ProjectId" /> and <see cref="GroupId" /> both convert
///         implicitly from a number and from a path.
///     </para>
/// </summary>
public interface IProtectedBranchesClient
{
    /// <summary>Lists a project's protected branches and wildcard patterns.</summary>
    IAsyncEnumerable<GitLabProtectedBranch> ListAsync(ProjectId projectId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Lists a project's protected branches using the supplied filter and offset-page settings. The
    ///     returned sequence follows GitLab's subsequent-page links automatically.
    /// </summary>
    /// <remarks>
    ///     <paramref name="options" /> is deliberately required to preserve source compatibility for
    ///     callers that pass a cancellation token positionally to the original overload.
    /// </remarks>
    IAsyncEnumerable<GitLabProtectedBranch> ListAsync(ProjectId projectId,
        ProjectProtectedBranchListOptions? options, CancellationToken cancellationToken = default);

    /// <summary>Retrieves one protected branch of a project by name or wildcard pattern.</summary>
    Task<GitLabProtectedBranch> GetAsync(ProjectId projectId, string name,
        CancellationToken cancellationToken = default);

    /// <summary>Protects a project branch, or several at once through a wildcard pattern.</summary>
    Task<GitLabProtectedBranch> ProtectAsync(ProjectId projectId, ProtectBranchRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Updates an existing project protection
    ///     (<c>PATCH /projects/:id/protected_branches/:name</c>). Only the fields set on
    ///     <paramref name="request" /> are sent.
    /// </summary>
    Task<GitLabProtectedBranch> UpdateAsync(ProjectId projectId, string name,
        UpdateProtectedBranchRequest request, CancellationToken cancellationToken = default);

    /// <summary>Removes a project's protection from a branch or wildcard pattern.</summary>
    Task UnprotectAsync(ProjectId projectId, string name, CancellationToken cancellationToken = default);

    /// <summary>Lists a group's protected branches (<c>GET /groups/:id/protected_branches</c>).</summary>
    IAsyncEnumerable<GitLabProtectedBranch> ListForGroupAsync(GroupId groupId,
        GroupProtectedBranchListOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>Retrieves one protected branch of a group by name or wildcard pattern.</summary>
    Task<GitLabProtectedBranch> GetForGroupAsync(GroupId groupId, string name,
        CancellationToken cancellationToken = default);

    /// <summary>Protects a branch across a group, or several at once through a wildcard pattern.</summary>
    Task<GitLabProtectedBranch> ProtectForGroupAsync(GroupId groupId, ProtectBranchRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Updates an existing group protection (<c>PATCH /groups/:id/protected_branches/:name</c>). Only
    ///     the fields set on <paramref name="request" /> are sent.
    /// </summary>
    Task<GitLabProtectedBranch> UpdateForGroupAsync(GroupId groupId, string name,
        UpdateProtectedBranchRequest request, CancellationToken cancellationToken = default);

    /// <summary>Removes a group's protection from a branch or wildcard pattern.</summary>
    Task UnprotectForGroupAsync(GroupId groupId, string name, CancellationToken cancellationToken = default);
}