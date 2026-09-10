using System.Net;
using System.Text;

using GitLab.Client.Endpoints;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;
using GitLab.Client.Query;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Endpoints;

/// <summary>
///     Covers the "notes-c" scope of the Notes tag: the vulnerability-note and project-wiki-page-note
///     update/delete/list/create routes. The repository, service and client members these exercise
///     already existed before this scope was assigned (see <see cref="NotesClient" />) - this file
///     only adds the test coverage that was missing for them, rather than duplicating
///     <c>NotesEndpointTests</c> for methods it already exercises
///     (<c>GetVulnerabilityNoteAsync</c>, <c>CreateVulnerabilityNoteAsync</c>,
///     <c>GetProjectWikiPageNoteAsync</c>, <c>DeleteProjectWikiPageNoteAsync</c>).
/// </summary>
public sealed class NotesRepositoryCTests
{
    [Fact]
    public async Task UpdateVulnerabilityNoteAsync_PutsToVulnerabilityNoteRoute_AndDeserializesBody()
    {
        const string Json = """{ "id": 77, "body": "re-triaged as false positive" }""";

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
        NotesClient repository = new(connection);

        GitLabNote note = await repository.UpdateVulnerabilityNoteAsync(1, 9, 77,
            new UpdateNoteRequest { Body = "re-triaged as false positive" }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/vulnerabilities/9/notes/77",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Contains("\"body\":\"re-triaged as false positive\"", sentBody, StringComparison.Ordinal);
        Assert.Equal("re-triaged as false positive", note.Body);
    }

    [Fact]
    public async Task DeleteVulnerabilityNoteAsync_BuildsVulnerabilityNoteRoute_AndEncodesNamespacedProject()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        NotesClient repository = new(connection);

        await repository.DeleteVulnerabilityNoteAsync("gitlab-org/gitlab", 9, 77,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/vulnerabilities/9/notes/77",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task ListProjectWikiPageNotesAsync_BuildsWikiPageNotesRoute_AndAppliesOptions()
    {
        const string Json = """
                            [
                              {
                                "id": 1201,
                                "body": "wiki page comment",
                                "noteable_type": "WikiPage::Meta"
                              }
                            ]
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        NotesClient repository = new(connection);

        List<GitLabNote> notes = new();
        await foreach (GitLabNote item in repository.ListProjectWikiPageNotesAsync(1, 84,
                           new NoteListOptions
                           {
                               OrderBy = "updated_at", Sort = "asc", ActivityFilter = "only_comments"
                           },
                           TestContext.Current.CancellationToken))
        {
            notes.Add(item);
        }

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/projects/1/wiki_pages/84/notes" +
            "?order_by=updated_at&sort=asc&activity_filter=only_comments",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("WikiPage::Meta", Assert.Single(notes).NoteableType);
    }

    [Fact]
    public async Task CreateProjectWikiPageNoteAsync_PostsToWikiPageNotesRoute()
    {
        const string Json = """{ "id": 1202, "body": "new wiki page comment" }""";

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
        NotesClient repository = new(connection);

        GitLabNote note = await repository.CreateProjectWikiPageNoteAsync("gitlab-org/gitlab", 84,
            new CreateNoteRequest { Body = "new wiki page comment" }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/wiki_pages/84/notes",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Contains("\"body\":\"new wiki page comment\"", sentBody, StringComparison.Ordinal);
        Assert.Equal(1202, note.Id);
    }

    [Fact]
    public async Task UpdateProjectWikiPageNoteAsync_PutsToWikiPageNoteRoute()
    {
        const string Json = """{ "id": 1201, "body": "edited wiki page comment" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        NotesClient repository = new(connection);

        GitLabNote note = await repository.UpdateProjectWikiPageNoteAsync(1, 84, 1201,
            new UpdateNoteRequest { Body = "edited wiki page comment" }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/wiki_pages/84/notes/1201",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("edited wiki page comment", note.Body);
    }
}