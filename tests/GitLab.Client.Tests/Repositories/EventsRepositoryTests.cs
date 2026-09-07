using System.Globalization;
using System.Net;
using System.Text;

using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Repositories;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Repositories;

public sealed class EventsRepositoryTests
{
    private const string PushEventJson = """
                                         [
                                           {
                                             "id": 9001,
                                             "project_id": 42,
                                             "action_name": "pushed to",
                                             "target_id": null,
                                             "target_iid": null,
                                             "target_type": null,
                                             "author_id": 7,
                                             "target_title": null,
                                             "created_at": "2024-06-11T09:12:33.123Z",
                                             "author": {
                                               "id": 7,
                                               "username": "root",
                                               "name": "Administrator",
                                               "state": "active",
                                               "web_url": "https://gitlab.example/root"
                                             },
                                             "author_username": "root",
                                             "imported": false,
                                             "imported_from": "none",
                                             "push_data": {
                                               "commit_count": 3,
                                               "action": "pushed",
                                               "ref_type": "branch",
                                               "commit_from": "0000000000000000000000000000000000000000",
                                               "commit_to": "c5feabde2d8cd023215af4d2ceeb7a64839fc428",
                                               "ref": "feature/events",
                                               "commit_title": "Add events feed"
                                             }
                                           }
                                         ]
                                         """;

    [Fact]
    public async Task ListAsync_BuildsInstanceEventsRoute_WithEveryFilter_AndDeserializesPushEvent()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(PushEventJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        EventsRepository repository = new(connection);

        EventListOptions options = new()
        {
            Scope = "all",
            Action = GitLabEventAction.Pushed,
            TargetType = GitLabEventTargetType.Issue,
            Before = new DateOnly(2024, 6, 30),
            After = new DateOnly(2024, 1, 1),
            Sort = GitLabEventSort.Asc,
            PerPage = 50
        };

        List<GitLabEvent> events = new();
        await foreach (GitLabEvent item in repository.ListAsync(options, TestContext.Current.CancellationToken))
        {
            events.Add(item);
        }

        // before/after are format: date in the spec, so they must be sent as yyyy-MM-dd, not as timestamps.
        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/events"
            + "?scope=all&action=pushed&target_type=issue&before=2024-06-30&after=2024-01-01&sort=asc&per_page=50",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabEvent single = Assert.Single(events);
        Assert.Equal(9001, single.Id);
        Assert.Equal(42, single.ProjectId);
        Assert.Equal("pushed to", single.ActionName);
        Assert.Equal(7, single.AuthorId);
        Assert.Null(single.TargetType);
        Assert.Null(single.TargetIid);

        // created_at is declared as a bare string with no format in the spec; GitLab still emits ISO-8601.
        Assert.Equal(DateTimeOffset.Parse("2024-06-11T09:12:33.123Z", CultureInfo.InvariantCulture),
            single.CreatedAt);

        Assert.Equal("root", single.AuthorUsername);
        Assert.Equal("Administrator", single.Author?.Name);
        Assert.Equal(new Uri("https://gitlab.example/root"), single.Author?.WebUrl);
        Assert.False(single.Imported);
        Assert.Equal("none", single.ImportedFrom);

        Assert.Equal(3, single.PushData?.CommitCount);
        Assert.Equal("pushed", single.PushData?.Action);
        Assert.Equal("branch", single.PushData?.RefType);
        Assert.Equal("feature/events", single.PushData?.Ref);
        Assert.Equal("Add events feed", single.PushData?.CommitTitle);
        Assert.Null(single.PushData?.RefCount);
        Assert.Null(single.Note);
        Assert.Null(single.WikiPage);
    }

    [Fact]
    public async Task ListAsync_WithoutOptions_BuildsBareEventsRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        EventsRepository repository = new(connection);

        List<GitLabEvent> events = new();
        await foreach (GitLabEvent item in repository.ListAsync(
                           cancellationToken: TestContext.Current.CancellationToken))
        {
            events.Add(item);
        }

        Assert.Equal("https://gitlab.example/api/v4/events", handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Empty(events);
    }

