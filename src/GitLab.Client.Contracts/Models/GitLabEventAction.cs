using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     The <c>action</c> filter of the GitLab activity feeds (<c>GET /events</c>,
///     <c>GET /projects/:id/events</c>, <c>GET /users/:id/events</c>).
///     <para>
///         The spec declares the parameter as a bare string, but GitLab validates it against this closed
///         list, so an enum keeps a typo a compile error instead of a silently unfiltered feed.
///     </para>
///     <para>
///         This is the filter vocabulary only. An event echoes its action back as human-readable prose -
///         "pushed to", "commented on" - in <see cref="GitLabEvent.ActionName" />, which is why that
///         member stays a string.
///     </para>
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabEventAction>))]
public enum GitLabEventAction
{
    [JsonStringEnumMemberName("approved")] Approved,

    [JsonStringEnumMemberName("closed")] Closed,

    [JsonStringEnumMemberName("commented")]
    Commented,

    [JsonStringEnumMemberName("created")] Created,

    [JsonStringEnumMemberName("destroyed")]
    Destroyed,

    [JsonStringEnumMemberName("expired")] Expired,

    [JsonStringEnumMemberName("joined")] Joined,

    [JsonStringEnumMemberName("left")] Left,

    [JsonStringEnumMemberName("merged")] Merged,

    [JsonStringEnumMemberName("pushed")] Pushed,

    [JsonStringEnumMemberName("reopened")] Reopened,

    [JsonStringEnumMemberName("updated")] Updated
}