using System.Net;
using System.Text;

using GitLab.Client.Endpoints;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;
using GitLab.Client.Query;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Endpoints;

public sealed class EpicsEndpointTests
{
    private const string EpicJson = """
                                    {
                                      "id": 123,
                                      "work_item_id": 456,
                                      "iid": 4,
                                      "group_id": 7,
                                      "title": "Roadmap",
                                      "state": "opened",
                                      "start_date": "2022-01-31T15:10:45.080Z",
                                      "web_url": "https://gitlab.example/groups/team/-/epics/4",
                                      "_links": { "self": "https://gitlab.example/api/v4/groups/7/epics/4" },
                                      "labels": ["planning"]
                                    }
                                    """;

    private const string EpicIssueJson = """
                                         {
                                           "id": 84,
                                           "iid": 14,
                                           "project_id": 4,
                                           "title": "Issue in roadmap",
                                           "state": "opened",
                                           "web_url": "https://gitlab.example/team/project/-/issues/14",
                                           "epic_issue_id": 11,
                                           "relative_position": 3
                                         }
                                         """;

    private const string EpicIssueLinkJson = """
                                             {
                                               "id": 11,
                                               "relative_position": 3,
                                               "epic": { "id": 123 },
                                               "issue": { "id": 84 }
                                             }
                                             """;

    private const string EpicBoardJson = """
                                         {
                                           "id": 1,
                                           "name": "Planning",
                                           "hide_backlog_list": false,
                                           "hide_closed_list": true,
                                           "group": { "id": 7, "name": "Team" },
                                           "labels": [{ "id": 2, "name": "workflow" }],
                                           "lists": [{ "id": 9, "position": 1, "list_type": "backlog" }]
                                         }
                                         """;

    private const string EpicBoardListJson = """
                                             {
                                               "id": 9,
                                               "label": { "id": 2, "name": "workflow" },
                                               "position": 1,
                                               "list_type": "backlog"
                                             }
                                             """;

    private const string RelatedEpicJson = """
                                           {
                                             "id": 124,
                                             "iid": 5,
                                             "title": "Dependency",
                                             "web_url": "https://gitlab.example/groups/team/-/epics/5",
                                             "related_epic_link_id": 41,
                                             "link_type": "blocks"
                                           }
                                           """;

    private const string RelatedEpicLinkJson = """
                                               {
                                                 "id": 41,
                                                 "link_type": "blocks",
                                                 "source_epic": { "id": 123, "iid": 4, "title": "Roadmap" },
                                                 "target_epic": { "id": 124, "iid": 5, "title": "Dependency" }
                                               }
                                               """;

    [Fact]
    public async Task ListAsync_EncodesNamespacedGroupAndAllTypedQueryFilters()
    {
        using StubHttpMessageHandler handler = new(_ => JsonResponse($"[{EpicJson}]"));
        using HttpClient httpClient = CreateHttpClient(handler);
        EpicsClient client = new(new GitLabApiConnection(httpClient));

        EpicListOptions options = new()
        {
            OrderBy = GitLabEpicOrderBy.UpdatedAt,
            Sort = GitLabEpicSort.Asc,
            Search = "road map",
            State = GitLabEpicState.Opened,
            AuthorId = 7,
            Labels = ["planning", "backend"],
            WithLabelsDetails = true,
            IncludeAncestorGroups = true,
            IncludeDescendantGroups = false,
            Confidential = true,
            PerPage = 50,
            NotLabels = ["blocked"],
            NotAuthorUsername = "bot"
        };

        List<GitLabEpic> epics = [];
        await foreach (GitLabEpic epic in client.ListAsync("group/subgroup", options,
                           TestContext.Current.CancellationToken))
        {
            epics.Add(epic);
        }

        string? uri = handler.LastRequest?.RequestUri?.AbsoluteUri;
        Assert.StartsWith("https://gitlab.example/api/v4/groups/group%2Fsubgroup/epics?", uri,
            StringComparison.Ordinal);
        Assert.Contains("order_by=updated_at", uri, StringComparison.Ordinal);
        Assert.Contains("sort=asc", uri, StringComparison.Ordinal);
        Assert.Contains("search=road%20map", uri, StringComparison.Ordinal);
        Assert.Contains("state=opened", uri, StringComparison.Ordinal);
        Assert.Contains("author_id=7", uri, StringComparison.Ordinal);
        Assert.Contains("labels=planning,backend", uri, StringComparison.Ordinal);
        Assert.Contains("with_labels_details=true", uri, StringComparison.Ordinal);
        Assert.Contains("include_ancestor_groups=true", uri, StringComparison.Ordinal);
        Assert.Contains("include_descendant_groups=false", uri, StringComparison.Ordinal);
        Assert.Contains("confidential=true", uri, StringComparison.Ordinal);
        Assert.Contains("per_page=50", uri, StringComparison.Ordinal);
        Assert.Contains("not[labels]=blocked", uri, StringComparison.Ordinal);
        Assert.Contains("not[author_username]=bot", uri, StringComparison.Ordinal);

        GitLabEpic listedEpic = Assert.Single(epics);
        Assert.Equal(456, listedEpic.WorkItemId);
        Assert.Equal("Roadmap", listedEpic.Title);
        Assert.Equal(new Uri("https://gitlab.example/groups/team/-/epics/4"), listedEpic.WebUrl);
        Assert.True(listedEpic.Links.HasValue);
        Assert.Equal("https://gitlab.example/api/v4/groups/7/epics/4",
            listedEpic.Links.Value.GetProperty("self").GetString());
    }

