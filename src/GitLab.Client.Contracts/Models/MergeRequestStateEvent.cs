using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     The <c>state_event</c> of <c>PUT /projects/:id/merge_requests/:iid</c> - the only two state
///     transitions the update endpoint accepts.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<MergeRequestStateEvent>))]
public enum MergeRequestStateEvent
{
    [JsonStringEnumMemberName("close")] Close,

    [JsonStringEnumMemberName("reopen")] Reopen
}