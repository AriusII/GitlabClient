using GitLab.Client.Abstractions;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the Suggestions resource: builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" />
///     and calls <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this
///     layer should build a route or touch <see cref="IGitLabApiConnection" /> directly.
///     <para>
///         Both routes are instance-level (<c>/suggestions/...</c>) rather than project-scoped, so no
///         <see cref="Domain.ProjectId" /> appears anywhere in this resource: suggestion IDs are globally
///         unique.
///     </para>
/// </summary>
[GenerateClientLayers(typeof(ISuggestionsService), typeof(ISuggestionsClient))]
internal interface ISuggestionsRepository
{
    Task<GitLabSuggestion> ApplyAsync(long suggestionId, ApplySuggestionRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabSuggestion> ApplyBatchAsync(ApplySuggestionBatchRequest request,
        CancellationToken cancellationToken = default);
}