using System.Net;
using System.Text;

using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Repositories;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Repositories;

public sealed class ProtectedEnvironmentsRepositoryTests
{
    private const string ProtectedEnvironmentJson = """
                                                    {
                                                      "name": "production",
                                                      "deploy_access_levels": [
                                                        {
                                                          "id": 12,
                                                          "access_level": 40,
                                                          "access_level_description": "Maintainers",
                                                          "user_id": null,
                                                          "group_id": null,
                                                          "group_inheritance_type": 0
                                                        },
                                                        {
                                                          "id": 13,
                                                          "access_level": null,
                                                          "access_level_description": "Deployer",
                                                          "user_id": 5,
                                                          "group_id": null,
                                                          "deploy_key_id": 7,
                                                          "member_role_id": 2,
                                                          "member_role_name": "Deployer",
                                                          "group_inheritance_type": 1
                                                        }
                                                      ],
                                                      "required_approval_count": 1,
                                                      "approval_rules": [
                                                        {
                                                          "id": 38,
                                                          "user_id": null,
                                                          "group_id": 134,
                                                          "access_level": null,
                                                          "access_level_description": "QA group",
                                                          "required_approvals": 2,
                                                          "group_inheritance_type": 0
                                                        }
                                                      ]
                                                    }
                                                    """;

    [Fact]
    public async Task ListForProjectAsync_BuildsProtectedEnvironmentsRoute_AndDeserializesNestedEntries()
    {
        string json = $"[{ProtectedEnvironmentJson}]";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ProtectedEnvironmentsRepository repository = new(connection);

        List<GitLabProtectedEnvironment> environments = new();
        await foreach (GitLabProtectedEnvironment item in
                       repository.ListForProjectAsync(1, TestContext.Current.CancellationToken))
        {
            environments.Add(item);
        }

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/protected_environments",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabProtectedEnvironment environment = Assert.Single(environments);
        Assert.Equal("production", environment.Name);
        Assert.Equal(1, environment.RequiredApprovalCount);

        Assert.NotNull(environment.DeployAccessLevels);
        Assert.Equal(2, environment.DeployAccessLevels!.Count);

        GitLabDeployAccessLevel byRole = environment.DeployAccessLevels[0];
        Assert.Equal(12, byRole.Id);
        Assert.Equal(40, byRole.AccessLevel);
        Assert.Equal("Maintainers", byRole.AccessLevelDescription);
        Assert.Null(byRole.UserId);
        Assert.Equal(0, byRole.GroupInheritanceType);

        GitLabDeployAccessLevel byUser = environment.DeployAccessLevels[1];
        Assert.Equal(5, byUser.UserId);
        Assert.Equal(7, byUser.DeployKeyId);
        Assert.Equal(2, byUser.MemberRoleId);
        Assert.Equal("Deployer", byUser.MemberRoleName);
        Assert.Equal(1, byUser.GroupInheritanceType);
        Assert.Null(byUser.AccessLevel);

        GitLabProtectedEnvironmentApprovalRule rule = Assert.Single(environment.ApprovalRules!);
        Assert.Equal(38, rule.Id);
        Assert.Equal(134, rule.GroupId);
        Assert.Equal("QA group", rule.AccessLevelDescription);
        Assert.Equal(2, rule.RequiredApprovals);
        Assert.Equal(0, rule.GroupInheritanceType);
    }

    [Fact]
    public async Task ListForProjectAsync_EncodesNamespacedProjectPath()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ProtectedEnvironmentsRepository repository = new(connection);

        await foreach (GitLabProtectedEnvironment _ in
                       repository.ListForProjectAsync("gitlab-org/gitlab", TestContext.Current.CancellationToken))
        {
            Assert.Fail("The stubbed response is an empty page.");
        }

        Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/protected_environments",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task GetForProjectAsync_EscapesSlashInEnvironmentName_AndDeserializesTheEnvironment()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(ProtectedEnvironmentJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ProtectedEnvironmentsRepository repository = new(connection);

        GitLabProtectedEnvironment environment =
            await repository.GetForProjectAsync(1, "review/feature-x", TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/protected_environments/review%2Ffeature-x",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("production", environment.Name);
        Assert.Equal(40, environment.DeployAccessLevels?[0].AccessLevel);
    }

