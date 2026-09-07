using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     Whether a membership is live or still waiting for an owner to act on it. Unlike GitLab's
///     open-ended status fields, this one is a two-value database enum ("awaiting" and "active") that
///     the spec pins on both the <c>state</c> filter of <c>GET .../members/all</c> and the body of
///     <c>PUT /groups/:id/members/:user_id/state</c>, so it is modelled as an enum rather than a string.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabMembershipState>))]
public enum GitLabMembershipState
{
    /// <summary>Invited or requested, but not yet approved - the member cannot use the group yet.</summary>
    [JsonStringEnumMemberName("awaiting")] Awaiting,

    /// <summary>Approved and in effect.</summary>
    [JsonStringEnumMemberName("active")] Active
}