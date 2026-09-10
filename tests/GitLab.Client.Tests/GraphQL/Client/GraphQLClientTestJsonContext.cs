using System.Text.Json.Serialization;

using GitLab.Client.GraphQL.Protocol;

namespace GitLab.Client.Tests.GraphQL.Client;

// This deliberately mirrors a consuming application's context: the public GraphQL escape hatch accepts the
// exact response metadata selected by its document, so it remains Native-AOT-safe without library-side dynamic
// JSON contracts.
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(GraphQLClientTestVariables))]
[JsonSerializable(typeof(GitLabGraphQLResponse<GraphQLClientTestData>))]
internal sealed partial class GraphQLClientTestJsonContext : JsonSerializerContext;

internal sealed record GraphQLClientTestVariables
{
    public required string FullPath { get; init; }
}

internal sealed record GraphQLClientTestData
{
    public GraphQLClientTestProject? Project { get; init; }
}

internal sealed record GraphQLClientTestProject
{
    public required string Name { get; init; }
}