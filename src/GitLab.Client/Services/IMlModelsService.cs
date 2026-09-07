using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for the model registry's registered models, sitting between the
///     public <c>IMlModelsClient</c> controller and <c>IMlModelsRepository</c>'s raw GitLab access.
///     Mirrors the repository's method shapes 1:1 today (its implementation is generated); this is the
///     seam where request validation, caching, or cross-resource composition would go once the resource
///     needs more than pass-through.
/// </summary>
internal interface IMlModelsService
{
    Task<GitLabMlModel> CreateAsync(ProjectId projectId, CreateMlModelRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabMlModel> GetAsync(ProjectId projectId, string name, CancellationToken cancellationToken = default);

    Task<GitLabMlModel> UpdateAsync(ProjectId projectId, UpdateMlModelRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabMlModel> DeleteAsync(ProjectId projectId, string name, CancellationToken cancellationToken = default);

    Task<GitLabMlModelSearchResult> SearchAsync(ProjectId projectId, MlModelSearchOptions? options = null,
        CancellationToken cancellationToken = default);

    Task<GitLabMlModelVersion> GetLatestVersionAsync(ProjectId projectId, string name,
        CancellationToken cancellationToken = default);

    Task<GitLabMlModelVersion> GetVersionByAliasAsync(ProjectId projectId, string name, string versionAlias,
        CancellationToken cancellationToken = default);
}