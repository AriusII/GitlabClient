using System.Globalization;
using System.Net;
using System.Text;

using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Endpoints;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Endpoints;

public sealed class DiscussionsEndpointTests
{
    private const string DiscussionId = "6a9c1750b37d513a43987b574953fceb50b03ce7";

    /// <summary>
    ///     A resolved merge-request thread carrying the note-level resolution fields GitLabNote gained with this
    ///     resource.
    /// </summary>
    private const string ResolvedDiscussionJson = """
                                                  {
                                                    "id": "6a9c1750b37d513a43987b574953fceb50b03ce7",
                                                    "individual_note": false,
                                                    "resolvable": true,
                                                    "resolved": true,
                                                    "notes": [
                                                      {
                                                        "id": 1126,
                                                        "type": "DiffNote",
                                                        "body": "Please rename this",
                                                        "author": {
                                                          "id": 7,
                                                          "username": "reviewer",
                                                          "name": "Rita Reviewer",
                                                          "state": "active",
                                                          "web_url": "https://gitlab.example/reviewer"
                                                        },
                                                        "created_at": "2026-01-15T09:30:00.000Z",
                                                        "updated_at": "2026-01-16T11:00:00.000Z",
                                                        "system": false,
                                                        "noteable_id": 4242,
                                                        "noteable_iid": 17,
                                                        "noteable_type": "MergeRequest",
                                                        "project_id": 42,
                                                        "commit_id": "2695effb5807a22ff3d138d593fd856244e155e7",
                                                        "position": {
                                                          "base_sha": "aa149113",
                                                          "start_sha": "b3a0a8c4",
                                                          "head_sha": "be3020c7",
                                                          "position_type": "text",
                                                          "old_path": "example.md",
                                                          "new_path": "example.md",
                                                          "old_line": 2,
                                                          "new_line": 4
                                                        },
                                                        "resolvable": true,
                                                        "resolved": true,
                                                        "resolved_by": {
                                                          "id": 9,
                                                          "username": "author",
                                                          "name": "Alex Author",
                                                          "state": "active",
                                                          "web_url": "https://gitlab.example/author"
                                                        },
                                                        "resolved_at": "2026-01-16T11:00:00.000Z",
                                                        "suggestions": [
                                                          {
                                                            "id": 22,
                                                            "from_line": 4,
                                                            "to_line": 4,
                                                            "appliable": true,
                                                            "applied": false,
                                                            "from_content": "oldName",
                                                            "to_content": "newName"
                                                          }
                                                        ],
                                                        "confidential": false,
                                                        "internal": false
                                                      }
                                                    ]
                                                  }
                                                  """;

    private const string IndividualNoteDiscussionJson = """
                                                        {
                                                          "id": "aa1b2c3d4e5f60718293a4b5c6d7e8f901234567",
                                                          "individual_note": true,
                                                          "notes": [
                                                            {
                                                              "id": 301,
                                                              "body": "Reproduced on 19.4",
                                                              "author": {
                                                                "id": 3,
                                                                "username": "triage",
                                                                "name": "Tina Triage",
                                                                "web_url": "https://gitlab.example/triage"
                                                              },
                                                              "created_at": "2026-02-01T08:00:00.000Z",
                                                              "system": false,
                                                              "noteable_type": "Issue",
                                                              "noteable_iid": 5
                                                            }
                                                          ]
                                                        }
                                                        """;

    private const string ReplyNoteJson = """
                                         {
                                           "id": 1127,
                                           "type": "DiscussionNote",
                                           "body": "Renamed, thanks.",
                                           "author": {
                                             "id": 9,
                                             "username": "author",
                                             "name": "Alex Author",
                                             "web_url": "https://gitlab.example/author"
                                           },
                                           "created_at": "2026-01-16T10:45:00.000Z",
                                           "system": false,
                                           "resolvable": true,
                                           "resolved": false,
                                           "internal": true
                                         }
                                         """;

    [Fact]
    public async Task ListForIssueAsync_BuildsIssueDiscussionsRoute_AndDeserializesNotes()
    {
        using StubHttpMessageHandler handler = Responds(HttpStatusCode.OK, $"[{IndividualNoteDiscussionJson}]");
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        DiscussionsClient repository = new(connection);

        List<GitLabDiscussion> discussions = [];
        await foreach (GitLabDiscussion item in repository.ListForIssueAsync(42, 5,
                           TestContext.Current.CancellationToken))
        {
            discussions.Add(item);
        }

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/42/issues/5/discussions",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabDiscussion discussion = Assert.Single(discussions);
        Assert.Equal("aa1b2c3d4e5f60718293a4b5c6d7e8f901234567", discussion.Id);
        Assert.True(discussion.IndividualNote);
        Assert.Null(discussion.Resolvable);

        GitLabNote note = Assert.Single(discussion.Notes!);
        Assert.Equal(301, note.Id);
        Assert.Equal("Reproduced on 19.4", note.Body);
        Assert.Equal("Issue", note.NoteableType);
        Assert.Equal(5, note.NoteableIid);
        Assert.Equal("triage", note.Author?.Username);
        Assert.False(note.System);
    }