    [Fact]
    public async Task CreateUpdateAndDeleteAsync_UseCanonicalRoutesAndStrictRequestBodies()
    {
        List<(HttpMethod Method, string Uri, string? Body)> calls = [];
        using StubHttpMessageHandler handler = new(request =>
        {
            calls.Add((request.Method, request.RequestUri!.AbsoluteUri,
                request.Content?.ReadAsStringAsync().GetAwaiter().GetResult()));
            return request.Method == HttpMethod.Delete
                ? new HttpResponseMessage(HttpStatusCode.NoContent)
                : JsonResponse(EpicJson,
                    request.Method == HttpMethod.Post ? HttpStatusCode.Created : HttpStatusCode.OK);
        });

        using HttpClient httpClient = CreateHttpClient(handler);
        EpicsClient client = new(new GitLabApiConnection(httpClient));

        GitLabEpic created = await client.CreateAsync(7,
            new CreateEpicRequest
            {
                Title = "Roadmap",
                Description = "Plan the work",
                StartDateFixed = "2026-01-02",
                Labels = ["planning"]
            }, TestContext.Current.CancellationToken);
        GitLabEpic updated = await client.UpdateAsync(7, 4,
            new UpdateEpicRequest { StateEvent = GitLabEpicStateEvent.Close, AddLabels = ["done"] },
            TestContext.Current.CancellationToken);
        await client.DeleteAsync(7, 4, TestContext.Current.CancellationToken);

        Assert.Collection(calls,
            call =>
            {
                Assert.Equal(HttpMethod.Post, call.Method);
                Assert.Equal("https://gitlab.example/api/v4/groups/7/epics", call.Uri);
                Assert.Equal(
                    """{"title":"Roadmap","description":"Plan the work","start_date_fixed":"2026-01-02","labels":["planning"]}""",
                    call.Body);
            },
            call =>
            {
                Assert.Equal(HttpMethod.Put, call.Method);
                Assert.Equal("https://gitlab.example/api/v4/groups/7/epics/4", call.Uri);
                Assert.Equal("""{"add_labels":["done"],"state_event":"close"}""", call.Body);
            },
            call =>
            {
                Assert.Equal(HttpMethod.Delete, call.Method);
                Assert.Equal("https://gitlab.example/api/v4/groups/7/epics/4", call.Uri);
                Assert.Null(call.Body);
            });
        Assert.Equal(123, created.Id);
        Assert.Equal("opened", updated.State);
    }

    [Fact]
    public async Task ChildEpicOperations_UseGlobalChildIdAndTypedReorderBody()
    {
        List<(HttpMethod Method, string Uri, string? Body)> calls = [];
        using StubHttpMessageHandler handler = new(request =>
        {
            calls.Add((request.Method, request.RequestUri!.AbsoluteUri,
                request.Content?.ReadAsStringAsync().GetAwaiter().GetResult()));
            return JsonResponse(request.Method == HttpMethod.Get ? $"[{EpicJson}]" : EpicJson,
                request.Method == HttpMethod.Post ? HttpStatusCode.Created : HttpStatusCode.OK);
        });

        using HttpClient httpClient = CreateHttpClient(handler);
        EpicsClient client = new(new GitLabApiConnection(httpClient));

        List<GitLabEpic> children = [];
        await foreach (GitLabEpic child in client.ListChildEpicsAsync("group/subgroup", 4,
                           TestContext.Current.CancellationToken))
        {
            children.Add(child);
        }

        await client.CreateChildEpicAsync(7, 4, new CreateChildEpicRequest { Title = "Child", Confidential = true },
            TestContext.Current.CancellationToken);
        await client.RelateChildEpicAsync(7, 4, 123, TestContext.Current.CancellationToken);
        await client.RemoveChildEpicAsync(7, 4, 123, TestContext.Current.CancellationToken);
        await client.ReorderChildEpicAsync(7, 4, 123, new ReorderEpicRequest { MoveBeforeId = 99 },
            TestContext.Current.CancellationToken);

        Assert.Single(children);
        Assert.Collection(calls,
            call => Assert.Equal("https://gitlab.example/api/v4/groups/group%2Fsubgroup/epics/4/epics", call.Uri),
            call => Assert.Equal("""{"title":"Child","confidential":true}""", call.Body),
            call => Assert.Equal(HttpMethod.Post, call.Method),
            call => Assert.Equal(HttpMethod.Delete, call.Method),
            call => Assert.Equal("""{"move_before_id":99}""", call.Body));
        Assert.All(calls.Skip(2),
            call => Assert.Equal("https://gitlab.example/api/v4/groups/7/epics/4/epics/123", call.Uri));
    }

