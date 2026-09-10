using System.Text.Json.Serialization.Metadata;

using GitLab.Client.Abstractions;
using GitLab.Client.GraphQL.Protocol;

using GitLabGraphQLJsonContext = GitLab.Client.GraphQL.Serialization.GitLabGraphQLJsonContext;

namespace GitLab.Client.Endpoints;

/// <summary>
///     Direct GraphQL endpoint client. Document-specific response metadata remains caller-supplied while the
///     protocol envelope itself always uses the library's dedicated source-generated context.
/// </summary>
internal sealed partial class GraphQLClient(IGitLabGraphQLConnection connection)
    : IGraphQLClient, IGraphQLWorkItemsClient
{
    public IGraphQLWorkItemsClient WorkItems => this;

    public Task<GitLabGraphQLResponse<TData>> ExecuteAsync<TData>(
        GitLabGraphQLRequest request,
        JsonTypeInfo<GitLabGraphQLResponse<TData>> responseTypeInfo,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(responseTypeInfo);
        ValidateRequest(request);

        // A GraphQL HTTP 200 can hold both partial data and top-level errors. The connection deliberately
        // only maps non-success HTTP responses to transport exceptions, so this envelope is returned intact.
        return connection.ExecuteAsync(
            request,
            GitLabGraphQLJsonContext.Default.GitLabGraphQLRequest,
            responseTypeInfo,
            cancellationToken);
    }

    public Task<GitLabGraphQLResponse<TData>[]> ExecuteBatchAsync<TData>(
        IReadOnlyList<GitLabGraphQLRequest> requests,
        JsonTypeInfo<GitLabGraphQLResponse<TData>[]> responseTypeInfo,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(requests);
        ArgumentNullException.ThrowIfNull(responseTypeInfo);

        if (requests.Count == 0)
        {
            throw new ArgumentException("At least one GraphQL request is required.", nameof(requests));
        }

        // The GraphQL endpoint accepts a JSON array for server-side multiplexing. Copy into the concrete array
        // registered in the source-generated context; serializing IReadOnlyList<T> would need a separate runtime
        // contract and weaken the AOT guarantee.
        GitLabGraphQLRequest[] multiplexedRequests = new GitLabGraphQLRequest[requests.Count];
        for (int index = 0; index < multiplexedRequests.Length; index++)
        {
            GitLabGraphQLRequest request = requests[index];
            ArgumentNullException.ThrowIfNull(request);
            ValidateRequest(request);
            multiplexedRequests[index] = request;
        }

        // GitLab preserves multiplex response order. Do not flatten envelopes: a response may carry both partial
        // data and top-level GraphQL errors and consumers need that information per submitted document.
        return connection.ExecuteAsync(
            multiplexedRequests,
            GitLabGraphQLJsonContext.Default.GitLabGraphQLRequestArray,
            responseTypeInfo,
            cancellationToken);
    }

    private static void ValidateRequest(GitLabGraphQLRequest request)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Query, nameof(GitLabGraphQLRequest.Query));
    }
}