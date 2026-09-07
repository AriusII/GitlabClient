using System.Text.Json;

using GitLab.Client.Abstractions;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Infrastructure.Serialization;

namespace GitLab.Client.Repositories;

internal sealed class ContainerRegistryRepository(IGitLabApiConnection connection) : IContainerRegistryRepository
{
    /// <summary>
    ///     The spec declares neither a request nor a response schema for this route - the body is the
    ///     container registry's own notification envelope
    ///     (<c>application/vnd.docker.distribution.events.v1+json</c>), not something GitLab's OpenAPI
    ///     document types - so it is carried as a raw <see cref="JsonElement" /> rather than an invented DTO.
    /// </summary>
    public Task IngestEventsAsync(JsonElement payload, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("container_registry_event")
                .Literal("events")
                .Build(),
            payload,
            GitLabJsonContext.Default.JsonElement,
            cancellationToken);
    }
}