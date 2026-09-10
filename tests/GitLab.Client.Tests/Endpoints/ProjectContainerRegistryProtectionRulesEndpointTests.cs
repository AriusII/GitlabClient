using System.Net;
using System.Text;

using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Endpoints;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Endpoints;

public sealed class ProjectContainerRegistryProtectionRulesEndpointTests
{
    private const string RuleJson = """
                                    {
                                      "id": 7,
                                      "project_id": 6,
                                      "repository_path_pattern": "flight/flight-*",
                                      "minimum_access_level_for_push": "maintainer",
                                      "minimum_access_level_for_delete": "owner"
                                    }
                                    """;

    [Fact]
    public async Task ListAsync_BuildsRulesRoute_AndDeserializesTheRule()
    {
        string json = $"[{RuleJson}]";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ProjectContainerRegistryProtectionRulesClient repository = new(connection);

        List<GitLabContainerRegistryProtectionRule> rules = new();
        await foreach (GitLabContainerRegistryProtectionRule rule in
                       repository.ListAsync(6, TestContext.Current.CancellationToken))
        {
            rules.Add(rule);
        }

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/6/registry/protection/repository/rules",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabContainerRegistryProtectionRule rule2 = Assert.Single(rules);
        Assert.Equal(7, rule2.Id);
        Assert.Equal("flight/flight-*", rule2.RepositoryPathPattern);
        Assert.Equal("maintainer", rule2.MinimumAccessLevelForPush);
        Assert.Equal("owner", rule2.MinimumAccessLevelForDelete);
    }

    [Fact]
    public async Task ListAsync_EncodesNamespacedProjectPath()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ProjectContainerRegistryProtectionRulesClient repository = new(connection);

        await foreach (GitLabContainerRegistryProtectionRule _ in
                       repository.ListAsync("gitlab-org/gitlab", TestContext.Current.CancellationToken))
        {
            Assert.Fail("The stubbed response is an empty page.");
        }

        Assert.Equal(
            "https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/registry/protection/repository/rules",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task CreateAsync_PostsTheRequestBody()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(RuleJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ProjectContainerRegistryProtectionRulesClient repository = new(connection);

        CreateContainerRegistryProtectionRuleRequest request = new()
        {
            RepositoryPathPattern = "flight/flight-*",
            MinimumAccessLevelForPush = GitLabContainerRegistryProtectionAccessLevel.Maintainer,
            MinimumAccessLevelForDelete = GitLabContainerRegistryProtectionAccessLevel.Owner
        };

        GitLabContainerRegistryProtectionRule rule =
            await repository.CreateAsync(6, request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/6/registry/protection/repository/rules",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(
            """
            {"repository_path_pattern":"flight/flight-*","minimum_access_level_for_push":"maintainer","minimum_access_level_for_delete":"owner"}
            """,
            sentBody);
        Assert.Equal(7, rule.Id);
    }

    [Fact]
    public async Task UpdateAsync_UnsetSerializesAsAnExplicitEmptyString()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(RuleJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ProjectContainerRegistryProtectionRulesClient repository = new(connection);

        // Unset must serialize as an explicit "" - distinct from a null member, which the serializer
        // omits entirely and which would leave the rule's current restriction untouched.
        UpdateContainerRegistryProtectionRuleRequest request = new()
        {
            MinimumAccessLevelForPush = GitLabContainerRegistryProtectionAccessLevelOrUnset.Unset
        };

        await repository.UpdateAsync(6, 7, request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Patch, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/6/registry/protection/repository/rules/7",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"minimum_access_level_for_push":""}""", sentBody);
    }

    [Fact]
    public async Task UpdateAsync_RaisesTheDeleteAccessLevel()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(RuleJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ProjectContainerRegistryProtectionRulesClient repository = new(connection);

        UpdateContainerRegistryProtectionRuleRequest request = new()
        {
            MinimumAccessLevelForDelete = GitLabContainerRegistryProtectionAccessLevelOrUnset.Admin
        };

        GitLabContainerRegistryProtectionRule rule =
            await repository.UpdateAsync(6, 7, request, TestContext.Current.CancellationToken);

        Assert.Equal("""{"minimum_access_level_for_delete":"admin"}""", sentBody);
        Assert.Equal("flight/flight-*", rule.RepositoryPathPattern);
    }

    [Fact]
    public async Task DeleteAsync_SendsDeleteToTheRuleRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ProjectContainerRegistryProtectionRulesClient repository = new(connection);

        await repository.DeleteAsync(6, 7, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/6/registry/protection/repository/rules/7",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task DeleteAsync_OnMissingRule_ThrowsGitLabNotFoundException()
    {
        const string Json = """{ "message": "404 Not found" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ProjectContainerRegistryProtectionRulesClient repository = new(connection);

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabNotFoundException>(() =>
            repository.DeleteAsync(6, 999, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
    }
}