    [Fact]
    public async Task GetForGroupAsync_BuildsGroupRoute_ForADeploymentTier()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(ProtectedEnvironmentJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ProtectedEnvironmentsRepository repository = new(connection);

        GitLabProtectedEnvironment environment = await repository.GetForGroupAsync("parent-group/subgroup",
            "production", TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/groups/parent-group%2Fsubgroup/protected_environments/production",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(1, environment.RequiredApprovalCount);
    }

    [Fact]
    public async Task ListForGroupAsync_BuildsGroupRoute()
    {
        string json = $"[{ProtectedEnvironmentJson}]";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ProtectedEnvironmentsRepository repository = new(connection);

        List<GitLabProtectedEnvironment> environments = new();
        await foreach (GitLabProtectedEnvironment item in
                       repository.ListForGroupAsync(9970, TestContext.Current.CancellationToken))
        {
            environments.Add(item);
        }

        Assert.Equal("https://gitlab.example/api/v4/groups/9970/protected_environments",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("production", Assert.Single(environments).Name);
    }

    [Fact]
    public async Task ProtectForProjectAsync_PostsTheNestedAccessArrays()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(ProtectedEnvironmentJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ProtectedEnvironmentsRepository repository = new(connection);

        ProtectEnvironmentRequest request = new()
        {
            Name = "production",
            DeployAccessLevels =
            [
                new DeployAccessLevelRequest { AccessLevel = 30 },
                new DeployAccessLevelRequest { UserId = 5, GroupInheritanceType = 1 }
            ],
            ApprovalRules = [new ProtectedEnvironmentApprovalRuleRequest { GroupId = 10, RequiredApprovals = 2 }]
        };

        GitLabProtectedEnvironment environment =
            await repository.ProtectForProjectAsync(42, request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/42/protected_environments",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("application/json", handler.LastRequest?.Content?.Headers.ContentType?.MediaType);

        // The create form of each item omits the update-only "id"/"_destroy" members entirely.
        Assert.Equal(
            """
            {"name":"production","deploy_access_levels":[{"access_level":30},{"user_id":5,"group_inheritance_type":1}],"approval_rules":[{"group_id":10,"required_approvals":2}]}
            """,
            sentBody);

        Assert.Equal("production", environment.Name);
    }

    [Fact]
    public async Task ProtectForGroupAsync_PostsToTheGroupRoute()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(ProtectedEnvironmentJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ProtectedEnvironmentsRepository repository = new(connection);

        // On a group, "name" carries the deployment tier rather than an environment name.
        ProtectEnvironmentRequest request = new()
        {
            Name = "staging",
            DeployAccessLevels = [new DeployAccessLevelRequest { GroupId = 134 }],
            RequiredApprovalCount = 2
        };

        GitLabProtectedEnvironment environment =
            await repository.ProtectForGroupAsync(9970, request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/9970/protected_environments",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(
            """{"name":"staging","deploy_access_levels":[{"group_id":134}],"required_approval_count":2}""",
            sentBody);
        Assert.Equal("production", environment.Name);
    }

    [Fact]
    public async Task UpdateForProjectAsync_EscapesTheEnvironmentName_AndSendsDestroyFlags()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(ProtectedEnvironmentJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ProtectedEnvironmentsRepository repository = new(connection);

        UpdateProtectedEnvironmentRequest request = new()
        {
            RequiredApprovalCount = 1,
            DeployAccessLevels = [new DeployAccessLevelRequest { Id = 42, Destroy = true }],
            ApprovalRules = [new ProtectedEnvironmentApprovalRuleRequest { Id = 7, RequiredApprovals = 3 }]
        };

        GitLabProtectedEnvironment environment = await repository.UpdateForProjectAsync("gitlab-org/gitlab",
            "review/feature-x", request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/protected_environments/review%2Ffeature-x",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        // "_destroy" must survive the snake_case policy verbatim, or the entry silently is not deleted.
        Assert.Equal(
            """
            {"required_approval_count":1,"deploy_access_levels":[{"id":42,"_destroy":true}],"approval_rules":[{"required_approvals":3,"id":7}]}
            """,
            sentBody);

        Assert.Equal(1, environment.RequiredApprovalCount);
    }

    [Fact]
    public async Task UpdateForGroupAsync_PutsToTheGroupTierRoute()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(ProtectedEnvironmentJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ProtectedEnvironmentsRepository repository = new(connection);

        UpdateProtectedEnvironmentRequest request = new()
        {
            DeployAccessLevels = [new DeployAccessLevelRequest { AccessLevel = 40, GroupInheritanceType = 1 }]
        };

        await repository.UpdateForGroupAsync(9970, "production", request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/9970/protected_environments/production",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"deploy_access_levels":[{"group_inheritance_type":1,"access_level":40}]}""", sentBody);
    }

    [Fact]
    public async Task UnprotectForProjectAsync_EscapesTheEnvironmentName_AndSendsDelete()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ProtectedEnvironmentsRepository repository = new(connection);

        await repository.UnprotectForProjectAsync(1, "review/feature-x", TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/protected_environments/review%2Ffeature-x",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task UnprotectForGroupAsync_SendsDeleteToTheGroupTierRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ProtectedEnvironmentsRepository repository = new(connection);

        await repository.UnprotectForGroupAsync("parent-group/subgroup", "production",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/groups/parent-group%2Fsubgroup/protected_environments/production",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task GetForProjectAsync_OnMissingEnvironment_ThrowsGitLabNotFoundException()
    {
        const string Json = """{ "message": "404 Not found" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ProtectedEnvironmentsRepository repository = new(connection);

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabNotFoundException>(() =>
            repository.GetForProjectAsync(1, "staging", TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        Assert.Equal("404 Not found", exception.Message);
    }

    [Fact]
    public async Task ProtectForProjectAsync_OnUnprocessableEntity_ThrowsGitLabValidationException()
    {
        const string Json = """{ "message": { "name": ["has already been protected"] } }""";

        using StubHttpMessageHandler handler = new(_ =>
            new HttpResponseMessage(HttpStatusCode.UnprocessableEntity)
            {
                Content = new StringContent(Json, Encoding.UTF8, "application/json")
            });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ProtectedEnvironmentsRepository repository = new(connection);

        ProtectEnvironmentRequest request = new()
        {
            Name = "production", DeployAccessLevels = [new DeployAccessLevelRequest { AccessLevel = 40 }]
        };

        GitLabValidationException exception = await Assert.ThrowsAsync<GitLabValidationException>(() =>
            repository.ProtectForProjectAsync(1, request, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, exception.StatusCode);
        Assert.Contains("has already been protected", exception.Message, StringComparison.Ordinal);
    }
}