using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     The <c>table_size</c> filter vocabulary of <c>GET /databases/:database_name/dictionary/tables</c>.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabDictionaryTableSize>))]
public enum GitLabDictionaryTableSize
{
    [JsonStringEnumMemberName("small")] Small,

    [JsonStringEnumMemberName("medium")] Medium,

    [JsonStringEnumMemberName("large")] Large,

    [JsonStringEnumMemberName("over_limit")]
    OverLimit
}