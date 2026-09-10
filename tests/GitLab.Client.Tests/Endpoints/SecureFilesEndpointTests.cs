using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

using GitLab.Client.Abstractions;
using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Endpoints;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Endpoints;

public sealed class SecureFilesEndpointTests
{
    private const string SecureFileJson = """
                                          {
                                            "id": 1,
                                            "name": "myfile.jks",
                                            "checksum": "16630b189ab34b2e3504f4758e1054d2e478deda510b2b08cc0ef38d12e80aac",
                                            "checksum_algorithm": "sha256",
                                            "created_at": "2022-02-22T22:22:22.222Z",
                                            "expires_at": null,
                                            "metadata": null,
                                            "file_extension": "jks"
                                          }
                                          """;

    private const string CertificateFileJson = """
                                               {
                                                 "id": 2,
                                                 "name": "myfile.cer",
                                                 "checksum": "16630b189ab34b2e3504f4758e1054d2e478deda510b2b08cc0ef38d12e80aac",
                                                 "checksum_algorithm": "sha256",
                                                 "created_at": "2022-02-22T22:22:22.222Z",
                                                 "expires_at": "2023-09-21T14:55:59.000Z",
                                                 "metadata": {
                                                   "id": "75949910542696343243264405377658443914",
                                                   "issuer": { "CN": "Apple Worldwide Developer Relations CA" },
                                                   "subject": { "CN": "Apple Development: Test" }
                                                 },
                                                 "file_extension": "cer"
                                               }
                                               """;

    private static readonly byte[] FileBytes = [0x4B, 0x45, 0x59, 0x53, 0x54, 0x4F, 0x52, 0x45];

    [Fact]
    public async Task ListAsync_BuildsSecureFilesRoute_AndDeserializesTheMetadata()
    {
        string json = $"[{SecureFileJson}]";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        SecureFilesClient repository = new(connection);

        List<GitLabSecureFile> files = new();
        await foreach (GitLabSecureFile item in repository.ListAsync(1, TestContext.Current.CancellationToken))
        {
            files.Add(item);
        }

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/secure_files",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabSecureFile file = Assert.Single(files);
        Assert.Equal(1, file.Id);
        Assert.Equal("myfile.jks", file.Name);
        Assert.Equal("sha256", file.ChecksumAlgorithm);
        Assert.Equal("jks", file.FileExtension);
        Assert.Null(file.ExpiresAt);
    }

    [Fact]
    public async Task ListAsync_EncodesNamespacedProjectPath()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        SecureFilesClient repository = new(connection);

        await foreach (GitLabSecureFile _ in
                       repository.ListAsync("gitlab-org/gitlab", TestContext.Current.CancellationToken))
        {
            Assert.Fail("The stubbed response is an empty page.");
        }

        Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/secure_files",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    /// <summary>
    ///     The "metadata" member is an untyped object whose shape depends on the file type, so it is kept as
    ///     a raw JsonElement rather than forced into a record.
    /// </summary>
    [Fact]
    public async Task GetAsync_KeepsTheUntypedMetadataObjectIntact()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(CertificateFileJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        SecureFilesClient repository = new(connection);

        GitLabSecureFile file = await repository.GetAsync(1, 2, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/secure_files/2",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        Assert.Equal("myfile.cer", file.Name);
        Assert.Equal(new DateTimeOffset(2023, 9, 21, 14, 55, 59, TimeSpan.Zero), file.ExpiresAt);

        JsonElement metadata = Assert.NotNull(file.Metadata);
        Assert.Equal(JsonValueKind.Object, metadata.ValueKind);
        Assert.Equal("Apple Development: Test",
            metadata.GetProperty("subject").GetProperty("CN").GetString());
    }

    [Fact]
    public async Task CreateAsync_PostsMultipartFormData_WithNameField_AndReturnsTheStoredMetadata()
    {
        string? sentBody = null;
        MediaTypeHeaderValue? sentContentType = null;

        using StubHttpMessageHandler handler = new(request =>
        {
            sentContentType = request.Content?.Headers.ContentType;
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();

            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(SecureFileJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        SecureFilesClient repository = new(connection);

        using MemoryStream content = new(FileBytes);
        GitLabFileUpload file = new()
        {
            Content = content, FileName = "myfile.jks", ContentType = "application/x-java-keystore"
        };

        GitLabSecureFile created =
            await repository.CreateAsync(1, "myfile.jks", file, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/secure_files",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("multipart/form-data", sentContentType?.MediaType);

        Assert.NotNull(sentBody);
        Assert.Contains("name=file", sentBody, StringComparison.Ordinal);
        Assert.Contains("name=name", sentBody, StringComparison.Ordinal);
        Assert.Contains("myfile.jks", sentBody, StringComparison.Ordinal);

        // The stream is borrowed, never owned: it must still be usable after the call returns.
        Assert.True(content.CanRead);

        Assert.Equal(1, created.Id);
        Assert.Equal("myfile.jks", created.Name);
        Assert.Equal("jks", created.FileExtension);
    }

    [Fact]
    public async Task DownloadAsync_StreamsTheRawBody()
    {
        using StubHttpMessageHandler handler = new(_ =>
        {
            HttpResponseMessage response = new(HttpStatusCode.OK) { Content = new ByteArrayContent(FileBytes) };
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

            return response;
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        SecureFilesClient repository = new(connection);

        using GitLabFileResponse file =
            await repository.DownloadAsync("gitlab-org/gitlab", 1, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/secure_files/1/download",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("application/octet-stream", file.ContentType);

        using MemoryStream copy = new();
        await file.Content.CopyToAsync(copy, TestContext.Current.CancellationToken);
        Assert.Equal(FileBytes, copy.ToArray());
    }

    [Fact]
    public async Task DeleteAsync_SendsDeleteToTheSecureFileRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        SecureFilesClient repository = new(connection);

        await repository.DeleteAsync("gitlab-org/gitlab", 42, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/secure_files/42",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task GetAsync_OnMissingFile_ThrowsGitLabNotFoundException()
    {
        const string Json = """{ "message": "404 Not found" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        SecureFilesClient repository = new(connection);

        GitLabNotFoundException exception = await Assert.ThrowsAsync<GitLabNotFoundException>(() =>
            repository.GetAsync(1, 404, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        Assert.Equal("404 Not found", exception.Message);
    }
}