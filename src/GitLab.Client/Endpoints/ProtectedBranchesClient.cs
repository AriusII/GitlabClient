using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;
using GitLab.Client.Query;

using GitLabJsonContext = GitLab.Client.Serialization.GitLabJsonContext;

namespace GitLab.Client.Endpoints;

internal sealed class ProtectedBranchesClient(IGitLabApiConnection connection) : IProtectedBranchesClient
{
    public IAsyncEnumerable<GitLabProtectedBranch> ListAsync(ProjectId projectId,
        CancellationToken cancellationToken = default)
    {
        return ListAsync(projectId, null, cancellationToken);
    }

    public IAsyncEnumerable<GitLabProtectedBranch> ListAsync(ProjectId projectId,
        ProjectProtectedBranchListOptions? options, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("protected_branches")
                .QueryFrom(options)
                .Build(),
            GitLabJsonContext.Default.GitLabProtectedBranchArray,
            cancellationToken);
    }

    public Task<GitLabProtectedBranch> GetAsync(ProjectId projectId, string name,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("protected_branches")
                .Escaped(name)
                .Build(),
            GitLabJsonContext.Default.GitLabProtectedBranch,
            cancellationToken);
    }

    public Task<GitLabProtectedBranch> ProtectAsync(ProjectId projectId, ProtectBranchRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("protected_branches")
                .Build(),
            request,
            GitLabJsonContext.Default.ProtectBranchRequest,
            GitLabJsonContext.Default.GitLabProtectedBranch,
            cancellationToken);
    }

    public Task<GitLabProtectedBranch> UpdateAsync(ProjectId projectId, string name,
        UpdateProtectedBranchRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PatchAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("protected_branches")
                .Escaped(name)
                .Build(),
            request,
            GitLabJsonContext.Default.UpdateProtectedBranchRequest,
            GitLabJsonContext.Default.GitLabProtectedBranch,
            cancellationToken);
    }

    public Task UnprotectAsync(ProjectId projectId, string name, CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("protected_branches")
                .Escaped(name)
                .Build(),
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabProtectedBranch> ListForGroupAsync(GroupId groupId,
        GroupProtectedBranchListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("groups")
                .Segment(groupId)
                .Literal("protected_branches")
                .QueryFrom(options)
                .Build(),
            GitLabJsonContext.Default.GitLabProtectedBranchArray,
            cancellationToken);
    }

    public Task<GitLabProtectedBranch> GetForGroupAsync(GroupId groupId, string name,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GroupBranchRoute(groupId, name),
            GitLabJsonContext.Default.GitLabProtectedBranch,
            cancellationToken);
    }

    public Task<GitLabProtectedBranch> ProtectForGroupAsync(GroupId groupId, ProtectBranchRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("groups")
                .Segment(groupId)
                .Literal("protected_branches")
                .Build(),
            request,
            GitLabJsonContext.Default.ProtectBranchRequest,
            GitLabJsonContext.Default.GitLabProtectedBranch,
            cancellationToken);
    }

    public Task<GitLabProtectedBranch> UpdateForGroupAsync(GroupId groupId, string name,
        UpdateProtectedBranchRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PatchAsync(
            GroupBranchRoute(groupId, name),
            request,
            GitLabJsonContext.Default.UpdateProtectedBranchRequest,
            GitLabJsonContext.Default.GitLabProtectedBranch,
            cancellationToken);
    }

    public Task UnprotectForGroupAsync(GroupId groupId, string name, CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(GroupBranchRoute(groupId, name), cancellationToken);
    }

    private static Uri GroupBranchRoute(GroupId groupId, string name)
    {
        return GitLabRouteBuilder.Create("groups")
            .Segment(groupId)
            .Literal("protected_branches")
            .Escaped(name)
            .Build();
    }
}