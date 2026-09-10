namespace GitLab.Client.Models.Requests;

/// <summary>
///     Request body for <c>PATCH /projects/:id/compliance_external_controls/:control_id/status</c> -
///     reports the outcome of a third-party compliance check back to GitLab.
/// </summary>
public sealed record SetComplianceExternalControlStatusRequest
{
    public required GitLabComplianceExternalControlStatus Status { get; init; }
}