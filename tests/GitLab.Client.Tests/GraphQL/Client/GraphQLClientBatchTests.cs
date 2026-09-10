using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

using GitLab.Client.Abstractions;
using GitLab.Client.Configuration;
using GitLab.Client.Endpoints;
using GitLab.Client.GraphQL.Protocol;
using GitLab.Client.Infrastructure.GraphQL;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Tests.TestSupport;

using Microsoft.Extensions.Options;

namespace GitLab.Client.Tests.GraphQL.Client;

public sealed class GraphQLClientBatchTests
{
    private static readonly Uri RestBaseAddress = new("https://gitlab.example/api/v4/");

    private static JsonTypeInfo<GitLabGraphQLResponse<GraphQLClientBatchTestData>[]> ResponseTypeInfo =>
        GraphQLClientBatchTestJsonContext.Default.GitLabGraphQLResponseGraphQLClientBatchTestDataArray;

    [Fact]
    public async Task ExecuteBatchAsync_PostsAnOrderedJsonArrayAndPreservesPartialResponses()
    {
        string? requestBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            requestBody = request.Content?.ReadAsStringAsync(TestContext.Current.CancellationToken)
                .GetAwaiter().GetResult();
            return JsonResponse(HttpStatusCode.OK, """
                                                   [
                                                     { "data": { "project": { "name": "First" } } },
                                                     {
                                                       "data": { "project": { "name": "Second" } },
                                                       "errors": [ { "message": "Partial result", "path": [ "project", "visibility" ] } ]
                                                     }
                                                   ]
                                                   """);
        });
        using HttpClient httpClient = new(handler) { BaseAddress = RestBaseAddress };
        GraphQLClient client = CreateClient(httpClient);

        GitLabGraphQLResponse<GraphQLClientBatchTestData>[] responses = await client.ExecuteBatchAsync(
            [CreateProjectRequest("first"), CreateProjectRequest("second")],
            ResponseTypeInfo,
            TestContext.Current.CancellationToken);

        string sentBody = Assert.IsType<string>(requestBody);
        using JsonDocument body = JsonDocument.Parse(sentBody);
        Assert.Equal(JsonValueKind.Array, body.RootElement.ValueKind);
        Assert.Equal(2, body.RootElement.GetArrayLength());
        Assert.Equal("first", body.RootElement[0].GetProperty("variables").GetProperty("fullPath").GetString());
        Assert.Equal("second", body.RootElement[1].GetProperty("variables").GetProperty("fullPath").GetString());

        Assert.Equal(2, responses.Length);
        Assert.Equal("First", responses[0].Data?.Project?.Name);
        Assert.False(responses[0].HasErrors);
        Assert.Equal("Second", responses[1].Data?.Project?.Name);
        GitLabGraphQLError error = Assert.Single(responses[1].Errors!);
        Assert.Equal("Partial result", error.Message);
        Assert.Equal("project", Assert.IsType<JsonElement>(error.Path)[0].GetString());
    }

    [Fact]
    public async Task ExecuteBatchAsync_RejectsInvalidInputBeforeSending()
    {
        using StubHttpMessageHandler handler = new(_ =>
            throw new InvalidOperationException("An invalid GraphQL multiplex request must not reach HTTP."));
        using HttpClient httpClient = new(handler) { BaseAddress = RestBaseAddress };
        GraphQLClient client = CreateClient(httpClient);

        await Assert.ThrowsAsync<ArgumentNullException>(() => client.ExecuteBatchAsync<GraphQLClientBatchTestData>(
            null!, ResponseTypeInfo, TestContext.Current.CancellationToken));
        await Assert.ThrowsAsync<ArgumentException>(() => client.ExecuteBatchAsync(
            Array.Empty<GitLabGraphQLRequest>(), ResponseTypeInfo, TestContext.Current.CancellationToken));
        await Assert.ThrowsAsync<ArgumentNullException>(() => client.ExecuteBatchAsync(
            [new GitLabGraphQLRequest { Query = null! }], ResponseTypeInfo, TestContext.Current.CancellationToken));
        await Assert.ThrowsAsync<ArgumentException>(() => client.ExecuteBatchAsync(
            [new GitLabGraphQLRequest { Query = " \t\r\n" }], ResponseTypeInfo, TestContext.Current.CancellationToken));

        Assert.Null(handler.LastRequest);
    }

    private static GraphQLClient CreateClient(HttpClient httpClient)
    {
        IGitLabApiConnection restConnection = new GitLabApiConnection(httpClient);
        IOptionsMonitor<GitLabClientOptions> options = new FixedOptionsMonitor<GitLabClientOptions>(
            new GitLabClientOptions { AccessToken = "glpat-test-token", BaseAddress = RestBaseAddress });
        IGitLabGraphQLConnection graphQLConnection = new GitLabGraphQLConnection(restConnection, options);

        return new GraphQLClient(graphQLConnection);
    }

    private static GitLabGraphQLRequest CreateProjectRequest(string fullPath)
    {
        return GitLabGraphQLRequest.Create(
            "query Project($fullPath: ID!) { project(fullPath: $fullPath) { name } }",
            new GraphQLClientBatchTestVariables { FullPath = fullPath },
            GraphQLClientBatchTestJsonContext.Default.GraphQLClientBatchTestVariables);
    }

    private static HttpResponseMessage JsonResponse(HttpStatusCode statusCode, string content)
    {
        return new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(content, Encoding.UTF8, "application/json")
        };
    }

    private sealed class FixedOptionsMonitor<TOptions>(TOptions currentValue) : IOptionsMonitor<TOptions>
        where TOptions : class
    {
        public TOptions CurrentValue => currentValue;

        public TOptions Get(string? name)
        {
            return currentValue;
        }

        public IDisposable OnChange(Action<TOptions, string?> listener)
        {
            return NoOpDisposable.Instance;
        }

        private sealed class NoOpDisposable : IDisposable
        {
            public static NoOpDisposable Instance { get; } = new();

            public void Dispose()
            {
            }
        }
    }
}

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(GraphQLClientBatchTestVariables))]
[JsonSerializable(typeof(GitLabGraphQLResponse<GraphQLClientBatchTestData>[]))]
internal sealed partial class GraphQLClientBatchTestJsonContext : JsonSerializerContext;

internal sealed record GraphQLClientBatchTestVariables
{
    public required string FullPath { get; init; }
}

internal sealed record GraphQLClientBatchTestData
{
    public GraphQLClientBatchTestProject? Project { get; init; }
}

internal sealed record GraphQLClientBatchTestProject
{
    public required string Name { get; init; }
}