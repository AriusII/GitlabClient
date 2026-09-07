using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     The result of a compliance external control check
///     (<c>PATCH /projects/:id/compliance_external_controls/:control_id/status</c>) - whether the
///     third-party control the project's compliance framework delegates to passed or failed.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabComplianceExternalControlStatus>))]
public enum GitLabComplianceExternalControlStatus
{
    [JsonStringEnumMemberName("pass")] Pass,

    [JsonStringEnumMemberName("fail")] Fail
}