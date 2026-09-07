using System.Net;
using System.Text;

using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Repositories;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Repositories;

public sealed class BoardsRepositoryTests
{
    private const string BoardJson = """
                                     {
                                       "id": 1,
                                       "name": "Development",
                                       "hide_backlog_list": false,
                                       "hide_closed_list": true,
                                       "weight": 3,
                                       "lists": [
                                         {
                                           "id": 546,
                                           "position": 1,
                                           "max_issue_count": 0,
                                           "max_issue_weight": 0,
                                           "limit_metric": "all_metrics",
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
        BoardsRepository repository = new(connection);

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
        BoardsRepository repository = new(connection);

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
        BoardsRepository repository = new(connection);

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
        BoardsRepository repository = new(connection);

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
        BoardsRepository repository = new(connection);

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
        BoardsRepository repository = new(connection);

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
        BoardsRepository repository = new(connection);

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
        BoardsRepository repository = new(connection);

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
        BoardsRepository repository = new(connection);

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
        BoardsRepository repository = new(connection);

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
        BoardsRepository repository = new(connection);

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
        BoardsRepository repository = new(connection);

        GitLabBoardList list = await repository.UpdateListPositionForProjectAsync("gitlab-org/gitlab", 1, 546,
            new UpdateBoardListRequest { Position = 2 }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/boards/1/lists/546",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"position":2}""", sentBody);
        Assert.Equal(1, list.Position);
    }

    [Fact]
    public async Task DeleteListForProjectAsync_SendsDeleteToListRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = CreateHttpClient(handler);
        GitLabApiConnection connection = new(httpClient);
        BoardsRepository repository = new(connection);

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
        BoardsRepository repository = new(connection);

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
        BoardsRepository repository = new(connection);

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
        BoardsRepository repository = new(connection);

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
        BoardsRepository repository = new(connection);

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
        BoardsRepository repository = new(connection);

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
        BoardsRepository repository = new(connection);

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
        BoardsRepository repository = new(connection);

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
        BoardsRepository repository = new(connection);

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
        BoardsRepository repository = new(connection);

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
        BoardsRepository repository = new(connection);

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
        BoardsRepository repository = new(connection);

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
        BoardsRepository repository = new(connection);

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