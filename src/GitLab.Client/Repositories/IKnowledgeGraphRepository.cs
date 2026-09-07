using GitLab.Client.Abstractions;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the Knowledge graph resource: builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" />
///     and calls <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this
///     layer should build a route or touch <see cref="IGitLabApiConnection" /> directly.
/// </summary>
[GenerateClientLayers(typeof(IKnowledgeGraphService), typeof(IKnowledgeGraphClient))]
internal interface IKnowledgeGraphRepository
{
    Task<IReadOnlyList<GitLabKnowledgeGraphNamespace>> ListNamespacesAsync(
        CancellationToken cancellationToken = default);

    Task<GitLabKnowledgeGraphNamespace> EnableNamespaceAsync(string id,
        CancellationToken cancellationToken = default);

    Task DisableNamespaceAsync(string id, CancellationToken cancellationToken = default);
}