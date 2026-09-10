using System.Net;
using System.Text;

using GitLab.Client.Endpoints;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Query;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Endpoints;

public sealed class BridgesEndpointTests
{
    [Fact]
    public async Task ListAsync_BuildsTheLegacyBridgesRoute_AndDeserializesTypedBridge()
    {
        const string json = """
                            [
                              {
                                "id": 7,
                                "status": "success",
                                "stage": "deploy",
                                "name": "trigger_downstream",
                                "ref": "release/19.4",
                                "allow_failure": false,
                                "web_url": "https://gitlab.example/gitlab-org/gitlab/-/jobs/7",
                                "project": {
                                  "ci_job_token_scope_enabled": true
                                },
                                "downstream_pipeline": {
                                  "id": 903,
                                  "project_id": 12,
                                  "sha": "c91957a858320c0e17f3a0eca7cfacbff50ea29a",
                                  "ref": "main",
                                  "status": "running",
                                  "web_url": "https://gitlab.example/gitlab-org/downstream/-/pipelines/903"
                                }
                              }
                            ]
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        BridgesClient client = new(connection);

        List<GitLabBridge> bridges = [];
        await foreach (GitLabBridge bridge in client.ListAsync("group/project", 47,
                           new TriggerJobListOptions { Scope = ["success", "failed"], PerPage = 20 },
                           TestContext.Current.CancellationToken))
        {
            bridges.Add(bridge);
        }

        string? requestUri = handler.LastRequest?.RequestUri?.AbsoluteUri;
        Assert.NotNull(requestUri);
        Assert.StartsWith("https://gitlab.example/api/v4/projects/group%2Fproject/pipelines/47/bridges?", requestUri,
            StringComparison.Ordinal);
        Assert.Contains("scope[]=success&scope[]=failed", requestUri, StringComparison.Ordinal);
        Assert.Contains("per_page=20", requestUri, StringComparison.Ordinal);

        GitLabBridge result = Assert.Single(bridges);
        Assert.Equal("trigger_downstream", result.Name);
        Assert.Equal("release/19.4", result.Ref);
        Assert.True(result.Project?.CiJobTokenScopeEnabled);
        Assert.Equal(903, result.DownstreamPipeline?.Id);
        Assert.Equal("running", result.DownstreamPipeline?.Status);
        Assert.Equal(new Uri("https://gitlab.example/gitlab-org/downstream/-/pipelines/903"),
            result.DownstreamPipeline?.WebUrl);
    }
}