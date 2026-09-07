using System.Text.Json;

using GitLab.Client.Abstractions;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Infrastructure.Serialization;

namespace GitLab.Client.Repositories;

internal sealed class InternalRepository(IGitLabApiConnection connection) : IInternalRepository
{
    /// <summary>
    ///     A Gitaly-internal RPC (<c>/internal/gitaly/...</c>), not a public consumer route - Gitaly calls
    ///     this back into Rails when deduplicating forks via object pools. The spec declares no response
    ///     schema, so the answer is a raw <see cref="JsonElement" /> rather than an invented shape.
    /// </summary>
    public Task<JsonElement> ListGitalyObjectPoolMembersAsync(IReadOnlyList<string> diskPaths, string storage,
        bool? upstreamOnly = null, CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("internal")
                .Literal("gitaly")
                .Literal("object_pool_members")
                .Query("disk_paths", diskPaths)
                .Query("storage", storage)
                .Query("upstream_only", upstreamOnly)
                .Build(),
            GitLabJsonContext.Default.JsonElement,
            cancellationToken);
    }

    /// <summary>The spec declares no response schema, so the answer is a raw <see cref="JsonElement" />.</summary>
    public Task<JsonElement> GetSwaggerDocumentationAsync(CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("swagger_doc").Build(),
            GitLabJsonContext.Default.JsonElement,
            cancellationToken);
    }

    /// <summary>The spec declares no response schema, so the answer is a raw <see cref="JsonElement" />.</summary>
    public Task<JsonElement> GetSwaggerDocumentationAsync(string name, string? locale = null,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("swagger_doc")
                .Escaped(name)
                .Query("locale", locale)
                .Build(),
            GitLabJsonContext.Default.JsonElement,
            cancellationToken);
    }
}