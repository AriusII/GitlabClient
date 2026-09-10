using System.Net;
using System.Text;

using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Endpoints;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Endpoints;

public sealed class BoardsEndpointTests
{
    private const string BoardJson = """
                                     {
                                       "id": 1,
                                       "name": "Development",
                                       "hide_backlog_list": false,
                                       "hide_closed_list": true,
                                       "weight": 3,
                                       "project": {
                                         "id": 12,
                                         "description": "SDK project",
                                         "name": "gitlab",
                                         "name_with_namespace": "GitLab Org / gitlab",
                                         "path": "gitlab",
                                         "path_with_namespace": "gitlab-org/gitlab",
                                         "created_at": "2020-05-07T04:27:17.016Z",
                                         "default_branch": "main",
                                         "tag_list": ["sdk"],
                                         "topics": ["api"],
                                         "ssh_url_to_repo": "git@gitlab.example:gitlab-org/gitlab.git",
                                         "http_url_to_repo": "https://gitlab.example/gitlab-org/gitlab.git",
                                         "web_url": "https://gitlab.example/gitlab-org/gitlab",
                                         "readme_url": "https://gitlab.example/gitlab-org/gitlab/-/blob/main/README.md",
                                         "forks_count": 4,
                                         "license_url": "https://choosealicense.com/licenses/mit/",
                                         "license": {
                                           "key": "mit",
                                           "name": "MIT License",
                                           "nickname": "MIT",
                                           "html_url": "https://choosealicense.com/licenses/mit/",
                                           "source_url": "https://spdx.org/licenses/MIT.html"
                                         },
                                         "avatar_url": "https://gitlab.example/uploads/project.png",
                                         "star_count": 5,
                                         "last_activity_at": "2024-08-01T12:00:00Z",
                                         "visibility": "public",
                                         "namespace": {
                                           "id": 5,
                                           "name": "GitLab Org",
                                           "path": "gitlab-org",
                                           "kind": "group",
                                           "full_path": "gitlab-org",
                                           "parent_id": 1,
                                           "avatar_url": "https://gitlab.example/uploads/group.png",
                                           "web_url": "https://gitlab.example/groups/gitlab-org"
                                         },
                                         "custom_attributes": { "key": "cost_center", "value": "42" },
                                         "repository_storage": "default"
                                       },
                                       "group": {
                                         "id": 5,
                                         "name": "GitLab Org",
                                         "web_url": "https://gitlab.example/groups/gitlab-org"
                                       },
                                       "milestone": { "unmodelled_milestone_field": "v2" },
                                       "assignee": {
                                         "id": 3,
                                         "username": "ada",
                                         "public_email": "ada@example.com",
                                         "name": "Ada Lovelace",
                                         "state": "active",
                                         "locked": false,
                                         "avatar_url": "https://gitlab.example/uploads/ada.png",
                                         "avatar_path": "/uploads/ada.png",
                                         "custom_attributes": [{ "key": "team", "value": "sdk" }],
                                         "web_url": "https://gitlab.example/ada"
                                       },
                                       "labels": {
                                         "id": 70,
                                         "name": "Backend",
                                         "description": "Backend work",
                                         "text_color": "#FFFFFF",
                                         "description_html": "<p>Backend work</p>",
                                         "color": "#000000",
                                         "archived": false
                                       },
                                       "lists": [
                                         {
                                           "id": 546,
                                           "position": 1,
                                           "max_issue_count": 0,
                                           "max_issue_weight": 0,
                                           "limit_metric": "all_metrics",
                                           "assignee": {
                                             "id": 4,
                                             "username": "grace",
                                             "public_email": "grace@example.com",
                                             "name": "Grace Hopper"
                                           },
                                           "label": {
                                             "id": 69,
                                             "name": "Testing",
                                             "color": "#F0AD4E",
                                             "description": null
                                           }
                                         },
                                         {
                                           "id": 547,
                                           "position": 2,
                                           "milestone": {
                                             "id": 44,
                                             "iid": 7,
                                             "project_id": 12,
                                             "title": "v2.0",
                                             "state": "active",
                                             "due_date": "2024-08-30",
                                             "web_url": "https://gitlab.example/gitlab-org/gitlab/-/milestones/7"
                                           }
                                         },
                                         {
                                           "id": 548,
                                           "position": 3,
                                           "iteration": {
                                             "id": 53,
                                             "iid": 13,
                                             "group_id": 5,
                                             "title": "Iteration 13",
                                             "state": 2,
                                             "start_date": "2024-03-04",
                                             "due_date": "2024-03-17",
                                             "web_url": "https://gitlab.example/groups/gitlab-org/-/iterations/53"
                                           }
                                         }
                                       ]
                                     }
                                     """;

