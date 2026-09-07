using System.Text.Json;
using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     A Conan recipe file listing keyed by URL - shared by the recipe manifest ("digest") and recipe
///     download-URLs endpoints, which GitLab documents with the identical <c>{ recipe_urls: object }</c>
///     shape. GitLab does not document the entries beyond "an object", so it is surfaced as a raw
///     <see cref="JsonElement" /> rather than a guessed dictionary shape.
/// </summary>
public sealed record GitLabConanRecipeUrls
{
    /// <summary>The recipe file names mapped to their URLs, exactly as GitLab returns them.</summary>
    [JsonPropertyName("recipe_urls")]
    public JsonElement? RecipeUrls { get; init; }
}