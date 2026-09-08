using System.Net;
using System.Text;

using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Repositories;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Repositories;

public sealed class ProjectPackageProtectionRulesRepositoryTests
{
    private const string RuleJson = """
                                    {
                                      "id": 32,
                                      "project_id": 6,
                                      "package_name_pattern": "@my-scope/my-package-*",
                                      "package_type": "npm",
                                      "minimum_access_level_for_delete": "owner",
                                      "minimum_access_level_for_push": "maintainer"
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
        ProjectPackageProtectionRulesRepository repository = new(connection);

        List<GitLabPackageProtectionRule> rules = new();
        await foreach (GitLabPackageProtectionRule rule in
                       repository.ListAsync(6, TestContext.Current.CancellationToken))
        {
            rules.Add(rule);
        }

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/6/packages/protection/rules",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabPackageProtectionRule rule2 = Assert.Single(rules);
        Assert.Equal(32, rule2.Id);
        Assert.Equal(6, rule2.ProjectId);
        Assert.Equal("@my-scope/my-package-*", rule2.PackageNamePattern);
        Assert.Equal("npm", rule2.PackageType);
        Assert.Equal("owner", rule2.MinimumAccessLevelForDelete);
        Assert.Equal("maintainer", rule2.MinimumAccessLevelForPush);
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
        ProjectPackageProtectionRulesRepository repository = new(connection);

        await foreach (GitLabPackageProtectionRule _ in
                       repository.ListAsync("gitlab-org/gitlab", TestContext.Current.CancellationToken))
        {
            Assert.Fail("The stubbed response is an empty page.");
        }

        Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/packages/protection/rules",
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
        ProjectPackageProtectionRulesRepository repository = new(connection);

        CreatePackageProtectionRuleRequest request = new()
        {
            PackageNamePattern = "@my-scope/my-package-*",
            PackageType = GitLabPackageProtectionRuleType.Npm,
            MinimumAccessLevelForDelete = GitLabPackageProtectionRuleDeleteAccessLevel.Owner,
            MinimumAccessLevelForPush = GitLabPackageProtectionRulePushAccessLevel.Maintainer
        };

        GitLabPackageProtectionRule rule =
            await repository.CreateAsync(6, request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/6/packages/protection/rules",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(
            """
            {"package_name_pattern":"@my-scope/my-package-*","package_type":"npm","minimum_access_level_for_delete":"owner","minimum_access_level_for_push":"maintainer"}
            """,
            sentBody);
        Assert.Equal(32, rule.Id);
    }

    [Fact]
    public async Task UpdateAsync_PatchesTheRuleRoute_WithOnlySetMembers()
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
        ProjectPackageProtectionRulesRepository repository = new(connection);

        UpdatePackageProtectionRuleRequest request = new()
        {
            MinimumAccessLevelForPush = GitLabPackageProtectionRulePushAccessLevel.Owner
        };

        GitLabPackageProtectionRule rule =
            await repository.UpdateAsync(6, 32, request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Patch, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/6/packages/protection/rules/32",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"minimum_access_level_for_push":"owner"}""", sentBody);
        Assert.Equal("npm", rule.PackageType);
    }

    [Fact]
    public async Task DeleteAsync_SendsDeleteToTheRuleRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ProjectPackageProtectionRulesRepository repository = new(connection);

        await repository.DeleteAsync(6, 32, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/6/packages/protection/rules/32",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task CreateAsync_OnUnprocessableEntity_ThrowsGitLabValidationException()
    {
        const string Json = """{ "message": { "package_name_pattern": ["has already been taken"] } }""";

        using StubHttpMessageHandler handler = new(_ =>
            new HttpResponseMessage(HttpStatusCode.UnprocessableEntity)
            {
                Content = new StringContent(Json, Encoding.UTF8, "application/json")
            });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ProjectPackageProtectionRulesRepository repository = new(connection);

        CreatePackageProtectionRuleRequest request = new()
        {
            PackageNamePattern = "@my-scope/my-package-*", PackageType = GitLabPackageProtectionRuleType.Npm
        };

        GitLabValidationException exception = await Assert.ThrowsAsync<GitLabValidationException>(() =>
            repository.CreateAsync(6, request, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, exception.StatusCode);
        Assert.Contains("has already been taken", exception.Message, StringComparison.Ordinal);
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
        ProjectPackageProtectionRulesRepository repository = new(connection);

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabNotFoundException>(() =>
            repository.DeleteAsync(6, 999, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
    }
}