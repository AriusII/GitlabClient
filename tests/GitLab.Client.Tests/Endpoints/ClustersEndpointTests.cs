using System.Net;
using System.Text;
using System.Text.Json;

using GitLab.Client.Domain;
using GitLab.Client.Endpoints;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Endpoints;

public sealed class ClustersEndpointTests
{
    private const string BaseAddress = "https://gitlab.example/api/v4/";

    private const string ClusterJson = """
                                       {
                                         "id": "41",
                                         "name": "production-k8s",
                                         "created_at": "2025-03-10T10:30:00.000Z",
                                         "domain": "apps.example.com",
                                         "enabled": true,
                                         "managed": false,
                                         "provider_type": "user",
                                         "platform_type": "kubernetes",
                                         "environment_scope": "*",
                                         "cluster_type": "instance_type",
                                         "namespace_per_environment": true,
                                         "user": { "id": 7, "username": "administrator", "name": "Administrator" },
                                         "platform_kubernetes": {
                                           "api_url": "https://kubernetes.example.com",
                                           "namespace": "gitlab-managed-apps",
                                           "authorization_type": "rbac",
                                           "ca_cert": "-----BEGIN CERTIFICATE-----"
                                         },
                                         "provider_gcp": {
                                           "cluster_id": "gke-production",
                                           "gcp_project_id": "platform-prod",
                                           "zone": "europe-west1-b",
                                           "endpoint": "https://gke.example.com"
                                         },
                                         "management_project": { "id": 20, "name": "cluster-management" }
                                       }
                                       """;

    private const string ProjectClusterJson = """
                                              {
                                                "id": "42",
                                                "name": "project-k8s",
                                                "enabled": true,
                                                "managed": true,
                                                "cluster_type": "project_type",
                                                "project": {
                                                  "id": 8,
                                                  "name": "application",
                                                  "path_with_namespace": "platform/application"
                                                }
                                              }
                                              """;

    private const string GroupClusterJson = """
                                            {
                                              "id": "43",
                                              "name": "group-k8s",
                                              "enabled": true,
                                              "cluster_type": "group_type",
                                              "group": {
                                                "id": 9,
                                                "name": "platform",
                                                "web_url": "https://gitlab.example/groups/platform"
                                              }
                                            }
                                            """;

    [Fact]
    public async Task ListForInstanceAsync_BuildsAdminRoute_AndDeserializesTheClusterProjection()
    {
        using StubHttpMessageHandler handler = JsonHandler($"[{ClusterJson}]");
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri(BaseAddress) };
        ClustersClient client = new(new GitLabApiConnection(httpClient));

