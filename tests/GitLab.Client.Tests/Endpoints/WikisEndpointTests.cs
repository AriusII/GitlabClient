using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

using GitLab.Client.Abstractions;
using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Endpoints;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;
using GitLab.Client.Tests.TestSupport;

using GitLabJsonContext = GitLab.Client.Serialization.GitLabJsonContext;

namespace GitLab.Client.Tests.Endpoints;

public sealed class WikisEndpointTests
{
    private const string AttachmentJson = """
                                          {
                                            "file_name": "dk.png",
                                            "file_path": "uploads/6a061c4cf9f1c28cb22c384b4b8d4e3c/dk.png",
                                            "branch": "main",
                                            "link": {
                                              "url": "uploads/6a061c4cf9f1c28cb22c384b4b8d4e3c/dk.png",
                                              "markdown": "![dk](uploads/6a061c4cf9f1c28cb22c384b4b8d4e3c/dk.png)"
                                            }
                                          }
                                          """;

    private const string FullPageJson = """
                                        {
                                          "slug": "home/setup",
                                          "title": "Home / Setup",
                                          "format": "markdown",
                                          "wiki_page_meta_id": 77,
                                          "content": "# Setup",
                                          "encoding": "UTF-8",
                                          "front_matter": { "toc": true, "tags": ["ops"] }
                                        }
                                        """;

