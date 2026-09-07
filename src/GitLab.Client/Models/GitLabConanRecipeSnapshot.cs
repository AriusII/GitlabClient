using System.Text.Json;
using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     A Conan recipe snapshot (<c>GET .../conans/:package_name/:package_version/:package_username/:package_channel</c>) -
///     the set of recipe files GitLab holds for one recipe, each mapped to a checksum. GitLab does not
///     document the shape of the entries beyond "an object", so it is surfaced as a raw
///     <see cref="JsonElement" /> rather than a guessed dictionary shape.
/// </summary>
public sealed record GitLabConanRecipeSnapshot
{
    /// <summary>The recipe file names mapped to their checksums, exactly as GitLab returns them.</summary>
    [JsonPropertyName("recipe_snapshot")]
    public JsonElement? RecipeSnapshot { get; init; }
}