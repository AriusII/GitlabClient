using System.Text.Json;
using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     A Conan package snapshot (<c>GET .../packages/:conan_package_reference</c>) - the set of package
///     binary files GitLab holds for one package reference, each mapped to a checksum. GitLab does not
///     document the shape of the entries beyond "an object", so it is surfaced as a raw
///     <see cref="JsonElement" /> rather than a guessed dictionary shape.
/// </summary>
public sealed record GitLabConanPackageSnapshot
{
    /// <summary>The package file names mapped to their checksums, exactly as GitLab returns them.</summary>
    [JsonPropertyName("package_snapshot")]
    public JsonElement? PackageSnapshot { get; init; }
}