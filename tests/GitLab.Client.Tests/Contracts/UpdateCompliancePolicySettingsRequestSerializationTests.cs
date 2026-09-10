using System.Text.Json;

using GitLab.Client.Models.Requests;
using GitLab.Client.Serialization;

namespace GitLab.Client.Tests.Contracts;

public sealed class UpdateCompliancePolicySettingsRequestSerializationTests
{
    [Fact]
    public void Serialize_ExplicitNullCspNamespaceId_RetainsTheRequiredNullMember()
    {
        UpdateCompliancePolicySettingsRequest request = new() { CspNamespaceId = null };

        string json = JsonSerializer.Serialize(request,
            GitLabJsonContext.Default.UpdateCompliancePolicySettingsRequest);

        Assert.Equal("{\"csp_namespace_id\":null}", json);
    }

    [Fact]
    public void Serialize_CspNamespaceId_WritesTheNumericValue()
    {
        UpdateCompliancePolicySettingsRequest request = new() { CspNamespaceId = 42 };

        string json = JsonSerializer.Serialize(request,
            GitLabJsonContext.Default.UpdateCompliancePolicySettingsRequest);

        Assert.Equal("{\"csp_namespace_id\":42}", json);
    }
}