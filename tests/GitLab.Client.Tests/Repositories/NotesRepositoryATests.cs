using System.Net;
using System.Text;

using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Repositories;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Repositories;

/// <summary>
///     Part A's slice of the Notes tag is the group wiki-page-meta note family and the project issue note
///     list - all five verbs already exist on <see cref="NotesRepository" /> (built by an earlier round),
///     but three of the group wiki-page-meta verbs (get/create/delete) and the list's query-string
///     projection had no dedicated coverage yet. This file closes that gap without touching
///     <c>NotesRepositoryTests.cs</c>, which other parts of this same round are also extending.
/// </summary>
public sealed class NotesRepositoryATests
{
    [Fact]
    public async Task GetGroupWikiPageNoteAsync_EncodesNamespacedGroupPath_AndBuildsNoteRoute()
    {
        const string Json = """
                            {
                              "id": 4004,
                              "body": "a group wiki page comment",
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

        GitLabNote note = await repository.GetGroupWikiPageNoteAsync("gitlab-org/subgroup", 84, 4004,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/groups/gitlab-org%2Fsubgroup/wiki_pages/84/notes/4004",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(4004, note.Id);
        Assert.Equal("a group wiki page comment", note.Body);
    }

    [Fact]
    public async Task
        CreateGroupWikiPageNoteAsync_PostsToGroupWikiPageNotesRoute_WithSerializedBody_AndDeserializesCreatedNote()
    {
        const string Json = """
                            {
                              "id": 4100,
                              "body": "New comment on group wiki page",
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

        CreateNoteRequest request = new() { Body = "New comment on group wiki page" };

        GitLabNote note = await repository.CreateGroupWikiPageNoteAsync(7, 84, request,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/7/wiki_pages/84/notes",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("application/json", handler.LastRequest?.Content?.Headers.ContentType?.MediaType);
        Assert.Contains("\"body\":\"New comment on group wiki page\"", sentBody, StringComparison.Ordinal);
        Assert.Equal(4100, note.Id);
        Assert.Equal("New comment on group wiki page", note.Body);
    }

    [Fact]
    public async Task DeleteGroupWikiPageNoteAsync_DeletesGroupWikiPageNoteRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        NotesRepository repository = new(connection);

        await repository.DeleteGroupWikiPageNoteAsync(7, 84, 4004, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/7/wiki_pages/84/notes/4004",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task DeleteGroupWikiPageNoteAsync_Tolerates200WithTheDeletedNoteAsBody()
    {
        // GitLab answers a note deletion with 200 and the deleted note, not the usual 204.
        const string Json = """{ "id": 4004, "body": "gone" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        NotesRepository repository = new(connection);

        await repository.DeleteGroupWikiPageNoteAsync("gitlab-org/subgroup", 84, 4004,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/gitlab-org%2Fsubgroup/wiki_pages/84/notes/4004",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task ListGroupWikiPageNotesAsync_WithOptions_BuildsQueryString()
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
            OrderBy = "updated_at", Sort = "asc", ActivityFilter = "only_activity", PerPage = 20
        };

        await foreach (GitLabNote _ in repository.ListGroupWikiPageNotesAsync(7, 84, options,
                           TestContext.Current.CancellationToken))
        {
            Assert.Fail("The stub returns an empty page.");
        }

        string? uri = handler.LastRequest?.RequestUri?.AbsoluteUri;
        Assert.StartsWith("https://gitlab.example/api/v4/groups/7/wiki_pages/84/notes?", uri,
            StringComparison.Ordinal);
        Assert.Contains("order_by=updated_at", uri, StringComparison.Ordinal);
        Assert.Contains("sort=asc", uri, StringComparison.Ordinal);
        Assert.Contains("activity_filter=only_activity", uri, StringComparison.Ordinal);
        Assert.Contains("per_page=20", uri, StringComparison.Ordinal);
    }
}