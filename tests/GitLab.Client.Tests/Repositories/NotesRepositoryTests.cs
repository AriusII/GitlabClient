using System.Net;
using System.Text;

using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Repositories;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Repositories;

public sealed class NotesRepositoryTests
{
    [Fact]
    public async Task ListIssueNotesAsync_BuildsIssueNotesRoute_AndDeserializesNotes()
    {
        const string Json = """
                            [
                              {
                                "id": 302,
                                "body": "This is a note on an issue",
                                "author": {
                                  "id": 1,
                                  "username": "pipin",
                                  "name": "Pip",
                                  "web_url": "https://gitlab.example/pipin"
                                },
                                "created_at": "2013-10-02T09:22:45Z",
                                "updated_at": "2013-10-02T09:22:45Z",
                                "system": false,
                                "resolvable": false
                              }
                            ]
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        NotesRepository repository = new(connection);

        List<GitLabNote> notes = new();
        await foreach (GitLabNote item in repository.ListIssueNotesAsync(1, 6,
                           cancellationToken: TestContext.Current.CancellationToken))
        {
            notes.Add(item);
        }

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Contains("/projects/1/issues/6/notes", handler.LastRequest?.RequestUri?.AbsoluteUri);
        GitLabNote note = Assert.Single(notes);
        Assert.Equal(302, note.Id);
        Assert.Equal("This is a note on an issue", note.Body);
        Assert.NotNull(note.Author);
        Assert.Equal("pipin", note.Author!.Username);
        Assert.False(note.System);
        Assert.False(note.Resolvable);
    }

    [Fact]
    public async Task GetIssueNoteAsync_EncodesNamespacedProjectPath_AndBuildsNoteRoute()
    {
        const string Json = """
                            {
                              "id": 302,
                              "body": "This is a note on an issue",
                              "system": false,
                              "resolvable": false
                            }
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        NotesRepository repository = new(connection);

        GitLabNote note =
            await repository.GetIssueNoteAsync("gitlab-org/gitlab", 6, 302, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/issues/6/notes/302",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(302, note.Id);
    }

    [Fact]
    public async Task CreateIssueNoteAsync_PostsToIssueNotesRoute_WithSerializedBody_AndDeserializesCreatedNote()
    {
        const string Json = """
                            {
                              "id": 404,
                              "body": "New comment on issue",
                              "system": false,
                              "resolvable": false
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

        CreateNoteRequest request = new() { Body = "New comment on issue" };

        GitLabNote note = await repository.CreateIssueNoteAsync(1, 6, request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/issues/6/notes",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("application/json", handler.LastRequest?.Content?.Headers.ContentType?.MediaType);
        Assert.Contains("\"body\":\"New comment on issue\"", sentBody, StringComparison.Ordinal);
        Assert.Equal(404, note.Id);
        Assert.Equal("New comment on issue", note.Body);
    }

    [Fact]
    public async Task ListMergeRequestNotesAsync_BuildsMergeRequestNotesRoute_AndDeserializesNotes()
    {
        const string Json = """
                            [
                              {
                                "id": 501,
                                "body": "This is a note on a merge request",
                                "author": {
                                  "id": 2,
                                  "username": "reviewer",
                                  "name": "Reviewer",
                                  "web_url": "https://gitlab.example/reviewer"
                                },
                                "created_at": "2013-10-02T09:22:45Z",
                                "system": true,
                                "resolvable": true
                              }
                            ]
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        NotesRepository repository = new(connection);

        List<GitLabNote> notes = new();
        await foreach (GitLabNote item in repository.ListMergeRequestNotesAsync("gitlab-org/gitlab", 12,
                           cancellationToken: TestContext.Current.CancellationToken))
        {
            notes.Add(item);
        }

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Contains("/projects/gitlab-org%2Fgitlab/merge_requests/12/notes",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        GitLabNote note = Assert.Single(notes);
        Assert.Equal(501, note.Id);
        Assert.True(note.System);
        Assert.True(note.Resolvable);
    }

    [Fact]
    public async Task GetMergeRequestNoteAsync_BuildsMergeRequestNoteRoute_AndDeserializesNote()
    {
        const string Json = """
                            {
                              "id": 501,
                              "body": "This is a note on a merge request",
                              "system": false,
                              "resolvable": false
                            }
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        NotesRepository repository = new(connection);

        GitLabNote note = await repository.GetMergeRequestNoteAsync(1, 12, 501, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/projects/1/merge_requests/12/notes/501",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(501, note.Id);
    }

    [Fact]
    public async Task
        CreateMergeRequestNoteAsync_PostsToMergeRequestNotesRoute_WithSerializedBody_AndDeserializesCreatedNote()
    {
        const string Json = """
                            {
                              "id": 606,
                              "body": "LGTM",
                              "system": false,
                              "resolvable": false
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

        CreateNoteRequest request = new() { Body = "LGTM" };

        GitLabNote note =
            await repository.CreateMergeRequestNoteAsync(42, 12, request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/42/merge_requests/12/notes",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Contains("\"body\":\"LGTM\"", sentBody, StringComparison.Ordinal);
        Assert.Equal(606, note.Id);
        Assert.Equal("LGTM", note.Body);
    }

    [Fact]
    public async Task GetIssueNoteAsync_OnErrorResponse_ThrowsGitLabApiExceptionWithMessage()
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
            repository.GetIssueNoteAsync(1, 6, 999, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        Assert.Equal("404 Note Not Found", exception.Message);
    }

    [Fact]
    public async Task GetMergeRequestNoteAsync_OnErrorResponse_ThrowsGitLabApiExceptionWithMessage()
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
            repository.GetMergeRequestNoteAsync(1, 12, 999, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        Assert.Equal("404 Note Not Found", exception.Message);
    }

    [Fact]
    public async Task ListIssueNotesAsync_WithOptions_BuildsQueryString()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        NotesRepository repository = new(connection);

        NoteListOptions options = new()
        {
            OrderBy = "updated_at", Sort = "asc", ActivityFilter = "only_comments", PerPage = 50
        };

        await foreach (GitLabNote _ in repository.ListIssueNotesAsync(1, 6, options,
                           TestContext.Current.CancellationToken))
        {
            Assert.Fail("The stub returns an empty page.");
        }

        string? uri = handler.LastRequest?.RequestUri?.AbsoluteUri;
        Assert.StartsWith("https://gitlab.example/api/v4/projects/1/issues/6/notes?", uri, StringComparison.Ordinal);
        Assert.Contains("order_by=updated_at", uri, StringComparison.Ordinal);
        Assert.Contains("sort=asc", uri, StringComparison.Ordinal);
        Assert.Contains("activity_filter=only_comments", uri, StringComparison.Ordinal);
        Assert.Contains("per_page=50", uri, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ListIssueNotesAsync_WithoutOptions_SendsNoQueryString()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        NotesRepository repository = new(connection);

        await foreach (GitLabNote _ in repository.ListIssueNotesAsync(1, 6,
                           cancellationToken: TestContext.Current.CancellationToken))
        {
            Assert.Fail("The stub returns an empty page.");
        }

        Assert.Equal("https://gitlab.example/api/v4/projects/1/issues/6/notes",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task UpdateIssueNoteAsync_PutsToIssueNoteRoute_WithSerializedBody()
    {
        const string Json = """
                            {
                              "id": 302,
                              "body": "Edited comment",
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

        UpdateNoteRequest request = new() { Body = "Edited comment" };

        GitLabNote note = await repository.UpdateIssueNoteAsync("gitlab-org/gitlab", 6, 302, request,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/issues/6/notes/302",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"body":"Edited comment"}""", sentBody);
        Assert.Equal("Edited comment", note.Body);
    }

    [Fact]
    public async Task DeleteIssueNoteAsync_DeletesIssueNoteRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        NotesRepository repository = new(connection);

        await repository.DeleteIssueNoteAsync(1, 6, 302, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/issues/6/notes/302",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task DeleteMergeRequestNoteAsync_Tolerates200WithTheDeletedNoteAsBody()
    {
        // GitLab answers a note deletion with 200 and the deleted note, not the usual 204.
        const string Json = """{ "id": 501, "body": "gone" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        NotesRepository repository = new(connection);

        await repository.DeleteMergeRequestNoteAsync(42, 12, 501, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/42/merge_requests/12/notes/501",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task ListSnippetNotesAsync_BuildsSnippetNotesRoute()
    {
        const string Json = """[ { "id": 11, "body": "snippet comment" } ]""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        NotesRepository repository = new(connection);

        List<GitLabNote> notes = new();
        await foreach (GitLabNote item in repository.ListSnippetNotesAsync("gitlab-org/gitlab", 52,
                           cancellationToken: TestContext.Current.CancellationToken))
        {
            notes.Add(item);
        }

        Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/snippets/52/notes",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(11, Assert.Single(notes).Id);
    }

    [Fact]
    public async Task UpdateSnippetNoteAsync_BuildsSnippetNoteRoute()
    {
        const string Json = """{ "id": 11, "body": "edited snippet comment" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        NotesRepository repository = new(connection);

        GitLabNote note = await repository.UpdateSnippetNoteAsync(1, 52, 11,
            new UpdateNoteRequest { Body = "edited snippet comment" }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/snippets/52/notes/11",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("edited snippet comment", note.Body);
    }

    [Fact]
    public async Task CreateVulnerabilityNoteAsync_PostsToVulnerabilityNotesRoute_WithInternalFlag()
    {
        const string Json = """{ "id": 77, "body": "triaged", "internal": true }""";

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

        CreateNoteRequest request = new()
        {
            Body = "triaged", Internal = true, CreatedAt = new DateTimeOffset(2024, 1, 2, 3, 4, 5, TimeSpan.Zero)
        };

        GitLabNote note =
            await repository.CreateVulnerabilityNoteAsync(1, 9, request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/vulnerabilities/9/notes",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Contains("\"internal\":true", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"created_at\":\"2024-01-02T03:04:05", sentBody, StringComparison.Ordinal);
        Assert.True(note.Internal);
    }

    [Fact]
    public async Task CreateMergeRequestNoteAsync_SerializesMergeRequestDiffHeadSha()
    {
        const string Json = """{ "id": 606, "body": "LGTM" }""";

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

        CreateNoteRequest request = new()
        {
            Body = "LGTM", MergeRequestDiffHeadSha = "6104942438c14ec7bd21c6cd5bd995272b3faff6"
        };

        await repository.CreateMergeRequestNoteAsync(1, 12, request, TestContext.Current.CancellationToken);

        Assert.Contains(
            "\"merge_request_diff_head_sha\":\"6104942438c14ec7bd21c6cd5bd995272b3faff6\"",
            sentBody,
            StringComparison.Ordinal);
        Assert.DoesNotContain("\"internal\"", sentBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetProjectWikiPageNoteAsync_BuildsWikiPageNoteRoute_AndDeserializesImportedFields()
    {
        const string Json = """
                            {
                              "id": 1201,
                              "body": "wiki page comment",
                              "noteable_type": "WikiPage::Meta",
                              "noteable_id": 84,
                              "imported": true,
                              "imported_from": "github"
                            }
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        NotesRepository repository = new(connection);

        GitLabNote note = await repository.GetProjectWikiPageNoteAsync("gitlab-org/gitlab", 84, 1201,
            TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/wiki_pages/84/notes/1201",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("WikiPage::Meta", note.NoteableType);
        Assert.True(note.Imported);
        Assert.Equal("github", note.ImportedFrom);
    }

    [Fact]
    public async Task DeleteProjectWikiPageNoteAsync_BuildsWikiPageNoteRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        NotesRepository repository = new(connection);

        await repository.DeleteProjectWikiPageNoteAsync(1, 84, 1201, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/wiki_pages/84/notes/1201",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task ListEpicNotesAsync_EncodesNamespacedGroupPath_AndAppliesOptions()
    {
        const string Json = """[ { "id": 9001, "body": "epic comment", "noteable_type": "Epic" } ]""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        NotesRepository repository = new(connection);

        List<GitLabNote> notes = new();
        await foreach (GitLabNote item in repository.ListEpicNotesAsync("gitlab-org/subgroup", 11,
                           new NoteListOptions { ActivityFilter = "only_activity" },
                           TestContext.Current.CancellationToken))
        {
            notes.Add(item);
        }

        Assert.Equal(
            "https://gitlab.example/api/v4/groups/gitlab-org%2Fsubgroup/epics/11/notes?activity_filter=only_activity",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("Epic", Assert.Single(notes).NoteableType);
    }

    [Fact]
    public async Task GetEpicNoteAsync_EncodesNamespacedGroupPath_AndBuildsNoteRoute()
    {
        const string Json = """{ "id": 9001, "body": "epic comment", "noteable_type": "Epic" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        NotesRepository repository = new(connection);

        GitLabNote note = await repository.GetEpicNoteAsync("gitlab-org/subgroup", 11, 9001,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/groups/gitlab-org%2Fsubgroup/epics/11/notes/9001",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(9001, note.Id);
        Assert.Equal("Epic", note.NoteableType);
    }

    [Fact]
    public async Task CreateEpicNoteAsync_PostsToGroupEpicNotesRoute()
    {
        const string Json = """{ "id": 9002, "body": "epic comment" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        NotesRepository repository = new(connection);

        GitLabNote note = await repository.CreateEpicNoteAsync(7, 11, new CreateNoteRequest { Body = "epic comment" },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/7/epics/11/notes",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(9002, note.Id);
    }

    [Fact]
    public async Task DeleteEpicNoteAsync_BuildsGroupEpicNoteRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        NotesRepository repository = new(connection);

        await repository.DeleteEpicNoteAsync("gitlab-org/subgroup", 11, 9002, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/gitlab-org%2Fsubgroup/epics/11/notes/9002",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task UpdateGroupWikiPageNoteAsync_BuildsGroupWikiPageNoteRoute()
    {
        const string Json = """{ "id": 4004, "body": "edited group wiki comment" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        NotesRepository repository = new(connection);

        GitLabNote note = await repository.UpdateGroupWikiPageNoteAsync(7, 84, 4004,
            new UpdateNoteRequest { Body = "edited group wiki comment" }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/7/wiki_pages/84/notes/4004",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("edited group wiki comment", note.Body);
    }

    [Fact]
    public async Task ListGroupWikiPageNotesAsync_BuildsGroupWikiPageNotesRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        NotesRepository repository = new(connection);

        await foreach (GitLabNote _ in repository.ListGroupWikiPageNotesAsync("gitlab-org/subgroup", 84,
                           cancellationToken: TestContext.Current.CancellationToken))
        {
            Assert.Fail("The stub returns an empty page.");
        }

        Assert.Equal("https://gitlab.example/api/v4/groups/gitlab-org%2Fsubgroup/wiki_pages/84/notes",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task GetVulnerabilityNoteAsync_BuildsVulnerabilityNoteRoute()
    {
        const string Json = """{ "id": 77, "body": "triaged" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        NotesRepository repository = new(connection);

        GitLabNote note = await repository.GetVulnerabilityNoteAsync(1, 9, 77, TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/1/vulnerabilities/9/notes/77",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(77, note.Id);
    }

    [Fact]
    public async Task UpdateEpicNoteAsync_OnForbiddenResponse_ThrowsGitLabForbiddenException()
    {
        const string Json = """{ "message": "403 Forbidden" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Forbidden)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        NotesRepository repository = new(connection);

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabForbiddenException>(() =>
            repository.UpdateEpicNoteAsync(7, 11, 9002, new UpdateNoteRequest { Body = "nope" },
                TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.Forbidden, exception.StatusCode);
        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
    }
}