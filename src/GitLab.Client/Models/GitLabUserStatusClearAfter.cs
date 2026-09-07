using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     How long a status stands before GitLab clears it. The API accepts only this fixed set of durations,
///     not an arbitrary interval or an absolute timestamp - the resulting
///     <see cref="GitLabUserStatus.ClearStatusAt" /> is computed server-side from the moment the status was
///     set.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabUserStatusClearAfter>))]
public enum GitLabUserStatusClearAfter
{
    /// <summary>Clear the status half an hour from now.</summary>
    [JsonStringEnumMemberName("30_minutes")]
    ThirtyMinutes,

    /// <summary>Clear the status three hours from now.</summary>
    [JsonStringEnumMemberName("3_hours")] ThreeHours,

    /// <summary>Clear the status eight hours from now - roughly a working day.</summary>
    [JsonStringEnumMemberName("8_hours")] EightHours,

    /// <summary>Clear the status one day from now.</summary>
    [JsonStringEnumMemberName("1_day")] OneDay,

    /// <summary>Clear the status three days from now.</summary>
    [JsonStringEnumMemberName("3_days")] ThreeDays,

    /// <summary>Clear the status seven days from now.</summary>
    [JsonStringEnumMemberName("7_days")] SevenDays,

    /// <summary>Clear the status thirty days from now.</summary>
    [JsonStringEnumMemberName("30_days")] ThirtyDays
}