using System.Net;
using System.Text;

using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Repositories;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Repositories;

public sealed class BranchesRepositoryTests
{
    [Fact]
    public async Task GetAsync_EscapesSlashInBranchName_AndDeserializesNestedCommit()
    {
        const string Json = """
                            {
                              "name": "feature/foo",
                              "commit": {
                                "id": "7b5c3cc8be40ee161ae89a06bba6229da1032a0",
                                "short_id": "7b5c3cc8",
                                "title": "Add feature",
                                "web_url": "https://gitlab.example/gitlab-org/gitlab/-/commit/7b5c3cc8be40ee161ae89a06bba6229da1032a0"
                              },
                              "merged": false,
                              "protected": false,
                              "default": false,
                              "web_url": "https://gitlab.example/gitlab-org/gitlab/-/tree/feature/foo"
                            }
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        BranchesRepository repository = new(connection);

        GitLabBranch branch = await repository.GetAsync(1, "feature/foo", TestContext.Current.CancellationToken);

        Assert.Contains("/projects/1/repository/branches/feature%2Ffoo", handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("feature/foo", branch.Name);
        Assert.NotNull(branch.Commit);
        Assert.Equal("7b5c3cc8be40ee161ae89a06bba6229da1032a0", branch.Commit!.Id);
        Assert.Equal("7b5c3cc8", branch.Commit.ShortId);
    }

    [Fact]
    public async Task GetAsync_OnErrorResponse_ThrowsGitLabApiExceptionWithMessage()
    {
        const string Json = """{ "message": "404 Branch Not Found" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        BranchesRepository repository = new(connection);

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabNotFoundException>(() =>
            repository.GetAsync(1, "missing-branch", TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        Assert.Equal("404 Branch Not Found", exception.Message);
    }

    [Fact]
    public async Task ListAsync_SendsEveryFilter_AndDeserializesCanPush()
    {
        const string Json = """
                            [
                              {
                                "name": "main",
                                "commit": {
                                  "id": "7b5c3cc8be40ee161ae89a06bba6229da1032a0",
                                  "short_id": "7b5c3cc8",
                                  "title": "Add feature",
                                  "web_url": "https://gitlab.example/gitlab-org/gitlab/-/commit/7b5c3cc8be40ee161ae89a06bba6229da1032a0"
                                },
                                "merged": false,
                                "protected": true,
                                "can_push": true,
                                "default": true,
                                "web_url": "https://gitlab.example/gitlab-org/gitlab/-/tree/main"
                              }
                            ]
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        BranchesRepository repository = new(connection);

        BranchListOptions options = new()
        {
            Search = "^feature",
            Regex = "feature/.*",
            Sort = "updated_desc",
            Page = 2,
            PageToken = "feature/login",
            PerPage = 20
        };

        List<GitLabBranch> branches = new();
        await foreach (GitLabBranch branch in repository.ListAsync(1, options, TestContext.Current.CancellationToken))
        {
            branches.Add(branch);
        }

        string? requestUri = handler.LastRequest?.RequestUri?.AbsoluteUri;
        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Contains("/projects/1/repository/branches", requestUri);
        Assert.Contains("search=%5Efeature", requestUri);
        Assert.Contains("regex=feature%2F.%2A", requestUri);
        Assert.Contains("sort=updated_desc", requestUri);
        Assert.Contains("page=2", requestUri);
        Assert.Contains("page_token=feature%2Flogin", requestUri);
        Assert.Contains("per_page=20", requestUri);

        GitLabBranch single = Assert.Single(branches);
        Assert.Equal("main", single.Name);
        Assert.True(single.CanPush);
        Assert.True(single.Protected);
        Assert.True(single.Default);
        Assert.False(single.Merged);
    }

    [Fact]
    public async Task CreateAsync_PostsToBranchesRoute_WithSerializedBody_AndDeserializesCreatedBranch()
    {
        const string Json = """
                            {
                              "name": "feature/login",
                              "commit": {
                                "id": "7b5c3cc8be40ee161ae89a06bba6229da1032a0",
                                "short_id": "7b5c3cc8",
                                "title": "Add feature",
                                "web_url": "https://gitlab.example/gitlab-org/gitlab/-/commit/7b5c3cc8be40ee161ae89a06bba6229da1032a0"
                              },
                              "merged": false,
                              "protected": false,
                              "can_push": true,
                              "default": false,
                              "web_url": "https://gitlab.example/gitlab-org/gitlab/-/tree/feature/login"
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
        BranchesRepository repository = new(connection);

        CreateBranchRequest request = new() { Branch = "feature/login", Ref = "main" };

        GitLabBranch branch =
            await repository.CreateAsync("gitlab-org/gitlab", request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/repository/branches",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("application/json", handler.LastRequest?.Content?.Headers.ContentType?.MediaType);
        Assert.Contains("\"branch\":\"feature/login\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"ref\":\"main\"", sentBody, StringComparison.Ordinal);
        Assert.Equal("feature/login", branch.Name);
        Assert.True(branch.CanPush);
    }

    [Fact]
    public async Task DeleteAsync_EscapesSlashInBranchName_AndEncodesNamespacedProjectPath()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        BranchesRepository repository = new(connection);

        await repository.DeleteAsync("gitlab-org/gitlab", "feature/login", TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/repository/branches/feature%2Flogin",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task DeleteMergedAsync_AcceptsThe202Accepted_ThatGitLabAnswersWith()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Accepted));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        BranchesRepository repository = new(connection);

        await repository.DeleteMergedAsync(1, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/repository/merged_branches",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task CreateAsync_OnExistingBranch_ThrowsGitLabValidationException()
    {
        const string Json = """{ "message": "Branch already exists" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        BranchesRepository repository = new(connection);

        CreateBranchRequest request = new() { Branch = "main", Ref = "main" };

        GitLabValidationException exception = await Assert.ThrowsAsync<GitLabValidationException>(() =>
            repository.CreateAsync(1, request, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);
        Assert.Equal("Branch already exists", exception.Message);
    }

    [Fact]
    public async Task DeleteAsync_OnProtectedBranch_ThrowsGitLabForbiddenException()
    {
        const string Json = """{ "message": "403 Forbidden" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Forbidden)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        BranchesRepository repository = new(connection);

        GitLabForbiddenException exception = await Assert.ThrowsAsync<GitLabForbiddenException>(() =>
            repository.DeleteAsync(1, "main", TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.Forbidden, exception.StatusCode);
    }

    [Fact]
    public async Task ExistsAsync_OnNoContent_EscapesSlashInBranchName_AndReturnsTrue()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        BranchesRepository repository = new(connection);

        bool exists = await repository.ExistsAsync(42, "feature/new-thing", TestContext.Current.CancellationToken);

        Assert.True(exists);
        Assert.Equal(HttpMethod.Head, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/42/repository/branches/feature%2Fnew-thing",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task ExistsAsync_OnNotFound_ReturnsFalse_RatherThanThrowing()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NotFound));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        BranchesRepository repository = new(connection);

        bool exists = await repository.ExistsAsync(42, "gone", TestContext.Current.CancellationToken);

        Assert.False(exists);
    }

    [Fact]
    public async Task ExistsAsync_OnForbidden_StillThrows_SoDeniedIsNeverReadAsMissing()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Forbidden));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        BranchesRepository repository = new(connection);

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabForbiddenException>(() =>
            repository.ExistsAsync(42, "main", TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.Forbidden, exception.StatusCode);
    }

    [Fact]
    public async Task ProtectAsync_PutsToProtectRoute_WithSerializedBody_AndDeserializesDeveloperFlags()
    {
        const string Json = """
                            {
                              "name": "release/1.0",
                              "merged": false,
                              "protected": true,
                              "developers_can_push": true,
                              "developers_can_merge": false,
                              "can_push": true,
                              "default": false,
                              "web_url": "https://gitlab.example/gitlab-org/gitlab/-/tree/release/1.0"
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
        BranchesRepository repository = new(connection);

        GitLabBranch branch = await repository.ProtectAsync(
            42,
            "release/1.0",
            new ProtectSingleBranchRequest { DevelopersCanPush = true, DevelopersCanMerge = false },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/42/repository/branches/release%2F1.0/protect",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Contains("\"developers_can_push\":true", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"developers_can_merge\":false", sentBody, StringComparison.Ordinal);
        Assert.True(branch.Protected);
        Assert.True(branch.DevelopersCanPush);
        Assert.False(branch.DevelopersCanMerge);
    }

    [Fact]
    public async Task UnprotectAsync_PutsAnEmptyBody_ToTheUnprotectRoute()
    {
        const string Json = """
                            {
                              "name": "main",
                              "protected": false,
                              "developers_can_push": false,
                              "developers_can_merge": false,
                              "web_url": "https://gitlab.example/gitlab-org/gitlab/-/tree/main"
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
        BranchesRepository repository = new(connection);

        GitLabBranch branch =
            await repository.UnprotectAsync("gitlab-org/gitlab", "main", TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/repository/branches/main/unprotect",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("{}", sentBody);
        Assert.False(branch.Protected);
    }
}