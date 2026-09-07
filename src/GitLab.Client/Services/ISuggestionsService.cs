using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for Suggestions, sitting between the public <c>ISuggestionsClient</c>
///     controller and <c>ISuggestionsRepository</c>'s raw GitLab access. Mirrors the repository's method
///     shapes 1:1 today (its implementation is generated); this is the seam where request validation,
///     caching, or cross-resource composition would go once the resource needs more than pass-through.
/// </summary>
internal interface ISuggestionsService
{
    Task<GitLabSuggestion> ApplyAsync(long suggestionId, ApplySuggestionRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabSuggestion> ApplyBatchAsync(ApplySuggestionBatchRequest request,
        CancellationToken cancellationToken = default);
}