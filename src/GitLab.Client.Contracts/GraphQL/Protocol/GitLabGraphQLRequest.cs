using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace GitLab.Client.GraphQL.Protocol;

/// <summary>
///     The JSON envelope accepted by GitLab's GraphQL endpoint.
/// </summary>
/// <remarks>
///     This contract deliberately holds variables as a <see cref="JsonElement" />. The generic
///     <see cref="Create{TVariables}(string, TVariables, JsonTypeInfo{TVariables}, string?)" /> factory
///     converts a caller-defined variables contract with supplied source-generated metadata, so the shared
///     transport never needs reflection-based serialization.
/// </remarks>
public sealed record GitLabGraphQLRequest
{
    /// <summary>The GraphQL document to execute.</summary>
    [JsonPropertyName("query")]
    public required string Query { get; init; }

    /// <summary>
    ///     The optional variables object. Its property names and shape are defined by the operation-specific
    ///     variables contract.
    /// </summary>
    [JsonPropertyName("variables")]
    public JsonElement? Variables { get; init; }

    /// <summary>
    ///     The optional operation name used when <see cref="Query" /> defines more than one operation.
    /// </summary>
    [JsonPropertyName("operationName")]
    public string? OperationName { get; init; }

    /// <summary>
    ///     Creates a GraphQL request while serializing <paramref name="variables" /> with explicitly supplied
    ///     source-generated metadata.
    /// </summary>
    /// <typeparam name="TVariables">The operation-specific GraphQL variables contract.</typeparam>
    /// <param name="query">The non-empty GraphQL document to execute.</param>
    /// <param name="variables">The variables object to include in the request envelope.</param>
    /// <param name="variablesTypeInfo">
    ///     Source-generated metadata for <typeparamref name="TVariables" /> from a caller-owned
    ///     <see cref="JsonSerializerContext" />.
    /// </param>
    /// <param name="operationName">The optional operation name.</param>
    /// <returns>A request envelope ready for source-generated JSON serialization.</returns>
    /// <exception cref="ArgumentException"><paramref name="query" /> is null, empty, or whitespace.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="variablesTypeInfo" /> is <see langword="null" />.</exception>
    public static GitLabGraphQLRequest Create<TVariables>(
        string query,
        TVariables variables,
        JsonTypeInfo<TVariables> variablesTypeInfo,
        string? operationName = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(query);
        ArgumentNullException.ThrowIfNull(variablesTypeInfo);

        return new GitLabGraphQLRequest
        {
            Query = query,
            Variables = JsonSerializer.SerializeToElement(variables, variablesTypeInfo),
            OperationName = string.IsNullOrWhiteSpace(operationName) ? null : operationName
        };
    }
}