using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for Snippets, sitting between the public <c>ISnippetsClient</c>
///     controller and <c>ISnippetsRepository</c>'s raw GitLab access. Mirrors the repository's method
///     shapes 1:1 today (its implementation is generated); this is the seam where request validation,
///     caching, or cross-resource composition would go once the resource needs more than pass-through.
/// </summary>
internal interface ISnippetsService
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