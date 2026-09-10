using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     What a requested migration entity points at on the source instance. Note the <c>_entity</c>
///     suffix: the request vocabulary is deliberately not the same as the response's
///     <see cref="GitLabBulkImportEntityType" />, and sending <c>group</c> where GitLab expects
///     <c>group_entity</c> is a 400.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabBulkImportEntitySourceType>))]
public enum GitLabBulkImportEntitySourceType
{
    [JsonStringEnumMemberName("group_entity")]
    GroupEntity,

    [JsonStringEnumMemberName("project_entity")]
    ProjectEntity
}