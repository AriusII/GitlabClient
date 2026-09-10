using System.Net;
using System.Text;

using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Endpoints;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Endpoints;

public sealed class DraftNotesEndpointTests
{
    private const string DraftNoteJson = """
                                         {
                                           "id": 5,
                                           "author_id": 23,
                                           "merge_request_id": 4242,
                                           "resolve_discussion": false,
                                           "discussion_id": "6a9c1750b37d513a43987b574953fceb50b03ce7",
                                           "note": "Extract this into a helper",
                                           "commit_id": "2695effb5807a22ff3d138d593fd856244e155e7",
                                           "line_code": "829c3b4b0f6b1a0ff0a1b2c3d4e5f6a7b8c9d0e1_12_14",
                                           "position": {
                                             "base_sha": "aa149113",
                                             "start_sha": "b3a0a8c4",
                                             "head_sha": "be3020c7",
                                             "position_type": "text",
                                             "old_path": "example.md",
                                             "new_path": "example.md",
                                             "old_line": 2,
                                             "new_line": 4
                                           }
                                         }
                                         """;

    private const string DraftNoteWithPartialPositionJson = """
                                                            {
                                                              "id": 6,
                                                              "position": {
                                                                "base_sha": null,
                                                                "start_sha": null,
                                                                "head_sha": null,
                                                                "old_path": null,
                                                                "new_path": null,
                                                                "position_type": "text",
                                                                "old_line": null,
                                                                "new_line": null,
                                                                "line_range": null
                                                              }
                                                            }
                                                            """;

    [Fact]
    public async Task ListAsync_BuildsDraftNotesRoute_AndDeserializesDraftNotes()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent($"[{DraftNoteJson}]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        DraftNotesClient repository = new(connection);

        List<GitLabDraftNote> notes = [];
        await foreach (GitLabDraftNote item in repository.ListAsync(42, 17, TestContext.Current.CancellationToken))
        {
            notes.Add(item);
        }

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/42/merge_requests/17/draft_notes",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabDraftNote note = Assert.Single(notes);
        Assert.Equal(5, note.Id);
        Assert.Equal(23, note.AuthorId);
        Assert.Equal(4242, note.MergeRequestId);
        Assert.False(note.ResolveDiscussion);
        Assert.Equal("6a9c1750b37d513a43987b574953fceb50b03ce7", note.DiscussionId);
        Assert.Equal("Extract this into a helper", note.Note);
        Assert.Equal("2695effb5807a22ff3d138d593fd856244e155e7", note.CommitId);
        Assert.Equal("829c3b4b0f6b1a0ff0a1b2c3d4e5f6a7b8c9d0e1_12_14", note.LineCode);
        Assert.Equal("aa149113", note.Position?.BaseSha);
        Assert.Equal(GitLabNotePositionType.Text, note.Position?.PositionType);
        Assert.Equal(4, note.Position?.NewLine);
    }

