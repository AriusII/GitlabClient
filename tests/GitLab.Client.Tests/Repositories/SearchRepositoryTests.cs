using System.Net;
using System.Text;
using System.Text.Json;

using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Repositories;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Repositories;

public sealed class SearchRepositoryTests
{
    private const string ProjectsJson = """
                                        [
                                          {
                                            "id": 7,
                                            "name": "gitlab",
                                            "path_with_namespace": "gitlab-org/gitlab",
                                            "description": "The one true repo",
                                            "visibility": "public",
                                            "web_url": "https://gitlab.example/gitlab-org/gitlab",
                                            "default_branch": "master",
                                            "star_count": 4200
                                          }
                                        ]
                                        """;

    private const string IssuesJson = """
                                      [
                                        {
                                          "id": 4302,
                                          "iid": 17,
                                          "project_id": 7,
                                          "title": "Login page is slow",
                                          "state": "opened",
                                          "author": {
                                            "id": 3,
                                            "username": "ada",
                                            "name": "Ada Lovelace",
                                            "web_url": "https://gitlab.example/ada"
                                          },
                                          "labels": ["bug", "frontend"],
                                          "web_url": "https://gitlab.example/gitlab-org/gitlab/-/issues/17"
                                        }
                                      ]
                                      """;

    private const string MergeRequestsJson = """
                                             [
                                               {
                                                 "id": 9901,
                                                 "iid": 42,
                                                 "project_id": 7,
                                                 "title": "Speed up the login page",
                                                 "state": "opened",
                                                 "source_branch": "feature/login",
                                                 "target_branch": "main",
                                                 "draft": false,
                                                 "merge_status": "can_be_merged",
                                                 "web_url": "https://gitlab.example/gitlab-org/gitlab/-/merge_requests/42"
                                               }
                                             ]
                                             """;

