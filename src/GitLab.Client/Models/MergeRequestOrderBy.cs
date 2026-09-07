using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>The <c>order_by</c> vocabulary of the merge request listings.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<MergeRequestOrderBy>))]
public enum MergeRequestOrderBy
{
    [JsonStringEnumMemberName("created_at")]
    CreatedAt,

    [JsonStringEnumMemberName("label_priority")]
    LabelPriority,

    [JsonStringEnumMemberName("milestone_due")]
    MilestoneDue,

    [JsonStringEnumMemberName("popularity")]
    Popularity,

    [JsonStringEnumMemberName("priority")] Priority,

    [JsonStringEnumMemberName("title")] Title,

    [JsonStringEnumMemberName("updated_at")]
    UpdatedAt,

    [JsonStringEnumMemberName("merged_at")]
    MergedAt
}