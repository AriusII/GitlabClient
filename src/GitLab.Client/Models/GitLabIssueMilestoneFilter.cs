using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     The <c>milestone_id</c> timebox filter of the issue listing and statistics endpoints, mutually
///     exclusive with the <c>milestone</c> title filter. The wire values are capitalized, unlike every
///     other query vocabulary in this API.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabIssueMilestoneFilter>))]
public enum GitLabIssueMilestoneFilter
{
    [JsonStringEnumMemberName("Any")] Any,

    [JsonStringEnumMemberName("None")] None,

    [JsonStringEnumMemberName("Upcoming")] Upcoming,

    [JsonStringEnumMemberName("Started")] Started
}