using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>What a migration entity migrates: a whole group, or a single project.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabBulkImportEntityType>))]
public enum GitLabBulkImportEntityType
{
    [JsonStringEnumMemberName("group")] Group,

    [JsonStringEnumMemberName("project")] Project
}