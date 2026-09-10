using System.Net;
using System.Text;

using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Endpoints;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;
using GitLab.Client.Query;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Endpoints;

public sealed class JobTokenScopeEndpointTests
{
    private const string ProjectJson = """
                                       {
                                         "id": 278964,
                                         "name": "GitLab",
                                         "path_with_namespace": "gitlab-org/gitlab",
                                         "visibility": "public",
                                         "web_url": "https://gitlab.com/gitlab-org/gitlab"
                                       }
                                       """;

    private const string GroupJson = """
                                     {
                                       "id": 9970,
                                       "web_url": "https://gitlab.com/groups/gitlab-org",
                                       "name": "GitLab.org"
                                     }
                                     """;

    [Fact]
    public async Task GetAsync_BuildsJobTokenScopeRoute_AndDeserializesTheFlags()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("""{"inbound_enabled": true, "outbound_enabled": false}""", Encoding.UTF8,
                "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        JobTokenScopeClient repository = new(connection);

        GitLabProjectJobTokenScope scope = await repository.GetAsync(1, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/job_token_scope",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.True(scope.InboundEnabled);
        Assert.False(scope.OutboundEnabled);
    }

    [Fact]
    public async Task GetAsync_EncodesNamespacedProjectPath()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("""{"inbound_enabled": true, "outbound_enabled": true}""", Encoding.UTF8,
                "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        JobTokenScopeClient repository = new(connection);

        await repository.GetAsync("gitlab-org/gitlab", TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/job_token_scope",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task UpdateAsync_PatchesTheEnabledFlag_AndSendsNoBodyBack()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.NoContent);
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        JobTokenScopeClient repository = new(connection);

        await repository.UpdateAsync(1, new UpdateProjectJobTokenScopeRequest { Enabled = true },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Patch, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/job_token_scope",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"enabled":true}""", sentBody);
    }

    [Fact]
    public async Task ListAllowlistAsync_BuildsAllowlistRoute_WithPaging_AndDeserializesProjects()
    {
        string json = $"[{ProjectJson}]";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        JobTokenScopeClient repository = new(connection);

        List<GitLabProject> projects = new();
        await foreach (GitLabProject item in repository.ListAllowlistAsync(1,
                           new JobTokenScopeAllowlistListOptions { Page = 2, PerPage = 50 },
                           TestContext.Current.CancellationToken))
        {
            projects.Add(item);
        }

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/job_token_scope/allowlist?page=2&per_page=50",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabProject project = Assert.Single(projects);
        Assert.Equal(278964, project.Id);
        Assert.Equal("gitlab-org/gitlab", project.PathWithNamespace);
    }

    [Fact]
    public async Task AddToAllowlistAsync_PostsTheTargetProjectId_AndDeserializesTheProject()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(ProjectJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        JobTokenScopeClient repository = new(connection);

        GitLabProject project = await repository.AddToAllowlistAsync(1,
            new AddProjectToJobTokenAllowlistRequest { TargetProjectId = 278964 },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/job_token_scope/allowlist",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"target_project_id":278964}""", sentBody);
        Assert.Equal(278964, project.Id);
    }

    [Fact]
    public async Task RemoveFromAllowlistAsync_SendsDeleteToTheTargetProjectRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        JobTokenScopeClient repository = new(connection);

        await repository.RemoveFromAllowlistAsync(1, 278964, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/job_token_scope/allowlist/278964",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task ListGroupsAllowlistAsync_BuildsGroupsAllowlistRoute_AndDeserializesGroups()
    {
        string json = $"[{GroupJson}]";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        JobTokenScopeClient repository = new(connection);

        List<GitLabJobTokenScopeGroup> groups = new();
        await foreach (GitLabJobTokenScopeGroup item in repository.ListGroupsAllowlistAsync(1, null,
                           TestContext.Current.CancellationToken))
        {
            groups.Add(item);
        }

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/job_token_scope/groups_allowlist",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabJobTokenScopeGroup group = Assert.Single(groups);
        Assert.Equal(9970, group.Id);
        Assert.Equal("GitLab.org", group.Name);
        Assert.Equal("https://gitlab.com/groups/gitlab-org", group.WebUrl?.ToString());
    }

    [Fact]
    public async Task ListGroupsAllowlistAsync_AppliesPagingOptions()
    {
        string json = $"[{GroupJson}]";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        JobTokenScopeClient repository = new(connection);

        await foreach (GitLabJobTokenScopeGroup _ in repository.ListGroupsAllowlistAsync(1,
                           new JobTokenScopeAllowlistListOptions { Page = 3, PerPage = 25 },
                           TestContext.Current.CancellationToken))
        {
            // Draining the page is enough - the assertion is on the request that was built.
        }

        Assert.Equal("https://gitlab.example/api/v4/projects/1/job_token_scope/groups_allowlist?page=3&per_page=25",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task AddGroupToAllowlistAsync_PostsTheTargetGroupId_AndDeserializesTheGroup()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(GroupJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        JobTokenScopeClient repository = new(connection);

        GitLabJobTokenScopeGroup group = await repository.AddGroupToAllowlistAsync(1,
            new AddGroupToJobTokenAllowlistRequest { TargetGroupId = 9970 },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/job_token_scope/groups_allowlist",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"target_group_id":9970}""", sentBody);
        Assert.Equal(9970, group.Id);
    }

    [Fact]
    public async Task RemoveGroupFromAllowlistAsync_SendsDeleteToTheTargetGroupRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        JobTokenScopeClient repository = new(connection);

        await repository.RemoveGroupFromAllowlistAsync(1, 9970, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/job_token_scope/groups_allowlist/9970",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task GetAsync_OnForbidden_ThrowsGitLabForbiddenException()
    {
        const string Json = """{ "message": "403 Forbidden" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Forbidden)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        JobTokenScopeClient repository = new(connection);

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabForbiddenException>(() =>
            repository.GetAsync(1, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.Forbidden, exception.StatusCode);
    }

    [Fact]
    public async Task AddToAllowlistAsync_OnUnprocessableEntity_ThrowsGitLabValidationException()
    {
        const string Json = """{ "message": { "target_project_id": ["is not a valid project"] } }""";

        using StubHttpMessageHandler handler = new(_ =>
            new HttpResponseMessage(HttpStatusCode.UnprocessableEntity)
            {
                Content = new StringContent(Json, Encoding.UTF8, "application/json")
            });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        JobTokenScopeClient repository = new(connection);

        GitLabValidationException exception = await Assert.ThrowsAsync<GitLabValidationException>(() =>
            repository.AddToAllowlistAsync(1, new AddProjectToJobTokenAllowlistRequest { TargetProjectId = -1 },
                TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, exception.StatusCode);
        Assert.Contains("is not a valid project", exception.Message, StringComparison.Ordinal);
    }
}