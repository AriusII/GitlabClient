using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     The hash algorithm a new repository's objects are named by (<c>repository_object_format</c>).
///     Fixed at creation time and never changed afterwards.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabRepositoryObjectFormat>))]
public enum GitLabRepositoryObjectFormat
{
    /// <summary>Git's traditional SHA-1 object format.</summary>
    [JsonStringEnumMemberName("sha1")] Sha1,

    /// <summary>The SHA-256 object format.</summary>
    [JsonStringEnumMemberName("sha256")] Sha256
}