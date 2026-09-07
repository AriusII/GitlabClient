using System.Net;
using System.Text;
using System.Text.Json;

using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Repositories;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Repositories;

public sealed class ContainerRegistryRepositoryTests
{
    [Fact]
    public async Task IngestEventsAsync_PostsToTheEventsRoute_WithTheRawPayload()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK);
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ContainerRegistryRepository repository = new(connection);

        using JsonDocument payloadDoc = JsonDocument.Parse("""{ "events": [ { "action": "push" } ] }""");

        await repository.IngestEventsAsync(payloadDoc.RootElement.Clone(), TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/container_registry_event/events",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"events":[{"action":"push"}]}""", sentBody);
    }

    [Fact]
    public async Task IngestEventsAsync_OnUnauthorized_ThrowsGitLabAuthenticationException()
    {
        const string Json = """{ "message": "Invalid Token" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Unauthorized)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ContainerRegistryRepository repository = new(connection);

        using JsonDocument payloadDoc = JsonDocument.Parse("""{ "events": [] }""");

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabAuthenticationException>(() =>
            repository.IngestEventsAsync(payloadDoc.RootElement.Clone(), TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.Unauthorized, exception.StatusCode);
    }
}