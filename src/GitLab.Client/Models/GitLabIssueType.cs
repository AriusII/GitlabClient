using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     The kind of work item an issue represents, as accepted by the create/update request bodies and by
///     the <c>issue_type</c> filter.
///     <para>
///         Deliberately not used for <see cref="GitLabIssue.IssueType" />: the spec types that response
///         field as a bare string, so a value GitLab adds later must not turn a healthy response into a
///         <see cref="System.Text.Json.JsonException" />.
///     </para>
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabIssueType>))]
public enum GitLabIssueType
{
    [JsonStringEnumMemberName("issue")] Issue,

    [JsonStringEnumMemberName("incident")] Incident,

    [JsonStringEnumMemberName("test_case")]
    TestCase,

    [JsonStringEnumMemberName("requirement")]
    Requirement,

    [JsonStringEnumMemberName("task")] Task,

    [JsonStringEnumMemberName("ticket")] Ticket
}