using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     How GitLab Duo is offered to a group (<c>duo_availability</c>).
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabGroupDuoAvailability>))]
public enum GitLabGroupDuoAvailability
{
    [JsonStringEnumMemberName("always_on")]
    AlwaysOn,

    [JsonStringEnumMemberName("default_on")]
    DefaultOn,

    [JsonStringEnumMemberName("default_off")]
    DefaultOff,

    [JsonStringEnumMemberName("never_on")] NeverOn
}