    private const string BoardListJson = """
                                         {
                                           "id": 546,
                                           "position": 1,
                                           "max_issue_count": 5,
                                           "max_issue_weight": 20,
                                           "limit_metric": "issue_count",
                                           "label": {
                                             "id": 69,
                                             "name": "Testing",
                                             "color": "#F0AD4E",
                                             "text_color": "#FFFFFF"
                                           }
                                         }
                                         """;

    [Fact]
    public async Task ListForProjectAsync_BuildsBoardsRoute_AndDeserializesBoardsWithNestedLists()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent($"[{BoardJson}]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = CreateHttpClient(handler);
        GitLabApiConnection connection = new(httpClient);
        BoardsClient repository = new(connection);

        List<GitLabBoard> boards = [];
        await foreach (GitLabBoard item in repository.ListForProjectAsync(12, TestContext.Current.CancellationToken))
        {
            boards.Add(item);
        }

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/12/boards",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabBoard board = Assert.Single(boards);
        Assert.Equal(1, board.Id);
        Assert.Equal("Development", board.Name);
        Assert.False(board.HideBacklogList);
        Assert.True(board.HideClosedList);
        Assert.Equal(3, board.Weight);
        Assert.Equal(12, board.Project?.Id);
        Assert.Equal("SDK project", board.Project?.Description);
        Assert.Equal("gitlab", board.Project?.Name);
        Assert.Equal("GitLab Org / gitlab", board.Project?.NameWithNamespace);
        Assert.Equal("gitlab", board.Project?.Path);
        Assert.Equal("gitlab-org/gitlab", board.Project?.PathWithNamespace);
        Assert.Equal(new DateTimeOffset(2020, 5, 7, 4, 27, 17, 16, TimeSpan.Zero), board.Project?.CreatedAt);
        Assert.Equal("main", board.Project?.DefaultBranch);
        Assert.Equal("sdk", Assert.Single(board.Project?.TagList ?? []));
        Assert.Equal("api", Assert.Single(board.Project?.Topics ?? []));
        Assert.Equal("git@gitlab.example:gitlab-org/gitlab.git", board.Project?.SshUrlToRepo);
        Assert.Equal(new Uri("https://gitlab.example/gitlab-org/gitlab.git"), board.Project?.HttpUrlToRepo);
        Assert.Equal(new Uri("https://gitlab.example/gitlab-org/gitlab"), board.Project?.WebUrl);
        Assert.Equal(new Uri("https://gitlab.example/gitlab-org/gitlab/-/blob/main/README.md"),
            board.Project?.ReadmeUrl);
        Assert.Equal(4, board.Project?.ForksCount);
        Assert.Equal(new Uri("https://choosealicense.com/licenses/mit/"), board.Project?.LicenseUrl);
        Assert.Equal("mit", board.Project?.License?.Key);
        Assert.Equal("MIT License", board.Project?.License?.Name);
        Assert.Equal("MIT", board.Project?.License?.Nickname);
        Assert.Equal("https://choosealicense.com/licenses/mit/", board.Project?.License?.HtmlUrl);
        Assert.Equal("https://spdx.org/licenses/MIT.html", board.Project?.License?.SourceUrl);
        Assert.Equal(new Uri("https://gitlab.example/uploads/project.png"), board.Project?.AvatarUrl);
        Assert.Equal(5, board.Project?.StarCount);
        Assert.Equal(new DateTimeOffset(2024, 8, 1, 12, 0, 0, TimeSpan.Zero), board.Project?.LastActivityAt);
        Assert.Equal("public", board.Project?.Visibility);
        Assert.Equal(5, board.Project?.Namespace?.Id);
        Assert.Equal("GitLab Org", board.Project?.Namespace?.Name);
        Assert.Equal("gitlab-org", board.Project?.Namespace?.Path);
        Assert.Equal("group", board.Project?.Namespace?.Kind);
        Assert.Equal("gitlab-org", board.Project?.Namespace?.FullPath);
        Assert.Equal(1, board.Project?.Namespace?.ParentId);
        Assert.Equal("https://gitlab.example/uploads/group.png", board.Project?.Namespace?.AvatarUrl);
        Assert.Equal("https://gitlab.example/groups/gitlab-org", board.Project?.Namespace?.WebUrl);
        Assert.Equal("cost_center", board.Project?.CustomAttributes?.Key);
        Assert.Equal("42", board.Project?.CustomAttributes?.Value);
        Assert.Equal("default", board.Project?.RepositoryStorage);
        Assert.Equal(5, board.Group?.Id);
        Assert.Equal("GitLab Org", board.Group?.Name);
        Assert.Equal("https://gitlab.example/groups/gitlab-org", board.Group?.WebUrl);
        Assert.Equal("v2", board.Milestone?.GetProperty("unmodelled_milestone_field").GetString());
        Assert.Equal(3, board.Assignee?.Id);
        Assert.Equal("ada", board.Assignee?.Username);
        Assert.Equal("ada@example.com", board.Assignee?.PublicEmail);
        Assert.Equal("Ada Lovelace", board.Assignee?.Name);
        Assert.Equal("active", board.Assignee?.State);
        Assert.False(board.Assignee?.Locked);
        Assert.Equal("https://gitlab.example/uploads/ada.png", board.Assignee?.AvatarUrl);
        Assert.Equal("/uploads/ada.png", board.Assignee?.AvatarPath);
        Assert.Equal("team", Assert.Single(board.Assignee?.CustomAttributes ?? []).Key);
        Assert.Equal("sdk", Assert.Single(board.Assignee?.CustomAttributes ?? []).Value);
        Assert.Equal("https://gitlab.example/ada", board.Assignee?.WebUrl);
        Assert.Equal(70, board.Labels?.Id);
        Assert.Equal("Backend", board.Labels?.Name);
        Assert.Equal("Backend work", board.Labels?.Description);
        Assert.Equal("#FFFFFF", board.Labels?.TextColor);
        Assert.Equal("<p>Backend work</p>", board.Labels?.DescriptionHtml);
        Assert.Equal("#000000", board.Labels?.Color);
        Assert.False(board.Labels?.Archived);

        Assert.NotNull(board.Lists);
        Assert.Equal(3, board.Lists!.Count);

        GitLabBoardList labelList = board.Lists[0];
        Assert.Equal(546, labelList.Id);
        Assert.Equal(1, labelList.Position);
        Assert.Equal("all_metrics", labelList.LimitMetric);
        Assert.Equal(0, labelList.MaxIssueCount);
        Assert.Equal(0, labelList.MaxIssueWeight);
        Assert.NotNull(labelList.Label);
        Assert.Equal(69, labelList.Label!.Id);
        Assert.Equal("Testing", labelList.Label.Name);
        Assert.Equal("#F0AD4E", labelList.Label.Color);
        Assert.Null(labelList.Milestone);
        Assert.Null(labelList.Iteration);
        Assert.Equal(4, labelList.Assignee?.Id);
        Assert.Equal("grace", labelList.Assignee?.Username);
        Assert.Equal("grace@example.com", labelList.Assignee?.PublicEmail);
        Assert.Equal("Grace Hopper", labelList.Assignee?.Name);

        // GitLabMilestone is reused verbatim for a milestone list.
        GitLabBoardList milestoneList = board.Lists[1];
        Assert.NotNull(milestoneList.Milestone);
        Assert.Equal(44, milestoneList.Milestone!.Id);
        Assert.Equal(7, milestoneList.Milestone.Iid);
        Assert.Equal("v2.0", milestoneList.Milestone.Title);
        Assert.Equal("active", milestoneList.Milestone.State);
        Assert.Equal(new DateOnly(2024, 8, 30), milestoneList.Milestone.DueDate);

        // GitLabIteration is reused verbatim for an iteration list - and its state stays an integer.
        GitLabBoardList iterationList = board.Lists[2];
        Assert.NotNull(iterationList.Iteration);
        Assert.Equal(53, iterationList.Iteration!.Id);
        Assert.Equal("Iteration 13", iterationList.Iteration.Title);
        Assert.Equal(2, iterationList.Iteration.State);
        Assert.Equal(new DateOnly(2024, 3, 17), iterationList.Iteration.DueDate);
    }

