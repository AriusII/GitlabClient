using System.Text.Json;
using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     One resource advertised by a NuGet V3 service index. GitLab follows the protocol's
///     <c>@id</c>, <c>@type</c>, and optional <c>comment</c> convention verbatim.
/// </summary>
/// <remarks>
///     NuGet permits <c>@type</c> to be either one string or an array of strings. It therefore
///     remains a <see cref="JsonElement" /> while the stable envelope is strongly typed, preserving
///     both protocol shapes without reflection or lossy string coercion.
/// </remarks>
public sealed record GitLabNugetServiceResource
{
    /// <summary>The absolute endpoint URI of the advertised NuGet service.</summary>
    [JsonPropertyName("@id")]
    public Uri? AtId { get; init; }

    /// <summary>
    ///     The NuGet service type, represented as either a JSON string or a JSON array of strings as
    ///     permitted by the V3 protocol.
    /// </summary>
    [JsonPropertyName("@type")]
    public JsonElement? AtType { get; init; }

    /// <summary>The optional human-readable description supplied by GitLab.</summary>
    public string? Comment { get; init; }
}