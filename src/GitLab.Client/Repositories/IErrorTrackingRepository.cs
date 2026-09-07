using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the Error tracking resource: builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" />
///     and calls <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this
///     layer should build a route or touch <see cref="IGitLabApiConnection" /> directly.
/// </summary>
[GenerateClientLayers(typeof(IErrorTrackingService), typeof(IErrorTrackingClient))]
internal interface IErrorTrackingRepository
{
    IAsyncEnumerable<GitLabErrorTrackingClientKey> ListClientKeysAsync(ProjectId projectId,
        CancellationToken cancellationToken = default);

    Task<GitLabErrorTrackingClientKey> CreateClientKeyAsync(ProjectId projectId,
        CancellationToken cancellationToken = default);

    Task<GitLabErrorTrackingClientKey> DeleteClientKeyAsync(ProjectId projectId, long keyId,
        CancellationToken cancellationToken = default);

    Task<GitLabErrorTrackingSettings> GetSettingsAsync(ProjectId projectId,
        CancellationToken cancellationToken = default);

    Task<GitLabErrorTrackingSettings> UpdateSettingsAsync(ProjectId projectId,
        UpdateErrorTrackingSettingsRequest request, CancellationToken cancellationToken = default);

    Task<GitLabErrorTrackingSettings> CreateSettingsAsync(ProjectId projectId,
        CreateErrorTrackingSettingsRequest request, CancellationToken cancellationToken = default);
}