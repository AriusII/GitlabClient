using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

using GitLab.Client.Abstractions;
using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Configuration;
using GitLab.Client.Endpoints;
using GitLab.Client.GraphQL.Protocol;
using GitLab.Client.Infrastructure.GraphQL;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Tests.TestSupport;

using Microsoft.Extensions.Options;

namespace GitLab.Client.Tests.GraphQL.Client;

public sealed class GraphQLClientTests
{
    private const string ProjectDocument = """
                                           query Project($fullPath: ID!) {
                                             project(fullPath: $fullPath) {
                                               name
                                             }
                                           }
                                           """;

    private static readonly Uri RestBaseAddress = new("https://gitlab.example/api/v4/");

    private static JsonTypeInfo<GitLabGraphQLResponse<GraphQLClientTestData>> ResponseTypeInfo =>
        (JsonTypeInfo<GitLabGraphQLResponse<GraphQLClientTestData>>)GraphQLClientTestJsonContext.Default
            .GetTypeInfo(typeof(GitLabGraphQLResponse<GraphQLClientTestData>))!;

    [Fact]
    public async Task ExecuteAsync_PostsCamelCaseRequestToTheVersionlessGraphQLEndpoint()
    {
        string? requestBody = null;
        string? contentType = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            requestBody = request.Content?.ReadAsStringAsync(TestContext.Current.CancellationToken)
                .GetAwaiter().GetResult();
            contentType = request.Content?.Headers.ContentType?.MediaType;

            return JsonResponse(HttpStatusCode.OK, """{ "data": { "project": { "name": "Client SDK" } } }""");
        });
        using HttpClient httpClient = new(handler) { BaseAddress = RestBaseAddress };
        GraphQLClient client = CreateClient(httpClient);

        GitLabGraphQLRequest request = CreateProjectRequest();
        GitLabGraphQLResponse<GraphQLClientTestData> response = await client.ExecuteAsync(
            request,
            ResponseTypeInfo,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/graphql", handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("application/json", contentType);

        string sentBody = Assert.IsType<string>(requestBody);
        using JsonDocument body = JsonDocument.Parse(sentBody);
        JsonElement root = body.RootElement;
        Assert.Equal(ProjectDocument, root.GetProperty("query").GetString());
        Assert.False(root.TryGetProperty("operationName", out _));
        Assert.Equal("group/client-sdk", root.GetProperty("variables").GetProperty("fullPath").GetString());
        Assert.False(root.GetProperty("variables").TryGetProperty("FullPath", out _));

        GraphQLClientTestProject project = Assert.IsType<GraphQLClientTestProject>(response.Data?.Project);
        Assert.Equal("Client SDK", project.Name);
    }

    [Fact]
    public async Task ExecuteAsync_LeavesPartialDataAndGraphQLErrorsTogetherInTheEnvelope()
    {
        const string responseJson = """
                                    {
                                      "data": { "project": { "name": "Client SDK" } },
                                      "errors": [
                                        {
                                          "message": "The confidential field is not available.",
                                          "path": [ "project", "confidential" ]
                                        }
                                      ]
                                    }
                                    """;
        using StubHttpMessageHandler handler = new(_ => JsonResponse(HttpStatusCode.OK, responseJson));
        using HttpClient httpClient = new(handler) { BaseAddress = RestBaseAddress };
        GraphQLClient client = CreateClient(httpClient);

        GitLabGraphQLResponse<GraphQLClientTestData> response = await client.ExecuteAsync(
            CreateProjectRequest(),
            ResponseTypeInfo,
            TestContext.Current.CancellationToken);

        // HTTP 200 is transport success. The caller needs both portions to decide whether partial data is useful.
        Assert.True(response.HasErrors);
        GraphQLClientTestProject project = Assert.IsType<GraphQLClientTestProject>(response.Data?.Project);
        Assert.Equal("Client SDK", project.Name);

        GitLabGraphQLError error = Assert.Single(response.Errors!);
        Assert.Equal("The confidential field is not available.", error.Message);
        JsonElement path = Assert.IsType<JsonElement>(error.Path);
        Assert.Equal(JsonValueKind.Array, path.ValueKind);
        Assert.Equal("project", path[0].GetString());
        Assert.Equal("confidential", path[1].GetString());
    }

    [Fact]
    public async Task ExecuteAsync_OnNonSuccessHttpResponse_UsesTheTypedTransportException()
    {
        using StubHttpMessageHandler handler = new(_ => JsonResponse(HttpStatusCode.Forbidden,
            """{ "message": "403 Forbidden" }"""));
        using HttpClient httpClient = new(handler) { BaseAddress = RestBaseAddress };
        GraphQLClient client = CreateClient(httpClient);

        GitLabForbiddenException exception = await Assert.ThrowsAsync<GitLabForbiddenException>(() =>
            client.ExecuteAsync(CreateProjectRequest(), ResponseTypeInfo, TestContext.Current.CancellationToken));

        Assert.Equal(HttpMethod.Post, exception.RequestMethod);
        Assert.Equal(new Uri("https://gitlab.example/api/graphql"), exception.RequestUri);
        Assert.Equal("403 Forbidden", exception.Message);
    }

    [Fact]
    public async Task ExecuteAsync_RejectsANullDirectlyInstantiatedQueryBeforeSending()
    {
        using StubHttpMessageHandler handler = new(_ =>
            throw new InvalidOperationException("An invalid GraphQL request must not reach the HTTP pipeline."));
        using HttpClient httpClient = new(handler) { BaseAddress = RestBaseAddress };
        GraphQLClient client = CreateClient(httpClient);
        GitLabGraphQLRequest request = new() { Query = null! };

        ArgumentNullException exception = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await client.ExecuteAsync(request, ResponseTypeInfo, TestContext.Current.CancellationToken)
                .ConfigureAwait(false));

        Assert.Equal("Query", exception.ParamName);
        Assert.Null(handler.LastRequest);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" \t\r\n ")]
    public async Task ExecuteAsync_RejectsAnEmptyOrWhitespaceDirectlyInstantiatedQueryBeforeSending(string query)
    {
        using StubHttpMessageHandler handler = new(_ =>
            throw new InvalidOperationException("An invalid GraphQL request must not reach the HTTP pipeline."));
        using HttpClient httpClient = new(handler) { BaseAddress = RestBaseAddress };
        GraphQLClient client = CreateClient(httpClient);
        GitLabGraphQLRequest request = new() { Query = query };

        ArgumentException exception = await Assert.ThrowsAsync<ArgumentException>(async () =>
            await client.ExecuteAsync(request, ResponseTypeInfo, TestContext.Current.CancellationToken)
                .ConfigureAwait(false));

        Assert.Equal("Query", exception.ParamName);
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

    private static GitLabGraphQLRequest CreateProjectRequest()
    {
        return GitLabGraphQLRequest.Create(
            ProjectDocument,
            new GraphQLClientTestVariables { FullPath = "group/client-sdk" },
            GraphQLClientTestJsonContext.Default.GraphQLClientTestVariables);
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