    [Fact]
    public async Task ListForProjectAsync_EncodesNamespacedProjectPath_AndDeserializesNoteEvent()
    {
        const string Json = """
                            [
                              {
                                "id": 9002,
                                "project_id": 42,
                                "action_name": "commented on",
                                "target_id": 5501,
                                "target_iid": 12,
                                "target_type": "Note",
                                "author_id": 7,
                                "target_title": "Flaky pipeline",
                                "created_at": "2024-06-12T14:00:00.000Z",
                                "note": {
                                  "id": 5501,
                                  "body": "Reproduced on main.",
                                  "system": false,
                                  "created_at": "2024-06-12T14:00:00.000Z",
                                  "author": {
                                    "id": 7,
                                    "username": "root",
                                    "name": "Administrator",
                                    "web_url": "https://gitlab.example/root"
                                  }
                                }
                              }
                            ]
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        EventsRepository repository = new(connection);

        List<GitLabEvent> events = new();
        await foreach (GitLabEvent item in repository.ListForProjectAsync("gitlab-org/gitlab",
                           new EventListOptions { TargetType = GitLabEventTargetType.Note, PerPage = 20 },
                           TestContext.Current.CancellationToken))
        {
            events.Add(item);
        }

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/events?target_type=note&per_page=20",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabEvent single = Assert.Single(events);
        Assert.Equal("commented on", single.ActionName);
        Assert.Equal("Note", single.TargetType);
        Assert.Equal(12, single.TargetIid);
        Assert.Equal("Flaky pipeline", single.TargetTitle);

        // The note payload reuses the existing GitLabNote DTO rather than redeclaring its shape.
        Assert.Equal(5501, single.Note?.Id);
        Assert.Equal("Reproduced on main.", single.Note?.Body);
        Assert.Equal("root", single.Note?.Author?.Username);
        Assert.False(single.Note?.System);
    }

    [Fact]
    public async Task ListForUserAsync_BuildsNumericUserEventsRoute_AndDeserializesWikiEvent()
    {
        const string Json = """
                            [
                              {
                                "id": 9003,
                                "project_id": 42,
                                "action_name": "updated",
                                "target_type": "WikiPage::Meta",
                                "author_id": 7,
                                "created_at": "2024-06-13T08:30:00.000Z",
                                "wiki_page": {
                                  "format": "markdown",
                                  "slug": "home/setup",
                                  "title": "Home / Setup",
                                  "wiki_page_meta_id": 77
                                }
                              }
                            ]
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        EventsRepository repository = new(connection);

        List<GitLabEvent> events = new();
        await foreach (GitLabEvent item in repository.ListForUserAsync(7,
                           new EventListOptions { Sort = GitLabEventSort.Desc }, TestContext.Current.CancellationToken))
        {
            events.Add(item);
        }

        Assert.Equal("https://gitlab.example/api/v4/users/7/events?sort=desc",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabEvent single = Assert.Single(events);
        Assert.Equal(9003, single.Id);

        // The wiki payload reuses the Wikis resource's GitLabWikiPage, in its content-free "basic" shape.
        Assert.Equal("home/setup", single.WikiPage?.Slug);
        Assert.Equal("Home / Setup", single.WikiPage?.Title);
        Assert.Equal(77, single.WikiPage?.WikiPageMetaId);
        Assert.Null(single.WikiPage?.Content);
    }

    [Fact]
    public async Task ListForProjectAsync_OnBulkPushEvent_ReadsRefCountAndAbsentCommitDetail()
    {
        const string Json = """
                            [
                              {
                                "id": 9004,
                                "project_id": 42,
                                "action_name": "pushed to",
                                "created_at": "2024-06-14T11:45:00.000Z",
                                "push_data": {
                                  "commit_count": 0,
                                  "action": "pushed",
                                  "ref_type": "branch",
                                  "ref_count": 5
                                }
                              }
                            ]
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        EventsRepository repository = new(connection);

        List<GitLabEvent> events = new();
        await foreach (GitLabEvent item in repository.ListForProjectAsync(42,
                           cancellationToken: TestContext.Current.CancellationToken))
        {
            events.Add(item);
        }

        Assert.Equal("https://gitlab.example/api/v4/projects/42/events",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        // A push past the activity limit collapses into one event with no per-commit detail at all.
        GitLabPushEventPayload? pushData = Assert.Single(events).PushData;
        Assert.Equal(0, pushData?.CommitCount);
        Assert.Equal(5, pushData?.RefCount);
        Assert.Null(pushData?.Ref);
        Assert.Null(pushData?.CommitFrom);
        Assert.Null(pushData?.CommitTo);
        Assert.Null(pushData?.CommitTitle);
    }

    [Fact]
    public async Task ListAsync_FollowsLinkHeaderAcrossPages()
    {
        int call = 0;
        using StubHttpMessageHandler handler = new(_ =>
        {
            call++;
            HttpResponseMessage response = new(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    call == 1
                        ? """[{ "id": 1, "action_name": "joined" }]"""
                        : """[{ "id": 2, "action_name": "left" }]""",
                    Encoding.UTF8,
                    "application/json")
            };

            if (call == 1)
            {
                response.Headers.TryAddWithoutValidation(
                    "Link",
                    "<https://gitlab.example/api/v4/events?page=2&per_page=1>; rel=\"next\"");
            }

            return response;
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        EventsRepository repository = new(connection);

        List<GitLabEvent> events = new();
        await foreach (GitLabEvent item in repository.ListAsync(new EventListOptions { PerPage = 1 },
                           TestContext.Current.CancellationToken))
        {
            events.Add(item);
        }

        Assert.Equal(2, call);
        Assert.Equal([1L, 2L], events.Select(static item => item.Id));
        Assert.Equal("https://gitlab.example/api/v4/events?page=2&per_page=1",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task ListAsync_OnUnauthenticatedRequest_ThrowsGitLabAuthenticationException()
    {
        const string Json = """{ "message": "401 Unauthorized" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Unauthorized)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        EventsRepository repository = new(connection);

        // The instance feed is the authenticated user's own; without a token it is a 401, not an empty list.
        GitLabAuthenticationException exception =
            await Assert.ThrowsAsync<GitLabAuthenticationException>(async () =>
            {
                await foreach (GitLabEvent _ in repository
                                   .ListAsync(cancellationToken: TestContext.Current.CancellationToken)
                                   .ConfigureAwait(false))
                {
                    // The exception is thrown while fetching the first page, before any item is yielded.
                }
            });

        Assert.Equal(HttpStatusCode.Unauthorized, exception.StatusCode);
        Assert.Equal("401 Unauthorized", exception.Message);
    }

    [Fact]
    public async Task ListForProjectAsync_OnMissingProject_ThrowsGitLabNotFoundException()
    {
        const string Json = """{ "message": "404 Project Not Found" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        EventsRepository repository = new(connection);

        GitLabNotFoundException exception = await Assert.ThrowsAsync<GitLabNotFoundException>(async () =>
        {
            await foreach (GitLabEvent _ in repository
                               .ListForProjectAsync(404, cancellationToken: TestContext.Current.CancellationToken)
                               .ConfigureAwait(false))
            {
                // The exception is thrown while fetching the first page, before any item is yielded.
            }
        });

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        Assert.Equal("404 Project Not Found", exception.Message);
    }

    [Fact]
    public async Task ListForProjectAsync_WritesTheEnumWireValues_NotTheCSharpMemberNames()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        EventsRepository repository = new(connection);

        await foreach (GitLabEvent _ in repository.ListForProjectAsync(
                           42,
                           new EventListOptions
                           {
                               Action = GitLabEventAction.Commented, TargetType = GitLabEventTargetType.MergeRequest
                           },
                           TestContext.Current.CancellationToken))
        {
            Assert.Fail("The stub returns an empty page.");
        }

        // GitLab runs Grape: an unknown query value is answered with an unfiltered 200, so the
        // multi-word member must reach the wire as merge_request rather than MergeRequest.
        Assert.Equal("https://gitlab.example/api/v4/projects/42/events?action=commented&target_type=merge_request",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }
}