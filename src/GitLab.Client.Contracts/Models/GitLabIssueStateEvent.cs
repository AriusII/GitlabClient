using System.Text.Json.Serialization;

using GitLab.Client.Models.Requests;

namespace GitLab.Client.Models;

/// <summary>
///     The <c>state_event</c> of <c>PUT /projects/:id/issues/:issue_iid</c>. GitLab models closing and
///     reopening as a transition to request, not as a state to assign, which is why
///     <see cref="UpdateIssueRequest" /> carries this rather than a <c>state</c> field.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabIssueStateEvent>))]
public enum GitLabIssueStateEvent
{
    [JsonStringEnumMemberName("close")] Close,

    [JsonStringEnumMemberName("reopen")] Reopen
}