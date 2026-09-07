using System.Text.Json;

namespace GitLab.Client.Models;

/// <summary>
///     Body for <c>POST /projects/remote-import</c>, which imports a project from an export archive
///     GitLab fetches itself rather than one you upload.
/// </summary>
/// <remarks>
///     <see cref="Url" /> is typically a pre-signed URL whose query string carries credentials for the
///     store holding the archive. Treat a populated request as a secret: never log it, and never fold it
///     into an error message.
/// </remarks>
public sealed record ImportProjectFromRemoteArchiveRequest
{
    /// <summary>Where GitLab should download the archive from. Must be reachable from the GitLab instance.</summary>
    public required Uri Url { get; init; }

    /// <summary>The path (and, unless <see cref="Name" /> is given, the name) of the project to create.</summary>
    public required string Path { get; init; }

    /// <summary>The name of the new project. Defaults to <see cref="Path" />.</summary>
    public string? Name { get; init; }

    /// <summary>
    ///     The ID of the namespace to import into. Defaults to the caller's own namespace. Mutually
    ///     exclusive with <see cref="NamespacePath" />.
    /// </summary>
    public long? NamespaceId { get; init; }

    /// <summary>
    ///     The path of the namespace to import into. Mutually exclusive with <see cref="NamespaceId" />.
    /// </summary>
    public string? NamespacePath { get; init; }

    /// <summary>Replace an existing project of the same name in the same namespace. Defaults to false.</summary>
    public bool? Overwrite { get; init; }

    /// <summary>
    ///     Project settings to apply on top of what the archive carries. The spec types it as an untyped
    ///     object accepting most of the project-creation parameters, so it is carried as a raw
    ///     <see cref="JsonElement" /> rather than forced into a shape GitLab does not promise.
    /// </summary>
    public JsonElement? OverrideParams { get; init; }
}