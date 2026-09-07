using System.Net;
using System.Text;

using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Repositories;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Repositories;

public sealed class DraftNotesRepositoryTests
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
                                           "line_code": "829c3b4b0f6b1a0ff0a1b2c3d4e5f6a7b8c9d0e1_12_14"
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
        DraftNotesRepository repository = new(connection);

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
        DraftNotesRepository repository = new(connection);

        GitLabDraftNote note = await repository.GetAsync(42, 17, 5, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/42/merge_requests/17/draft_notes/5",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(5, note.Id);
        Assert.Equal("Extract this into a helper", note.Note);
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
        DraftNotesRepository repository = new(connection);

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
        DraftNotesRepository repository = new(connection);

        CreateDraftNoteRequest request = new()
        {
            Note = "Extract this into a helper",
            InReplyToDiscussionId = "6a9c1750b37d513a43987b574953fceb50b03ce7",
            CommitId = "2695effb5807a22ff3d138d593fd856244e155e7",
            ResolveDiscussion = true
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
        DraftNotesRepository repository = new(connection);

        await repository.CreateAsync(42, 17, new CreateDraftNoteRequest { Note = "Looks good" },
            TestContext.Current.CancellationToken);

        Assert.Equal("""{"note":"Looks good"}""", sentBody);
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
        DraftNotesRepository repository = new(connection);

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
    public async Task DeleteAsync_SendsDeleteToDraftNoteRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        DraftNotesRepository repository = new(connection);

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
        DraftNotesRepository repository = new(connection);

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
        DraftNotesRepository repository = new(connection);

        PublishDraftNotesRequest request = new()
        {
            ReviewerState = "requested_changes", Note = "A few naming nits.", Internal = true
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
        DraftNotesRepository repository = new(connection);

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
        DraftNotesRepository repository = new(connection);

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
        DraftNotesRepository repository = new(connection);

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabValidationException>(() =>
            repository.PublishAsync(42, 17, 5, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);
        Assert.Equal("400 Bad request", exception.Message);
    }
}