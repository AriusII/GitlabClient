using System.Text.Json.Serialization.Metadata;

using GitLab.Client.Abstractions;
using GitLab.Client.Configuration;

using Microsoft.Extensions.Options;

namespace GitLab.Client.Infrastructure.GraphQL;

/// <summary>
///     GraphQL's thin transport adapter over the shared GitLab HTTP connection. It intentionally delegates
///     to the regular POST path, preserving authentication, rate-limit observation, timeout handling and the
///     existing no-write-retry policy without introducing a second <see cref="HttpClient" /> pipeline.
/// </summary>
internal sealed class GitLabGraphQLConnection : IGitLabGraphQLConnection
{
    private readonly IGitLabApiConnection _connection;
    private readonly IOptionsMonitor<GitLabClientOptions> _options;

    public GitLabGraphQLConnection(IGitLabApiConnection connection, IOptionsMonitor<GitLabClientOptions> options)
    {
        ArgumentNullException.ThrowIfNull(connection);
        ArgumentNullException.ThrowIfNull(options);

        _connection = connection;
        _options = options;
    }

    public Task<TResponse> ExecuteAsync<TRequest, TResponse>(
        TRequest request,
        JsonTypeInfo<TRequest> requestTypeInfo,
        JsonTypeInfo<TResponse> responseTypeInfo,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(requestTypeInfo);
        ArgumentNullException.ThrowIfNull(responseTypeInfo);

        // Read CurrentValue for every operation so configuration reloads update an explicit GraphQL endpoint
        // in the same way the named HttpClient updates its REST BaseAddress and timeout.
        Uri endpoint = GitLabGraphQLEndpoint.Resolve(_options.CurrentValue);

        // POST is deliberately used once. GitLabRetryHandler retries only safe HTTP methods, so a mutation is
        // never automatically replayed after an ambiguous transient response.
        return _connection.PostAsync(endpoint, request, requestTypeInfo, responseTypeInfo, cancellationToken);
    }
}