using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

using GitLab.Client.Abstractions;
using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Repositories;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Repositories;

public sealed class MlModelPackageFilesRepositoryTests
{
    private static readonly Uri BaseAddress = new("https://gitlab.example/api/v4/");

    private static readonly byte[] FileBytes = [0x50, 0x4B, 0x03, 0x04];

    private static (HttpClient httpClient, StubHttpMessageHandler handler) CreateClient(
        Func<HttpRequestMessage, HttpResponseMessage> respond)
    {
        StubHttpMessageHandler handler = new(respond);
        HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        return (httpClient, handler);
    }

    [Fact]
    public async Task DownloadFileAsync_BuildsTheRoute_WithTheStatusQuery()
    {
        (HttpClient httpClient, StubHttpMessageHandler handler) = CreateClient(_ =>
            new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(FileBytes) });

        GitLabApiConnection connection = new(httpClient);
        MlModelPackageFilesRepository repository = new(connection);

        using GitLabFileResponse file = await repository.DownloadFileAsync(1, 9, "model.gguf",
            new MlModelPackageFileStatusOptions { Status = GitLabPackageFileStatus.Hidden },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/packages/ml_models/9/files/model.gguf?status=hidden",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(HttpStatusCode.OK, file.StatusCode);
    }

    [Fact]
    public async Task DownloadFileAsync_WithNoOptions_OmitsTheStatusQuery()
    {
        (HttpClient httpClient, StubHttpMessageHandler handler) = CreateClient(_ =>
            new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(FileBytes) });

        GitLabApiConnection connection = new(httpClient);
        MlModelPackageFilesRepository repository = new(connection);

        using GitLabFileResponse file = await repository.DownloadFileAsync(1, 9, "model.gguf",
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/1/packages/ml_models/9/files/model.gguf",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(HttpStatusCode.OK, file.StatusCode);
    }

    [Fact]
    public async Task DownloadFileAsync_EncodesNamespacedProjectPath_AndEscapesTheFileName()
    {
        (HttpClient httpClient, StubHttpMessageHandler handler) = CreateClient(_ =>
            new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(FileBytes) });

        GitLabApiConnection connection = new(httpClient);
        MlModelPackageFilesRepository repository = new(connection);

        using GitLabFileResponse file = await repository.DownloadFileAsync("gitlab-org/gitlab", 9,
            "model v2.gguf", cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(
            "https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/packages/ml_models/9/files/model%20v2.gguf",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(HttpStatusCode.OK, file.StatusCode);
    }

    [Fact]
    public async Task DownloadFileAsync_OnMissingFile_ThrowsGitLabNotFoundException()
    {
        const string Json = """{ "message": "404 Not Found" }""";

        (HttpClient httpClient, _) = CreateClient(_ =>
            new HttpResponseMessage(HttpStatusCode.NotFound)
            {
                Content = new StringContent(Json, Encoding.UTF8, "application/json")
            });

        GitLabApiConnection connection = new(httpClient);
        MlModelPackageFilesRepository repository = new(connection);

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabNotFoundException>(() =>
            repository.DownloadFileAsync(1, 9, "missing.gguf",
                cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
    }

    [Fact]
    public async Task UploadFileAsync_PutsMultipartFormData_WithTheStatusField_AndReturnsTheJsonElement()
    {
        const string Json = """{ "message": "201 Created" }""";

        string? sentBody = null;
        MediaTypeHeaderValue? sentContentType = null;

        (HttpClient httpClient, StubHttpMessageHandler handler) = CreateClient(request =>
        {
            sentContentType = request.Content?.Headers.ContentType;
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();

            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(Json, Encoding.UTF8, "application/json")
            };
        });

        GitLabApiConnection connection = new(httpClient);
        MlModelPackageFilesRepository repository = new(connection);

        using MemoryStream content = new([0x00, 0x01, 0x02]);
        GitLabFileUpload file = new() { Content = content, FileName = "model.gguf" };

        JsonElement response = await repository.UploadFileAsync(1, 9, "model.gguf", file,
            GitLabPackageFileStatus.Hidden, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/packages/ml_models/9/files/model.gguf",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("multipart/form-data", sentContentType?.MediaType);

        Assert.NotNull(sentBody);
        string unquoted = sentBody.Replace("\"", string.Empty, StringComparison.Ordinal);
        Assert.Contains("name=file", unquoted, StringComparison.Ordinal);
        Assert.Contains("model.gguf", unquoted, StringComparison.Ordinal);
        Assert.Contains("name=status", unquoted, StringComparison.Ordinal);
        Assert.Contains("hidden", unquoted, StringComparison.Ordinal);

        Assert.Equal("201 Created", response.GetProperty("message").GetString());

        // The stream is borrowed, never owned.
        Assert.True(content.CanRead);
    }

    [Fact]
    public async Task UploadFileAsync_OmitsTheStatusFormField_WhenNotProvided()
    {
        string? sentBody = null;

        (HttpClient httpClient, StubHttpMessageHandler handler) = CreateClient(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();

            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent("{}", Encoding.UTF8, "application/json")
            };
        });

        GitLabApiConnection connection = new(httpClient);
        MlModelPackageFilesRepository repository = new(connection);

        using MemoryStream content = new([0x00, 0x01]);
        GitLabFileUpload file = new() { Content = content, FileName = "model.gguf" };

        await repository.UploadFileAsync(1, 9, "model.gguf", file,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(sentBody);
        string unquoted = sentBody.Replace("\"", string.Empty, StringComparison.Ordinal);
        Assert.Contains("name=file", unquoted, StringComparison.Ordinal);
        Assert.DoesNotContain("name=status", unquoted, StringComparison.Ordinal);
        Assert.NotNull(handler.LastRequest);
    }

    [Fact]
    public async Task AuthorizeFileUploadAsync_PutsTheStatusBody_ToTheAuthorizeRoute()
    {
        string? sentBody = null;

        (HttpClient httpClient, StubHttpMessageHandler handler) = CreateClient(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK);
        });

        GitLabApiConnection connection = new(httpClient);
        MlModelPackageFilesRepository repository = new(connection);

        await repository.AuthorizeFileUploadAsync(1, 9, "model.gguf",
            new AuthorizeMlModelPackageFileUploadRequest { Status = GitLabPackageFileStatus.Default },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/projects/1/packages/ml_models/9/files/model.gguf/authorize",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"status":"default"}""", sentBody);
    }

