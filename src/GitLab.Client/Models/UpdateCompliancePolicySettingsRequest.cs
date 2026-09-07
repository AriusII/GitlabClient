namespace GitLab.Client.Models;

/// <summary>Request body for <c>PUT /admin/security/compliance_policy_settings</c>.</summary>
public sealed record UpdateCompliancePolicySettingsRequest
{
    /// <summary>The ID of the namespace to designate as the instance's centralized security policies (CSP) namespace.</summary>
    public required long CspNamespaceId { get; init; }
}