using System.Text.Json;
using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     Every revision of a Conan recipe (Conan v2 protocol only -
///     <c>GET .../conans/:package_name/:package_version/:package_username/:package_channel/revisions</c>).
///     GitLab does not document the shape of each entry beyond "an object", so <see cref="Revisions" /> is
///     surfaced as raw <see cref="JsonElement" /> values rather than a guessed per-entry shape.
/// </summary>
public sealed record GitLabConanRecipeRevisions
{
    /// <summary>The full recipe reference these revisions belong to.</summary>
    [JsonPropertyName("reference")]
    public string? Reference { get; init; }

    /// <summary>The recipe's revisions, newest first as GitLab orders them.</summary>
    [JsonPropertyName("revisions")]
    public IReadOnlyList<JsonElement>? Revisions { get; init; }
}