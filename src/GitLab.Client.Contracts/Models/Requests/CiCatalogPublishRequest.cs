using System.Text.Json;
using System.Text.Json.Serialization;

namespace GitLab.Client.Models.Requests;

/// <summary>
///     Request body for <c>POST /projects/:id/catalog/publish</c>. The spec types <c>metadata</c> as a
///     bare, nullable <c>object</c> whose shape is the caller's own component metadata (inputs, spec
///     version, and so on), so it is kept as a raw <see cref="JsonElement" /> rather than an invented DTO.
/// </summary>
public sealed record CiCatalogPublishRequest
{
    /// <summary>
    ///     The metadata for the release. The API schema marks the member as required although its value may
    ///     be <c>null</c>. Keep the member in the JSON payload in that case; omitting it changes the request
    ///     shape and makes GitLab reject an otherwise schema-valid payload.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public required JsonElement? Metadata { get; init; }
}