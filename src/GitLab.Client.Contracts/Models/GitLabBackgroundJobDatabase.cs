using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     The Rails database a batched background migration, batched background operation or schema
///     migration runs against. GitLab defaults to <see cref="Main" /> wherever this is optional.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabBackgroundJobDatabase>))]
public enum GitLabBackgroundJobDatabase
{
    [JsonStringEnumMemberName("main")] Main,

    [JsonStringEnumMemberName("ci")] Ci,

    [JsonStringEnumMemberName("sec")] Sec,

    [JsonStringEnumMemberName("embedding")]
    Embedding,

    [JsonStringEnumMemberName("geo")] Geo
}