    [Fact]
    public async Task GetForIssueAsync_EscapesDiscussionIdSegment_AndDeserializesResolutionFields()
    {
        using StubHttpMessageHandler handler = Responds(HttpStatusCode.OK, ResolvedDiscussionJson);
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        DiscussionsClient repository = new(connection);

        GitLabDiscussion discussion =
            await repository.GetForIssueAsync(42, 5, "thread/with slash", TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/42/issues/5/discussions/thread%2Fwith%20slash",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        Assert.Equal(DiscussionId, discussion.Id);
        Assert.False(discussion.IndividualNote);
        Assert.True(discussion.Resolvable);
        Assert.True(discussion.Resolved);

        GitLabNote note = Assert.Single(discussion.Notes!);
        Assert.Equal("DiffNote", note.Type);
        Assert.True(note.Resolved);
        Assert.Equal("author", note.ResolvedBy?.Username);
        Assert.Equal(DateTimeOffset.Parse("2026-01-16T11:00:00Z", CultureInfo.InvariantCulture), note.ResolvedAt);
        Assert.Equal(4242, note.NoteableId);
        Assert.Equal(42, note.ProjectId);
        Assert.Equal("2695effb5807a22ff3d138d593fd856244e155e7", note.CommitId);
        Assert.Equal("aa149113", note.Position?.BaseSha);
        Assert.Equal(GitLabNotePositionType.Text, note.Position?.PositionType);
        Assert.Equal(4, note.Position?.NewLine);
        GitLabSuggestion suggestion = Assert.Single(note.Suggestions!);
        Assert.Equal(22, suggestion.Id);
        Assert.True(suggestion.Appliable);
        Assert.Equal("newName", suggestion.ToContent);
        Assert.False(note.Confidential);
        Assert.False(note.Internal);
    }

    [Fact]
    public async Task GetForIssueAsync_EncodesNamespacedProjectPath()
    {
        using StubHttpMessageHandler handler = Responds(HttpStatusCode.OK, IndividualNoteDiscussionJson);
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        DiscussionsClient repository = new(connection);

        await repository.GetForIssueAsync("gitlab-org/gitlab", 5, DiscussionId,
            TestContext.Current.CancellationToken);

        Assert.Equal(
            $"https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/issues/5/discussions/{DiscussionId}",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task CreateForIssueAsync_PostsBodyAndCreatedAt_AndDeserializesDiscussion()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler =
            Records(HttpStatusCode.Created, IndividualNoteDiscussionJson, body => sentBody = body);
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        DiscussionsClient repository = new(connection);

        CreateDiscussionRequest request = new()
        {
            Body = "Reproduced on 19.4",
            CreatedAt = DateTimeOffset.Parse("2026-02-01T08:00:00Z", CultureInfo.InvariantCulture)
        };

        GitLabDiscussion discussion =
            await repository.CreateForIssueAsync(42, 5, request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/42/issues/5/discussions",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("application/json", handler.LastRequest?.Content?.Headers.ContentType?.MediaType);
        Assert.Contains("\"body\":\"Reproduced on 19.4\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"created_at\":\"2026-02-01T08:00:00", sentBody, StringComparison.Ordinal);
        Assert.Equal("aa1b2c3d4e5f60718293a4b5c6d7e8f901234567", discussion.Id);
    }

    [Fact]
    public async Task CreateForIssueAsync_OmitsCreatedAt_WhenNotSupplied()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler =
            Records(HttpStatusCode.Created, IndividualNoteDiscussionJson, body => sentBody = body);
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        DiscussionsClient repository = new(connection);

        await repository.CreateForIssueAsync(42, 5, new CreateDiscussionRequest { Body = "Just the body" },
            TestContext.Current.CancellationToken);

        Assert.Equal("""{"body":"Just the body"}""", sentBody);
    }

    [Fact]
    public async Task ResolveForIssueAsync_PutsResolvedFlag_AndEscapesDiscussionId()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = Records(HttpStatusCode.OK, ResolvedDiscussionJson,
            body => sentBody = body);
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        DiscussionsClient repository = new(connection);

        GitLabDiscussion discussion = await repository.ResolveForIssueAsync(42, 5, "thread/one",
            new ResolveDiscussionRequest { Resolved = true }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/42/issues/5/discussions/thread%2Fone",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"resolved":true}""", sentBody);
        Assert.True(discussion.Resolved);
    }

    [Fact]
    public async Task AddNoteToIssueDiscussionAsync_PostsReplyToNotesRoute_AndDeserializesNote()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = Records(HttpStatusCode.Created, ReplyNoteJson, body => sentBody = body);
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        DiscussionsClient repository = new(connection);

        GitLabNote note = await repository.AddNoteToIssueDiscussionAsync(42, 5, DiscussionId,
            new CreateDiscussionNoteRequest { Body = "Renamed, thanks." }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal($"https://gitlab.example/api/v4/projects/42/issues/5/discussions/{DiscussionId}/notes",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"body":"Renamed, thanks."}""", sentBody);
        Assert.Equal(1127, note.Id);
        Assert.Equal("DiscussionNote", note.Type);
        Assert.False(note.Resolved);
        Assert.True(note.Internal);
    }

    [Fact]
    public async Task ListForMergeRequestAsync_BuildsMergeRequestDiscussionsRoute_AndDeserializesDiscussions()
    {
        using StubHttpMessageHandler handler = Responds(HttpStatusCode.OK, $"[{ResolvedDiscussionJson}]");
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        DiscussionsClient repository = new(connection);

        List<GitLabDiscussion> discussions = [];
        await foreach (GitLabDiscussion item in repository.ListForMergeRequestAsync(42, 17,
                           TestContext.Current.CancellationToken))
        {
            discussions.Add(item);
        }

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/42/merge_requests/17/discussions",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabDiscussion discussion = Assert.Single(discussions);
        Assert.Equal(DiscussionId, discussion.Id);
        Assert.True(discussion.Resolved);
        Assert.Equal("Please rename this", Assert.Single(discussion.Notes!).Body);
    }

    [Fact]
    public async Task GetForMergeRequestAsync_BuildsRoute_AndDeserializesDiscussion()
    {
        using StubHttpMessageHandler handler = Responds(HttpStatusCode.OK, ResolvedDiscussionJson);
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        DiscussionsClient repository = new(connection);

        GitLabDiscussion discussion =
            await repository.GetForMergeRequestAsync("gitlab-org/gitlab", 17, DiscussionId,
                TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal(
            $"https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/merge_requests/17/discussions/{DiscussionId}",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.True(discussion.Resolvable);
        Assert.Equal("Rita Reviewer", Assert.Single(discussion.Notes!).Author?.Name);
    }

    [Fact]
    public async Task CreateForMergeRequestAsync_PostsBody_AndDeserializesDiscussion()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = Records(HttpStatusCode.Created, ResolvedDiscussionJson,
            body => sentBody = body);
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        DiscussionsClient repository = new(connection);

        GitLabDiscussion discussion = await repository.CreateForMergeRequestAsync(42, 17,
            new CreateDiscussionRequest { Body = "Please rename this" }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/42/merge_requests/17/discussions",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"body":"Please rename this"}""", sentBody);
        Assert.Equal(DiscussionId, discussion.Id);
    }

