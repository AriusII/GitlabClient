using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the model registry's registered models: builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" />
///     and calls <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this
///     layer should build a route or touch <see cref="IGitLabApiConnection" /> directly.
/// </summary>
[GenerateClientLayers(typeof(IMlModelsService), typeof(IMlModelsClient))]
internal interface IMlModelsRepository
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