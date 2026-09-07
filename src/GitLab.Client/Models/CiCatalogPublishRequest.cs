using System.Text.Json;

namespace GitLab.Client.Models;

/// <summary>
///     Request body for <c>POST /projects/:id/catalog/publish</c>. The spec types <c>metadata</c> as a
///     bare, nullable <c>object</c> whose shape is the caller's own component metadata (inputs, spec
///     version, and so on), so it is kept as a raw <see cref="JsonElement" /> rather than an invented DTO.
/// </summary>
public sealed record CiCatalogPublishRequest
{
    /// <summary>The metadata for the release.</summary>
    public required JsonElement? Metadata { get; init; }
}