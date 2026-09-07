using System.Text.Json;

using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the Features resource: builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" />
///     and calls <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this
///     layer should build a route or touch <see cref="IGitLabApiConnection" /> directly.
/// </summary>
[GenerateClientLayers(typeof(IFeaturesService), typeof(IFeaturesClient))]
internal interface IFeaturesRepository
{
    IAsyncEnumerable<GitLabFeature> ListAsync(CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabFeatureDefinition> ListDefinitionsAsync(CancellationToken cancellationToken = default);

    Task<GitLabFeature> SetAsync(string name, SetFeatureRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(string name, CancellationToken cancellationToken = default);

    Task<JsonElement> GetUnleashFeaturesAsync(ProjectId projectId, UnleashClientOptions? options = null,
        CancellationToken cancellationToken = default);

    Task<JsonElement> GetUnleashClientFeaturesAsync(ProjectId projectId, UnleashClientOptions? options = null,
        CancellationToken cancellationToken = default);

    Task RegisterUnleashClientAsync(ProjectId projectId, UnleashClientRegistrationRequest request,
        CancellationToken cancellationToken = default);

    Task ReportUnleashMetricsAsync(ProjectId projectId, UnleashClientRegistrationRequest request,
        CancellationToken cancellationToken = default);
}