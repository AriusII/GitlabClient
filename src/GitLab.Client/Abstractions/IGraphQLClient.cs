using System.Text.Json.Serialization.Metadata;

using GitLab.Client.GraphQL.Protocol;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Executes explicitly typed GitLab GraphQL documents against <c>/api/graphql</c>.
/// </summary>
/// <remarks>
///     GraphQL is distinct from the versioned REST v4 API: it has its own versionless schema and uses a
///     document to select the response shape. Supply source-generated metadata for the exact closed
///     <see cref="GitLabGraphQLResponse{TData}" /> consumed by the document; this keeps the escape hatch
///     Native-AOT and trimming safe without reflection-based JSON serialization.
///     <para>
///         A successful HTTP response can contain both partial <c>data</c> and top-level GraphQL
///         <c>errors</c>. Those errors are deliberately returned in the response envelope rather than
///         converted to a REST transport exception. Non-success HTTP responses still use the library's
///         normal typed transport exceptions.
///     </para>
/// </remarks>
public interface IGraphQLClient
{
    /// <summary>
    ///     Typed GraphQL operations for GitLab Work Items. These operations retain the GraphQL response envelope so
    ///     callers can inspect both partial data and GraphQL errors.
    /// </summary>
    IGraphQLWorkItemsClient WorkItems { get; }

    /// <summary>Executes one GraphQL query or mutation document.</summary>
    /// <typeparam name="TData">The data shape selected by the GraphQL document.</typeparam>
    /// <param name="request">The GraphQL document, optional operation name, and optional variables.</param>
    /// <param name="responseTypeInfo">
    ///     Source-generated metadata for the exact closed response envelope selected by the document.
    /// </param>
    /// <param name="cancellationToken">Cancels the HTTP operation.</param>
    /// <returns>The GraphQL response envelope, including any partial data and top-level errors.</returns>
    Task<GitLabGraphQLResponse<TData>> ExecuteAsync<TData>(
        GitLabGraphQLRequest request,
        JsonTypeInfo<GitLabGraphQLResponse<TData>> responseTypeInfo,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Executes multiple GraphQL documents in one GitLab GraphQL multiplex request.
    /// </summary>
    /// <typeparam name="TData">The common data shape selected by every GraphQL document in the batch.</typeparam>
    /// <param name="requests">The ordered GraphQL documents to multiplex. The collection must not be empty.</param>
    /// <param name="responseTypeInfo">
    ///     Source-generated metadata for the exact array of response envelopes selected by the documents.
    /// </param>
    /// <param name="cancellationToken">Cancels the single HTTP operation.</param>
    /// <returns>
    ///     The ordered response envelopes returned by GitLab. Each envelope independently preserves partial data and
    ///     top-level GraphQL errors.
    /// </returns>
    /// <remarks>
    ///     This is GitLab GraphQL multiplexing: one JSON array is posted to <c>/api/graphql</c>. It is distinct from
    ///     client-side parallelism and is best suited to documents with the same selected <typeparamref name="TData" />
    ///     shape.
    /// </remarks>
    Task<GitLabGraphQLResponse<TData>[]> ExecuteBatchAsync<TData>(
        IReadOnlyList<GitLabGraphQLRequest> requests,
        JsonTypeInfo<GitLabGraphQLResponse<TData>[]> responseTypeInfo,
        CancellationToken cancellationToken = default);
}