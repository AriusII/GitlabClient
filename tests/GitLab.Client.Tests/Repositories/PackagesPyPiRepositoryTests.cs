using System.Net;
using System.Net.Http.Headers;
using System.Text;

using GitLab.Client.Abstractions;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Repositories;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Repositories;

public sealed class PackagesPyPiRepositoryTests
{
    private static readonly Uri BaseAddress = new("https://gitlab.example/api/v4/");

    private static readonly byte[] PackageBytes = [0x50, 0x4B, 0x03, 0x04];

    [Fact]
    public async Task DownloadFileForGroupAsync_UsesThePluralGroupsRoot()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(PackageBytes)
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        PackagesPyPiRepository repository = new(connection);

        using GitLabFileResponse response = await repository.DownloadFileForGroupAsync(
            9,
            "5y57017232013c8ac80647f4ca153k3726f6cba62d055cd747844ed95b3c65ff",
            "my.pypi.package-0.0.1.tar.gz",
            TestContext.Current.CancellationToken);

        // Unlike Composer's "/group/:id", PyPI's group endpoints use the library's usual "/groups/:id".
        Assert.Equal(
            "https://gitlab.example/api/v4/groups/9/-/packages/pypi/files/"
            + "5y57017232013c8ac80647f4ca153k3726f6cba62d055cd747844ed95b3c65ff/my.pypi.package-0.0.1.tar.gz",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetSimpleIndexForGroupAsync_BuildsTheSimpleRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("<html></html>", Encoding.UTF8, "text/html")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        PackagesPyPiRepository repository = new(connection);

        using GitLabFileResponse response =
            await repository.GetSimpleIndexForGroupAsync(9, TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/groups/9/-/packages/pypi/simple",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("text/html", response.ContentType);
    }

    [Fact]
    public async Task GetSimplePackageForGroupAsync_EscapesThePackageName()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("<html></html>", Encoding.UTF8, "text/html")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        PackagesPyPiRepository repository = new(connection);

        using GitLabFileResponse response = await repository.GetSimplePackageForGroupAsync(9, "my.pypi.package",
            TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/groups/9/-/packages/pypi/simple/my.pypi.package",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task UploadAsync_PostsMultipartFormData_UnderTheContentFieldName_WithMetadataAsFormFields()
    {
        string? sentBody = null;
        MediaTypeHeaderValue? sentContentType = null;

        using StubHttpMessageHandler handler = new(request =>
        {
            sentContentType = request.Content?.Headers.ContentType;
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created);
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        PackagesPyPiRepository repository = new(connection);

        using MemoryStream content = new(PackageBytes);
        GitLabFileUpload upload = new() { Content = content, FileName = "my.pypi.package-0.0.1.tar.gz" };
        PyPiPackageUploadRequest metadata = new()
        {
            Name = "my.pypi.package", RequiresPython = ">=3.7", Sha256Digest = "ba7816bf"
        };

        await repository.UploadAsync(7, upload, metadata, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/7/packages/pypi",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("multipart/form-data", sentContentType?.MediaType);
        Assert.NotNull(sentBody);
        Assert.Contains("name=content", sentBody, StringComparison.Ordinal);
        Assert.DoesNotContain("name=file", sentBody, StringComparison.Ordinal);
        Assert.Contains("name=name", sentBody, StringComparison.Ordinal);
        Assert.Contains("my.pypi.package", sentBody, StringComparison.Ordinal);
        Assert.Contains("name=requires_python", sentBody, StringComparison.Ordinal);
        Assert.Contains(">=3.7", sentBody, StringComparison.Ordinal);
        Assert.Contains("name=sha256_digest", sentBody, StringComparison.Ordinal);
        Assert.DoesNotContain("md5_digest", sentBody, StringComparison.Ordinal);

        // The stream is borrowed, never owned: it must still be usable after the call returns.
        Assert.True(content.CanRead);
    }

    [Fact]
    public async Task AuthorizeUploadAsync_PostsToTheAuthorizeRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK));

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        PackagesPyPiRepository repository = new(connection);

        await repository.AuthorizeUploadAsync(7, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/7/packages/pypi/authorize",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task DownloadFileAsync_BuildsTheProjectFileRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(PackageBytes)
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        PackagesPyPiRepository repository = new(connection);

        using GitLabFileResponse response = await repository.DownloadFileAsync(
            7,
            "5y57017232013c8ac80647f4ca153k3726f6cba62d055cd747844ed95b3c65ff",
            "my.pypi.package-0.0.1.tar.gz",
            TestContext.Current.CancellationToken);

        Assert.Equal(
            "https://gitlab.example/api/v4/projects/7/packages/pypi/files/"
            + "5y57017232013c8ac80647f4ca153k3726f6cba62d055cd747844ed95b3c65ff/my.pypi.package-0.0.1.tar.gz",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetSimpleIndexForProjectAsync_BuildsTheSimpleRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("<html></html>", Encoding.UTF8, "text/html")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        PackagesPyPiRepository repository = new(connection);

        using GitLabFileResponse response =
            await repository.GetSimpleIndexForProjectAsync(7, TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/7/packages/pypi/simple",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("text/html", response.ContentType);
    }

    [Fact]
    public async Task GetSimplePackageForProjectAsync_EscapesThePackageName()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("<html></html>", Encoding.UTF8, "text/html")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        PackagesPyPiRepository repository = new(connection);

        using GitLabFileResponse response = await repository.GetSimplePackageForProjectAsync(7, "my.pypi.package",
            TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/7/packages/pypi/simple/my.pypi.package",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }
}