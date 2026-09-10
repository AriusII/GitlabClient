using System.Net;
using System.Text;

using GitLab.Client.Endpoints;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Endpoints;

public sealed class ComplianceSettingsEndpointTests
{
    [Fact]
    public async Task GetAsync_BuildsAdminSecurityComplianceRoute_AndDeserializesTheNamespaceId()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("""{"csp_namespace_id": 42}""", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ComplianceSettingsClient repository = new(connection);

        GitLabCompliancePolicySettings settings = await repository.GetAsync(TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/admin/security/compliance_policy_settings",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(42, settings.CspNamespaceId);
    }

    [Fact]
    public async Task UpdateAsync_PutsTheNamespaceId_ToTheSameRoute()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"csp_namespace_id": 99}""", Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ComplianceSettingsClient repository = new(connection);

        GitLabCompliancePolicySettings settings = await repository.UpdateAsync(
            new UpdateCompliancePolicySettingsRequest { CspNamespaceId = 99 },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/admin/security/compliance_policy_settings",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("{\"csp_namespace_id\":99}", sentBody);
        Assert.Equal(99, settings.CspNamespaceId);
    }

    [Fact]
    public async Task SetExternalControlStatusAsync_PatchesTheProjectControlStatusRoute()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();

            return new HttpResponseMessage(HttpStatusCode.OK);
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ComplianceSettingsClient repository = new(connection);

        await repository.SetExternalControlStatusAsync(
            "gitlab-org/gitlab",
            5,
            new SetComplianceExternalControlStatusRequest { Status = GitLabComplianceExternalControlStatus.Pass },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Patch, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/compliance_external_controls/5/status",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("{\"status\":\"pass\"}", sentBody);
    }
}