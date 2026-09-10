using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     The <c>due_date</c> filter of the issue listings. The spec also lists an empty string, which means
///     "no filter"; that is expressed here by leaving the option unset rather than by a member that would
///     serialize to a trailing <c>due_date=</c>.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabIssueDueDateFilter>))]
public enum GitLabIssueDueDateFilter
{
    /// <summary>Issues with no due date at all. GitLab spells this <c>0</c>.</summary>
    [JsonStringEnumMemberName("0")] NoDueDate,

    /// <summary>Issues with any due date set.</summary>
    [JsonStringEnumMemberName("any")] Any,

    [JsonStringEnumMemberName("today")] Today,

    [JsonStringEnumMemberName("tomorrow")] Tomorrow,

    [JsonStringEnumMemberName("overdue")] Overdue,

    [JsonStringEnumMemberName("week")] Week,

    [JsonStringEnumMemberName("month")] Month,

    [JsonStringEnumMemberName("next_month_and_previous_two_weeks")]
    NextMonthAndPreviousTwoWeeks
}