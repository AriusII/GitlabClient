using GitLab.Client.Abstractions;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;

using GitLabJsonContext = GitLab.Client.Serialization.GitLabJsonContext;

namespace GitLab.Client.Endpoints;

internal sealed class SuggestionsClient(IGitLabApiConnection connection) : ISuggestionsClient
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