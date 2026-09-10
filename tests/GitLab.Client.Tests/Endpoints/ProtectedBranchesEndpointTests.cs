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

public sealed class ProtectedBranchesEndpointTests
{
    [Fact]
    public async Task ListAsync_BuildsProtectedBranchesRoute_AndDeserializesAllPages()
    {
        const string Json = """
                            [
                              {
                                "name": "main",
                                "allow_force_push": false,
                                "push_access_levels": [
                                  { "access_level": 40, "access_level_description": "Maintainers" }
                                ],
                                "merge_access_levels": [
                                  { "access_level": 40, "access_level_description": "Maintainers" }
                                ]
                              }
                            ]
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ProtectedBranchesClient repository = new(connection);

        List<GitLabProtectedBranch> branches = new();
        await foreach (GitLabProtectedBranch item in repository.ListAsync(1, TestContext.Current.CancellationToken))
        {
            branches.Add(item);
        }

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/protected_branches",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        GitLabProtectedBranch branch = Assert.Single(branches);
        Assert.Equal("main", branch.Name);
        Assert.False(branch.AllowForcePush);
        Assert.NotNull(branch.PushAccessLevels);
        Assert.Equal(40, branch.PushAccessLevels![0].AccessLevel);
        Assert.Equal("Maintainers", branch.PushAccessLevels[0].AccessLevelDescription);
        Assert.NotNull(branch.MergeAccessLevels);
        Assert.Equal(40, branch.MergeAccessLevels![0].AccessLevel);
    }

    [Fact]
    public async Task ListAsync_WithOptions_ProjectsEverySupportedProjectFilter()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ProtectedBranchesClient repository = new(connection);
        ProjectProtectedBranchListOptions options = new() { Search = "release", Page = 2, PerPage = 30 };

        await foreach (GitLabProtectedBranch _ in repository.ListAsync("gitlab-org/gitlab", options,
                           TestContext.Current.CancellationToken))
        {
            Assert.Fail("The stubbed response is an empty page.");
        }

        string? requestUri = handler.LastRequest?.RequestUri?.AbsoluteUri;
        Assert.Contains("/projects/gitlab-org%2Fgitlab/protected_branches?", requestUri, StringComparison.Ordinal);
        Assert.Contains("search=release", requestUri, StringComparison.Ordinal);
        Assert.Contains("page=2", requestUri, StringComparison.Ordinal);
        Assert.Contains("per_page=30", requestUri, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetAsync_EscapesWildcardSlashInName_AndDeserializesProtectedBranch()
    {
        const string Json = """
                            {
                              "name": "release/*",
                              "allow_force_push": true,
                              "push_access_levels": [
                                { "access_level": 30, "access_level_description": "Developers + Maintainers" }
                              ],
                              "merge_access_levels": [
                                { "access_level": 30, "access_level_description": "Developers + Maintainers" }
                              ]
                            }
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ProtectedBranchesClient repository = new(connection);

        GitLabProtectedBranch branch = await repository.GetAsync(1, "release/*", TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/projects/1/protected_branches/release%2F%2A",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("release/*", branch.Name);
        Assert.True(branch.AllowForcePush);
    }

    [Fact]
    public async Task ProtectAsync_PostsToProtectedBranchesRoute_WithSerializedBody_AndDeserializesResult()
    {
        const string Json = """
                            {
                              "name": "release/*",
                              "allow_force_push": true,
                              "push_access_levels": [],
                              "merge_access_levels": []
                            }
                            """;

        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(Json, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ProtectedBranchesClient repository = new(connection);

        ProtectBranchRequest request = new() { Name = "release/*", AllowForcePush = true };

        GitLabProtectedBranch branch =
            await repository.ProtectAsync(42, request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/42/protected_branches",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("application/json", handler.LastRequest?.Content?.Headers.ContentType?.MediaType);
        Assert.Contains("\"name\":\"release/*\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"allow_force_push\":true", sentBody, StringComparison.Ordinal);
        Assert.Equal("release/*", branch.Name);
        Assert.True(branch.AllowForcePush);
    }

    [Fact]
    public async Task UnprotectAsync_SendsDeleteToEscapedNameRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ProtectedBranchesClient repository = new(connection);

        await repository.UnprotectAsync(1, "feature/foo", TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/projects/1/protected_branches/feature%2Ffoo",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task ListAsync_EncodesNamespacedProjectPath()
    {
        const string Json = """[{ "name": "main", "allow_force_push": false }]""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ProtectedBranchesClient repository = new(connection);

        await foreach (GitLabProtectedBranch _ in repository.ListAsync("gitlab-org/gitlab",
                           TestContext.Current.CancellationToken))
        {
        }

        Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/protected_branches",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task GetAsync_OnErrorResponse_ThrowsGitLabApiExceptionWithMessage()
    {
        const string Json = """{ "message": "404 Protected Branch Not Found" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ProtectedBranchesClient repository = new(connection);

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabNotFoundException>(() =>
            repository.GetAsync(1, "missing-branch", TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        Assert.Equal("404 Protected Branch Not Found", exception.Message);
    }

    [Fact]
    public async Task UpdateAsync_PatchesTheEscapedNameRoute_WithOnlyTheSpecifiedFields()
    {
        const string Json = """
                            {
                              "id": 12,
                              "name": "release/*",
                              "allow_force_push": true,
                              "code_owner_approval_required": false,
                              "inherited": false
                            }
                            """;

        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(Json, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ProtectedBranchesClient repository = new(connection);

        UpdateProtectedBranchRequest request = new()
        {
            AllowForcePush = true, AllowedToPush = [new ProtectedBranchAccessRequest { Id = 99, Destroy = true }]
        };

        GitLabProtectedBranch branch =
            await repository.UpdateAsync(1, "release/*", request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Patch, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/protected_branches/release%2F%2A",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"allow_force_push":true,"allowed_to_push":[{"id":99,"_destroy":true}]}""", sentBody);
        Assert.Equal(12, branch.Id);
        Assert.False(branch.Inherited);
    }

    [Fact]
    public async Task ListForGroupAsync_BuildsTheGroupRoute_AndProjectsTheSearchOption()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ProtectedBranchesClient repository = new(connection);

        GroupProtectedBranchListOptions options = new() { Search = "release", Page = 2, PerPage = 30 };

        await foreach (GitLabProtectedBranch _ in repository.ListForGroupAsync("gitlab-org/subgroup", options,
                           TestContext.Current.CancellationToken))
        {
            Assert.Fail("The stubbed response is an empty page.");
        }

        string? requestUri = handler.LastRequest?.RequestUri?.AbsoluteUri;
        Assert.Contains("/groups/gitlab-org%2Fsubgroup/protected_branches?", requestUri, StringComparison.Ordinal);
        Assert.Contains("search=release", requestUri, StringComparison.Ordinal);
        Assert.Contains("page=2", requestUri, StringComparison.Ordinal);
        Assert.Contains("per_page=30", requestUri, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetForGroupAsync_EscapesTheWildcardName_AndDeserializesEveryAccessRuleShape()
    {
        const string Json = """
                            {
                              "id": 5,
                              "name": "release/*",
                              "allow_force_push": false,
                              "code_owner_approval_required": true,
                              "push_access_levels": [
                                {
                                  "id": 1001,
                                  "access_level": null,
                                  "access_level_description": "Deploy",
                                  "deploy_key_id": 7
                                }
                              ],
                              "merge_access_levels": [
                                {
                                  "id": 2001,
                                  "access_level": 30,
                                  "access_level_description": "Lead Developer",
                                  "member_role_id": 42,
                                  "member_role_name": "Lead Developer"
                                }
                              ],
                              "unprotect_access_levels": [
                                {
                                  "id": 3001,
                                  "access_level": 60,
                                  "access_level_description": "Administrators",
                                  "user_id": 9,
                                  "group_id": null
                                }
                              ]
                            }
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ProtectedBranchesClient repository = new(connection);

        GitLabProtectedBranch branch =
            await repository.GetForGroupAsync(9, "release/*", TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/9/protected_branches/release%2F%2A",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(5, branch.Id);
        Assert.True(branch.CodeOwnerApprovalRequired);
        Assert.Null(branch.Inherited);
        GitLabAccessLevel push = Assert.Single(branch.PushAccessLevels!);
        Assert.Equal(1001, push.Id);
        Assert.Null(push.AccessLevel);
        Assert.Equal(7, push.DeployKeyId);
        GitLabAccessLevel merge = Assert.Single(branch.MergeAccessLevels!);
        Assert.Equal(2001, merge.Id);
        Assert.Equal(42, merge.MemberRoleId);
        Assert.Equal("Lead Developer", merge.MemberRoleName);
        GitLabAccessLevel unprotect = Assert.Single(branch.UnprotectAccessLevels!);
        Assert.Equal(3001, unprotect.Id);
        Assert.Equal(60, unprotect.AccessLevel);
        Assert.Equal("Administrators", unprotect.AccessLevelDescription);
        Assert.Equal(9, unprotect.UserId);
        Assert.Null(unprotect.GroupId);
    }

    [Fact]
    public async Task ProtectForGroupAsync_PostsTheAccessLevelsAndAllowedToArrays()
    {
        const string Json = """{ "id": 6, "name": "main", "allow_force_push": false }""";

        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(Json, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ProtectedBranchesClient repository = new(connection);

        ProtectBranchRequest request = new()
        {
            Name = "main",
            PushAccessLevel = 40,
            MergeAccessLevel = 30,
            UnprotectAccessLevel = 60,
            CodeOwnerApprovalRequired = true,
            AllowedToMerge = [new ProtectedBranchAccessRequest { MemberRoleId = 42 }]
        };

        GitLabProtectedBranch branch =
            await repository.ProtectForGroupAsync("gitlab-org", request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/gitlab-org/protected_branches",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Contains("\"push_access_level\":40", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"merge_access_level\":30", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"unprotect_access_level\":60", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"allowed_to_merge\":[{\"member_role_id\":42}]", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"code_owner_approval_required\":true", sentBody, StringComparison.Ordinal);
        Assert.Equal(6, branch.Id);
    }

    [Fact]
    public async Task UpdateForGroupAsync_PatchesTheGroupBranchRoute()
    {
        const string Json = """{ "id": 5, "name": "main", "allow_force_push": false }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ProtectedBranchesClient repository = new(connection);

        UpdateProtectedBranchRequest request = new() { CodeOwnerApprovalRequired = true };

        await repository.UpdateForGroupAsync("gitlab-org/subgroup", "main", request,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Patch, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/gitlab-org%2Fsubgroup/protected_branches/main",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task UnprotectForGroupAsync_SendsDeleteToTheEscapedGroupBranchRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ProtectedBranchesClient repository = new(connection);

        await repository.UnprotectForGroupAsync(9, "release/*", TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/9/protected_branches/release%2F%2A",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task UpdateForGroupAsync_OnErrorResponse_ThrowsGitLabApiExceptionWithMessage()
    {
        const string Json = """{ "message": "404 Protected branch Not Found" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ProtectedBranchesClient repository = new(connection);

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabNotFoundException>(() =>
            repository.UpdateForGroupAsync(9, "missing", new UpdateProtectedBranchRequest(),
                TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        Assert.Equal("404 Protected branch Not Found", exception.Message);
    }
}