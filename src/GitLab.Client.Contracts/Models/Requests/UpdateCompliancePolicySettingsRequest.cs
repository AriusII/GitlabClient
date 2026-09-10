using System.Text.Json.Serialization;

namespace GitLab.Client.Models.Requests;

/// <summary>Request body for <c>PUT /admin/security/compliance_policy_settings</c>.</summary>
public sealed record UpdateCompliancePolicySettingsRequest
{
    /// <summary>
    ///     The ID of the namespace to designate as the instance's centralized security policies (CSP) namespace.
    ///     GitLab requires the member but accepts an explicit <see langword="null" /> to clear the setting, so it
    ///     must not inherit the context's default null-omission policy.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public required long? CspNamespaceId { get; init; }
}