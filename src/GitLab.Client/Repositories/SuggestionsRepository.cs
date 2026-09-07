using GitLab.Client.Abstractions;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Infrastructure.Serialization;
using GitLab.Client.Models;

namespace GitLab.Client.Repositories;

internal sealed class SuggestionsRepository(IGitLabApiConnection connection) : ISuggestionsRepository
{
    public Task<GitLabSuggestion> ApplyAsync(long suggestionId, ApplySuggestionRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            GitLabRouteBuilder.Create("suggestions").Segment(suggestionId).Literal("apply").Build(),
            request,
            GitLabJsonContext.Default.ApplySuggestionRequest,
            GitLabJsonContext.Default.GitLabSuggestion,
            cancellationToken);
    }

    public Task<GitLabSuggestion> ApplyBatchAsync(ApplySuggestionBatchRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            GitLabRouteBuilder.Create("suggestions").Literal("batch_apply").Build(),
            request,
            GitLabJsonContext.Default.ApplySuggestionBatchRequest,
            GitLabJsonContext.Default.GitLabSuggestion,
            cancellationToken);
    }
}