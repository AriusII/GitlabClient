using System.Net;
using System.Net.Http.Headers;
using System.Text;

using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Repositories;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Repositories;

public sealed class AlertManagementRepositoryTests
{
    private const string MetricImageJson = """
                                           {
                                             "id": 23,
                                             "created_at": "2020-11-13T00:06:18.084Z",
                                             "filename": "sample_2054",
                                             "file_path": "/uploads/-/system/alert_metric_image/file/23/sample_2054.png",
                                             "url": "https://example.com/metric",
                                             "url_text": "An example metric"
                                           }
                                           """;

    private static readonly Uri BaseAddress = new("https://gitlab.example/api/v4/");

    private static readonly byte[] FileBytes = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];

    [Fact]
    public async Task ListMetricImagesAsync_BuildsTheMetricImagesRoute_AndDeserializesThePage()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent($"[{MetricImageJson}]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        AlertManagementRepository repository = new(connection);

        List<GitLabMetricImage> images = [];
        await foreach (GitLabMetricImage image in repository.ListMetricImagesAsync(7, 42,
                           TestContext.Current.CancellationToken))
        {
            images.Add(image);
        }

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/7/alert_management_alerts/42/metric_images",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabMetricImage only = Assert.Single(images);
        Assert.Equal(23, only.Id);
        Assert.Equal(new DateTimeOffset(2020, 11, 13, 0, 6, 18, 84, TimeSpan.Zero), only.CreatedAt);
        Assert.Equal("sample_2054", only.Filename);
        Assert.Equal("/uploads/-/system/alert_metric_image/file/23/sample_2054.png", only.FilePath);
        Assert.Equal("https://example.com/metric", only.Url?.OriginalString);
        Assert.Equal("An example metric", only.Caption);
    }

    [Fact]
    public async Task ListMetricImagesAsync_EncodesANamespacedProjectPath()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        AlertManagementRepository repository = new(connection);

        await foreach (GitLabMetricImage _ in repository.ListMetricImagesAsync(
                           ProjectId.FromPath("group/subgroup/project"), 3,
                           TestContext.Current.CancellationToken))
        {
            // Draining the sequence is what issues the request.
        }

        Assert.Equal(
            "https://gitlab.example/api/v4/projects/group%2Fsubgroup%2Fproject/alert_management_alerts/3/metric_images",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task UploadMetricImageAsync_PostsMultipartFormData_WithTheOptionalUrlFields()
    {
        string? sentBody = null;
        MediaTypeHeaderValue? sentContentType = null;

        using StubHttpMessageHandler handler = new(request =>
        {
            sentContentType = request.Content?.Headers.ContentType;
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(MetricImageJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        AlertManagementRepository repository = new(connection);

        using MemoryStream content = new(FileBytes);
        GitLabFileUpload file = new() { Content = content, FileName = "sample_2054.png", ContentType = "image/png" };

        GitLabMetricImage image = await repository.UploadMetricImageAsync(7, 42, file,
            new Uri("https://example.com/metric"), "An example metric", TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/7/alert_management_alerts/42/metric_images",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        Assert.Equal("multipart/form-data", sentContentType?.MediaType);
        Assert.NotNull(sentBody);
        Assert.Contains("name=file", sentBody, StringComparison.Ordinal);
        Assert.Contains("sample_2054.png", sentBody, StringComparison.Ordinal);
        Assert.Contains("name=url", sentBody, StringComparison.Ordinal);
        Assert.Contains("https://example.com/metric", sentBody, StringComparison.Ordinal);
        Assert.Contains("name=url_text", sentBody, StringComparison.Ordinal);
        Assert.Contains("An example metric", sentBody, StringComparison.Ordinal);

        // The stream is borrowed, never owned: it must still be usable after the call returns.
        Assert.True(content.CanRead);

        Assert.Equal(23, image.Id);
    }

    [Fact]
    public async Task UploadMetricImageAsync_OmitsTheOptionalFormFields_WhenNeitherIsGiven()
    {
        string? sentBody = null;

        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(MetricImageJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        AlertManagementRepository repository = new(connection);

        using MemoryStream content = new(FileBytes);
        GitLabFileUpload file = new() { Content = content, FileName = "sample_2054.png", ContentType = "image/png" };

        await repository.UploadMetricImageAsync(7, 42, file, cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(sentBody);
        Assert.DoesNotContain("name=url", sentBody, StringComparison.Ordinal);
        Assert.DoesNotContain("name=url_text", sentBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task AuthorizeMetricImageUploadAsync_PostsToTheAuthorizeRoute_AndIgnoresWorkhorsesBody()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("""{"TempPath":"/var/opt/gitlab/uploads/tmp"}""", Encoding.UTF8,
                "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        AlertManagementRepository repository = new(connection);

        await repository.AuthorizeMetricImageUploadAsync(7, 42, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/projects/7/alert_management_alerts/42/metric_images/authorize",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task DeleteMetricImageAsync_DeletesByNumericMetricImageId()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        AlertManagementRepository repository = new(connection);

        await repository.DeleteMetricImageAsync(7, 42, 23, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/projects/7/alert_management_alerts/42/metric_images/23",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task UpdateMetricImageAsync_PutsTheLinkFields_AndDeserializesTheUpdatedImage()
    {
        string? sentBody = null;

        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(MetricImageJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        AlertManagementRepository repository = new(connection);

        UpdateMetricImageRequest request = new()
        {
            Url = new Uri("https://example.com/metric"), Caption = "An example metric"
        };

        GitLabMetricImage image =
            await repository.UpdateMetricImageAsync(7, 42, 23, request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/projects/7/alert_management_alerts/42/metric_images/23",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        Assert.NotNull(sentBody);
        Assert.Contains("\"url\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"url_text\":\"An example metric\"", sentBody, StringComparison.Ordinal);

        Assert.Equal(23, image.Id);
        Assert.Equal("sample_2054", image.Filename);
    }
}