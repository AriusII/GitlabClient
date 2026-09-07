using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the Snippets resource: builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" />
///     and calls <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this
///     layer should build a route or touch <see cref="IGitLabApiConnection" /> directly.
/// </summary>
[GenerateClientLayers(typeof(ISnippetsService), typeof(ISnippetsClient))]
internal interface ISnippetsRepository
{
    IAsyncEnumerable<GitLabSnippet> ListAsync(SnippetListOptions? options = null,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabSnippet> ListAllAsync(AllSnippetListOptions? options = null,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabSnippet> ListPublicAsync(SnippetListOptions? options = null,
        CancellationToken cancellationToken = default);

    Task<GitLabSnippet> GetAsync(long snippetId, CancellationToken cancellationToken = default);

    Task<GitLabSnippet> CreateAsync(CreateSnippetRequest request, CancellationToken cancellationToken = default);

    Task<GitLabSnippet> UpdateAsync(long snippetId, UpdateSnippetRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(long snippetId, CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> GetRawAsync(long snippetId, CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> GetRawFileAsync(long snippetId, string refName, string filePath,
        CancellationToken cancellationToken = default);

    Task<GitLabUserAgentDetail> GetUserAgentDetailAsync(long snippetId,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabSnippet> ListForProjectAsync(ProjectId projectId,
        CancellationToken cancellationToken = default);

    Task<GitLabSnippet> GetForProjectAsync(ProjectId projectId, long snippetId,
        CancellationToken cancellationToken = default);

    Task<GitLabSnippet> CreateForProjectAsync(ProjectId projectId, CreateSnippetRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabSnippet> UpdateForProjectAsync(ProjectId projectId, long snippetId, UpdateSnippetRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteForProjectAsync(ProjectId projectId, long snippetId, CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> GetRawForProjectAsync(ProjectId projectId, long snippetId,
        CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> GetRawFileForProjectAsync(ProjectId projectId, long snippetId, string refName,
        string filePath, CancellationToken cancellationToken = default);

    Task<GitLabUserAgentDetail> GetUserAgentDetailForProjectAsync(ProjectId projectId, long snippetId,
        CancellationToken cancellationToken = default);
}