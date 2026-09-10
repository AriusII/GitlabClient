using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

using GitLab.Client.GraphQL.Protocol;

using GitLabGraphQLJsonContext = GitLab.Client.GraphQL.Serialization.GitLabGraphQLJsonContext;

namespace GitLab.Client.Tests.GraphQL.Protocol;

/// <summary>
///     Pins the GraphQL wire envelope independently from the REST snake_case context. GraphQL variables and the
///     envelope itself must remain source-generated and camelCase so native-AOT callers can supply arbitrary typed
///     operation variables without a reflection serialization fallback.
/// </summary>
public sealed class GitLabGraphQLProtocolTests
{
    [Fact]
    public void Create_SerializesCamelCaseVariablesAndOperationNameThroughSourceGeneratedContexts()
    {
        GitLabGraphQLRequest request = GitLabGraphQLRequest.Create(
            "query GetWorkItem($namespaceFullPath: ID!, $workItemIid: String!) { workItem { id } }",
            CreateVariables(),
            GraphQLProtocolTestJsonContext.Default.GraphQLProtocolVariables,
            "GetWorkItem");

        string json = JsonSerializer.Serialize(request, GitLabGraphQLJsonContext.Default.GitLabGraphQLRequest);

        using JsonDocument document = JsonDocument.Parse(json);
        JsonElement envelope = document.RootElement;
        JsonElement variables = envelope.GetProperty("variables");

        Assert.Equal(
            "query GetWorkItem($namespaceFullPath: ID!, $workItemIid: String!) { workItem { id } }",
            envelope.GetProperty("query").GetString());
        Assert.Equal("GetWorkItem", envelope.GetProperty("operationName").GetString());
        Assert.Equal("group/project", variables.GetProperty("namespaceFullPath").GetString());
        Assert.Equal("42", variables.GetProperty("workItemIid").GetString());
        Assert.False(variables.TryGetProperty("namespace_full_path", out _));
        Assert.False(envelope.TryGetProperty("operation_name", out _));
    }

    [Fact]
    public void Create_NormalizesWhitespaceOperationNameAndOmitsItFromTheWireEnvelope()
    {
        GitLabGraphQLRequest request = GitLabGraphQLRequest.Create(
            "query { currentUser { id } }",
            CreateVariables(),
            GraphQLProtocolTestJsonContext.Default.GraphQLProtocolVariables,
            " \t\r\n ");

        string json = JsonSerializer.Serialize(request, GitLabGraphQLJsonContext.Default.GitLabGraphQLRequest);

        using JsonDocument document = JsonDocument.Parse(json);

        Assert.Null(request.OperationName);
        Assert.False(document.RootElement.TryGetProperty("operationName", out _));
    }

    [Fact]
    public void Create_RejectsANullQuery()
    {
        ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() =>
            GitLabGraphQLRequest.Create(
                null!,
                CreateVariables(),
                GraphQLProtocolTestJsonContext.Default.GraphQLProtocolVariables));

        Assert.Equal("query", exception.ParamName);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" \t\r\n ")]
    public void Create_RejectsAnEmptyOrWhitespaceQuery(string query)
    {
        ArgumentException exception = Assert.Throws<ArgumentException>(() =>
            GitLabGraphQLRequest.Create(
                query,
                CreateVariables(),
                GraphQLProtocolTestJsonContext.Default.GraphQLProtocolVariables));

        Assert.Equal("query", exception.ParamName);
    }

    [Fact]
    public void Create_RejectsNullSourceGeneratedVariablesMetadata()
    {
        ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() =>
            GitLabGraphQLRequest.Create(
                "query { currentUser { id } }",
                CreateVariables(),
                null!));

        Assert.Equal("variablesTypeInfo", exception.ParamName);
    }

    [Fact]
    public void Response_DeserializesPartialDataAndErrorsTogetherThroughSourceGeneratedMetadata()
    {
        const string json =
            """
            {
              "data": { "title": "Visible work item" },
              "errors": [
                {
                  "message": "The description widget is not available.",
                  "locations": [{ "line": 4, "column": 9 }],
                  "path": ["workItem", "descriptionWidget"],
                  "extensions": { "code": "RESOURCE_NOT_AVAILABLE" }
                }
              ],
              "extensions": { "requestId": "graphql-request-19" }
            }
            """;

        JsonTypeInfo<GitLabGraphQLResponse<GraphQLProtocolResponseData>> typeInfo =
            GetResponseTypeInfo();
        GitLabGraphQLResponse<GraphQLProtocolResponseData> response =
            JsonSerializer.Deserialize(json, typeInfo)
            ?? throw new InvalidOperationException(
                "The source-generated GraphQL response metadata should deserialize.");

        GitLabGraphQLError error = Assert.Single(response.Errors!);
        GitLabGraphQLErrorLocation location = Assert.Single(error.Locations!);

        Assert.Equal("Visible work item", response.Data?.Title);
        Assert.True(response.HasErrors);
        Assert.Equal("The description widget is not available.", error.Message);
        Assert.Equal(4, location.Line);
        Assert.Equal(9, location.Column);
        Assert.True(error.Path.HasValue);
        Assert.Equal("workItem", error.Path.Value[0].GetString());
        Assert.Equal("descriptionWidget", error.Path.Value[1].GetString());
        Assert.True(error.Extensions.HasValue);
        Assert.Equal("RESOURCE_NOT_AVAILABLE", error.Extensions.Value.GetProperty("code").GetString());
        Assert.True(response.Extensions.HasValue);
        Assert.Equal("graphql-request-19", response.Extensions.Value.GetProperty("requestId").GetString());
    }

    [Fact]
    public void Response_HasErrorsIsFalseWhenErrorsAreAbsentOrEmpty()
    {
        GitLabGraphQLResponse<GraphQLProtocolResponseData> absent = new()
        {
            Data = new GraphQLProtocolResponseData { Title = "Visible work item" }
        };
        GitLabGraphQLResponse<GraphQLProtocolResponseData> empty = new() { Errors = Array.Empty<GitLabGraphQLError>() };

        Assert.False(absent.HasErrors);
        Assert.False(empty.HasErrors);
    }

    private static GraphQLProtocolVariables CreateVariables()
    {
        return new GraphQLProtocolVariables { NamespaceFullPath = "group/project", WorkItemIid = "42" };
    }

    private static JsonTypeInfo<GitLabGraphQLResponse<GraphQLProtocolResponseData>> GetResponseTypeInfo()
    {
        return (JsonTypeInfo<GitLabGraphQLResponse<GraphQLProtocolResponseData>>)
            (GraphQLProtocolTestJsonContext.Default.GetTypeInfo(
                 typeof(GitLabGraphQLResponse<GraphQLProtocolResponseData>))
             ?? throw new InvalidOperationException("The test response envelope must have generated JSON metadata."));
    }
}

internal sealed record GraphQLProtocolVariables
{
    public required string NamespaceFullPath { get; init; }

    public required string WorkItemIid { get; init; }
}

internal sealed record GraphQLProtocolResponseData
{
    public required string Title { get; init; }
}

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    RespectNullableAnnotations = true)]
[JsonSerializable(typeof(GraphQLProtocolVariables))]
[JsonSerializable(typeof(GitLabGraphQLResponse<GraphQLProtocolResponseData>))]
internal sealed partial class GraphQLProtocolTestJsonContext : JsonSerializerContext;