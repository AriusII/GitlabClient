using System.Text.Json;
using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     The upload URLs GitLab hands back for a Conan recipe or package
///     (<c>POST .../upload_urls</c>, <c>POST .../packages/:conan_package_reference/upload_urls</c>) -
///     shared by both endpoints, which GitLab documents with the identical
///     <c>{ upload_urls: object }</c> shape. GitLab does not document the entries beyond "an object", so
///     it is surfaced as a raw <see cref="JsonElement" /> rather than a guessed dictionary shape.
/// </summary>
public sealed record GitLabConanUploadUrls
{
    /// <summary>The file names mapped to the URL each should be uploaded to, exactly as GitLab returns them.</summary>
    [JsonPropertyName("upload_urls")]
    public JsonElement? UploadUrls { get; init; }
}