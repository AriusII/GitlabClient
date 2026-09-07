using System.Net;
using System.Net.Http.Headers;

using GitLab.Client.Abstractions;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Repositories;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Repositories;

public sealed class PackagesRpmRepositoryTests
{
    private static readonly Uri BaseAddress = new("https://gitlab.example/api/v4/");

    private static readonly byte[] RpmBytes = [0xED, 0xAB, 0xEE, 0xDB];

    [Fact]
    public async Task UploadAsync_PostsMultipartFormData_UnderTheDefaultFileFieldName()
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
        PackagesRpmRepository repository = new(connection);

        using MemoryStream content = new(RpmBytes);
        GitLabFileUpload file = new() { Content = content, FileName = "my-package-1.0-1.x86_64.rpm" };

        await repository.UploadAsync(7, file, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/7/packages/rpm",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("multipart/form-data", sentContentType?.MediaType);
        Assert.NotNull(sentBody);
        Assert.Contains("name=file", sentBody, StringComparison.Ordinal);

        // The stream is borrowed, never owned: it must still be usable after the call returns.
        Assert.True(content.CanRead);
    }

    [Fact]
    public async Task AuthorizeUploadAsync_PostsToTheAuthorizeRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Created));

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        PackagesRpmRepository repository = new(connection);

        await repository.AuthorizeUploadAsync(7, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/7/packages/rpm/authorize",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task DownloadRepositoryMetadataAsync_EscapesTheFileName()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(RpmBytes)
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        PackagesRpmRepository repository = new(connection);

        using GitLabFileResponse response =
            await repository.DownloadRepositoryMetadataAsync(7, "repomd.xml", TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/7/packages/rpm/repodata/repomd.xml",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task DownloadPackageFileAsync_BuildsTheRouteFromTheNumericFileId()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(RpmBytes)
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        PackagesRpmRepository repository = new(connection);

        using GitLabFileResponse response = await repository.DownloadPackageFileAsync(7, 42,
            "my-package-1.0-1.x86_64.rpm", TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/7/packages/rpm/42/my-package-1.0-1.x86_64.rpm",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}