    [Fact]
    public async Task ResolveForMergeRequestAsync_PutsUnresolvedFlag_AndReturnsUpdatedThread()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = Records(HttpStatusCode.OK, ResolvedDiscussionJson,
            body => sentBody = body);
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        DiscussionsClient repository = new(connection);

        await repository.ResolveForMergeRequestAsync(42, 17, DiscussionId,
            new ResolveDiscussionRequest { Resolved = false }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal($"https://gitlab.example/api/v4/projects/42/merge_requests/17/discussions/{DiscussionId}",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"resolved":false}""", sentBody);
    }

    [Fact]
    public async Task AddNoteToMergeRequestDiscussionAsync_PostsReplyToNotesRoute()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = Records(HttpStatusCode.Created, ReplyNoteJson, body => sentBody = body);
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        DiscussionsClient repository = new(connection);

        GitLabNote note = await repository.AddNoteToMergeRequestDiscussionAsync(42, 17, "thread/one",
            new CreateDiscussionNoteRequest { Body = "Renamed, thanks." }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/42/merge_requests/17/discussions/thread%2Fone/notes",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"body":"Renamed, thanks."}""", sentBody);
        Assert.Equal("Renamed, thanks.", note.Body);
        Assert.Equal("author", note.Author?.Username);
    }

    [Fact]
    public async Task ListForCommitAsync_KeepsRepositorySegment_AndEscapesRefLikeSha()
    {
        using StubHttpMessageHandler handler = Responds(HttpStatusCode.OK, $"[{ResolvedDiscussionJson}]");
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        DiscussionsClient repository = new(connection);

        List<GitLabDiscussion> discussions = [];
        await foreach (GitLabDiscussion item in repository.ListForCommitAsync("gitlab-org/gitlab", "refs/heads/main",
                           TestContext.Current.CancellationToken))
        {
            discussions.Add(item);
        }

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/repository/commits/refs%2Fheads%2Fmain/discussions",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(DiscussionId, Assert.Single(discussions).Id);
    }

    [Fact]
    public async Task GetForCommitAsync_EscapesBothShaAndDiscussionId_AndDeserializesDiscussion()
    {
        using StubHttpMessageHandler handler = Responds(HttpStatusCode.OK, ResolvedDiscussionJson);
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        DiscussionsClient repository = new(connection);

        GitLabDiscussion discussion = await repository.GetForCommitAsync(42, "refs/heads/main", "thread/one",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/projects/42/repository/commits/refs%2Fheads%2Fmain/discussions/thread%2Fone",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("2695effb5807a22ff3d138d593fd856244e155e7", Assert.Single(discussion.Notes!).CommitId);
    }

    [Fact]
    public async Task CreateForCommitAsync_PostsBody_AndDeserializesDiscussion()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = Records(HttpStatusCode.Created, ResolvedDiscussionJson,
            body => sentBody = body);
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        DiscussionsClient repository = new(connection);

        GitLabDiscussion discussion = await repository.CreateForCommitAsync(42,
            "2695effb5807a22ff3d138d593fd856244e155e7", new CreateDiscussionRequest { Body = "Nice cleanup" },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/projects/42/repository/commits/2695effb5807a22ff3d138d593fd856244e155e7/discussions",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"body":"Nice cleanup"}""", sentBody);
        Assert.Equal(DiscussionId, discussion.Id);
    }

    [Fact]
    public async Task AddNoteToCommitDiscussionAsync_PostsReplyToNotesRoute_AndDeserializesNote()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = Records(HttpStatusCode.Created, ReplyNoteJson, body => sentBody = body);
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        DiscussionsClient repository = new(connection);

        GitLabNote note = await repository.AddNoteToCommitDiscussionAsync(42,
            "2695effb5807a22ff3d138d593fd856244e155e7", DiscussionId,
            new CreateDiscussionNoteRequest { Body = "Renamed, thanks." }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/projects/42/repository/commits/2695effb5807a22ff3d138d593fd856244e155e7/"
            + $"discussions/{DiscussionId}/notes",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"body":"Renamed, thanks."}""", sentBody);
        Assert.Equal(1127, note.Id);
    }

    [Fact]
    public async Task GetForIssueAsync_OnNotFound_ThrowsGitLabNotFoundException()
    {
        using StubHttpMessageHandler handler =
            Responds(HttpStatusCode.NotFound, """{ "message": "404 Discussion Not Found" }""");
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        DiscussionsClient repository = new(connection);

        GitLabNotFoundException exception = await Assert.ThrowsAsync<GitLabNotFoundException>(() =>
            repository.GetForIssueAsync(42, 5, DiscussionId, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        Assert.Equal("404 Discussion Not Found", exception.Message);
    }

    [Fact]
    public async Task ResolveForMergeRequestAsync_OnForbidden_ThrowsGitLabForbiddenException()
    {
        using StubHttpMessageHandler handler =
            Responds(HttpStatusCode.Forbidden, """{ "message": "403 Forbidden" }""");
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        DiscussionsClient repository = new(connection);

        GitLabForbiddenException exception = await Assert.ThrowsAsync<GitLabForbiddenException>(() =>
            repository.ResolveForMergeRequestAsync(42, 17, DiscussionId,
                new ResolveDiscussionRequest { Resolved = true }, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.Forbidden, exception.StatusCode);
        Assert.Equal("403 Forbidden", exception.Message);
    }

    [Fact]
    public async Task ListNotesInIssueDiscussionAsync_BuildsDiscussionNotesRoute_AndStreamsNotes()
    {
        using StubHttpMessageHandler handler = Responds(HttpStatusCode.OK, $"[{ReplyNoteJson}]");
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        DiscussionsClient repository = new(connection);

        List<GitLabNote> notes = [];
        await foreach (GitLabNote item in repository.ListNotesInIssueDiscussionAsync(42, 5, "thread/one",
                           TestContext.Current.CancellationToken))
        {
            notes.Add(item);
        }

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/42/issues/5/discussions/thread%2Fone/notes",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(1127, Assert.Single(notes).Id);
    }

    [Fact]
    public async Task GetNoteInIssueDiscussionAsync_BuildsSingleNoteRoute_AndDeserializesNote()
    {
        using StubHttpMessageHandler handler = Responds(HttpStatusCode.OK, ReplyNoteJson);
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        DiscussionsClient repository = new(connection);

        GitLabNote note = await repository.GetNoteInIssueDiscussionAsync(42, 5, DiscussionId, 1127,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal($"https://gitlab.example/api/v4/projects/42/issues/5/discussions/{DiscussionId}/notes/1127",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("Renamed, thanks.", note.Body);
    }

    [Fact]
    public async Task UpdateNoteInIssueDiscussionAsync_PutsBodyOnly()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = Records(HttpStatusCode.OK, ReplyNoteJson, body => sentBody = body);
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        DiscussionsClient repository = new(connection);

        await repository.UpdateNoteInIssueDiscussionAsync(42, 5, DiscussionId, 1127,
            new UpdateDiscussionNoteRequest { Body = "Renamed, thanks." }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal($"https://gitlab.example/api/v4/projects/42/issues/5/discussions/{DiscussionId}/notes/1127",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"body":"Renamed, thanks."}""", sentBody);
    }

    [Fact]
    public async Task ResolveNoteInIssueDiscussionAsync_PutsResolvedOnly_ToTheSameNoteRoute()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = Records(HttpStatusCode.OK, ReplyNoteJson, body => sentBody = body);
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        DiscussionsClient repository = new(connection);

        await repository.ResolveNoteInIssueDiscussionAsync(42, 5, DiscussionId, 1127,
            new ResolveDiscussionRequest { Resolved = true }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal($"https://gitlab.example/api/v4/projects/42/issues/5/discussions/{DiscussionId}/notes/1127",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"resolved":true}""", sentBody);
    }

    [Fact]
    public async Task DeleteNoteFromIssueDiscussionAsync_SendsDeleteToNoteRoute()
    {
        using StubHttpMessageHandler handler = RespondsNoContent();
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        DiscussionsClient repository = new(connection);

        await repository.DeleteNoteFromIssueDiscussionAsync("gitlab-org/gitlab", 5, "thread/one", 1127,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/issues/5/discussions/thread%2Fone/notes/1127",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    /// <summary>
    ///     The one request shape in this resource that is genuinely nested: a diff-anchored merge request
    ///     thread. Asserted as one exact payload because the nesting - and the snake_case of every member
    ///     inside it - is the whole point of modelling <c>position</c> as its own record.
    /// </summary>
    [Fact]
    public async Task CreateForMergeRequestAsync_WithPosition_SerializesTheNestedDiffAnchor()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = Records(HttpStatusCode.Created, ResolvedDiscussionJson,
            body => sentBody = body);
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        DiscussionsClient repository = new(connection);

        CreateDiscussionRequest request = new()
        {
            Body = "Please rename this",
            Position = new GitLabNotePosition
            {
                BaseSha = "aa149113",
                StartSha = "b3a0a8c4",
                HeadSha = "be3020c7",
                PositionType = GitLabNotePositionType.Text,
                NewPath = "example.md",
                NewLine = 4,
                OldPath = "example.md",
                OldLine = 2,
                LineRange = new GitLabNoteLineRange
                {
                    Start = new GitLabNoteLinePosition
                    {
                        LineCode = "1c497fbb3a46b78edf04cc2a2fa33f67e3ffbe2a_2_4",
                        Type = "new",
                        OldLine = 2,
                        NewLine = 4
                    },
                    End = new GitLabNoteLinePosition
                    {
                        LineCode = "1c497fbb3a46b78edf04cc2a2fa33f67e3ffbe2a_2_6",
                        Type = "new",
                        OldLine = 2,
                        NewLine = 6
                    }
                }
            }
        };

        await repository.CreateForMergeRequestAsync(42, 17, request, TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/42/merge_requests/17/discussions",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(
            """{"body":"Please rename this","position":{"base_sha":"aa149113","start_sha":"b3a0a8c4","head_sha":"be3020c7","position_type":"text","new_path":"example.md","new_line":4,"old_path":"example.md","old_line":2,"line_range":{"start":{"line_code":"1c497fbb3a46b78edf04cc2a2fa33f67e3ffbe2a_2_4","type":"new","old_line":2,"new_line":4},"end":{"line_code":"1c497fbb3a46b78edf04cc2a2fa33f67e3ffbe2a_2_6","type":"new","old_line":2,"new_line":6}}}}""",
            sentBody);
    }

    [Fact]
    public async Task ListNotesInMergeRequestDiscussionAsync_BuildsDiscussionNotesRoute()
    {
        using StubHttpMessageHandler handler = Responds(HttpStatusCode.OK, $"[{ReplyNoteJson}]");
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        DiscussionsClient repository = new(connection);

        List<GitLabNote> notes = [];
        await foreach (GitLabNote item in repository.ListNotesInMergeRequestDiscussionAsync(42, 17, DiscussionId,
                           TestContext.Current.CancellationToken))
        {
            notes.Add(item);
        }

        Assert.Equal($"https://gitlab.example/api/v4/projects/42/merge_requests/17/discussions/{DiscussionId}/notes",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("DiscussionNote", Assert.Single(notes).Type);
    }

    [Fact]
    public async Task GetNoteInMergeRequestDiscussionAsync_BuildsSingleNoteRoute()
    {
        using StubHttpMessageHandler handler = Responds(HttpStatusCode.OK, ReplyNoteJson);
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        DiscussionsClient repository = new(connection);

        GitLabNote note = await repository.GetNoteInMergeRequestDiscussionAsync(42, 17, "thread/one", 1127,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/42/merge_requests/17/discussions/thread%2Fone/notes/1127",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.False(note.Resolved);
    }

    [Fact]
    public async Task UpdateNoteInMergeRequestDiscussionAsync_PutsBodyOnly()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = Records(HttpStatusCode.OK, ReplyNoteJson, body => sentBody = body);
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        DiscussionsClient repository = new(connection);

        await repository.UpdateNoteInMergeRequestDiscussionAsync(42, 17, DiscussionId, 1127,
            new UpdateDiscussionNoteRequest { Body = "Renamed, thanks." }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("""{"body":"Renamed, thanks."}""", sentBody);
    }

    /// <summary>
    ///     GitLab validates the note-level PUT as "exactly one of body, resolved", so the resolving half is its
    ///     own method sending its own payload to the very same route.
    /// </summary>
    [Fact]
    public async Task ResolveNoteInMergeRequestDiscussionAsync_PutsResolvedOnly_ToTheSameNoteRoute()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = Records(HttpStatusCode.OK, ReplyNoteJson, body => sentBody = body);
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        DiscussionsClient repository = new(connection);

        await repository.ResolveNoteInMergeRequestDiscussionAsync(42, 17, DiscussionId, 1127,
            new ResolveDiscussionRequest { Resolved = true }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal(
            $"https://gitlab.example/api/v4/projects/42/merge_requests/17/discussions/{DiscussionId}/notes/1127",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"resolved":true}""", sentBody);
    }

    [Fact]
    public async Task DeleteNoteFromMergeRequestDiscussionAsync_SendsDeleteToNoteRoute()
    {
        using StubHttpMessageHandler handler = RespondsNoContent();
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        DiscussionsClient repository = new(connection);

        await repository.DeleteNoteFromMergeRequestDiscussionAsync(42, 17, DiscussionId, 1127,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal(
            $"https://gitlab.example/api/v4/projects/42/merge_requests/17/discussions/{DiscussionId}/notes/1127",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task ListNotesInCommitDiscussionAsync_EscapesShaAndDiscussionId()
    {
        using StubHttpMessageHandler handler = Responds(HttpStatusCode.OK, $"[{ReplyNoteJson}]");
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        DiscussionsClient repository = new(connection);

        List<GitLabNote> notes = [];
        await foreach (GitLabNote item in repository.ListNotesInCommitDiscussionAsync(42, "refs/heads/main",
                           "thread/one", TestContext.Current.CancellationToken))
        {
            notes.Add(item);
        }

        Assert.Equal(
            "https://gitlab.example/api/v4/projects/42/repository/commits/refs%2Fheads%2Fmain/discussions/"
            + "thread%2Fone/notes",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Single(notes);
    }

    [Fact]
    public async Task GetNoteInCommitDiscussionAsync_EscapesShaAndDiscussionId()
    {
        using StubHttpMessageHandler handler = Responds(HttpStatusCode.OK, ReplyNoteJson);
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        DiscussionsClient repository = new(connection);

        await repository.GetNoteInCommitDiscussionAsync(42, "refs/heads/main", "thread/one", 1127,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/projects/42/repository/commits/refs%2Fheads%2Fmain/discussions/"
            + "thread%2Fone/notes/1127",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task UpdateNoteInCommitDiscussionAsync_PutsBodyToNoteRoute()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = Records(HttpStatusCode.OK, ReplyNoteJson, body => sentBody = body);
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        DiscussionsClient repository = new(connection);

        await repository.UpdateNoteInCommitDiscussionAsync(42, "2695effb5807a22ff3d138d593fd856244e155e7",
            DiscussionId, 1127, new UpdateDiscussionNoteRequest { Body = "Renamed, thanks." },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/projects/42/repository/commits/2695effb5807a22ff3d138d593fd856244e155e7/"
            + $"discussions/{DiscussionId}/notes/1127",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"body":"Renamed, thanks."}""", sentBody);
    }

    [Fact]
    public async Task ResolveNoteInCommitDiscussionAsync_PutsResolvedOnly_AndEscapesPathParameters()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = Records(HttpStatusCode.OK, ReplyNoteJson, body => sentBody = body);
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        DiscussionsClient repository = new(connection);

        await repository.ResolveNoteInCommitDiscussionAsync(42, "refs/heads/main", "thread/one", 1127,
            new ResolveDiscussionRequest { Resolved = false }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/projects/42/repository/commits/refs%2Fheads%2Fmain/discussions/"
            + "thread%2Fone/notes/1127",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"resolved":false}""", sentBody);
    }

    [Fact]
    public async Task DeleteNoteFromCommitDiscussionAsync_SendsDeleteToNoteRoute()
    {
        using StubHttpMessageHandler handler = RespondsNoContent();
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        DiscussionsClient repository = new(connection);

        await repository.DeleteNoteFromCommitDiscussionAsync(42, "2695effb5807a22ff3d138d593fd856244e155e7",
            DiscussionId, 1127, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/projects/42/repository/commits/2695effb5807a22ff3d138d593fd856244e155e7/"
            + $"discussions/{DiscussionId}/notes/1127",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task ListForSnippetAsync_BuildsSnippetDiscussionsRoute()
    {
        using StubHttpMessageHandler handler = Responds(HttpStatusCode.OK, $"[{IndividualNoteDiscussionJson}]");
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        DiscussionsClient repository = new(connection);

        List<GitLabDiscussion> discussions = [];
        await foreach (GitLabDiscussion item in repository.ListForSnippetAsync("gitlab-org/gitlab", 9,
                           TestContext.Current.CancellationToken))
        {
            discussions.Add(item);
        }

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/snippets/9/discussions",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.True(Assert.Single(discussions).IndividualNote);
    }

    [Fact]
    public async Task GetForSnippetAsync_EscapesDiscussionId()
    {
        using StubHttpMessageHandler handler = Responds(HttpStatusCode.OK, IndividualNoteDiscussionJson);
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        DiscussionsClient repository = new(connection);

        await repository.GetForSnippetAsync(42, 9, "thread/one", TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/42/snippets/9/discussions/thread%2Fone",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task CreateForSnippetAsync_PostsBody()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = Records(HttpStatusCode.Created, IndividualNoteDiscussionJson,
            body => sentBody = body);
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        DiscussionsClient repository = new(connection);

        await repository.CreateForSnippetAsync(42, 9, new CreateDiscussionRequest { Body = "Reproduced on 19.4" },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/42/snippets/9/discussions",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"body":"Reproduced on 19.4"}""", sentBody);
    }

    [Fact]
    public async Task ListNotesInSnippetDiscussionAsync_BuildsDiscussionNotesRoute()
    {
        using StubHttpMessageHandler handler = Responds(HttpStatusCode.OK, $"[{ReplyNoteJson}]");
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        DiscussionsClient repository = new(connection);

        List<GitLabNote> notes = [];
        await foreach (GitLabNote item in repository.ListNotesInSnippetDiscussionAsync(42, 9, DiscussionId,
                           TestContext.Current.CancellationToken))
        {
            notes.Add(item);
        }

        Assert.Equal($"https://gitlab.example/api/v4/projects/42/snippets/9/discussions/{DiscussionId}/notes",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Single(notes);
    }

    [Fact]
    public async Task AddNoteToSnippetDiscussionAsync_PostsBodyAndCreatedAt()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = Records(HttpStatusCode.Created, ReplyNoteJson, body => sentBody = body);
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        DiscussionsClient repository = new(connection);

        await repository.AddNoteToSnippetDiscussionAsync(42, 9, DiscussionId,
            new CreateDiscussionNoteRequest
            {
                Body = "Renamed, thanks.",
                CreatedAt = DateTimeOffset.Parse("2026-01-16T10:45:00Z", CultureInfo.InvariantCulture)
            },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal($"https://gitlab.example/api/v4/projects/42/snippets/9/discussions/{DiscussionId}/notes",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Contains("\"body\":\"Renamed, thanks.\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"created_at\":\"2026-01-16T10:45:00", sentBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetNoteInSnippetDiscussionAsync_BuildsSingleNoteRoute()
    {
        using StubHttpMessageHandler handler = Responds(HttpStatusCode.OK, ReplyNoteJson);
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        DiscussionsClient repository = new(connection);

        await repository.GetNoteInSnippetDiscussionAsync(42, 9, DiscussionId, 1127,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal($"https://gitlab.example/api/v4/projects/42/snippets/9/discussions/{DiscussionId}/notes/1127",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task UpdateNoteInSnippetDiscussionAsync_PutsBody()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = Records(HttpStatusCode.OK, ReplyNoteJson, body => sentBody = body);
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        DiscussionsClient repository = new(connection);

        await repository.UpdateNoteInSnippetDiscussionAsync(42, 9, DiscussionId, 1127,
            new UpdateDiscussionNoteRequest { Body = "Renamed, thanks." }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("""{"body":"Renamed, thanks."}""", sentBody);
    }

    [Fact]
    public async Task ResolveNoteInSnippetDiscussionAsync_PutsResolvedOnly_AndEncodesTheProjectPath()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = Records(HttpStatusCode.OK, ReplyNoteJson, body => sentBody = body);
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        DiscussionsClient repository = new(connection);

        await repository.ResolveNoteInSnippetDiscussionAsync("gitlab-org/gitlab", 9, DiscussionId, 1127,
            new ResolveDiscussionRequest { Resolved = true }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal(
            $"https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/snippets/9/discussions/{DiscussionId}/notes/1127",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"resolved":true}""", sentBody);
    }

    [Fact]
    public async Task DeleteNoteFromSnippetDiscussionAsync_SendsDeleteToNoteRoute()
    {
        using StubHttpMessageHandler handler = RespondsNoContent();
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        DiscussionsClient repository = new(connection);

        await repository.DeleteNoteFromSnippetDiscussionAsync(42, 9, DiscussionId, 1127,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal($"https://gitlab.example/api/v4/projects/42/snippets/9/discussions/{DiscussionId}/notes/1127",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task ListForEpicAsync_BuildsGroupScopedRoute_AndEncodesNamespacedGroupPath()
    {
        using StubHttpMessageHandler handler = Responds(HttpStatusCode.OK, $"[{IndividualNoteDiscussionJson}]");
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        DiscussionsClient repository = new(connection);

        List<GitLabDiscussion> discussions = [];
        await foreach (GitLabDiscussion item in repository.ListForEpicAsync("gitlab-org/subgroup", 11,
                           TestContext.Current.CancellationToken))
        {
            discussions.Add(item);
        }

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/gitlab-org%2Fsubgroup/epics/11/discussions",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Single(discussions);
    }

    [Fact]
    public async Task GetForEpicAsync_EscapesDiscussionId()
    {
        using StubHttpMessageHandler handler = Responds(HttpStatusCode.OK, IndividualNoteDiscussionJson);
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        DiscussionsClient repository = new(connection);

        await repository.GetForEpicAsync(7, 11, "thread/one", TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/groups/7/epics/11/discussions/thread%2Fone",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task CreateForEpicAsync_PostsBody()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = Records(HttpStatusCode.Created, IndividualNoteDiscussionJson,
            body => sentBody = body);
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        DiscussionsClient repository = new(connection);

        await repository.CreateForEpicAsync(7, 11, new CreateDiscussionRequest { Body = "Scope creep" },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/7/epics/11/discussions",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"body":"Scope creep"}""", sentBody);
    }

    [Fact]
    public async Task ResolveForEpicAsync_PutsResolvedFlag()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = Records(HttpStatusCode.OK, ResolvedDiscussionJson,
            body => sentBody = body);
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        DiscussionsClient repository = new(connection);

        GitLabDiscussion discussion = await repository.ResolveForEpicAsync(7, 11, DiscussionId,
            new ResolveDiscussionRequest { Resolved = true }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal($"https://gitlab.example/api/v4/groups/7/epics/11/discussions/{DiscussionId}",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"resolved":true}""", sentBody);
        Assert.True(discussion.Resolved);
    }

    [Fact]
    public async Task ListNotesInEpicDiscussionAsync_BuildsDiscussionNotesRoute()
    {
        using StubHttpMessageHandler handler = Responds(HttpStatusCode.OK, $"[{ReplyNoteJson}]");
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        DiscussionsClient repository = new(connection);

        List<GitLabNote> notes = [];
        await foreach (GitLabNote item in repository.ListNotesInEpicDiscussionAsync(7, 11, DiscussionId,
                           TestContext.Current.CancellationToken))
        {
            notes.Add(item);
        }

        Assert.Equal($"https://gitlab.example/api/v4/groups/7/epics/11/discussions/{DiscussionId}/notes",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("Renamed, thanks.", Assert.Single(notes).Body);
    }

    [Fact]
    public async Task AddNoteToEpicDiscussionAsync_PostsBodyAndCreatedAt()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = Records(HttpStatusCode.Created, ReplyNoteJson, body => sentBody = body);
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        DiscussionsClient repository = new(connection);

        GitLabNote note = await repository.AddNoteToEpicDiscussionAsync(7, 11, DiscussionId,
            new CreateDiscussionNoteRequest
            {
                Body = "Renamed, thanks.",
                CreatedAt = DateTimeOffset.Parse("2026-01-16T10:45:00Z", CultureInfo.InvariantCulture)
            },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal($"https://gitlab.example/api/v4/groups/7/epics/11/discussions/{DiscussionId}/notes",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Contains("\"created_at\":\"2026-01-16T10:45:00", sentBody, StringComparison.Ordinal);
        Assert.Equal(1127, note.Id);
    }

    [Fact]
    public async Task GetNoteInEpicDiscussionAsync_BuildsSingleNoteRoute()
    {
        using StubHttpMessageHandler handler = Responds(HttpStatusCode.OK, ReplyNoteJson);
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        DiscussionsClient repository = new(connection);

        await repository.GetNoteInEpicDiscussionAsync(7, 11, "thread/one", 1127,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/7/epics/11/discussions/thread%2Fone/notes/1127",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task UpdateNoteInEpicDiscussionAsync_PutsBody()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = Records(HttpStatusCode.OK, ReplyNoteJson, body => sentBody = body);
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        DiscussionsClient repository = new(connection);

        await repository.UpdateNoteInEpicDiscussionAsync(7, 11, DiscussionId, 1127,
            new UpdateDiscussionNoteRequest { Body = "Renamed, thanks." }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal($"https://gitlab.example/api/v4/groups/7/epics/11/discussions/{DiscussionId}/notes/1127",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"body":"Renamed, thanks."}""", sentBody);
    }

    [Fact]
    public async Task ResolveNoteInEpicDiscussionAsync_PutsResolvedOnly_AndEncodesTheGroupPath()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = Records(HttpStatusCode.OK, ReplyNoteJson, body => sentBody = body);
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        DiscussionsClient repository = new(connection);

        await repository.ResolveNoteInEpicDiscussionAsync("gitlab-org/subgroup", 11, DiscussionId, 1127,
            new ResolveDiscussionRequest { Resolved = false }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal(
            $"https://gitlab.example/api/v4/groups/gitlab-org%2Fsubgroup/epics/11/discussions/{DiscussionId}/notes/1127",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"resolved":false}""", sentBody);
    }

    [Fact]
    public async Task DeleteNoteFromEpicDiscussionAsync_SendsDeleteToNoteRoute()
    {
        using StubHttpMessageHandler handler = RespondsNoContent();
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        DiscussionsClient repository = new(connection);

        await repository.DeleteNoteFromEpicDiscussionAsync("gitlab-org/subgroup", 11, DiscussionId, 1127,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/groups/gitlab-org%2Fsubgroup/epics/11/discussions/"
            + $"{DiscussionId}/notes/1127",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    private static StubHttpMessageHandler RespondsNoContent()
    {
        return new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.NoContent));
    }

    private static StubHttpMessageHandler Responds(HttpStatusCode status, string json)
    {
        return new StubHttpMessageHandler(_ => new HttpResponseMessage(status)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });
    }

    private static StubHttpMessageHandler Records(HttpStatusCode status, string json, Action<string?> captureBody)
    {
        return new StubHttpMessageHandler(request =>
        {
            captureBody(request.Content?.ReadAsStringAsync().GetAwaiter().GetResult());
            return new HttpResponseMessage(status)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
        });
    }
}