    [Fact]
    public async Task EpicIssueOperations_PreserveAssociationIdsAndPaging()
    {
        List<(HttpMethod Method, string Uri, string? Body)> calls = [];
        using StubHttpMessageHandler handler = new(request =>
        {
            calls.Add((request.Method, request.RequestUri!.AbsoluteUri,
                request.Content?.ReadAsStringAsync().GetAwaiter().GetResult()));
            string body = request.Method == HttpMethod.Get
                ? $"[{EpicIssueJson}]"
                : request.Method == HttpMethod.Put
                    ? EpicIssueJson
                    : EpicIssueLinkJson;
            return JsonResponse(body, request.Method == HttpMethod.Post ? HttpStatusCode.Created : HttpStatusCode.OK);
        });

        using HttpClient httpClient = CreateHttpClient(handler);
        EpicsClient client = new(new GitLabApiConnection(httpClient));

        List<GitLabEpicIssue> issues = [];
        await foreach (GitLabEpicIssue issue in client.ListIssuesAsync(7, 4,
                           new EpicIssueListOptions { PerPage = 100 }, TestContext.Current.CancellationToken))
        {
            issues.Add(issue);
        }

        GitLabEpicIssueLink added = await client.AddIssueAsync(7, 4, 84, TestContext.Current.CancellationToken);
        GitLabEpicIssueLink removed = await client.RemoveIssueAsync(7, 4, 11, TestContext.Current.CancellationToken);
        GitLabEpicIssue reordered = await client.ReorderIssueAsync(7, 4, 11,
            new ReorderEpicIssueRequest { MoveAfterId = 10, PerPage = 50 }, TestContext.Current.CancellationToken);

        GitLabEpicIssue listed = Assert.Single(issues);
        Assert.Equal(11, listed.EpicIssueId);
        Assert.Equal(3, listed.RelativePosition);
        Assert.Equal(11, added.Id);
        Assert.True(removed.Issue.HasValue);
        Assert.Equal(11, reordered.EpicIssueId);
        Assert.Equal("https://gitlab.example/api/v4/groups/7/epics/4/issues?per_page=100", calls[0].Uri);
        Assert.Equal("https://gitlab.example/api/v4/groups/7/epics/4/issues/84", calls[1].Uri);
        Assert.Equal("https://gitlab.example/api/v4/groups/7/epics/4/issues/11", calls[2].Uri);
        Assert.Equal("""{"move_after_id":10,"per_page":50}""", calls[3].Body);
    }

    [Fact]
    public async Task EpicBoardOperations_UseDedicatedLegacyEpicBoardDtos()
    {
        List<(HttpMethod Method, string Uri)> calls = [];
        using StubHttpMessageHandler handler = new(request =>
        {
            calls.Add((request.Method, request.RequestUri!.AbsoluteUri));
            string body = request.RequestUri.AbsolutePath.EndsWith("/lists", StringComparison.Ordinal)
                          || request.RequestUri.AbsolutePath.EndsWith("/lists/9", StringComparison.Ordinal)
                ? request.RequestUri.AbsolutePath.EndsWith("/9", StringComparison.Ordinal)
                    ? EpicBoardListJson
                    : $"[{EpicBoardListJson}]"
                : request.RequestUri.AbsolutePath.EndsWith("/1", StringComparison.Ordinal)
                    ? EpicBoardJson
                    : $"[{EpicBoardJson}]";
            return JsonResponse(body);
        });

        using HttpClient httpClient = CreateHttpClient(handler);
        EpicsClient client = new(new GitLabApiConnection(httpClient));

        List<GitLabEpicBoard> boards = [];
        await foreach (GitLabEpicBoard board in client.ListBoardsAsync(7, TestContext.Current.CancellationToken))
        {
            boards.Add(board);
        }

        GitLabEpicBoard boardById = await client.GetBoardAsync(7, 1, TestContext.Current.CancellationToken);
        List<GitLabEpicBoardList> lists = [];
        await foreach (GitLabEpicBoardList list in client.ListBoardListsAsync(7, 1,
                           TestContext.Current.CancellationToken))
        {
            lists.Add(list);
        }

        GitLabEpicBoardList listById = await client.GetBoardListAsync(7, 1, 9,
            TestContext.Current.CancellationToken);

        Assert.Equal("Planning", Assert.Single(boards).Name);
        Assert.Equal("workflow", boardById.Labels?[0].Name);
        Assert.Equal("backlog", Assert.Single(lists).ListType);
        Assert.Equal(9, listById.Id);
        Assert.Equal("https://gitlab.example/api/v4/groups/7/epic_boards", calls[0].Uri);
        Assert.Equal("https://gitlab.example/api/v4/groups/7/epic_boards/1", calls[1].Uri);
        Assert.Equal("https://gitlab.example/api/v4/groups/7/epic_boards/1/lists", calls[2].Uri);
        Assert.Equal("https://gitlab.example/api/v4/groups/7/epic_boards/1/lists/9", calls[3].Uri);
    }

