using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;
using GitLab.Client.Query;

using GitLabJsonContext = GitLab.Client.Serialization.GitLabJsonContext;

namespace GitLab.Client.Endpoints;

internal sealed class ProtectedTagsClient(IGitLabApiConnection connection) : IProtectedTagsClient
{
    public IAsyncEnumerable<GitLabProtectedTag> ListAsync(ProjectId projectId,
        CancellationToken cancellationToken = default)
    {
        return ListAsync(projectId, null, cancellationToken);
    }

    public IAsyncEnumerable<GitLabProtectedTag> ListAsync(ProjectId projectId, ProtectedTagListOptions? options,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("protected_tags")
                .QueryFrom(options)
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