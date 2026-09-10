using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>The direction of a two-way relation between epics.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabEpicLinkType>))]
public enum GitLabEpicLinkType
{
    [JsonStringEnumMemberName("relates_to")]
    RelatesTo,

    [JsonStringEnumMemberName("blocks")] Blocks,

    [JsonStringEnumMemberName("is_blocked_by")]
    IsBlockedBy
}