    [Fact]
    public async Task RelatedEpicOperations_UseRelationDtosAndGroupFilters()
    {
        List<(HttpMethod Method, string Uri, string? Body)> calls = [];
        using StubHttpMessageHandler handler = new(request =>
        {
            calls.Add((request.Method, request.RequestUri!.AbsoluteUri,
                request.Content?.ReadAsStringAsync().GetAwaiter().GetResult()));
            string body = request.Method == HttpMethod.Get ? $"[{RelatedEpicJson}]" : RelatedEpicLinkJson;
            return JsonResponse(body, request.Method == HttpMethod.Post ? HttpStatusCode.Created : HttpStatusCode.OK);
        });

        using HttpClient httpClient = CreateHttpClient(handler);
        EpicsClient client = new(new GitLabApiConnection(httpClient));

        List<GitLabRelatedEpic> related = [];
        await foreach (GitLabRelatedEpic item in client.ListRelatedAsync(7, 4, TestContext.Current.CancellationToken))
        {
            related.Add(item);
        }

        GitLabRelatedEpicLink created = await client.CreateRelatedLinkAsync(7, 4,
            new CreateRelatedEpicLinkRequest
            {
                TargetGroupId = "other/group", TargetEpicIid = 5, LinkType = GitLabEpicLinkType.Blocks
            }, TestContext.Current.CancellationToken);
        GitLabRelatedEpicLink deleted = await client.DeleteRelatedLinkAsync(7, 4, 41,
            TestContext.Current.CancellationToken);
        List<GitLabRelatedEpic> groupLinks = [];
        await foreach (GitLabRelatedEpic item in client.ListRelatedForGroupAsync("group/subgroup",
                           new RelatedEpicLinkListOptions
                           {
                               UpdatedAfter = new DateTimeOffset(2026, 1, 2, 3, 4, 5, TimeSpan.Zero), PerPage = 25
                           },
                           TestContext.Current.CancellationToken))
        {
            groupLinks.Add(item);
        }

        Assert.Equal("blocks", Assert.Single(related).LinkType);
        Assert.Equal(123, created.SourceEpic?.Id);
        Assert.Equal(124, deleted.TargetEpic?.Id);
        Assert.Single(groupLinks);
        Assert.Equal("https://gitlab.example/api/v4/groups/7/epics/4/related_epics", calls[0].Uri);
        Assert.Equal("""{"target_group_id":"other/group","target_epic_iid":5,"link_type":"blocks"}""", calls[1].Body);
        Assert.Equal("https://gitlab.example/api/v4/groups/7/epics/4/related_epics/41", calls[2].Uri);
        Assert.StartsWith("https://gitlab.example/api/v4/groups/group%2Fsubgroup/related_epic_links?", calls[3].Uri,
            StringComparison.Ordinal);
        Assert.Contains("updated_after=2026-01-02T03:04:05Z", calls[3].Uri,
            StringComparison.Ordinal);
        Assert.Contains("per_page=25", calls[3].Uri, StringComparison.Ordinal);
    }

    [Fact]
    public async Task CreateTodoAsync_PostsToEpicTodoRoute()
    {
        const string todo = """
                            { "id": 99, "target_type": "Epic", "state": "pending" }
                            """;
        using StubHttpMessageHandler handler = new(_ => JsonResponse(todo, HttpStatusCode.Created));
        using HttpClient httpClient = CreateHttpClient(handler);
        EpicsClient client = new(new GitLabApiConnection(httpClient));

        GitLabTodo result = await client.CreateTodoAsync("group/subgroup", 4, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/group%2Fsubgroup/epics/4/todo",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Null(handler.LastRequest?.Content);
        Assert.Equal(99, result.Id);
        Assert.Equal("Epic", result.TargetType);
    }

    private static HttpClient CreateHttpClient(StubHttpMessageHandler handler)
    {
        return new HttpClient(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
    }

    private static HttpResponseMessage JsonResponse(string json, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        return new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
    }
}