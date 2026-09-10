using System.Net;
using System.Net.Http.Headers;
using System.Text;

using GitLab.Client.Abstractions;
using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Endpoints;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Endpoints;

public sealed class AttestationsEndpointTests
{
    private static readonly byte[] BundleBytes = [0x7B, 0x22, 0x62, 0x75, 0x6E, 0x64, 0x6C, 0x65, 0x22, 0x7D];

    [Fact]
    public async Task DownloadAsync_BuildsTheAttestationDownloadRoute_AndStreamsTheRawBody()
    {
        using StubHttpMessageHandler handler = new(_ =>
        {
            HttpResponseMessage response = new(HttpStatusCode.OK) { Content = new ByteArrayContent(BundleBytes) };
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

            return response;
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        AttestationsClient repository = new(connection);

        using GitLabFileResponse bundle =
            await repository.DownloadAsync(1, 7, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/attestations/7/download",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("application/octet-stream", bundle.ContentType);

        using MemoryStream copy = new();
        await bundle.Content.CopyToAsync(copy, TestContext.Current.CancellationToken);
        Assert.Equal(BundleBytes, copy.ToArray());
    }

    [Fact]
    public async Task DownloadAsync_EncodesNamespacedProjectPath()
    {
        using StubHttpMessageHandler handler = new(_ =>
            new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(BundleBytes) });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        AttestationsClient repository = new(connection);

        using GitLabFileResponse bundle =
            await repository.DownloadAsync("gitlab-org/gitlab", 3, TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/attestations/3/download",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task DownloadAsync_OnMissingAttestation_ThrowsGitLabNotFoundException()
    {
        const string Json = """{ "message": "404 Not found" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        AttestationsClient repository = new(connection);

        GitLabNotFoundException exception = await Assert.ThrowsAsync<GitLabNotFoundException>(() =>
            repository.DownloadAsync(1, 404, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        Assert.Equal("404 Not found", exception.Message);
    }
}