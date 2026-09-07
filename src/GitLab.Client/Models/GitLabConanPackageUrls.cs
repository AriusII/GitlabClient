using System.Text.Json;
using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     A Conan package file listing keyed by URL - shared by the package manifest ("digest") and package
///     download-URLs endpoints, which GitLab documents with the identical <c>{ package_urls: object }</c>
///     shape. GitLab does not document the entries beyond "an object", so it is surfaced as a raw
///     <see cref="JsonElement" /> rather than a guessed dictionary shape.
/// </summary>
public sealed record GitLabConanPackageUrls
{
    /// <summary>The package file names mapped to their URLs, exactly as GitLab returns them.</summary>
    [JsonPropertyName("package_urls")]
    public JsonElement? PackageUrls { get; init; }
}