using System.Text.Json;

using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for Features, sitting between the public <c>IFeaturesClient</c>
///     controller and <c>IFeaturesRepository</c>'s raw GitLab access. Mirrors the repository's method
///     shapes 1:1 today (its implementation is generated); this is the seam where request validation,
///     caching, or cross-resource composition would go once the resource needs more than pass-through.
/// </summary>
internal interface IFeaturesService
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