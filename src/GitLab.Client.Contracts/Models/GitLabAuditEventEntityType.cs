using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     The entity categories accepted by the required <c>entity_type</c> filter of
///     <c>GET /audit_events</c>.
///     <para>
///         This is a request-side vocabulary. The <see cref="GitLabAuditEvent.EntityType" /> returned in an
///         audit record remains a string because a GitLab instance can report Ruby entity types that are not
///         accepted by the instance-log filter.
///     </para>
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabAuditEventEntityType>))]
public enum GitLabAuditEventEntityType
{
    [JsonStringEnumMemberName("Project")] Project,

    [JsonStringEnumMemberName("User")] User,

    [JsonStringEnumMemberName("Group")] Group,

    [JsonStringEnumMemberName("Gitlab::Audit::InstanceScope")]
    InstanceScope
}