    private static readonly byte[] FileBytes = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];

    [Theory]
    [InlineData(GitLabWikiFormat.Markdown, "markdown")]
    [InlineData(GitLabWikiFormat.Rdoc, "rdoc")]
    [InlineData(GitLabWikiFormat.Asciidoc, "asciidoc")]
    [InlineData(GitLabWikiFormat.Org, "org")]
    public void WikiFormat_SerializesOfficialWireValues_InCreateAndUpdateRequests(GitLabWikiFormat format,
        string expectedWireValue)
    {
        string createJson = JsonSerializer.Serialize(
            new CreateWikiPageRequest { Title = "Docs", Content = "Content", Format = format },
            GitLabJsonContext.Default.CreateWikiPageRequest);
        string updateJson = JsonSerializer.Serialize(
            new UpdateWikiPageRequest { Format = format },
            GitLabJsonContext.Default.UpdateWikiPageRequest);

        Assert.Equal($"{{\"title\":\"Docs\",\"content\":\"Content\",\"format\":\"{expectedWireValue}\"}}",
            createJson);
        Assert.Equal($"{{\"format\":\"{expectedWireValue}\"}}", updateJson);
    }

    [Fact]
    public async Task ListForProjectAsync_BuildsWikisRoute_WithContentQuery_AndDeserializesPages()
    {
        const string Json = $"[{FullPageJson}]";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        WikisClient repository = new(connection);

        List<GitLabWikiPage> pages = new();
        await foreach (GitLabWikiPage page in repository.ListForProjectAsync(42, true,
                           TestContext.Current.CancellationToken))
        {
            pages.Add(page);
        }

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/42/wikis?with_content=true",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabWikiPage single = Assert.Single(pages);
        Assert.Equal("home/setup", single.Slug);
        Assert.Equal("Home / Setup", single.Title);
        Assert.Equal("markdown", single.Format);
        Assert.Equal(77, single.WikiPageMetaId);
        Assert.Equal("# Setup", single.Content);
        Assert.Equal("UTF-8", single.Encoding);
        Assert.True(single.FrontMatter?.GetProperty("toc").GetBoolean());
    }

    [Fact]
    public async Task ListForProjectAsync_OmitsWithContent_WhenNotRequested_AndLeavesContentNull()
    {
        const string Json = """
                            [
                              { "slug": "home", "title": "Home", "format": "markdown", "wiki_page_meta_id": 1 }
                            ]
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        WikisClient repository = new(connection);

        List<GitLabWikiPage> pages = new();
        await foreach (GitLabWikiPage page in repository.ListForProjectAsync("gitlab-org/gitlab",
                           cancellationToken: TestContext.Current.CancellationToken))
        {
            pages.Add(page);
        }

        // The basic list shape carries no content/encoding/front_matter at all, and an unsupplied
        // with_content must not be sent as with_content=false.
        Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/wikis",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabWikiPage single = Assert.Single(pages);
        Assert.Equal("home", single.Slug);
        Assert.Null(single.Content);
        Assert.Null(single.Encoding);
        Assert.Null(single.FrontMatter);
    }

    [Fact]
    public async Task GetForProjectAsync_EscapesNestedSlug_AndAppendsVersionAndRenderHtml()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(FullPageJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        WikisClient repository = new(connection);

        GitLabWikiPage page = await repository.GetForProjectAsync(1, "home/setup", "1a2b3c4d", true,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/projects/1/wikis/home%2Fsetup?version=1a2b3c4d&render_html=true",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        Assert.Equal("home/setup", page.Slug);
        Assert.Equal("Home / Setup", page.Title);
        Assert.Equal("UTF-8", page.Encoding);
        Assert.Equal("ops", page.FrontMatter?.GetProperty("tags")[0].GetString());
    }

    [Fact]
    public async Task GetForProjectAsync_EncodesNamespacedProjectPath()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(FullPageJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        WikisClient repository = new(connection);

        await repository.GetForProjectAsync("gitlab-org/gitlab", "home",
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/wikis/home",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task CreateForProjectAsync_PostsSerializedBody_AndReturnsPageWithServerDerivedSlug()
    {
        const string Json = """
                            {
                              "slug": "Release-Notes",
                              "title": "Release Notes",
                              "format": "asciidoc",
                              "wiki_page_meta_id": 12,
                              "content": "Notes",
                              "encoding": "UTF-8"
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
        WikisClient repository = new(connection);

        using JsonDocument frontMatter = JsonDocument.Parse("""{"layout":"docs"}""");
        CreateWikiPageRequest request = new()
        {
            Title = "Release Notes",
            Content = "Notes",
            Format = GitLabWikiFormat.Asciidoc,
            FrontMatter = frontMatter.RootElement
        };

        GitLabWikiPage page = await repository.CreateForProjectAsync(42, request,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/42/wikis",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("application/json", handler.LastRequest?.Content?.Headers.ContentType?.MediaType);
        Assert.Contains("\"title\":\"Release Notes\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"content\":\"Notes\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"format\":\"asciidoc\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"front_matter\":{\"layout\":\"docs\"}", sentBody, StringComparison.Ordinal);

        // GitLab derives the slug from the title, so it is not predictable from the request.
        Assert.Equal("Release-Notes", page.Slug);
        Assert.Equal("Release Notes", page.Title);
    }

    [Fact]
    public async Task UpdateForProjectAsync_PutsOnlySuppliedMembers_AndEscapesSlug()
    {
        const string Json = """
                            {
                              "slug": "home/setup",
                              "title": "Home / Setup",
                              "format": "markdown",
                              "content": "Updated",
                              "encoding": "UTF-8"
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
        WikisClient repository = new(connection);

        UpdateWikiPageRequest request = new() { Content = "Updated" };

        GitLabWikiPage page = await repository.UpdateForProjectAsync(1, "home/setup", request,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/wikis/home%2Fsetup",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"content":"Updated"}""", sentBody);
        Assert.Equal("Updated", page.Content);
    }

    [Fact]
    public async Task DeleteForProjectAsync_EscapesSlug_AndSendsDeleteRequest()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        WikisClient repository = new(connection);

        await repository.DeleteForProjectAsync("gitlab-org/gitlab", "home/setup",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/wikis/home%2Fsetup",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task UploadAttachmentForProjectAsync_ForcesFileContentDispositionName_WithOptionalBranchField()
    {
        string? sentBody = null;
        MediaTypeHeaderValue? sentContentType = null;

        using StubHttpMessageHandler handler = new(request =>
        {
            sentContentType = request.Content?.Headers.ContentType;
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();

            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(AttachmentJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        WikisClient repository = new(connection);

        using MemoryStream content = new(FileBytes);
        GitLabFileUpload file = new()
        {
            Content = content, FileName = "dk.png", ContentType = "image/png", FieldName = "caller_file"
        };

        GitLabWikiAttachment attachment = await repository.UploadAttachmentForProjectAsync(1234, file, "main",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1234/wikis/attachments",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("multipart/form-data", sentContentType?.MediaType);

        Assert.NotNull(sentBody);
        Assert.Contains("Content-Disposition: form-data; name=file; filename=dk.png", sentBody,
            StringComparison.Ordinal);
        Assert.DoesNotContain("name=caller_file", sentBody, StringComparison.Ordinal);
        Assert.Contains("name=branch", sentBody, StringComparison.Ordinal);
        Assert.Contains("dk.png", sentBody, StringComparison.Ordinal);
        Assert.Contains("main", sentBody, StringComparison.Ordinal);

        // The stream is borrowed, never owned: it must still be usable after the call returns.
        Assert.True(content.CanRead);

        Assert.Equal("dk.png", attachment.FileName);
        Assert.Equal("uploads/6a061c4cf9f1c28cb22c384b4b8d4e3c/dk.png", attachment.FilePath);
        Assert.Equal("main", attachment.Branch);
        Assert.Equal("![dk](uploads/6a061c4cf9f1c28cb22c384b4b8d4e3c/dk.png)", attachment.Link?.Markdown);
    }

    [Fact]
    public async Task UploadAttachmentForProjectAsync_OmitsBranchField_WhenNotSupplied()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(AttachmentJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        WikisClient repository = new(connection);

        using MemoryStream content = new(FileBytes);
        GitLabFileUpload file = new() { Content = content, FileName = "dk.png" };

        await repository.UploadAttachmentForProjectAsync(1, file,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(sentBody);
        Assert.DoesNotContain("name=branch", sentBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ListForGroupAsync_BuildsGroupWikisRoute_AndEncodesNamespacedGroupPath()
    {
        const string Json = $"[{FullPageJson}]";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        WikisClient repository = new(connection);

        List<GitLabWikiPage> pages = new();
        await foreach (GitLabWikiPage page in repository.ListForGroupAsync("gitlab-org/subgroup", true,
                           TestContext.Current.CancellationToken))
        {
            pages.Add(page);
        }

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/gitlab-org%2Fsubgroup/wikis?with_content=true",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("home/setup", Assert.Single(pages).Slug);
    }

    [Fact]
    public async Task GetForGroupAsync_EscapesNestedSlug_AndDeserializesPage()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(FullPageJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        WikisClient repository = new(connection);

        GitLabWikiPage page = await repository.GetForGroupAsync(9, "home/setup",
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/9/wikis/home%2Fsetup",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(77, page.WikiPageMetaId);
        Assert.Equal("markdown", page.Format);
    }

    [Fact]
    public async Task CreateForGroupAsync_PostsToGroupWikisRoute_WithSerializedBody()
    {
        const string Json = """
                            {
                              "slug": "Policies",
                              "title": "Policies",
                              "format": "markdown",
                              "content": "Body",
                              "encoding": "UTF-8"
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
        WikisClient repository = new(connection);

        GitLabWikiPage page = await repository.CreateForGroupAsync(9,
            new CreateWikiPageRequest { Title = "Policies", Content = "Body" },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/9/wikis", handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"title":"Policies","content":"Body"}""", sentBody);
        Assert.Equal("Policies", page.Slug);
    }

    [Fact]
    public async Task UpdateForGroupAsync_PutsToGroupWikisRoute_WithSerializedBody()
    {
        const string Json = """
                            {
                              "slug": "Policies",
                              "title": "Group Policies",
                              "format": "markdown",
                              "content": "Body",
                              "encoding": "UTF-8"
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
        WikisClient repository = new(connection);

        GitLabWikiPage page = await repository.UpdateForGroupAsync("gitlab-org/subgroup", "Policies",
            new UpdateWikiPageRequest { Title = "Group Policies" }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/gitlab-org%2Fsubgroup/wikis/Policies",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"title":"Group Policies"}""", sentBody);
        Assert.Equal("Group Policies", page.Title);
    }

    [Fact]
    public async Task DeleteForGroupAsync_SendsDeleteToGroupWikisRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        WikisClient repository = new(connection);

        await repository.DeleteForGroupAsync(9, "home/setup", TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/9/wikis/home%2Fsetup",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task UploadAttachmentForGroupAsync_ForcesFileContentDispositionName_AndEncodesNamespacedGroupPath()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(AttachmentJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        WikisClient repository = new(connection);

        using MemoryStream content = new(FileBytes);
        GitLabFileUpload file = new()
        {
            Content = content, FileName = "dk.png", ContentType = "image/png", FieldName = "caller_file"
        };

        GitLabWikiAttachment attachment = await repository.UploadAttachmentForGroupAsync("gitlab-org/subgroup",
            file, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/gitlab-org%2Fsubgroup/wikis/attachments",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.NotNull(sentBody);
        Assert.Contains("Content-Disposition: form-data; name=file; filename=dk.png", sentBody,
            StringComparison.Ordinal);
        Assert.DoesNotContain("name=caller_file", sentBody, StringComparison.Ordinal);
        Assert.Contains("dk.png", sentBody, StringComparison.Ordinal);
        Assert.Equal("uploads/6a061c4cf9f1c28cb22c384b4b8d4e3c/dk.png", attachment.Link?.Url?.OriginalString);
    }

    [Fact]
    public async Task GetForProjectAsync_OnMissingPage_ThrowsGitLabNotFoundException()
    {
        const string Json = """{ "message": "404 Wiki Page Not Found" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        WikisClient repository = new(connection);

        GitLabNotFoundException exception = await Assert.ThrowsAsync<GitLabNotFoundException>(() =>
            repository.GetForProjectAsync(1, "missing", cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        Assert.Equal("404 Wiki Page Not Found", exception.Message);
    }

    [Fact]
    public async Task CreateForProjectAsync_OnValidationError_ThrowsGitLabValidationExceptionWithFieldErrors()
    {
        const string Json = """{ "message": { "title": ["cannot be blank"] } }""";

        using StubHttpMessageHandler handler = new(_ =>
            new HttpResponseMessage(HttpStatusCode.UnprocessableEntity)
            {
                Content = new StringContent(Json, Encoding.UTF8, "application/json")
            });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        WikisClient repository = new(connection);

        GitLabValidationException exception = await Assert.ThrowsAsync<GitLabValidationException>(() =>
            repository.CreateForProjectAsync(1, new CreateWikiPageRequest { Title = " ", Content = "x" },
                TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, exception.StatusCode);
        Assert.Equal("cannot be blank", Assert.Single(exception.Errors["title"]));
    }

    [Fact]
    public async Task ListForGroupAsync_OnFreeTierGroup_ThrowsGitLabForbiddenException()
    {
        const string Json = """{ "message": "403 Forbidden" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Forbidden)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        WikisClient repository = new(connection);

        // Group wikis are a Premium feature; a Free-tier instance answers 403 rather than an empty list.
        GitLabForbiddenException exception = await Assert.ThrowsAsync<GitLabForbiddenException>(async () =>
        {
            await foreach (GitLabWikiPage _ in repository.ListForGroupAsync(9,
                                   cancellationToken: TestContext.Current.CancellationToken)
                               .ConfigureAwait(false))
            {
                // The exception is thrown while fetching the first page, before any item is yielded.
            }
        });

        Assert.Equal(HttpStatusCode.Forbidden, exception.StatusCode);
    }
}