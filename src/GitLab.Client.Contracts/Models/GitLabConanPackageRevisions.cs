using System.Text.Json;
using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     Every revision of a Conan package reference (Conan v2 protocol only -
///     <c>GET .../revisions/:recipe_revision/packages/:conan_package_reference/revisions</c>). GitLab does
///     not document the shape of each entry beyond "an object", so <see cref="Revisions" /> is surfaced as
///     raw <see cref="JsonElement" /> values rather than a guessed per-entry shape.
/// </summary>
public sealed record GitLabConanPackageRevisions
{
    /// <summary>The package reference these revisions belong to. GitLab writes this field camelCase, not snake_case.</summary>
    [JsonPropertyName("packageReference")]
    public string? PackageReference { get; init; }

    /// <summary>The package's revisions, newest first as GitLab orders them.</summary>
    [JsonPropertyName("revisions")]
    public IReadOnlyList<JsonElement>? Revisions { get; init; }
}