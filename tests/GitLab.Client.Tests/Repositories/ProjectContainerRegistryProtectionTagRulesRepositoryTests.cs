using System.Net;
using System.Text;

using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Repositories;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Repositories;

public sealed class ProjectContainerRegistryProtectionTagRulesRepositoryTests
{
    private const string RuleJson = """
                                    {
                                      "id": 11,
                                      "project_id": 6,
                                      "tag_name_pattern": "v*-release",
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
        ProjectContainerRegistryProtectionTagRulesRepository repository = new(connection);

        List<GitLabContainerRegistryProtectionTagRule> rules = new();
        await foreach (GitLabContainerRegistryProtectionTagRule rule in
                       repository.ListAsync(6, TestContext.Current.CancellationToken))
        {
            rules.Add(rule);
        }

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/6/registry/protection/tag/rules",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabContainerRegistryProtectionTagRule rule2 = Assert.Single(rules);
        Assert.Equal(11, rule2.Id);
        Assert.Equal("v*-release", rule2.TagNamePattern);
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
        ProjectContainerRegistryProtectionTagRulesRepository repository = new(connection);

        await foreach (GitLabContainerRegistryProtectionTagRule _ in
                       repository.ListAsync("gitlab-org/gitlab", TestContext.Current.CancellationToken))
        {
            Assert.Fail("The stubbed response is an empty page.");
        }

        Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/registry/protection/tag/rules",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task CreateAsync_PostsTheRequestBody_WithBothAccessLevelsRequired()
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
        ProjectContainerRegistryProtectionTagRulesRepository repository = new(connection);

        CreateContainerRegistryProtectionTagRuleRequest request = new()
        {
            TagNamePattern = "v*-release",
            MinimumAccessLevelForPush = GitLabContainerRegistryProtectionAccessLevel.Maintainer,
            MinimumAccessLevelForDelete = GitLabContainerRegistryProtectionAccessLevel.Owner
        };

        GitLabContainerRegistryProtectionTagRule rule =
            await repository.CreateAsync(6, request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/6/registry/protection/tag/rules",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(
            """
            {"tag_name_pattern":"v*-release","minimum_access_level_for_push":"maintainer","minimum_access_level_for_delete":"owner"}
            """,
            sentBody);
        Assert.Equal(11, rule.Id);
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
        ProjectContainerRegistryProtectionTagRulesRepository repository = new(connection);

        UpdateContainerRegistryProtectionTagRuleRequest request = new()
        {
            MinimumAccessLevelForDelete = GitLabContainerRegistryProtectionAccessLevelOrUnset.Unset
        };

        await repository.UpdateAsync(6, 11, request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Patch, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/6/registry/protection/tag/rules/11",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"minimum_access_level_for_delete":""}""", sentBody);
    }

    [Fact]
    public async Task UpdateAsync_RepointsTheTagNamePattern()
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
        ProjectContainerRegistryProtectionTagRulesRepository repository = new(connection);

        UpdateContainerRegistryProtectionTagRuleRequest request = new() { TagNamePattern = "release-*" };

        GitLabContainerRegistryProtectionTagRule rule =
            await repository.UpdateAsync(6, 11, request, TestContext.Current.CancellationToken);

        Assert.Equal("""{"tag_name_pattern":"release-*"}""", sentBody);
        Assert.Equal(11, rule.Id);
    }

    [Fact]
    public async Task DeleteAsync_SendsDeleteToTheRuleRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ProjectContainerRegistryProtectionTagRulesRepository repository = new(connection);

        await repository.DeleteAsync(6, 11, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/6/registry/protection/tag/rules/11",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task CreateAsync_OnUnprocessableEntity_ThrowsGitLabValidationException()
    {
        const string Json = """{ "message": { "tag_name_pattern": ["has already been taken"] } }""";

        using StubHttpMessageHandler handler = new(_ =>
            new HttpResponseMessage(HttpStatusCode.UnprocessableEntity)
            {
                Content = new StringContent(Json, Encoding.UTF8, "application/json")
            });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ProjectContainerRegistryProtectionTagRulesRepository repository = new(connection);

        CreateContainerRegistryProtectionTagRuleRequest request = new()
        {
            TagNamePattern = "v*-release",
            MinimumAccessLevelForPush = GitLabContainerRegistryProtectionAccessLevel.Maintainer,
            MinimumAccessLevelForDelete = GitLabContainerRegistryProtectionAccessLevel.Owner
        };

        GitLabValidationException exception = await Assert.ThrowsAsync<GitLabValidationException>(() =>
            repository.CreateAsync(6, request, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, exception.StatusCode);
        Assert.Contains("has already been taken", exception.Message, StringComparison.Ordinal);
    }
}