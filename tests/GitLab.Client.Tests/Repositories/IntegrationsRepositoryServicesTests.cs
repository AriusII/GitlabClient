using System.Net;
using System.Text;

using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Repositories;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Repositories;

/// <summary>
///     <see cref="IntegrationsRepository.ListServicesAsync" />: the list-all-active-integrations
///     operation the spec still exposes under the older <c>/projects/:id/services</c> path, alongside
///     <see cref="IntegrationsRepository.ListAsync" />'s current <c>/integrations</c> route (see
///     <c>IntegrationsRepositoryETests</c> for the sibling <c>GetServiceAsync</c> /
///     <c>DisableServiceAsync</c> pair on that same alias). What these tests guard is that the route
///     goes to <c>/services</c>, not <c>/integrations</c>, and that pagination and deserialization behave
///     exactly like the modern route.
/// </summary>
public sealed class IntegrationsRepositoryServicesTests
{
    private const string BasicIntegrationJson = """
                                                {
                                                  "id": 75,
                                                  "title": "Jenkins",
                                                  "slug": "jenkins",
                                                  "created_at": "2019-11-20T11:20:25.297Z",
                                                  "updated_at": "2019-11-20T12:24:37.498Z",
                                                  "active": true,
                                                  "commit_events": true,
                                                  "push_events": true,
                                                  "issues_events": true,
                                                  "incident_events": false,
                                                  "alert_events": true,
                                                  "confidential_issues_events": true,
                                                  "merge_requests_events": true,
                                                  "tag_push_events": false,
                                                  "deployment_events": false,
                                                  "note_events": true,
                                                  "confidential_note_events": true,
                                                  "pipeline_events": true,
                                                  "wiki_page_events": true,
                                                  "job_events": true,
                                                  "comment_on_event_enabled": true,
                                                  "inherited": false,
                                                  "vulnerability_events": false
                                                }
                                                """;

    [Fact]
    public async Task ListServicesAsync_BuildsTheProjectServicesRoute_AndDeserializesTheBasicEntity()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent($"[{BasicIntegrationJson}]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        IntegrationsRepository repository = new(connection);

        List<GitLabIntegration> integrations = [];
        await foreach (GitLabIntegration item in
                       repository.ListServicesAsync(1, TestContext.Current.CancellationToken))
        {
            integrations.Add(item);
        }

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);

        // The legacy alias, not /integrations.
        Assert.Equal("https://gitlab.example/api/v4/projects/1/services",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabIntegration integration = Assert.Single(integrations);
        Assert.Equal(75, integration.Id);
        Assert.Equal(GitLabIntegrationSlug.Jenkins, integration.Slug);
        Assert.True(integration.Active);

        // The listing entity is IntegrationBasic: it carries no "properties" member at all.
        Assert.Null(integration.Properties);
    }

    [Fact]
    public async Task ListServicesAsync_EncodesANamespacedProjectPath()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        IntegrationsRepository repository = new(connection);

        await foreach (GitLabIntegration _ in
                       repository.ListServicesAsync("gitlab-org/gitlab", TestContext.Current.CancellationToken))
        {
            Assert.Fail("The stubbed response is an empty page.");
        }

        Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/services",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task ListServicesAsync_FollowsThePaginationLinkHeader()
    {
        int callCount = 0;
        using StubHttpMessageHandler handler = new(request =>
        {
            callCount++;
            if (request.RequestUri!.AbsoluteUri == "https://gitlab.example/api/v4/projects/1/services")
            {
                HttpResponseMessage response = new(HttpStatusCode.OK)
                {
                    Content = new StringContent($"[{BasicIntegrationJson}]", Encoding.UTF8, "application/json")
                };
                response.Headers.TryAddWithoutValidation("Link",
                    "<https://gitlab.example/api/v4/projects/1/services?page=2>; rel=\"next\"");
                return response;
            }

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("[]", Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        IntegrationsRepository repository = new(connection);

        List<GitLabIntegration> integrations = [];
        await foreach (GitLabIntegration item in
                       repository.ListServicesAsync(1, TestContext.Current.CancellationToken))
        {
            integrations.Add(item);
        }

        Assert.Equal(2, callCount);
        Assert.Single(integrations);
    }
}