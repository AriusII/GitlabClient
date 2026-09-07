using System.Text.Json;

using GitLab.Client.Abstractions;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the Internal resource - a home for spec operations that carry no
///     OpenAPI tag and share no natural home with any other resource in this scope. Builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" />
///     and calls <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this
///     layer should build a route or touch <see cref="IGitLabApiConnection" /> directly.
/// </summary>
[GenerateClientLayers(typeof(IInternalService), typeof(IInternalClient))]
internal interface IInternalRepository
{
    Task<JsonElement> ListGitalyObjectPoolMembersAsync(IReadOnlyList<string> diskPaths, string storage,
        bool? upstreamOnly = null, CancellationToken cancellationToken = default);

    Task<JsonElement> GetSwaggerDocumentationAsync(CancellationToken cancellationToken = default);

    Task<JsonElement> GetSwaggerDocumentationAsync(string name, string? locale = null,
        CancellationToken cancellationToken = default);
}