    private static StubHttpMessageHandler RespondWith(string json)
    {
        return new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });
    }

    private static async Task<List<T>> CollectAsync<T>(IAsyncEnumerable<T> source)
    {
        List<T> items = new();
        await foreach (T item in source.ConfigureAwait(false))
        {
            items.Add(item);
        }

        return items;
    }

    [Fact]
    public async Task SearchProjectsAsync_HitsTheInstanceRoute_WithTheProjectsScope_AndDeserializesProjects()
    {
        using StubHttpMessageHandler handler = RespondWith(ProjectsJson);

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        SearchRepository repository = new(connection);

        List<GitLabProject> projects = await CollectAsync(repository.SearchProjectsAsync("gitlab ci",
            new SearchListOptions { PerPage = 20 }, TestContext.Current.CancellationToken));

        string? requestUri = handler.LastRequest?.RequestUri?.AbsoluteUri;
        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/search?search=gitlab%20ci&scope=projects&per_page=20", requestUri);

        GitLabProject single = Assert.Single(projects);
        Assert.Equal(7, single.Id);
        Assert.Equal("gitlab-org/gitlab", single.PathWithNamespace);
        Assert.Equal(GitLabVisibility.Public, single.Visibility);
        Assert.Equal("master", single.DefaultBranch);
        Assert.Equal(4200, single.StarCount);
    }

    [Fact]
    public async Task SearchIssuesAsync_SendsStateAndConfidential_AndDeserializesIssues()
    {
        using StubHttpMessageHandler handler = RespondWith(IssuesJson);

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        SearchRepository repository = new(connection);

        SearchListOptions options = new() { State = "opened", Confidential = false, PerPage = 50 };

        List<GitLabIssue> issues =
            await CollectAsync(repository.SearchIssuesAsync("login", options, TestContext.Current.CancellationToken));

        string? requestUri = handler.LastRequest?.RequestUri?.AbsoluteUri;
        Assert.Equal(
            "https://gitlab.example/api/v4/search?search=login&scope=issues&state=opened&confidential=false&per_page=50",
            requestUri);

        GitLabIssue single = Assert.Single(issues);
        Assert.Equal(17, single.Iid);
        Assert.Equal("Login page is slow", single.Title);
        Assert.Equal("opened", single.State);
        Assert.NotNull(single.Author);
        Assert.Equal("ada", single.Author!.Username);
        Assert.Equal(["bug", "frontend"], single.Labels);
    }

    [Fact]
    public async Task SearchMergeRequestsAsync_EscapesTheSearchTerm_AndDeserializesMergeRequests()
    {
        using StubHttpMessageHandler handler = RespondWith(MergeRequestsJson);

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        SearchRepository repository = new(connection);

        List<GitLabMergeRequest> mergeRequests = await CollectAsync(
            repository.SearchMergeRequestsAsync("fix: login/logout", null, TestContext.Current.CancellationToken));

        string? requestUri = handler.LastRequest?.RequestUri?.AbsoluteUri;
        Assert.Equal("https://gitlab.example/api/v4/search?search=fix%3A%20login%2Flogout&scope=merge_requests",
            requestUri);

        GitLabMergeRequest single = Assert.Single(mergeRequests);
        Assert.Equal(42, single.Iid);
        Assert.Equal("feature/login", single.SourceBranch);
        Assert.Equal("main", single.TargetBranch);
        Assert.Equal("can_be_merged", single.MergeStatus);
        Assert.False(single.Draft);
    }

    [Fact]
    public async Task SearchUsersAsync_HitsTheInstanceRoute_WithTheUsersScope_AndDeserializesUsers()
    {
        const string Json = """
                            [
                              {
                                "id": 3,
                                "username": "ada",
                                "name": "Ada Lovelace",
                                "state": "active",
                                "avatar_url": "https://gitlab.example/uploads/-/system/user/avatar/3/avatar.png",
                                "web_url": "https://gitlab.example/ada"
                              }
                            ]
                            """;

        using StubHttpMessageHandler handler = RespondWith(Json);

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        SearchRepository repository = new(connection);

        List<GitLabUser> users =
            await CollectAsync(repository.SearchUsersAsync("ada", null, TestContext.Current.CancellationToken));

        Assert.Equal("https://gitlab.example/api/v4/search?search=ada&scope=users",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabUser single = Assert.Single(users);
        Assert.Equal(3, single.Id);
        Assert.Equal("ada", single.Username);
        Assert.Equal("Ada Lovelace", single.Name);
        Assert.Equal("active", single.State);
    }

    [Fact]
    public async Task SearchGroupProjectsAsync_EncodesNamespacedGroupPath_AndUsesTheRealSearchRoute()
    {
        using StubHttpMessageHandler handler = RespondWith(ProjectsJson);

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        SearchRepository repository = new(connection);

        List<GitLabProject> projects = await CollectAsync(repository.SearchGroupProjectsAsync("gitlab-org/subgroup",
            "gitlab", new SearchListOptions { PerPage = 10 }, TestContext.Current.CancellationToken));

        string? requestUri = handler.LastRequest?.RequestUri?.AbsoluteUri;

        // The vendored spec spells this "/groups/{id}/(-/)search"; "(-/)" is a Grape artifact and must
        // never reach the wire.
        Assert.DoesNotContain("(-/)", requestUri, StringComparison.Ordinal);
        Assert.Equal(
            "https://gitlab.example/api/v4/groups/gitlab-org%2Fsubgroup/search?search=gitlab&scope=projects&per_page=10",
            requestUri);

        Assert.Equal("gitlab-org/gitlab", Assert.Single(projects).PathWithNamespace);
    }

    [Fact]
    public async Task SearchGroupIssuesAsync_HitsTheGroupRoute_WithTheIssuesScope()
    {
        using StubHttpMessageHandler handler = RespondWith(IssuesJson);

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        SearchRepository repository = new(connection);

        List<GitLabIssue> issues = await CollectAsync(repository.SearchGroupIssuesAsync(9, "login",
            new SearchListOptions { State = "closed" }, TestContext.Current.CancellationToken));

        Assert.Equal("https://gitlab.example/api/v4/groups/9/search?search=login&scope=issues&state=closed",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("Login page is slow", Assert.Single(issues).Title);
    }

    [Fact]
    public async Task SearchGroupMergeRequestsAsync_HitsTheGroupRoute_WithTheMergeRequestsScope()
    {
        using StubHttpMessageHandler handler = RespondWith(MergeRequestsJson);

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        SearchRepository repository = new(connection);

        List<GitLabMergeRequest> mergeRequests = await CollectAsync(
            repository.SearchGroupMergeRequestsAsync(9, "login", null, TestContext.Current.CancellationToken));

        Assert.Equal("https://gitlab.example/api/v4/groups/9/search?search=login&scope=merge_requests",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(9901, Assert.Single(mergeRequests).Id);
    }

    [Fact]
    public async Task SearchProjectIssuesAsync_EncodesNamespacedProjectPath_AndSendsTheRefFilter()
    {
        using StubHttpMessageHandler handler = RespondWith(IssuesJson);

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        SearchRepository repository = new(connection);

        ProjectSearchListOptions options = new()
        {
            Ref = "release/1.0", State = "opened", Confidential = true, PerPage = 25
        };

        List<GitLabIssue> issues = await CollectAsync(repository.SearchProjectIssuesAsync("gitlab-org/gitlab", "login",
            options, TestContext.Current.CancellationToken));

        string? requestUri = handler.LastRequest?.RequestUri?.AbsoluteUri;
        Assert.DoesNotContain("(-/)", requestUri, StringComparison.Ordinal);
        Assert.Equal(
            "https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/search?search=login&scope=issues"
            + "&ref=release%2F1.0&state=opened&confidential=true&per_page=25",
            requestUri);

        Assert.Equal(4302, Assert.Single(issues).Id);
    }

    [Fact]
    public async Task SearchProjectMergeRequestsAsync_HitsTheProjectRoute_WithTheMergeRequestsScope()
    {
        using StubHttpMessageHandler handler = RespondWith(MergeRequestsJson);

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        SearchRepository repository = new(connection);

        List<GitLabMergeRequest> mergeRequests = await CollectAsync(
            repository.SearchProjectMergeRequestsAsync(7, "login", null, TestContext.Current.CancellationToken));

        Assert.Equal("https://gitlab.example/api/v4/projects/7/search?search=login&scope=merge_requests",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("Speed up the login page", Assert.Single(mergeRequests).Title);
    }

    [Fact]
    public async Task SearchProjectCommitsAsync_HitsTheProjectRoute_WithTheCommitsScope_AndDeserializesCommits()
    {
        const string Json = """
                            [
                              {
                                "id": "12d65c8dd2b2676fa3ac47d955accc085a37a9c1",
                                "short_id": "12d65c8d",
                                "title": "Speed up the login page",
                                "author_name": "Ada Lovelace",
                                "committed_date": "2024-03-04T05:06:07.000Z",
                                "web_url": "https://gitlab.example/gitlab-org/gitlab/-/commit/12d65c8dd2b2676fa3ac47d955accc085a37a9c1"
                              }
                            ]
                            """;

        using StubHttpMessageHandler handler = RespondWith(Json);

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        SearchRepository repository = new(connection);

        List<GitLabCommit> commits = await CollectAsync(repository.SearchProjectCommitsAsync(7, "login",
            new ProjectSearchListOptions { Ref = "main" }, TestContext.Current.CancellationToken));

        Assert.Equal("https://gitlab.example/api/v4/projects/7/search?search=login&scope=commits&ref=main",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabCommit single = Assert.Single(commits);
        Assert.Equal("12d65c8d", single.ShortId);
        Assert.Equal("Ada Lovelace", single.AuthorName);
    }

    [Fact]
    public async Task SearchProjectNotesAsync_HitsTheProjectRoute_WithTheNotesScope_AndDeserializesNotes()
    {
        const string Json = """
                            [
                              {
                                "id": 302,
                                "body": "Looks good to me",
                                "author": {
                                  "id": 3,
                                  "username": "ada",
                                  "name": "Ada Lovelace",
                                  "web_url": "https://gitlab.example/ada"
                                },
                                "system": false,
                                "resolvable": true
                              }
                            ]
                            """;

        using StubHttpMessageHandler handler = RespondWith(Json);

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        SearchRepository repository = new(connection);

        List<GitLabNote> notes = await CollectAsync(
            repository.SearchProjectNotesAsync(7, "looks good", null, TestContext.Current.CancellationToken));

        Assert.Equal("https://gitlab.example/api/v4/projects/7/search?search=looks%20good&scope=notes",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabNote single = Assert.Single(notes);
        Assert.Equal(302, single.Id);
        Assert.Equal("Looks good to me", single.Body);
        Assert.False(single.System);
        Assert.True(single.Resolvable);
        Assert.Equal("ada", single.Author?.Username);
    }

    [Fact]
    public async Task SearchProjectMilestonesAsync_HitsTheProjectRoute_WithTheMilestonesScope()
    {
        const string Json = """
                            [
                              {
                                "id": 88,
                                "iid": 3,
                                "project_id": 7,
                                "title": "1.0",
                                "state": "active",
                                "due_date": "2024-06-30",
                                "start_date": "2024-01-01",
                                "expired": false,
                                "web_url": "https://gitlab.example/gitlab-org/gitlab/-/milestones/3"
                              }
                            ]
                            """;

        using StubHttpMessageHandler handler = RespondWith(Json);

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        SearchRepository repository = new(connection);

        List<GitLabMilestone> milestones = await CollectAsync(
            repository.SearchProjectMilestonesAsync(7, "1.0", null, TestContext.Current.CancellationToken));

        Assert.Equal("https://gitlab.example/api/v4/projects/7/search?search=1.0&scope=milestones",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabMilestone single = Assert.Single(milestones);
        Assert.Equal(3, single.Iid);
        Assert.Equal("1.0", single.Title);
        Assert.Equal(new DateOnly(2024, 6, 30), single.DueDate);
        Assert.False(single.Expired);
    }

    [Fact]
    public async Task SearchGroupIssuesAsync_SendsTheNewlyAddedFilterFields_InDeclarationOrder()
    {
        using StubHttpMessageHandler handler = RespondWith(IssuesJson);

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        SearchRepository repository = new(connection);

        SearchListOptions options = new()
        {
            Type = ["issue", "epic"],
            IncludeArchived = true,
            NumContextLines = 5,
            Regex = true,
            Page = 2
        };

        List<GitLabIssue> issues = await CollectAsync(
            repository.SearchGroupIssuesAsync(9, "login", options, TestContext.Current.CancellationToken));

        Assert.Equal(
            "https://gitlab.example/api/v4/groups/9/search?search=login&scope=issues"
            + "&type=issue,epic&include_archived=true&num_context_lines=5&regex=true&page=2",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Single(issues);
    }

    [Fact]
    public async Task SearchProjectMergeRequestsAsync_SendsTheNewlyAddedFilterFields()
    {
        using StubHttpMessageHandler handler = RespondWith(MergeRequestsJson);

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        SearchRepository repository = new(connection);

        ProjectSearchListOptions options = new() { Fields = ["title"], Regex = false, Page = 3 };

        List<GitLabMergeRequest> mergeRequests = await CollectAsync(repository.SearchProjectMergeRequestsAsync(7,
            "login", options, TestContext.Current.CancellationToken));

        Assert.Equal(
            "https://gitlab.example/api/v4/projects/7/search?search=login&scope=merge_requests"
            + "&fields=title&regex=false&page=3",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Single(mergeRequests);
    }

    [Fact]
    public async Task ListSearchMigrationsAsync_HitsTheAdminMigrationsRoute_AndReturnsTheRawJson()
    {
        const string Json = """
                            [
                              { "version": 20230427555555, "name": "BackfillHiddenOnMergeRequests" }
                            ]
                            """;

        using StubHttpMessageHandler handler = RespondWith(Json);

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        SearchRepository repository = new(connection);

        JsonElement migrations =
            await repository.ListSearchMigrationsAsync(TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/admin/search/migrations",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(JsonValueKind.Array, migrations.ValueKind);
        Assert.Equal("BackfillHiddenOnMergeRequests", migrations[0].GetProperty("name").GetString());
    }

    [Fact]
    public async Task GetSearchMigrationAsync_EscapesTheMigrationId_AndDeserializesTheMigration()
    {
        const string Json = """
                            {
                              "version": 20230427555555,
                              "name": "Backfill/Hidden On MergeRequests",
                              "started_at": "2023-05-14T12:30:50.355Z",
                              "completed_at": "2023-05-16T12:30:50.355Z",
                              "completed": true,
                              "obsolete": false,
                              "migration_state": { "task_id": "abc123" }
                            }
                            """;

        using StubHttpMessageHandler handler = RespondWith(Json);

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        SearchRepository repository = new(connection);

        GitLabSearchMigration migration = await repository.GetSearchMigrationAsync("Backfill/Hidden On MergeRequests",
            TestContext.Current.CancellationToken);

        Assert.Equal(
            "https://gitlab.example/api/v4/admin/search/migrations/Backfill%2FHidden%20On%20MergeRequests",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        Assert.Equal(20230427555555, migration.Version);
        Assert.True(migration.Completed);
        Assert.False(migration.Obsolete);
        Assert.Equal("abc123", migration.MigrationState?.GetProperty("task_id").GetString());
    }

    [Fact]
    public async Task SearchProjectSemanticCodeAsync_HitsTheSemanticRoute_AndDeserializesGroupedResults()
    {
        const string Json = """
                            {
                              "confidence": "high",
                              "results": [
                                {
                                  "path": "app/models/user.rb",
                                  "blob_id": "abc123def456",
                                  "file_url": "https://gitlab.example/group/project/-/blob/main/user.rb",
                                  "score": 0.92,
                                  "snippet_ranges": [
                                    { "start_line": 42, "end_line": 44, "content": "def authenticate\nend", "score": 0.85 }
                                  ]
                                }
                              ]
                            }
                            """;

        using StubHttpMessageHandler handler = RespondWith(Json);

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        SearchRepository repository = new(connection);

        GitLabSemanticCodeSearchResult result = await repository.SearchProjectSemanticCodeAsync(7,
            "authentication middleware", new SemanticCodeSearchOptions { DirectoryPath = "app/services/", Knn = 32 },
            TestContext.Current.CancellationToken);

        Assert.Equal(
            "https://gitlab.example/api/v4/projects/7/search/semantic?q=authentication%20middleware"
            + "&directory_path=app%2Fservices%2F&knn=32",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        Assert.Equal("high", result.Confidence);
        GitLabSemanticCodeSearchMatch match = Assert.Single(result.Results!);
        Assert.Equal("app/models/user.rb", match.Path);
        Assert.Equal(0.92, match.Score);
        GitLabSemanticCodeSnippetRange range = Assert.Single(match.SnippetRanges!);
        Assert.Equal(42, range.StartLine);
        Assert.Equal(44, range.EndLine);
    }

    [Fact]
    public async Task SearchProjectsAsync_OnUnauthorized_ThrowsGitLabAuthenticationException()
    {
        const string Json = """{ "message": "401 Unauthorized" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Unauthorized)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        SearchRepository repository = new(connection);

        GitLabAuthenticationException exception = await Assert.ThrowsAsync<GitLabAuthenticationException>(async () =>
            await CollectAsync(repository.SearchProjectsAsync("gitlab", null, TestContext.Current.CancellationToken))
                .ConfigureAwait(false));

        Assert.Equal(HttpStatusCode.Unauthorized, exception.StatusCode);
        Assert.Equal("401 Unauthorized", exception.Message);
    }

    [Fact]
    public async Task SearchGroupIssuesAsync_OnRateLimit_ThrowsGitLabRateLimitExceededException()
    {
        const string Json = """{ "message": "Retry later" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.TooManyRequests)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        SearchRepository repository = new(connection);

        GitLabRateLimitExceededException exception =
            await Assert.ThrowsAsync<GitLabRateLimitExceededException>(async () =>
                await CollectAsync(repository.SearchGroupIssuesAsync(9, "login", null,
                    TestContext.Current.CancellationToken)).ConfigureAwait(false));

        Assert.Equal(HttpStatusCode.TooManyRequests, exception.StatusCode);
    }
}