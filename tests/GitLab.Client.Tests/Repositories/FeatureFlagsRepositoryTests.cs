using System.Net;
using System.Text;
using System.Text.Json;

using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Repositories;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Repositories;

public sealed class FeatureFlagsRepositoryTests
{
    private const string FeatureFlagJson = """
                                           {
                                             "name": "merge_train",
                                             "description": "This feature is about merge train",
                                             "active": true,
                                             "version": "new_version_flag",
                                             "created_at": "2020-05-13T19:56:33.119Z",
                                             "updated_at": "2020-05-13T19:56:33.119Z",
                                             "scopes": [],
                                             "strategies": [
                                               {
                                                 "id": 36,
                                                 "name": "gradualRolloutUserId",
                                                 "parameters": {
                                                   "groupId": "default",
                                                   "percentage": "50"
                                                 },
                                                 "scopes": [
                                                   {
                                                     "id": 37,
                                                     "environment_scope": "production"
                                                   }
                                                 ]
                                               },
                                               {
                                                 "id": 37,
                                                 "name": "gitlabUserList",
                                                 "parameters": {},
                                                 "scopes": [],
                                                 "user_list": {
                                                   "id": 1,
                                                   "iid": 1,
                                                   "name": "My user list",
                                                   "user_xids": "user1,user2,user3"
                                                 }
                                               }
                                             ]
                                           }
                                           """;

    private const string UserListJson = """
                                        {
                                          "id": 1,
                                          "iid": 1,
                                          "project_id": 1,
                                          "created_at": "2020-02-04T08:13:10.507Z",
                                          "updated_at": "2020-02-04T08:13:10.507Z",
                                          "name": "My user list",
                                          "user_xids": "user1,user2,user3",
                                          "path": "/gitlab-org/gitlab/-/feature_flags_user_lists/1",
                                          "edit_path": "/gitlab-org/gitlab/-/feature_flags_user_lists/1/edit"
                                        }
                                        """;

    [Fact]
    public async Task ListAsync_BuildsFeatureFlagsRoute_AndDeserializesStrategiesAndScopes()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent($"[{FeatureFlagJson}]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        FeatureFlagsRepository repository = new(connection);

        List<GitLabFeatureFlag> flags = new();
        await foreach (GitLabFeatureFlag flag in repository.ListAsync(1,
                           cancellationToken: TestContext.Current.CancellationToken))
        {
            flags.Add(flag);
        }

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/feature_flags",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabFeatureFlag single = Assert.Single(flags);
        Assert.Equal("merge_train", single.Name);
        Assert.True(single.Active);
        Assert.Equal("new_version_flag", single.Version);
        Assert.Empty(single.Scopes!);

        Assert.Equal(2, single.Strategies!.Count);

        GitLabFeatureFlagStrategy rollout = single.Strategies[0];
        Assert.Equal(36, rollout.Id);

        // Unleash's own camelCase vocabulary, not GitLab's snake_case - it must survive verbatim.
        Assert.Equal("gradualRolloutUserId", rollout.Name);
        Assert.Equal("50", rollout.Parameters?.GetProperty("percentage").GetString());
        Assert.Equal("default", rollout.Parameters?.GetProperty("groupId").GetString());

        GitLabFeatureFlagScope scope = Assert.Single(rollout.Scopes!);
        Assert.Equal(37, scope.Id);
        Assert.Equal("production", scope.EnvironmentScope);

        GitLabFeatureFlagStrategy userList = single.Strategies[1];
        Assert.Equal("gitlabUserList", userList.Name);
        Assert.Equal(1, userList.UserList?.Iid);
        Assert.Equal("user1,user2,user3", userList.UserList?.UserXids);
    }

    [Fact]
    public async Task ListAsync_EncodesNamespacedProjectPath_AndProjectsTheScopeFilter()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        FeatureFlagsRepository repository = new(connection);

        FeatureFlagListOptions options = new() { Scope = GitLabFeatureFlagState.Disabled, Page = 2, PerPage = 50 };

        await foreach (GitLabFeatureFlag _ in repository.ListAsync("gitlab-org/gitlab", options,
                           TestContext.Current.CancellationToken))
        {
            Assert.Fail("The stubbed response is an empty page.");
        }

