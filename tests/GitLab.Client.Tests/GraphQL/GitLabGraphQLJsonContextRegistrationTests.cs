using GitLab.Client.Abstractions;
using GitLab.Client.GraphQL.Protocol;
using GitLab.Client.GraphQL.WorkItems;
using GitLab.Client.GraphQL.WorkItems.Identifiers;
using GitLab.Client.GraphQL.WorkItems.Models;

using GitLabGraphQLJsonContext = GitLab.Client.GraphQL.Serialization.GitLabGraphQLJsonContext;

namespace GitLab.Client.Tests.GraphQL;

/// <summary>
///     Ensures that every GraphQL contract carried by the curated SDK surface has metadata from the
///     library's source-generated context. This is the GraphQL counterpart to the REST registration guard.
/// </summary>
/// <remarks>
///     The generic document executor deliberately accepts caller-owned metadata for arbitrary documents;
///     it is therefore outside this curated-context guard. The Work Items methods on
///     <see cref="IGraphQLWorkItemsClient" /> are the closed SDK surface and every response envelope they
///     return must be registered here.
/// </remarks>
public sealed class GitLabGraphQLJsonContextRegistrationTests
{
    private const string GraphQLNamespacePrefix = "GitLab.Client.GraphQL";

    // These types are public SDK inputs or client-side controls, but are not JSON payload contracts:
    //
    // - Locator is converted to the serializable GitLabWorkItemByLocatorQueryVariables by its constructor.
    // - WidgetProfile selects a static GraphQL document in GraphQLClient; it is never placed into variables.
    // - RelativePosition is emitted/read manually by the hierarchy update converter, whose properties are ignored.
    //
    // Keeping the reason alongside each type makes a new exclusion reviewable instead of silently weakening
    // this AOT guard.
    private static readonly Dictionary<Type, string> NonPayloadContractExclusions =
        new()
        {
            [typeof(GitLabWorkItemLocator)] =
                "Converted into GitLabWorkItemByLocatorQueryVariables before JSON serialization.",
            [typeof(GitLabWorkItemWidgetProfile)] =
                "A client-side selection profile, not a GraphQL variable or response member.",
            [typeof(GitLabWorkItemRelativePosition)] =
                "Serialized manually by GitLabWorkItemHierarchyUpdateWidgetInputJsonConverter."
        };

    [Fact]
    public void EveryCuratedGraphQLPayloadContract_HasJsonMetadata()
    {
        List<Type> missing = GetCuratedGraphQLPayloadContractTypes()
            .Where(static type => GitLabGraphQLJsonContext.Default.GetTypeInfo(type) is null)
            .ToList();

        Assert.True(missing.Count == 0, BuildMissingMessage("GraphQL payload contract", missing));
    }

    [Fact]
    public void EveryCuratedGraphQLOperationResponseEnvelope_HasJsonMetadata()
    {
        List<Type> missing = GetCuratedResponseEnvelopeTypes()
            .Where(static type => GitLabGraphQLJsonContext.Default.GetTypeInfo(type) is null)
            .ToList();

        Assert.True(missing.Count == 0, BuildMissingMessage("curated GraphQL response envelope", missing));
    }

    private static List<Type> GetCuratedGraphQLPayloadContractTypes()
    {
        return typeof(GitLabGraphQLRequest).Assembly
            .GetExportedTypes()
            .Where(static type => type.Namespace is { } currentNamespace &&
                                  (string.Equals(currentNamespace, GraphQLNamespacePrefix,
                                       StringComparison.Ordinal) ||
                                   currentNamespace.StartsWith(GraphQLNamespacePrefix + ".",
                                       StringComparison.Ordinal)))
            // GitLabGraphQLResponse<TData> is intentionally an open protocol envelope. The closed
            // envelopes actually exposed by the curated client surface are checked separately below.
            .Where(static type => !type.ContainsGenericParameters)
            .Where(static type => type.IsClass || type.IsEnum)
            .Where(static type => !NonPayloadContractExclusions.ContainsKey(type))
            .OrderBy(static type => type.FullName, StringComparer.Ordinal)
            .ToList();
    }

    private static List<Type> GetCuratedResponseEnvelopeTypes()
    {
        return typeof(IGraphQLWorkItemsClient).GetMethods()
            .Select(static method => method.ReturnType)
            .Where(static returnType => returnType.IsGenericType &&
                                        returnType.GetGenericTypeDefinition() == typeof(Task<>))
            .Select(static returnType => returnType.GetGenericArguments()[0])
            .Where(static resultType => resultType.IsGenericType &&
                                        resultType.GetGenericTypeDefinition() == typeof(GitLabGraphQLResponse<>))
            .Distinct()
            .OrderBy(static type => type.FullName, StringComparer.Ordinal)
            .ToList();
    }

    private static string BuildMissingMessage(string contractKind, List<Type> missing)
    {
        string exclusions = string.Join(
            Environment.NewLine,
            NonPayloadContractExclusions.OrderBy(static entry => entry.Key.FullName, StringComparer.Ordinal)
                .Select(static entry => $"- {entry.Key.FullName}: {entry.Value}"));

        return $"""
                {missing.Count} {contractKind}(s) have no metadata in GitLabGraphQLJsonContext:
                {string.Join(Environment.NewLine, missing.Select(static type => "- " + type.FullName))}

                Explicit non-payload exclusions:
                {exclusions}
                """;
    }
}