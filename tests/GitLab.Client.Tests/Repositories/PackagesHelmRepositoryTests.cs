using System.Net;
using System.Net.Http.Headers;
using System.Text;

using GitLab.Client.Abstractions;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Repositories;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Repositories;

public sealed class PackagesHelmRepositoryTests
{
    private static readonly Uri BaseAddress = new("https://gitlab.example/api/v4/");

    private static readonly byte[] ChartBytes = [0x1F, 0x8B, 0x08, 0x00];

    [Fact]
    public async Task UploadChartAsync_PostsMultipartFormData_UnderTheChartFieldName()
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
        PackagesHelmRepository repository = new(connection);

        using MemoryStream content = new(ChartBytes);
        GitLabFileUpload chart = new() { Content = content, FileName = "mychart-0.1.0.tgz" };

        await repository.UploadChartAsync(7, "stable", chart, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/7/packages/helm/api/stable/charts",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("multipart/form-data", sentContentType?.MediaType);
        Assert.NotNull(sentBody);
        Assert.Contains("name=chart", sentBody, StringComparison.Ordinal);
        Assert.DoesNotContain("name=file", sentBody, StringComparison.Ordinal);

        // The stream is borrowed, never owned: it must still be usable after the call returns.
        Assert.True(content.CanRead);
    }

    [Fact]
    public async Task UploadChartAsync_ThrowsWhenChartIsNull()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Created));

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        PackagesHelmRepository repository = new(connection);

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            repository.UploadChartAsync(7, "stable", null!, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task AuthorizeChartUploadAsync_PostsToTheAuthorizeRoute_ForTheGivenChannel()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK));

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        PackagesHelmRepository repository = new(connection);

        await repository.AuthorizeChartUploadAsync(7, "stable", TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/projects/7/packages/helm/api/stable/charts/authorize",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task DownloadChartAsync_AppendsTheTgzExtension_AndEscapesTheFileName()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(ChartBytes)
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        PackagesHelmRepository repository = new(connection);

        using GitLabFileResponse response = await repository.DownloadChartAsync(7, "stable", "my chart-0.1.0",
            TestContext.Current.CancellationToken);

        Assert.Equal(
            "https://gitlab.example/api/v4/projects/7/packages/helm/stable/charts/my%20chart-0.1.0.tgz",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task DownloadChartIndexAsync_BuildsTheIndexYamlRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("apiVersion: v1", Encoding.UTF8, "text/yaml")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        PackagesHelmRepository repository = new(connection);

        using GitLabFileResponse response =
            await repository.DownloadChartIndexAsync(7, "stable", TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/7/packages/helm/stable/index.yaml",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}