    [Fact]
    public async Task ListForProjectAsync_EncodesNamespacedProjectPath()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = CreateHttpClient(handler);
        GitLabApiConnection connection = new(httpClient);
        BoardsClient repository = new(connection);

        await foreach (GitLabBoard _ in repository.ListForProjectAsync("gitlab-org/gitlab",
                           TestContext.Current.CancellationToken))
        {
            Assert.Fail("The stub returns an empty page.");
        }

        Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/boards",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task GetForProjectAsync_BuildsBoardRoute_AndDeserializesBoard()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(BoardJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = CreateHttpClient(handler);
        GitLabApiConnection connection = new(httpClient);
        BoardsClient repository = new(connection);

        GitLabBoard board = await repository.GetForProjectAsync("gitlab-org/gitlab", 1,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/boards/1",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(1, board.Id);
        Assert.Equal("Development", board.Name);
    }

    [Fact]
    public async Task GetForProjectAsync_OnMissingBoard_ThrowsGitLabNotFoundException()
    {
        const string Json = """{ "message": "404 Board Not Found" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = CreateHttpClient(handler);
        GitLabApiConnection connection = new(httpClient);
        BoardsClient repository = new(connection);

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabNotFoundException>(() =>
            repository.GetForProjectAsync(12, 999, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        Assert.Equal("404 Board Not Found", exception.Message);
    }

    [Fact]
    public async Task CreateForProjectAsync_PostsName_AndDeserializesCreatedBoard()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(BoardJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = CreateHttpClient(handler);
        GitLabApiConnection connection = new(httpClient);
        BoardsClient repository = new(connection);

        GitLabBoard board = await repository.CreateForProjectAsync(12, new CreateBoardRequest { Name = "Development" },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/12/boards",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("application/json", handler.LastRequest?.Content?.Headers.ContentType?.MediaType);
        Assert.Equal("""{"name":"Development"}""", sentBody);
        Assert.Equal("Development", board.Name);
    }

    [Fact]
    public async Task UpdateForProjectAsync_PutsOnlyTheFieldsThatWereSet()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(BoardJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = CreateHttpClient(handler);
        GitLabApiConnection connection = new(httpClient);
        BoardsClient repository = new(connection);

        UpdateBoardRequest request = new()
        {
            Name = "Development board",
            HideClosedList = true,
            MilestoneId = 44,
            Labels = "Testing,Backend",
            Weight = 3
        };

        GitLabBoard board =
            await repository.UpdateForProjectAsync(12, 1, request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/12/boards/1",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Contains("""
                        "name":"Development board"
                        """, sentBody, StringComparison.Ordinal);
        Assert.Contains("""
                        "hide_closed_list":true
                        """, sentBody, StringComparison.Ordinal);
        Assert.Contains("""
                        "milestone_id":44
                        """, sentBody, StringComparison.Ordinal);
        Assert.Contains("""
                        "labels":"Testing,Backend"
                        """, sentBody, StringComparison.Ordinal);
        Assert.Contains("""
                        "weight":3
                        """, sentBody, StringComparison.Ordinal);

        // Unset fields must be omitted, not sent as null - sending null would clear them server-side.
        Assert.DoesNotContain("hide_backlog_list", sentBody, StringComparison.Ordinal);
        Assert.DoesNotContain("assignee_id", sentBody, StringComparison.Ordinal);

        Assert.Equal(1, board.Id);
    }

    [Fact]
    public async Task DeleteForProjectAsync_SendsDeleteToBoardRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = CreateHttpClient(handler);
        GitLabApiConnection connection = new(httpClient);
        BoardsClient repository = new(connection);

        await repository.DeleteForProjectAsync("gitlab-org/gitlab", 1, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/boards/1",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task ListListsForProjectAsync_BuildsNestedListsRoute_AndDeserializesLists()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent($"[{BoardListJson}]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = CreateHttpClient(handler);
        GitLabApiConnection connection = new(httpClient);
        BoardsClient repository = new(connection);

        List<GitLabBoardList> lists = [];
        await foreach (GitLabBoardList item in repository.ListListsForProjectAsync(12, 1,
                           TestContext.Current.CancellationToken))
        {
            lists.Add(item);
        }

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/12/boards/1/lists",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabBoardList list = Assert.Single(lists);
        Assert.Equal(546, list.Id);
        Assert.Equal(1, list.Position);
        Assert.Equal(5, list.MaxIssueCount);
        Assert.Equal(20, list.MaxIssueWeight);
        Assert.Equal("issue_count", list.LimitMetric);
        Assert.Equal("#FFFFFF", list.Label?.TextColor);
    }

    [Fact]
    public async Task ListListsForProjectAsync_EncodesNamespacedProjectPath()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = CreateHttpClient(handler);
        GitLabApiConnection connection = new(httpClient);
        BoardsClient repository = new(connection);

        await foreach (GitLabBoardList _ in repository.ListListsForProjectAsync("gitlab-org/gitlab", 1,
                           TestContext.Current.CancellationToken))
        {
            Assert.Fail("The stub returns an empty page.");
        }

        Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/boards/1/lists",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task GetListForProjectAsync_BuildsListRoute_AndDeserializesList()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(BoardListJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = CreateHttpClient(handler);
        GitLabApiConnection connection = new(httpClient);
        BoardsClient repository = new(connection);

        GitLabBoardList list =
            await repository.GetListForProjectAsync(12, 1, 546, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/12/boards/1/lists/546",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(546, list.Id);
        Assert.Equal("Testing", list.Label?.Name);
    }

    [Fact]
    public async Task CreateListForProjectAsync_PostsTheSingleScopeId_AndDeserializesCreatedList()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(BoardListJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = CreateHttpClient(handler);
        GitLabApiConnection connection = new(httpClient);
        BoardsClient repository = new(connection);

        GitLabBoardList list = await repository.CreateListForProjectAsync(12, 1,
            new CreateBoardListRequest { LabelId = 69 },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/12/boards/1/lists",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        // Exactly one scope id goes on the wire; the other three are omitted, not nulled.
        Assert.Equal("""{"label_id":69}""", sentBody);
        Assert.Equal(546, list.Id);
    }

    [Fact]
    public async Task CreateListForProjectAsync_PostsEverySupportedScopeSelectorWithoutNulls()
    {
        List<string> sentBodies = [];
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBodies.Add(request.Content?.ReadAsStringAsync().GetAwaiter().GetResult() ?? string.Empty);
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(BoardListJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = CreateHttpClient(handler);
        GitLabApiConnection connection = new(httpClient);
        BoardsClient repository = new(connection);

        CreateBoardListRequest[] requests =
        [
            new() { LabelId = 69 },
            new() { MilestoneId = 44 },
            new() { IterationId = 53 },
            new() { AssigneeId = 7 }
        ];

        foreach (CreateBoardListRequest request in requests)
        {
            GitLabBoardList list = await repository.CreateListForProjectAsync(12, 1, request,
                TestContext.Current.CancellationToken);
            Assert.Equal(546, list.Id);
        }

        Assert.Equal(
        [
            "{\"label_id\":69}",
            "{\"milestone_id\":44}",
            "{\"iteration_id\":53}",
            "{\"assignee_id\":7}"
        ], sentBodies);
        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/12/boards/1/lists",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task UpdateListPositionForProjectAsync_PutsPosition_AndDeserializesUpdatedList()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(BoardListJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = CreateHttpClient(handler);
        GitLabApiConnection connection = new(httpClient);
        BoardsClient repository = new(connection);

        GitLabBoardList list = await repository.UpdateListPositionForProjectAsync("gitlab-org/gitlab", 1, 546,
            new UpdateBoardListRequest { Position = 2 }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/boards/1/lists/546",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"position":2}""", sentBody);
        Assert.Equal(1, list.Position);
    }

    [Fact]
    public async Task UpdateForProjectAsync_PutsEveryOptionalBoardScopeUsingTheGitLabWireNames()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(BoardJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = CreateHttpClient(handler);
        GitLabApiConnection connection = new(httpClient);
        BoardsClient repository = new(connection);

        GitLabBoard board = await repository.UpdateForProjectAsync(12, 1,
            new UpdateBoardRequest
            {
                Name = "Delivery",
                HideBacklogList = true,
                HideClosedList = false,
                AssigneeId = 7,
                MilestoneId = 44,
                Labels = "Backend,Testing",
                Weight = 3
            }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/12/boards/1",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(
            """{"name":"Delivery","hide_backlog_list":true,"hide_closed_list":false,"assignee_id":7,"milestone_id":44,"labels":"Backend,Testing","weight":3}""",
            sentBody);
        Assert.Equal(1, board.Id);
    }

    [Fact]
    public async Task DeleteListForProjectAsync_SendsDeleteToListRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = CreateHttpClient(handler);
        GitLabApiConnection connection = new(httpClient);
        BoardsClient repository = new(connection);

        await repository.DeleteListForProjectAsync(12, 1, 546, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/12/boards/1/lists/546",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task CreateListForProjectAsync_OnBadRequest_ThrowsGitLabValidationException()
    {
        // GitLab requires exactly one of label_id / milestone_id / iteration_id / assignee_id.
        const string Json =
            """{ "message": "400 Bad request - One and only one of label_id, milestone_id, iteration_id or assignee_id is required" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = CreateHttpClient(handler);
        GitLabApiConnection connection = new(httpClient);
        BoardsClient repository = new(connection);

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabValidationException>(() =>
            repository.CreateListForProjectAsync(12, 1, new CreateBoardListRequest(),
                TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);
        Assert.Contains("One and only one of label_id", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ListForGroupAsync_BuildsGroupBoardsRoute_AndEncodesNamespacedGroupPath()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent($"[{BoardJson}]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = CreateHttpClient(handler);
        GitLabApiConnection connection = new(httpClient);
        BoardsClient repository = new(connection);

        List<GitLabBoard> boards = [];
        await foreach (GitLabBoard item in repository.ListForGroupAsync("gitlab-org/subgroup",
                           TestContext.Current.CancellationToken))
        {
            boards.Add(item);
        }

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/gitlab-org%2Fsubgroup/boards",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabBoard board = Assert.Single(boards);
        Assert.Equal(1, board.Id);
        Assert.Equal("Development", board.Name);
        Assert.Equal(3, board.Lists?.Count);
    }

    [Fact]
    public async Task GetForGroupAsync_BuildsGroupBoardRoute_AndDeserializesBoard()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(BoardJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = CreateHttpClient(handler);
        GitLabApiConnection connection = new(httpClient);
        BoardsClient repository = new(connection);

        GitLabBoard board = await repository.GetForGroupAsync(5, 1, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/5/boards/1",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("Development", board.Name);
        Assert.True(board.HideClosedList);
    }

    [Fact]
    public async Task GetForGroupAsync_OnMissingBoard_ThrowsGitLabNotFoundException()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent("""{ "message": "404 Board Not Found" }""", Encoding.UTF8,
                "application/json")
        });

        using HttpClient httpClient = CreateHttpClient(handler);
        GitLabApiConnection connection = new(httpClient);
        BoardsClient repository = new(connection);

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabNotFoundException>(() =>
            repository.GetForGroupAsync(5, 999, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
    }

    [Fact]
    public async Task CreateForGroupAsync_PostsName_AndDeserializesCreatedBoard()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(BoardJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = CreateHttpClient(handler);
        GitLabApiConnection connection = new(httpClient);
        BoardsClient repository = new(connection);

        GitLabBoard board = await repository.CreateForGroupAsync(5, new CreateBoardRequest { Name = "Development" },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/5/boards",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"name":"Development"}""", sentBody);
        Assert.Equal(1, board.Id);
    }

    [Fact]
    public async Task UpdateForGroupAsync_PutsOnlyTheFieldsThatWereSet()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(BoardJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = CreateHttpClient(handler);
        GitLabApiConnection connection = new(httpClient);
        BoardsClient repository = new(connection);

        UpdateBoardRequest request = new() { Name = "Group board", HideBacklogList = true, AssigneeId = 7 };

        GitLabBoard board = await repository.UpdateForGroupAsync(5, 1, request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/5/boards/1",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Contains("""
                        "name":"Group board"
                        """, sentBody, StringComparison.Ordinal);
        Assert.Contains("""
                        "hide_backlog_list":true
                        """, sentBody, StringComparison.Ordinal);
        Assert.Contains("""
                        "assignee_id":7
                        """, sentBody, StringComparison.Ordinal);
        Assert.DoesNotContain("hide_closed_list", sentBody, StringComparison.Ordinal);
        Assert.DoesNotContain("milestone_id", sentBody, StringComparison.Ordinal);
        Assert.Equal(1, board.Id);
    }

    [Fact]
    public async Task DeleteForGroupAsync_SendsDeleteToGroupBoardRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = CreateHttpClient(handler);
        GitLabApiConnection connection = new(httpClient);
        BoardsClient repository = new(connection);

        await repository.DeleteForGroupAsync("gitlab-org/subgroup", 1, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/gitlab-org%2Fsubgroup/boards/1",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task ListListsForGroupAsync_BuildsNestedGroupListsRoute_AndDeserializesLists()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent($"[{BoardListJson}]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = CreateHttpClient(handler);
        GitLabApiConnection connection = new(httpClient);
        BoardsClient repository = new(connection);

        List<GitLabBoardList> lists = [];
        await foreach (GitLabBoardList item in repository.ListListsForGroupAsync("gitlab-org/subgroup", 1,
                           TestContext.Current.CancellationToken))
        {
            lists.Add(item);
        }

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/gitlab-org%2Fsubgroup/boards/1/lists",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabBoardList list = Assert.Single(lists);
        Assert.Equal(546, list.Id);
        Assert.Equal(1, list.Position);
        Assert.Equal("Testing", list.Label?.Name);
        Assert.Equal("issue_count", list.LimitMetric);
    }

    [Fact]
    public async Task GetListForGroupAsync_BuildsGroupListRoute_AndDeserializesList()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(BoardListJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = CreateHttpClient(handler);
        GitLabApiConnection connection = new(httpClient);
        BoardsClient repository = new(connection);

        GitLabBoardList list = await repository.GetListForGroupAsync(5, 1, 546,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/5/boards/1/lists/546",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(546, list.Id);
    }

    [Fact]
    public async Task CreateListForGroupAsync_PostsTheSingleScopeId_AndDeserializesCreatedList()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(BoardListJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = CreateHttpClient(handler);
        GitLabApiConnection connection = new(httpClient);
        BoardsClient repository = new(connection);

        GitLabBoardList list = await repository.CreateListForGroupAsync(5, 1,
            new CreateBoardListRequest { LabelId = 69 }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/5/boards/1/lists",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        // The three scope ids that were not set must be omitted: GitLab rejects a body carrying more than one.
        Assert.Equal("""{"label_id":69}""", sentBody);
        Assert.Equal(546, list.Id);
    }

    [Fact]
    public async Task UpdateListPositionForGroupAsync_PutsPosition_AndDeserializesUpdatedList()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(BoardListJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = CreateHttpClient(handler);
        GitLabApiConnection connection = new(httpClient);
        BoardsClient repository = new(connection);

        GitLabBoardList list = await repository.UpdateListPositionForGroupAsync(5, 1, 546,
            new UpdateBoardListRequest { Position = 2 }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/5/boards/1/lists/546",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"position":2}""", sentBody);
        Assert.Equal(546, list.Id);
    }

    [Fact]
    public async Task DeleteListForGroupAsync_SendsDeleteToGroupListRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = CreateHttpClient(handler);
        GitLabApiConnection connection = new(httpClient);
        BoardsClient repository = new(connection);

        await repository.DeleteListForGroupAsync(5, 1, 546, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/5/boards/1/lists/546",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    private static HttpClient CreateHttpClient(StubHttpMessageHandler handler)
    {
        return new HttpClient(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
    }
}