namespace GitLab.Client.Models;

/// <summary>
///     Instance-wide security-policy settings, as returned by
///     <c>GET /admin/security/compliance_policy_settings</c> and
///     <c>PUT /admin/security/compliance_policy_settings</c>.
/// </summary>
public sealed record GitLabCompliancePolicySettings
{
    /// <summary>
    ///     The ID of the namespace holding the instance's centralized security policies, or
    ///     <see langword="null" /> when no compliance security policies (CSP) namespace has been
    ///     configured yet.
    /// </summary>
    public long? CspNamespaceId { get; init; }
}