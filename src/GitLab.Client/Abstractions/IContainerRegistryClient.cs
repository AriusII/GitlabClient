using System.Text.Json;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps the GitLab "Container registry" API area (<c>/container_registry_event/events</c>): the
///     webhook the container registry itself calls back into GitLab on on push, delete and other
///     registry operations. Not something an ordinary API consumer calls - it exists so a self-managed
///     registry can be pointed at this endpoint.
/// </summary>
public interface IContainerRegistryClient
{
    /// <summary>
    ///     Receives one notification envelope from the container registry
    ///     (<c>POST /container_registry_event/events</c>). The payload is the registry's own
    ///     <c>application/vnd.docker.distribution.events.v1+json</c> shape, which GitLab's OpenAPI document
    ///     does not type, so it is carried as a raw <see cref="JsonElement" /> rather than an invented DTO.
    /// </summary>
    Task IngestEventsAsync(JsonElement payload, CancellationToken cancellationToken = default);
}