    [Fact]
    public async Task GetAsync_BuildsDraftNoteRoute_AndDeserializesDraftNote()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(DraftNoteJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        DraftNotesClient repository = new(connection);

        GitLabDraftNote note = await repository.GetAsync(42, 17, 5, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/42/merge_requests/17/draft_notes/5",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(5, note.Id);
        Assert.Equal("Extract this into a helper", note.Note);
    }

    [Fact]
    public async Task GetAsync_DeserializesResponsePositionWithNullDiffAnchors()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(DraftNoteWithPartialPositionJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        DraftNotesClient repository = new(connection);

        GitLabDraftNote note = await repository.GetAsync(42, 17, 6, TestContext.Current.CancellationToken);

        Assert.Equal(6, note.Id);
        Assert.NotNull(note.Position);
        Assert.Null(note.Position.BaseSha);
        Assert.Null(note.Position.StartSha);
        Assert.Null(note.Position.HeadSha);
        Assert.Equal(GitLabNotePositionType.Text, note.Position.PositionType);
        Assert.Null(note.Position.NewPath);
        Assert.Null(note.Position.OldPath);
    }

    [Fact]
    public async Task GetAsync_EncodesNamespacedProjectPath()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(DraftNoteJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        DraftNotesClient repository = new(connection);

        await repository.GetAsync("gitlab-org/gitlab", 17, 5, TestContext.Current.CancellationToken);

        Assert.Equal(
            "https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/merge_requests/17/draft_notes/5",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task CreateAsync_PostsNoteBody_AndDeserializesCreatedDraftNote()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(DraftNoteJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        DraftNotesClient repository = new(connection);

        CreateDraftNoteRequest request = new()
        {
            Note = "Extract this into a helper",
            InReplyToDiscussionId = "6a9c1750b37d513a43987b574953fceb50b03ce7",
            CommitId = "2695effb5807a22ff3d138d593fd856244e155e7",
            ResolveDiscussion = true,
            Position = TextPosition()
        };

        GitLabDraftNote note = await repository.CreateAsync(42, 17, request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/42/merge_requests/17/draft_notes",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("application/json", handler.LastRequest?.Content?.Headers.ContentType?.MediaType);
        Assert.Contains("\"note\":\"Extract this into a helper\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"in_reply_to_discussion_id\":\"6a9c1750b37d513a43987b574953fceb50b03ce7\"", sentBody,
            StringComparison.Ordinal);
        Assert.Contains("\"commit_id\":\"2695effb5807a22ff3d138d593fd856244e155e7\"", sentBody,
            StringComparison.Ordinal);
        Assert.Contains("\"resolve_discussion\":true", sentBody, StringComparison.Ordinal);
        Assert.Contains(
            "\"position\":{\"base_sha\":\"aa149113\",\"start_sha\":\"b3a0a8c4\",\"head_sha\":\"be3020c7\",\"position_type\":\"text\"",
            sentBody,
            StringComparison.Ordinal);
        Assert.Equal(5, note.Id);
        Assert.Equal(23, note.AuthorId);
    }

    /// <summary>
    ///     The content field on this API is <c>note</c>, not the <c>body</c> every other GitLab comment
    ///     endpoint uses - getting it wrong yields a 400 that reads like a validation bug, so pin the exact
    ///     payload.
    /// </summary>
    [Fact]
    public async Task CreateAsync_SendsOnlyTheNoteField_WhenNothingElseIsSet()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(DraftNoteJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        DraftNotesClient repository = new(connection);

        await repository.CreateAsync(42, 17, new CreateDraftNoteRequest { Note = "Looks good" },
            TestContext.Current.CancellationToken);

        Assert.Equal("""{"note":"Looks good"}""", sentBody);
    }

    [Fact]
    public async Task CreateAsync_SerializesFractionalImageCoordinates()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(DraftNoteJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        DraftNotesClient repository = new(connection);

        await repository.CreateAsync(42, 17,
            new CreateDraftNoteRequest
            {
                Note = "Pin this image location",
                Position = new GitLabNotePosition
                {
                    BaseSha = "aa149113",
                    StartSha = "b3a0a8c4",
                    HeadSha = "be3020c7",
                    PositionType = GitLabNotePositionType.Image,
                    Width = 640,
                    Height = 480,
                    X = 12.5,
                    Y = 48.25
                }
            }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Contains("\"position_type\":\"image\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"x\":12.5", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"y\":48.25", sentBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task UpdateAsync_PutsNoteBody_AndDeserializesUpdatedDraftNote()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(DraftNoteJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        DraftNotesClient repository = new(connection);

        GitLabDraftNote note = await repository.UpdateAsync(42, 17, 5,
            new UpdateDraftNoteRequest { Note = "Extract this into a helper" },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/42/merge_requests/17/draft_notes/5",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"note":"Extract this into a helper"}""", sentBody);
        Assert.Equal("Extract this into a helper", note.Note);
        Assert.Equal("829c3b4b0f6b1a0ff0a1b2c3d4e5f6a7b8c9d0e1_12_14", note.LineCode);
    }

    [Fact]
    public async Task UpdateAsync_PutsPositionWithoutReplacingTheNote()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(DraftNoteJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        DraftNotesClient repository = new(connection);

        await repository.UpdateAsync(42, 17, 5, new UpdateDraftNoteRequest { Position = TextPosition() },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/42/merge_requests/17/draft_notes/5",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(
            """{"position":{"base_sha":"aa149113","start_sha":"b3a0a8c4","head_sha":"be3020c7","position_type":"text","new_path":"example.md","new_line":4,"old_path":"example.md","old_line":2}}""",
            sentBody);
    }

    [Fact]
    public async Task DeleteAsync_SendsDeleteToDraftNoteRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        DraftNotesClient repository = new(connection);

        await repository.DeleteAsync("gitlab-org/gitlab", 17, 5, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/merge_requests/17/draft_notes/5",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Null(handler.LastRequest?.Content);
    }

    [Fact]
    public async Task PublishAsync_PutsToPublishRoute_WithNoRequestBody()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        DraftNotesClient repository = new(connection);

        await repository.PublishAsync(42, 17, 5, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/42/merge_requests/17/draft_notes/5/publish",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Null(handler.LastRequest?.Content);
    }

    [Fact]
    public async Task PublishAllAsync_PostsReviewSummaryToBulkPublishRoute()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.NoContent);
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        DraftNotesClient repository = new(connection);

        PublishDraftNotesRequest request = new()
        {
            ReviewerState = GitLabReviewerState.RequestedChanges, Note = "A few naming nits.", Internal = true
        };

        await repository.PublishAllAsync(42, 17, request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/42/merge_requests/17/draft_notes/bulk_publish",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("application/json", handler.LastRequest?.Content?.Headers.ContentType?.MediaType);
        Assert.Equal("""{"reviewer_state":"requested_changes","note":"A few naming nits.","internal":true}""",
            sentBody);
    }

    [Fact]
    public async Task PublishAllAsync_SendsAnEmptyObject_WhenNoReviewStateIsSupplied()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.NoContent);
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        DraftNotesClient repository = new(connection);

        await repository.PublishAllAsync(42, 17, new PublishDraftNotesRequest(),
            TestContext.Current.CancellationToken);

        Assert.Equal("{}", sentBody);
    }

    [Fact]
    public async Task GetAsync_OnErrorResponse_ThrowsGitLabNotFoundException()
    {
        const string Json = """{ "message": "404 Draft Note Not Found" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        DraftNotesClient repository = new(connection);

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabNotFoundException>(() =>
            repository.GetAsync(42, 17, 999, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        Assert.Equal("404 Draft Note Not Found", exception.Message);
    }

    [Fact]
    public async Task PublishAsync_OnErrorResponse_ThrowsGitLabValidationException()
    {
        const string Json = """{ "message": "400 Bad request" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        DraftNotesClient repository = new(connection);

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabValidationException>(() =>
            repository.PublishAsync(42, 17, 5, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);
        Assert.Equal("400 Bad request", exception.Message);
    }

    private static GitLabNotePosition TextPosition()
    {
        return new GitLabNotePosition
        {
            BaseSha = "aa149113",
            StartSha = "b3a0a8c4",
            HeadSha = "be3020c7",
            PositionType = GitLabNotePositionType.Text,
            NewPath = "example.md",
            NewLine = 4,
            OldPath = "example.md",
            OldLine = 2
        };
    }
}