        Assert.Equal(
            "https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/feature_flags?scope=disabled&page=2&per_page=50",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task GetAsync_EscapesTheFlagName()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(FeatureFlagJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        FeatureFlagsRepository repository = new(connection);

        GitLabFeatureFlag flag =
            await repository.GetAsync(1, "team/merge train", TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/1/feature_flags/team%2Fmerge%20train",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("merge_train", flag.Name);
    }

    [Fact]
    public async Task CreateAsync_PostsTheStrategyArray_WithUnleashParametersVerbatim()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(FeatureFlagJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        FeatureFlagsRepository repository = new(connection);

        using JsonDocument parameters = JsonDocument.Parse("""{"groupId":"default","percentage":"50"}""");

        CreateFeatureFlagRequest request = new()
        {
            Name = "merge_train",
            Description = "This feature is about merge train",
            Strategies =
            [
                new FeatureFlagStrategyRequest
                {
                    Name = "gradualRolloutUserId",
                    Parameters = parameters.RootElement,
                    Scopes = [new FeatureFlagStrategyScopeRequest { EnvironmentScope = "production" }]
                }
            ]
        };

        GitLabFeatureFlag flag = await repository.CreateAsync(42, request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/42/feature_flags",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        // The camelCase strategy name and the free-form parameter keys must not be snake_cased.
        Assert.Equal(
            """
            {"name":"merge_train","description":"This feature is about merge train","strategies":[{"name":"gradualRolloutUserId","parameters":{"groupId":"default","percentage":"50"},"scopes":[{"environment_scope":"production"}]}]}
            """,
            sentBody);

        Assert.Equal("merge_train", flag.Name);
    }

    [Fact]
    public async Task UpdateAsync_EscapesTheFlagName_AndSendsDestroyFlags()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(FeatureFlagJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        FeatureFlagsRepository repository = new(connection);

        UpdateFeatureFlagRequest request = new()
        {
            Active = false, Strategies = [new FeatureFlagStrategyRequest { Id = 36, Destroy = true }]
        };

        await repository.UpdateAsync("gitlab-org/gitlab", "team/merge train", request,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/feature_flags/team%2Fmerge%20train",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        // "_destroy" must survive the snake_case policy verbatim, or the strategy silently is not deleted.
        Assert.Equal("""{"active":false,"strategies":[{"id":36,"_destroy":true}]}""", sentBody);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsTheDeletedFlag()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(FeatureFlagJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        FeatureFlagsRepository repository = new(connection);

        GitLabFeatureFlag flag =
            await repository.DeleteAsync(1, "merge_train", TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/feature_flags/merge_train",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        // This DELETE answers 200 with the removed flag rather than 204.
        Assert.Equal("merge_train", flag.Name);
    }

    [Fact]
    public async Task GetSettingsAsync_BuildsTheSettingsRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("""{"minimum_role":"maintainer"}""", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        FeatureFlagsRepository repository = new(connection);

        GitLabFeatureFlagSettings settings =
            await repository.GetSettingsAsync(1, TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/1/feature_flags_settings",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("maintainer", settings.MinimumRole);
    }

    [Fact]
    public async Task UpdateSettingsAsync_SendsTheMinimumRoleAsItsWireValue()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"minimum_role":"no_one_allowed"}""", Encoding.UTF8,
                    "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        FeatureFlagsRepository repository = new(connection);

        UpdateFeatureFlagSettingsRequest request = new() { MinimumRole = GitLabMinimumRole.NoOneAllowed };

        GitLabFeatureFlagSettings settings =
            await repository.UpdateSettingsAsync(1, request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("""{"minimum_role":"no_one_allowed"}""", sentBody);
        Assert.Equal("no_one_allowed", settings.MinimumRole);
    }

    [Fact]
    public async Task ListUserListsAsync_BuildsTheUserListRoute_WithSearch_AndDeserializesPaths()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent($"[{UserListJson}]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        FeatureFlagsRepository repository = new(connection);

        List<GitLabFeatureFlagUserList> lists = new();
        await foreach (GitLabFeatureFlagUserList list in repository.ListUserListsAsync(1,
                           new FeatureFlagUserListOptions { Search = "my list" },
                           TestContext.Current.CancellationToken))
        {
            lists.Add(list);
        }

        Assert.Equal("https://gitlab.example/api/v4/projects/1/feature_flags_user_lists?search=my%20list",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabFeatureFlagUserList single = Assert.Single(lists);
        Assert.Equal(1, single.Id);
        Assert.Equal(1, single.Iid);
        Assert.Equal("My user list", single.Name);
        Assert.Equal("user1,user2,user3", single.UserXids);
        Assert.Equal("/gitlab-org/gitlab/-/feature_flags_user_lists/1", single.Path);
        Assert.Equal("/gitlab-org/gitlab/-/feature_flags_user_lists/1/edit", single.EditPath);
    }

    [Fact]
    public async Task GetUserListAsync_AddressesTheListByIid()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(UserListJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        FeatureFlagsRepository repository = new(connection);

        GitLabFeatureFlagUserList list =
            await repository.GetUserListAsync("gitlab-org/gitlab", 1, TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/feature_flags_user_lists/1",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("My user list", list.Name);
    }

    [Fact]
    public async Task CreateUserListAsync_PostsNameAndUserXids()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(UserListJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        FeatureFlagsRepository repository = new(connection);

        CreateFeatureFlagUserListRequest request = new() { Name = "My user list", UserXids = "user1,user2,user3" };

        GitLabFeatureFlagUserList list =
            await repository.CreateUserListAsync(1, request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/feature_flags_user_lists",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"name":"My user list","user_xids":"user1,user2,user3"}""", sentBody);
        Assert.Equal(1, list.Iid);
    }

    [Fact]
    public async Task UpdateUserListAsync_PutsOnlyTheMembersThatWereSet()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(UserListJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        FeatureFlagsRepository repository = new(connection);

        await repository.UpdateUserListAsync(1, 7, new UpdateFeatureFlagUserListRequest { Name = "Renamed" },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/feature_flags_user_lists/7",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        // user_xids was left unset, so it must be omitted rather than sent as null and clear the list.
        Assert.Equal("""{"name":"Renamed"}""", sentBody);
    }

    [Fact]
    public async Task DeleteUserListAsync_SendsDeleteAndAcceptsNoContent()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        FeatureFlagsRepository repository = new(connection);

        await repository.DeleteUserListAsync(1, 7, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/feature_flags_user_lists/7",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task GetAsync_OnMissingFlag_ThrowsGitLabNotFoundException()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent("""{ "message": "404 Not found" }""", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        FeatureFlagsRepository repository = new(connection);

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabNotFoundException>(() =>
            repository.GetAsync(1, "missing_flag", TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        Assert.Equal("404 Not found", exception.Message);
    }
}