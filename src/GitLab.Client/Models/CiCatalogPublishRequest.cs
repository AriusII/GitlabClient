using System.Text.Json;

namespace GitLab.Client.Models;

/// <summary>
///     Request body for <c>POST /projects/:id/catalog/publish</c>. The spec types <c>metadata</c> as a
///     bare, nullable <c>object</c> whose shape is the caller's own component metadata (inputs, spec
///     version, and so on), so it is kept as a raw <see cref="JsonElement" /> rather than an invented DTO.
/// </summary>
public sealed record CiCatalogPublishRequest
{
    /// <summary>
    ///     The metadata for the release. The spec marks the <c>metadata</c> key itself as required even
    ///     though its value may be <c>null</c>; this client's serializer omits null properties from the
    ///     outbound body entirely (see <see cref="Infrastructure.Serialization.GitLabJsonContext" />), so
    ///     setting this to <c>null</c> sends a body with no <c>metadata</c> key at all rather than an
    ///     explicit <c>"metadata": null</c>.
    /// </summary>
    public required JsonElement? Metadata { get; init; }
}