    [Fact]
    public async Task AuthorizeFileUploadAsync_WithNoRequest_SendsAnEmptyJsonBody()
    {
        string? sentBody = null;

        (HttpClient httpClient, StubHttpMessageHandler handler) = CreateClient(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK);
        });

        GitLabApiConnection connection = new(httpClient);
        MlModelPackageFilesRepository repository = new(connection);

        await repository.AuthorizeFileUploadAsync(1, 9, "model.gguf",
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("{}", sentBody);
        Assert.NotNull(handler.LastRequest);
    }

    [Fact]
    public async Task DownloadFileByPathAsync_EscapesTheDirectoryPath_AndTheFileName()
    {
        (HttpClient httpClient, StubHttpMessageHandler handler) = CreateClient(_ =>
            new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(FileBytes) });

        GitLabApiConnection connection = new(httpClient);
        MlModelPackageFilesRepository repository = new(connection);

        using GitLabFileResponse file = await repository.DownloadFileByPathAsync(1, 9, "weights/v1",
            "model.gguf", cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(
            "https://gitlab.example/api/v4/projects/1/packages/ml_models/9/files/weights%2Fv1/model.gguf",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(HttpStatusCode.OK, file.StatusCode);
    }

    [Fact]
    public async Task UploadFileByPathAsync_PutsToThePathRoute_WithMultipartFormData()
    {
        (HttpClient httpClient, StubHttpMessageHandler handler) = CreateClient(_ =>
            new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent("{}", Encoding.UTF8, "application/json")
            });

        GitLabApiConnection connection = new(httpClient);
        MlModelPackageFilesRepository repository = new(connection);

        using MemoryStream content = new([0x00]);
        GitLabFileUpload file = new() { Content = content, FileName = "model.gguf" };

        await repository.UploadFileByPathAsync(1, 9, "weights/v1", "model.gguf", file,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/projects/1/packages/ml_models/9/files/weights%2Fv1/model.gguf",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task AuthorizeFileUploadByPathAsync_PutsToThePathAuthorizeRoute()
    {
        string? sentBody = null;

        (HttpClient httpClient, StubHttpMessageHandler handler) = CreateClient(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK);
        });

        GitLabApiConnection connection = new(httpClient);
        MlModelPackageFilesRepository repository = new(connection);

        await repository.AuthorizeFileUploadByPathAsync(1, 9, "weights/v1", "model.gguf",
            new AuthorizeMlModelPackageFileUploadRequest { Status = GitLabPackageFileStatus.Hidden },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/projects/1/packages/ml_models/9/files/weights%2Fv1/model.gguf/authorize",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"status":"hidden"}""", sentBody);
    }
}