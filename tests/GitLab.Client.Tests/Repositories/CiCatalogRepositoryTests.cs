using System.Net;
using System.Text;
using System.Text.Json;

using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Repositories;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Repositories;

public sealed class CiCatalogRepositoryTests
{
    [Fact]
    public async Task PublishAsync_PostsToTheCatalogPublishRoute_AndDeserializesTheCatalogUrl()
    {
        const string Json = """
                            { "catalog_url": "https://gitlab.example/explore/catalog/my-namespace/my-component" }
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
        CiCatalogRepository repository = new(connection);

        using JsonDocument metadataDoc = JsonDocument.Parse("""{ "version": "1.0.0" }""");
        CiCatalogPublishRequest request = new() { Metadata = metadataDoc.RootElement.Clone() };

        GitLabCiCatalogPublishResult result =
            await repository.PublishAsync("gitlab-org/my-component", request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fmy-component/catalog/publish",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Contains("\"metadata\":{\"version\":\"1.0.0\"}", sentBody, StringComparison.Ordinal);

        Assert.Equal(new Uri("https://gitlab.example/explore/catalog/my-namespace/my-component"), result.CatalogUrl);
    }

    [Fact]
    public async Task PublishAsync_OnErrorResponse_ThrowsGitLabValidationException()
    {
        const string Json = """{ "message": "Invalid metadata" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.UnprocessableEntity)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        CiCatalogRepository repository = new(connection);

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabValidationException>(() =>
            repository.PublishAsync(7, new CiCatalogPublishRequest { Metadata = null },
                TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, exception.StatusCode);
    }
}