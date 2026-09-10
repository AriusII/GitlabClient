using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     The transition accepted by <c>state_event</c> on a milestone update.
///     GitLab deliberately models this as an action rather than accepting a writable <c>state</c> field.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabMilestoneStateEvent>))]
public enum GitLabMilestoneStateEvent
{
    /// <summary>Closes the milestone.</summary>
    [JsonStringEnumMemberName("close")] Close,

    /// <summary>Reopens a closed milestone.</summary>
    [JsonStringEnumMemberName("activate")] Activate
}