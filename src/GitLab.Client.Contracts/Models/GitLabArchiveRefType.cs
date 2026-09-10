using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     Disambiguates what <c>sha</c> names when a branch and a tag share it
///     (<c>GET /projects/:id/repository/archive</c>).
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabArchiveRefType>))]
public enum GitLabArchiveRefType
{
    /// <summary>Resolve the ref as a branch.</summary>
    [JsonStringEnumMemberName("heads")] Heads,

    /// <summary>Resolve the ref as a tag.</summary>
    [JsonStringEnumMemberName("tags")] Tags
}