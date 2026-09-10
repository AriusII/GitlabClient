using System.Text.Json.Serialization.Metadata;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Internal GraphQL transport boundary. It deliberately exposes only a source-generated JSON operation:
///     GraphQL documents do not use REST routes, query projection, or RFC 5988 pagination.
/// </summary>
internal interface IGitLabGraphQLConnection
{
    /// <summary>
    ///     Posts one GraphQL query or mutation through GitLab's shared authenticated HTTP pipeline. HTTP
    ///     failures retain the normal GitLab exception hierarchy; a successful GraphQL response with top-level
    ///     errors is deserialized into <typeparamref name="TResponse" /> for the caller to inspect.
    /// </summary>
    Task<TResponse> ExecuteAsync<TRequest, TResponse>(
        TRequest request,
        JsonTypeInfo<TRequest> requestTypeInfo,
        JsonTypeInfo<TResponse> responseTypeInfo,
        CancellationToken cancellationToken = default);
}