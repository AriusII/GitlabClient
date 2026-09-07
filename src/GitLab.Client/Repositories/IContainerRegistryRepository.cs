using System.Text.Json;

using GitLab.Client.Abstractions;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the Container registry resource: builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" />
///     and calls <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this
///     layer should build a route or touch <see cref="IGitLabApiConnection" /> directly.
/// </summary>
[GenerateClientLayers(typeof(IContainerRegistryService), typeof(IContainerRegistryClient))]
internal interface IContainerRegistryRepository
{
    Task IngestEventsAsync(JsonElement payload, CancellationToken cancellationToken = default);
}