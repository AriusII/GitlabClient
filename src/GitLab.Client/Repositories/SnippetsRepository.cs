using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Infrastructure.Serialization;
using GitLab.Client.Models;

namespace GitLab.Client.Repositories;

internal sealed class SnippetsRepository(IGitLabApiConnection connection) : ISnippetsRepository
{
    public IAsyncEnumerable<GitLabSnippet> ListAsync(SnippetListOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("snippets").QueryFrom(options).Build(),
            GitLabJsonContext.Default.GitLabSnippetArray,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabSnippet> ListAllAsync(AllSnippetListOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("snippets").Literal("all").QueryFrom(options).Build(),
            GitLabJsonContext.Default.GitLabSnippetArray,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabSnippet> ListPublicAsync(SnippetListOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("snippets").Literal("public").QueryFrom(options).Build(),
            GitLabJsonContext.Default.GitLabSnippetArray,
            cancellationToken);
    }

    public Task<GitLabSnippet> GetAsync(long snippetId, CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("snippets").Segment(snippetId).Build(),
            GitLabJsonContext.Default.GitLabSnippet,
            cancellationToken);
    }

    public Task<GitLabSnippet> CreateAsync(CreateSnippetRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("snippets").Build(),
            request,
            GitLabJsonContext.Default.CreateSnippetRequest,
            GitLabJsonContext.Default.GitLabSnippet,
            cancellationToken);
    }

    public Task<GitLabSnippet> UpdateAsync(long snippetId, UpdateSnippetRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            GitLabRouteBuilder.Create("snippets").Segment(snippetId).Build(),
            request,
            GitLabJsonContext.Default.UpdateSnippetRequest,
            GitLabJsonContext.Default.GitLabSnippet,
            cancellationToken);
    }

    public Task DeleteAsync(long snippetId, CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("snippets").Segment(snippetId).Build(),
            cancellationToken);
    }

    public Task<GitLabFileResponse> GetRawAsync(long snippetId, CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            GitLabRouteBuilder.Create("snippets").Segment(snippetId).Literal("raw").Build(),
            cancellationToken);
    }

    public Task<GitLabFileResponse> GetRawFileAsync(long snippetId, string refName, string filePath,
        CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            GitLabRouteBuilder.Create("snippets").Segment(snippetId).Literal("files").Escaped(refName)
                .Escaped(filePath).Literal("raw").Build(),
            cancellationToken);
    }

    public Task<GitLabUserAgentDetail> GetUserAgentDetailAsync(long snippetId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("snippets").Segment(snippetId).Literal("user_agent_detail").Build(),
            GitLabJsonContext.Default.GitLabUserAgentDetail,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabSnippet> ListForProjectAsync(ProjectId projectId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("snippets").Build(),
            GitLabJsonContext.Default.GitLabSnippetArray,
            cancellationToken);
    }

    public Task<GitLabSnippet> GetForProjectAsync(ProjectId projectId, long snippetId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("snippets").Segment(snippetId).Build(),
            GitLabJsonContext.Default.GitLabSnippet,
            cancellationToken);
    }

    public Task<GitLabSnippet> CreateForProjectAsync(ProjectId projectId, CreateSnippetRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("snippets").Build(),
            request,
            GitLabJsonContext.Default.CreateSnippetRequest,
            GitLabJsonContext.Default.GitLabSnippet,
            cancellationToken);
    }

    public Task<GitLabSnippet> UpdateForProjectAsync(ProjectId projectId, long snippetId,
        UpdateSnippetRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("snippets").Segment(snippetId).Build(),
            request,
            GitLabJsonContext.Default.UpdateSnippetRequest,
            GitLabJsonContext.Default.GitLabSnippet,
            cancellationToken);
    }

    public Task DeleteForProjectAsync(ProjectId projectId, long snippetId,
        CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("snippets").Segment(snippetId).Build(),
            cancellationToken);
    }

    public Task<GitLabFileResponse> GetRawForProjectAsync(ProjectId projectId, long snippetId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("snippets").Segment(snippetId)
                .Literal("raw").Build(),
            cancellationToken);
    }

    public Task<GitLabFileResponse> GetRawFileForProjectAsync(ProjectId projectId, long snippetId, string refName,
        string filePath, CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("snippets").Segment(snippetId)
                .Literal("files").Escaped(refName).Escaped(filePath).Literal("raw").Build(),
            cancellationToken);
    }

    public Task<GitLabUserAgentDetail> GetUserAgentDetailForProjectAsync(ProjectId projectId, long snippetId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("snippets").Segment(snippetId)
                .Literal("user_agent_detail").Build(),
            GitLabJsonContext.Default.GitLabUserAgentDetail,
            cancellationToken);
    }
}