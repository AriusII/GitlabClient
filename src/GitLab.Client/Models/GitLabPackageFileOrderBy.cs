using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>The sort key of <c>GET /projects/:id/packages/:package_id/package_files</c>.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabPackageFileOrderBy>))]
public enum GitLabPackageFileOrderBy
{
    [JsonStringEnumMemberName("id")] Id,

    [JsonStringEnumMemberName("created_at")]
    CreatedAt,

    [JsonStringEnumMemberName("file_name")]
    FileName
}