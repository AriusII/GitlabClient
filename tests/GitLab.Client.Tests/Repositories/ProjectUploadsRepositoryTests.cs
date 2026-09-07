using System.Net;
using System.Net.Http.Headers;
using System.Text;

using GitLab.Client.Abstractions;
using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Repositories;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Repositories;

public sealed class ProjectUploadsRepositoryTests
{
    private const string UploadJson = """
                                      {
                                        "id": 1,
                                        "size": 1024,
                                        "filename": "screenshot 1.png",
                                        "created_at": "2024-06-20T15:53:00.000Z",
                                        "uploaded_by": {
                                          "id": 18,
                                          "username": "user_1",
                                          "name": "Member One",
                                          "public_email": "user1@example.com"
                                        }
                                      }
                                      """;

    private const string UploadLinkJson = """
                                          {
                                            "id": 5,
                                            "alt": "dk",
                                            "url": "/uploads/66dbcd21ec5d24ed6ea225176098d52b/dk.png",
                                            "full_path": "/-/project/1234/uploads/66dbcd21ec5d24ed6ea225176098d52b/dk.png",
                                            "markdown": "![dk](/uploads/66dbcd21ec5d24ed6ea225176098d52b/dk.png)"
                                          }
                                          """;

    private static readonly Uri BaseAddress = new("https://gitlab.example/api/v4/");

    private static readonly byte[] FileBytes = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];

    [Fact]
    public async Task ListAsync_BuildsTheUploadsRoute_AndDeserializesTheTrimmedUploader()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent($"[{UploadJson}]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        ProjectUploadsRepository repository = new(connection);

        List<GitLabProjectUpload> uploads = [];
        await foreach (GitLabProjectUpload upload in repository.ListAsync(7, TestContext.Current.CancellationToken))
        {
            uploads.Add(upload);
        }

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/7/uploads",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabProjectUpload only = Assert.Single(uploads);
        Assert.Equal(1, only.Id);
        Assert.Equal(1024L, only.Size);
        Assert.Equal("screenshot 1.png", only.Filename);
        Assert.Equal(new DateTimeOffset(2024, 6, 20, 15, 53, 0, TimeSpan.Zero), only.CreatedAt);

        Assert.NotNull(only.UploadedBy);
        Assert.Equal(18, only.UploadedBy!.Id);
        Assert.Equal("user_1", only.UploadedBy.Username);
        Assert.Equal("user1@example.com", only.UploadedBy.PublicEmail);
    }

    [Fact]
    public async Task ListAsync_EncodesANamespacedProjectPath()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        ProjectUploadsRepository repository = new(connection);

        await foreach (GitLabProjectUpload _ in repository.ListAsync(ProjectId.FromPath("group/subgroup/project"),
                           TestContext.Current.CancellationToken))
        {
            // Draining the sequence is what issues the request.
        }

        Assert.Equal("https://gitlab.example/api/v4/projects/group%2Fsubgroup%2Fproject/uploads",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task UploadAsync_PostsMultipartFormData_AndReturnsTheMarkdownLink()
    {
        string? sentBody = null;
        MediaTypeHeaderValue? sentContentType = null;

        using StubHttpMessageHandler handler = new(request =>
        {
            sentContentType = request.Content?.Headers.ContentType;
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();

            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(UploadLinkJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        ProjectUploadsRepository repository = new(connection);

        using MemoryStream content = new(FileBytes);
        GitLabFileUpload file = new() { Content = content, FileName = "dk.png", ContentType = "image/png" };

        GitLabProjectUploadLink link =
            await repository.UploadAsync(1234, file, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1234/uploads",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        Assert.Equal("multipart/form-data", sentContentType?.MediaType);
        Assert.NotNull(sentBody);
        Assert.Contains("name=file", sentBody, StringComparison.Ordinal);
        Assert.Contains("dk.png", sentBody, StringComparison.Ordinal);
        Assert.Contains("image/png", sentBody, StringComparison.Ordinal);

        // The stream is borrowed, never owned: it must still be usable after the call returns.
        Assert.True(content.CanRead);

        Assert.Equal(5, link.Id);
        Assert.Equal("dk", link.Alt);
        Assert.Equal("/uploads/66dbcd21ec5d24ed6ea225176098d52b/dk.png", link.Url?.OriginalString);
        Assert.Equal("![dk](/uploads/66dbcd21ec5d24ed6ea225176098d52b/dk.png)", link.Markdown);
    }

    [Fact]
    public async Task AuthorizeUploadAsync_PostsToTheAuthorizeRoute_AndIgnoresWorkhorsesBody()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("""{"TempPath":"/var/opt/gitlab/uploads/tmp"}""", Encoding.UTF8,
                "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        ProjectUploadsRepository repository = new(connection);

        await repository.AuthorizeUploadAsync(7, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/7/uploads/authorize",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task DownloadAsync_StreamsTheRawBody_ForANumericUploadId()
    {
        using StubHttpMessageHandler handler = new(_ =>
        {
            HttpResponseMessage response = new(HttpStatusCode.OK) { Content = new ByteArrayContent(FileBytes) };

            response.Content.Headers.ContentType = new MediaTypeHeaderValue("image/png");
            response.Content.Headers.ContentDisposition =
                new ContentDispositionHeaderValue("attachment") { FileName = "\"dk.png\"" };

            return response;
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        ProjectUploadsRepository repository = new(connection);

        using GitLabFileResponse file =
            await repository.DownloadAsync(7, 42, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/7/uploads/42",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        Assert.Equal("image/png", file.ContentType);
        Assert.Equal("dk.png", file.FileName);

        using MemoryStream copy = new();
        await file.Content.CopyToAsync(copy, TestContext.Current.CancellationToken);
        Assert.Equal(FileBytes, copy.ToArray());
    }

    [Fact]
    public async Task DeleteAsync_DeletesByNumericUploadId()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        ProjectUploadsRepository repository = new(connection);

        await repository.DeleteAsync(7, 42, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/7/uploads/42",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task DownloadBySecretAsync_EscapesTheSecretAndTheFileName()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(FileBytes)
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        ProjectUploadsRepository repository = new(connection);

        using GitLabFileResponse file = await repository.DownloadBySecretAsync(
            7,
            "66dbcd21ec5d24ed6ea225176098d52b",
            "screenshot 1.png",
            TestContext.Current.CancellationToken);

        // Secret and file name are caller-supplied free text: a space here has to reach GitLab
        // percent-encoded, or the route stops matching and the 404 reads like a missing upload.
        Assert.Equal(
            "https://gitlab.example/api/v4/projects/7/uploads/66dbcd21ec5d24ed6ea225176098d52b/screenshot%201.png",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        Assert.Equal(HttpStatusCode.OK, file.StatusCode);
    }

    [Fact]
    public async Task DeleteBySecretAsync_EscapesASlashBearingFileName()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        ProjectUploadsRepository repository = new(connection);

        await repository.DeleteBySecretAsync(7, "66dbcd21ec5d24ed6ea225176098d52b", "a/b.png",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/projects/7/uploads/66dbcd21ec5d24ed6ea225176098d52b/a%2Fb.png",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task DownloadAsync_MapsA404ToTheTypedNotFoundException()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent("""{"message":"404 Upload Not Found"}""", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        ProjectUploadsRepository repository = new(connection);

        GitLabNotFoundException exception = await Assert.ThrowsAsync<GitLabNotFoundException>(() =>
            repository.DownloadAsync(7, 999, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
    }
}