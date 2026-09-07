using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     The latest revision of a Conan recipe or package (Conan v2 protocol only -
///     <c>GET .../conans/:package_name/:package_version/:package_username/:package_channel/latest</c> and
///     the package-reference equivalent). Revisions are content-addressed, so this is also how a v2 client
///     discovers the current revision hash without listing every one.
/// </summary>
public sealed record GitLabConanRevisionInfo
{
    /// <summary>The revision hash.</summary>
    [JsonPropertyName("revision")]
    public string? Revision { get; init; }

    /// <summary>When this revision was created, as GitLab formats it (not necessarily ISO 8601).</summary>
    [JsonPropertyName("time")]
    public string? Time { get; init; }
}