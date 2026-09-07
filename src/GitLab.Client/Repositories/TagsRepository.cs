using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Infrastructure.Serialization;
using GitLab.Client.Models;

namespace GitLab.Client.Repositories;

internal sealed class TagsRepository(IGitLabApiConnection connection) : ITagsRepository
{
    public IAsyncEnumerable<GitLabTag> ListAsync(ProjectId projectId, TagListOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("repository")
                .Literal("tags")
                .QueryFrom(options)
                .Build(),
            GitLabJsonContext.Default.GitLabTagArray,
            cancellationToken);
    }

    public Task<GitLabTag> GetAsync(ProjectId projectId, string tagName, CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("repository")
                .Literal("tags")
                .Escaped(tagName)
                .Build(),
            GitLabJsonContext.Default.GitLabTag,
            cancellationToken);
    }

    public Task<GitLabTag> CreateAsync(ProjectId projectId, CreateTagRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("repository")
                .Literal("tags")
                .Build(),
            request,
            GitLabJsonContext.Default.CreateTagRequest,
            GitLabJsonContext.Default.GitLabTag,
            cancellationToken);
    }

    public Task DeleteAsync(ProjectId projectId, string tagName, CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("repository")
                .Literal("tags")
                .Escaped(tagName)
                .Build(),
            cancellationToken);
    }

    public Task<GitLabTagSignature> GetSignatureAsync(ProjectId projectId, string tagName,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("repository")
                .Literal("tags")
                .Escaped(tagName)
                .Literal("signature")
                .Build(),
            GitLabJsonContext.Default.GitLabTagSignature,
            cancellationToken);
    }
}