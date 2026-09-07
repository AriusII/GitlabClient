using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>The <c>order_by</c> field of the issue listings. GitLab defaults to <see cref="CreatedAt" />.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabIssueOrderBy>))]
public enum GitLabIssueOrderBy
{
    [JsonStringEnumMemberName("created_at")]
    CreatedAt,

    [JsonStringEnumMemberName("due_date")] DueDate,

    [JsonStringEnumMemberName("label_priority")]
    LabelPriority,

    [JsonStringEnumMemberName("milestone_due")]
    MilestoneDue,

    [JsonStringEnumMemberName("popularity")]
    Popularity,

    [JsonStringEnumMemberName("priority")] Priority,

    [JsonStringEnumMemberName("relative_position")]
    RelativePosition,

    [JsonStringEnumMemberName("title")] Title,

    [JsonStringEnumMemberName("updated_at")]
    UpdatedAt,

    [JsonStringEnumMemberName("weight")] Weight
}