        List<GitLabCluster> clusters = [];
        await foreach (GitLabCluster cluster in client.ListForInstanceAsync(TestContext.Current.CancellationToken))
        {
            clusters.Add(cluster);
        }

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/admin/clusters", handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabCluster only = Assert.Single(clusters);
        Assert.Equal(41, only.Id);
        Assert.Equal("production-k8s", only.Name);
        Assert.True(only.Enabled);
        Assert.False(only.Managed);
        Assert.Equal("administrator", only.User?.Username);
        Assert.Equal(new Uri("https://kubernetes.example.com/"), only.PlatformKubernetes?.ApiUrl);
        Assert.Equal("rbac", only.PlatformKubernetes?.AuthorizationType);
        Assert.Equal("platform-prod", only.ProviderGcp?.GcpProjectId);
        Assert.Equal(20, only.ManagementProject?.Id);
    }

    [Fact]
    public async Task GetForInstanceAsync_BuildsTheSingleClusterRoute()
    {
        using StubHttpMessageHandler handler = JsonHandler(ClusterJson);
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri(BaseAddress) };
        ClustersClient client = new(new GitLabApiConnection(httpClient));

        GitLabCluster cluster = await client.GetForInstanceAsync(41, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/admin/clusters/41", handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(41, cluster.Id);
    }

    [Fact]
    public async Task CreateForInstanceAsync_PostsToAddRoute_AndPreservesTheNestedKubernetesRequest()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return JsonResponse(ClusterJson, HttpStatusCode.Created);
        });
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri(BaseAddress) };
        ClustersClient client = new(new GitLabApiConnection(httpClient));

        GitLabCluster cluster =
            await client.CreateForInstanceAsync(CreateRequest(), TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/admin/clusters/add", handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.NotNull(sentBody);
        using JsonDocument requestDocument = JsonDocument.Parse(sentBody);
        JsonElement requestBody = requestDocument.RootElement;
        Assert.Equal("production-k8s", requestBody.GetProperty("name").GetString());
        JsonElement kubernetes = requestBody.GetProperty("platform_kubernetes_attributes");
        Assert.Equal("https://kubernetes.example.com", kubernetes.GetProperty("api_url").GetString());
        Assert.Equal("rbac", kubernetes.GetProperty("authorization_type").GetString());
        Assert.Equal("kubernetes-api-token", kubernetes.GetProperty("token").GetString());
        Assert.Equal(41, cluster.Id);
    }

    [Fact]
    public async Task UpdateForInstanceAsync_PutsThePartialRequestToTheSingleClusterRoute()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return JsonResponse(ClusterJson);
        });
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri(BaseAddress) };
        ClustersClient client = new(new GitLabApiConnection(httpClient));

        GitLabCluster cluster = await client.UpdateForInstanceAsync(41,
            new UpdateClusterRequest
            {
                Enabled = false,
                PlatformKubernetesAttributes = new UpdateClusterKubernetesAttributes { Namespace = "applications" }
            }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/admin/clusters/41", handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"enabled":false,"platform_kubernetes_attributes":{"namespace":"applications"}}""", sentBody);
        Assert.Equal(41, cluster.Id);
    }

    [Fact]
    public async Task DeleteForInstanceAsync_DeserializesTheDeletedCluster()
    {
        using StubHttpMessageHandler handler = JsonHandler(ClusterJson);
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri(BaseAddress) };
        ClustersClient client = new(new GitLabApiConnection(httpClient));

        GitLabCluster cluster = await client.DeleteForInstanceAsync(41, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/admin/clusters/41", handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("production-k8s", cluster.Name);
    }

    [Fact]
    public async Task ListForGroupAsync_EncodesTheFullGroupPath_AndUsesTheGroupProjection()
    {
        using StubHttpMessageHandler handler = JsonHandler($"[{GroupClusterJson}]");
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri(BaseAddress) };
        ClustersClient client = new(new GitLabApiConnection(httpClient));

        List<GitLabGroupCluster> clusters = [];
        await foreach (GitLabGroupCluster cluster in client.ListForGroupAsync(GroupId.FromPath("platform/operations"),
                           TestContext.Current.CancellationToken))
        {
            clusters.Add(cluster);
        }

        Assert.Equal("https://gitlab.example/api/v4/groups/platform%2Foperations/clusters",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        GitLabGroupCluster only = Assert.Single(clusters);
        Assert.Equal(43, only.Id);
        Assert.Equal("platform", only.Group?.Name);
    }

    [Fact]
    public async Task GetForGroupAsync_BuildsTheSingleGroupClusterRoute()
    {
        using StubHttpMessageHandler handler = JsonHandler(GroupClusterJson);
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri(BaseAddress) };
        ClustersClient client = new(new GitLabApiConnection(httpClient));

        GitLabGroupCluster cluster = await client.GetForGroupAsync(9, 43, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/9/clusters/43",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("https://gitlab.example/groups/platform", cluster.Group?.WebUrl);
    }

    [Fact]
    public async Task CreateForGroupAsync_PostsToTheUserRoute()
    {
        using StubHttpMessageHandler handler = new(_ => JsonResponse(GroupClusterJson, HttpStatusCode.Created));
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri(BaseAddress) };
        ClustersClient client = new(new GitLabApiConnection(httpClient));

        GitLabGroupCluster cluster =
            await client.CreateForGroupAsync(9, CreateRequest(), TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/9/clusters/user",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(43, cluster.Id);
    }

    [Fact]
    public async Task UpdateForGroupAsync_PutsToTheSingleClusterRoute()
    {
        using StubHttpMessageHandler handler = JsonHandler(GroupClusterJson);
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri(BaseAddress) };
        ClustersClient client = new(new GitLabApiConnection(httpClient));

        GitLabGroupCluster cluster = await client.UpdateForGroupAsync(9, 43,
            new UpdateClusterRequest { Domain = "apps.group.example" }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/9/clusters/43",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("group-k8s", cluster.Name);
    }

    [Fact]
    public async Task DeleteForGroupAsync_UsesTheTypedDeleteResponse()
    {
        using StubHttpMessageHandler handler = JsonHandler(GroupClusterJson);
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri(BaseAddress) };
        ClustersClient client = new(new GitLabApiConnection(httpClient));

        GitLabGroupCluster cluster = await client.DeleteForGroupAsync(9, 43, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/9/clusters/43",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(43, cluster.Id);
    }

    [Fact]
    public async Task ListForProjectAsync_EncodesTheNamespacedProjectPath()
    {
        using StubHttpMessageHandler handler = JsonHandler($"[{ClusterJson}]");
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri(BaseAddress) };
        ClustersClient client = new(new GitLabApiConnection(httpClient));

        List<GitLabCluster> clusters = [];
        await foreach (GitLabCluster cluster in client.ListForProjectAsync(ProjectId.FromPath("platform/application"),
                           TestContext.Current.CancellationToken))
        {
            clusters.Add(cluster);
        }

        Assert.Equal("https://gitlab.example/api/v4/projects/platform%2Fapplication/clusters",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(41, Assert.Single(clusters).Id);
    }

    [Fact]
    public async Task GetForProjectAsync_BuildsTheSingleProjectClusterRoute()
    {
        using StubHttpMessageHandler handler = JsonHandler(ProjectClusterJson);
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri(BaseAddress) };
        ClustersClient client = new(new GitLabApiConnection(httpClient));

        GitLabProjectCluster cluster = await client.GetForProjectAsync(8, 42, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/8/clusters/42",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("platform/application", cluster.Project?.PathWithNamespace);
    }

    [Fact]
    public async Task CreateForProjectAsync_PostsToTheUserRoute()
    {
        using StubHttpMessageHandler handler = new(_ => JsonResponse(ProjectClusterJson, HttpStatusCode.Created));
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri(BaseAddress) };
        ClustersClient client = new(new GitLabApiConnection(httpClient));

        GitLabProjectCluster cluster = await client.CreateForProjectAsync(8, CreateRequest(),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/8/clusters/user",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(42, cluster.Id);
    }

    [Fact]
    public async Task UpdateForProjectAsync_PutsToTheSingleClusterRoute()
    {
        using StubHttpMessageHandler handler = JsonHandler(ProjectClusterJson);
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri(BaseAddress) };
        ClustersClient client = new(new GitLabApiConnection(httpClient));

        GitLabProjectCluster cluster = await client.UpdateForProjectAsync(8, 42,
            new UpdateClusterRequest { Managed = false }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/8/clusters/42",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.True(cluster.Managed);
    }

    [Fact]
    public async Task DeleteForProjectAsync_UsesTheTypedDeleteResponse()
    {
        using StubHttpMessageHandler handler = JsonHandler(ProjectClusterJson);
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri(BaseAddress) };
        ClustersClient client = new(new GitLabApiConnection(httpClient));

        GitLabProjectCluster cluster = await client.DeleteForProjectAsync(8, 42,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/8/clusters/42",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("project-k8s", cluster.Name);
    }

    [Fact]
    public async Task DiscoverCertificateBasedAsync_SendsTheRequiredGroupId_AndPreservesTheUnspecifiedPartitions()
    {
        const string DiscoveredClustersJson = """
                                              {
                                                "groups": { "9": [{ "id": 43, "name": "group-k8s" }] },
                                                "projects": { "8": [{ "id": 42, "name": "project-k8s" }] }
                                              }
                                              """;

        using StubHttpMessageHandler handler = JsonHandler(DiscoveredClustersJson);
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri(BaseAddress) };
        ClustersClient client = new(new GitLabApiConnection(httpClient));

        GitLabDiscoveredCertificateBasedClusters discovered = await client.DiscoverCertificateBasedAsync(9,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/discover-cert-based-clusters?group_id=9",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.True(discovered.Groups.HasValue);
        Assert.True(discovered.Projects.HasValue);
        Assert.Equal(43, discovered.Groups.Value.GetProperty("9")[0].GetProperty("id").GetInt32());
        Assert.Equal(42, discovered.Projects.Value.GetProperty("8")[0].GetProperty("id").GetInt32());
    }

    private static CreateClusterRequest CreateRequest()
    {
        return new CreateClusterRequest
        {
            Name = "production-k8s",
            Domain = "apps.example.com",
            PlatformKubernetesAttributes = new CreateClusterKubernetesAttributes
            {
                ApiUrl = new Uri("https://kubernetes.example.com"),
                Token = "kubernetes-api-token",
                AuthorizationType = GitLabClusterAuthorizationType.RoleBasedAccessControl
            }
        };
    }

    private static StubHttpMessageHandler JsonHandler(string json)
    {
        return new StubHttpMessageHandler(_ => JsonResponse(json));
    }

    private static HttpResponseMessage JsonResponse(string json, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        return new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
    }
}