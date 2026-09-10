using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps GitLab's compliance-policy settings APIs: the instance-wide security-policy namespace
///     (<c>/admin/security/compliance_policy_settings</c>, administrators only) and a project's external
///     compliance controls (<c>/projects/:id/compliance_external_controls/:control_id/status</c>).
/// </summary>
public interface IComplianceSettingsClient
{
    /// <summary>Gets the instance's centralized security policies (CSP) namespace setting. Requires administrator access.</summary>
    Task<GitLabCompliancePolicySettings> GetAsync(CancellationToken cancellationToken = default);

    /// <summary>Sets the instance's centralized security policies (CSP) namespace. Requires administrator access.</summary>
    Task<GitLabCompliancePolicySettings> UpdateAsync(UpdateCompliancePolicySettingsRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Reports the outcome of a third-party compliance check back to GitLab for one of a project's
    ///     external control bindings.
    /// </summary>
    Task SetExternalControlStatusAsync(ProjectId projectId, long controlId,
        SetComplianceExternalControlStatusRequest request, CancellationToken cancellationToken = default);
}