using System.Net;
using System.Text;

using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Repositories;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Repositories;

/// <summary>
///     Covers the notes-b scope of the Notes tag: the merge request note update route, and the
///     snippet/vulnerability note routes. <see cref="NotesRepository" /> and every method exercised here
///     already existed before this scope's work began (see <c>NotesRepositoryTests</c>) - this file only
///     closes gaps in that existing coverage rather than adding new production code, because the ten
///     operations assigned to notes-b (<c>GET/PUT/DELETE</c> merge request notes, the full snippet note
///     CRUD, and the vulnerability note list/create) were all already wrapped by the original
///     <c>NotesRepository</c>.
/// </summary>
public sealed class NotesRepositoryBTests
{
    [Fact]
    public async Task UpdateMergeRequestNoteAsync_BuildsMergeRequestNoteRoute_AndSendsSerializedBody()
    {
        const string Json = """
                            {
                              "id": 501,
                              "body": "Edited review comment",
                              "system": false
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
        NotesRepository repository = new(connection);

        UpdateNoteRequest request = new() { Body = "Edited review comment" };

        GitLabNote note = await repository.UpdateMergeRequestNoteAsync("gitlab-org/gitlab", 12, 501, request,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/merge_requests/12/notes/501",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"body":"Edited review comment"}""", sentBody);
        Assert.Equal("Edited review comment", note.Body);
    }

    [Fact]
    public async Task UpdateMergeRequestNoteAsync_OnUnprocessableEntity_ThrowsGitLabValidationException()
    {
        const string Json = """{ "message": { "body": ["can't be blank"] } }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.UnprocessableEntity)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        NotesRepository repository = new(connection);

        UpdateNoteRequest request = new() { Body = string.Empty };

        GitLabValidationException exception = await Assert.ThrowsAsync<GitLabValidationException>(() =>
            repository.UpdateMergeRequestNoteAsync(1, 12, 501, request, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, exception.StatusCode);
        Assert.Contains("can't be blank", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task CreateSnippetNoteAsync_PostsToSnippetNotesRoute_WithSerializedBody_AndDeserializesCreatedNote()
    {
        const string Json = """
                            {
                              "id": 88,
                              "body": "Nice snippet",
                              "system": false
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
        NotesRepository repository = new(connection);

        CreateNoteRequest request = new() { Body = "Nice snippet" };

        GitLabNote note =
            await repository.CreateSnippetNoteAsync(42, 52, request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/42/snippets/52/notes",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"body":"Nice snippet"}""", sentBody);
        Assert.Equal(88, note.Id);
        Assert.Equal("Nice snippet", note.Body);
    }

    [Fact]
    public async Task GetSnippetNoteAsync_EncodesNamespacedProjectPath_AndDeserializesNote()
    {
        const string Json = """
                            {
                              "id": 11,
                              "body": "snippet comment",
                              "system": false
                            }
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        NotesRepository repository = new(connection);

        GitLabNote note = await repository.GetSnippetNoteAsync("gitlab-org/gitlab", 52, 11,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/snippets/52/notes/11",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(11, note.Id);
        Assert.Equal("snippet comment", note.Body);
    }

    [Fact]
    public async Task GetSnippetNoteAsync_OnMissingNote_ThrowsGitLabNotFoundException()
    {
        const string Json = """{ "message": "404 Note Not Found" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        NotesRepository repository = new(connection);

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabNotFoundException>(() =>
            repository.GetSnippetNoteAsync(1, 52, 999, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        Assert.Equal("404 Note Not Found", exception.Message);
    }

    [Fact]
    public async Task DeleteSnippetNoteAsync_DeletesSnippetNoteRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        NotesRepository repository = new(connection);

        await repository.DeleteSnippetNoteAsync(1, 52, 11, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/snippets/52/notes/11",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task DeleteSnippetNoteAsync_Tolerates200WithTheDeletedNoteAsBody()
    {
        // GitLab answers a note deletion with 200 and the deleted note, not the usual 204.
        const string Json = """{ "id": 11, "body": "gone" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        NotesRepository repository = new(connection);

        await repository.DeleteSnippetNoteAsync(42, 52, 11, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/42/snippets/52/notes/11",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task ListVulnerabilityNotesAsync_BuildsVulnerabilityNotesRoute_AndDeserializesNotes()
    {
        const string Json = """[ { "id": 77, "body": "triaged", "internal": true } ]""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        NotesRepository repository = new(connection);

        List<GitLabNote> notes = new();
        await foreach (GitLabNote item in repository.ListVulnerabilityNotesAsync(1, 9,
                           cancellationToken: TestContext.Current.CancellationToken))
        {
            notes.Add(item);
        }

        Assert.Equal("https://gitlab.example/api/v4/projects/1/vulnerabilities/9/notes",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        GitLabNote note = Assert.Single(notes);
        Assert.Equal(77, note.Id);
        Assert.True(note.Internal);
    }

    [Fact]
    public async Task ListVulnerabilityNotesAsync_WithOptions_BuildsQueryString()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        NotesRepository repository = new(connection);

        NoteListOptions options = new() { OrderBy = "created_at", Sort = "desc", ActivityFilter = "only_activity" };

        await foreach (GitLabNote _ in repository.ListVulnerabilityNotesAsync("gitlab-org/gitlab", 9, options,
                           TestContext.Current.CancellationToken))
        {
            Assert.Fail("The stub returns an empty page.");
        }

        string? uri = handler.LastRequest?.RequestUri?.AbsoluteUri;
        Assert.StartsWith("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/vulnerabilities/9/notes?", uri,
            StringComparison.Ordinal);
        Assert.Contains("order_by=created_at", uri, StringComparison.Ordinal);
        Assert.Contains("sort=desc", uri, StringComparison.Ordinal);
        Assert.Contains("activity_filter=only_activity", uri, StringComparison.Ordinal);
    }
}