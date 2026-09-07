using System.Text.Json;
using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     The files GitLab holds for one Conan recipe or package revision (Conan v2 protocol only - shared
///     by the recipe-revision and package-revision file-listing endpoints, which GitLab documents with the
///     identical <c>{ files: object }</c> shape). GitLab does not document the entries beyond "an object",
///     so it is surfaced as a raw <see cref="JsonElement" /> rather than a guessed dictionary shape.
/// </summary>
public sealed record GitLabConanFileList
{
    /// <summary>The file names mapped to their metadata, exactly as GitLab returns them.</summary>
    [JsonPropertyName("files")]
    public JsonElement? Files { get; init; }
}