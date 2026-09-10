using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>The file formats <c>POST /groups/:id/dependency_list_exports</c> can produce.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabGroupDependencyListExportType>))]
public enum GitLabGroupDependencyListExportType
{
    /// <summary>A JSON array of dependencies. The default when the request omits the format.</summary>
    [JsonStringEnumMemberName("json_array")]
    JsonArray,

    /// <summary>Comma-separated values.</summary>
    [JsonStringEnumMemberName("csv")] Csv
}