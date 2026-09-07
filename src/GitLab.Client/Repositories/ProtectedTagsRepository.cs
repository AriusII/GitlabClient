using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Infrastructure.Serialization;
using GitLab.Client.Models;

namespace GitLab.Client.Repositories;

internal sealed class ProtectedTagsRepository(IGitLabApiConnection connection) : IProtectedTagsRepository
{
    public IAsyncEnumerable<GitLabProtectedTag> ListAsync(ProjectId projectId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("protected_tags")
                .Build(),
            GitLabJsonContext.Default.GitLabProtectedTagArray,
            cancellationToken);
    }

    public Task<GitLabProtectedTag> GetAsync(ProjectId projectId, string name,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("protected_tags")
                .Escaped(name)
                .Build(),
            GitLabJsonContext.Default.GitLabProtectedTag,
            cancellationToken);
    }

    public Task<GitLabProtectedTag> ProtectAsync(ProjectId projectId, ProtectTagRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("protected_tags")
                .Build(),
            request,
            GitLabJsonContext.Default.ProtectTagRequest,
            GitLabJsonContext.Default.GitLabProtectedTag,
            cancellationToken);
    }

    public Task UnprotectAsync(ProjectId projectId, string name, CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("protected_tags")
                .Escaped(name)
                .Build(),
            cancellationToken);
    }
}