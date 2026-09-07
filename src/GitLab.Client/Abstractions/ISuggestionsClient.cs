using GitLab.Client.Models;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps the GitLab "Suggestions" API area (<c>/suggestions</c>) - applying the code suggestions that
///     appear inside merge request review comments.
///     <para>
///         Both routes are instance-level rather than project-scoped, because suggestion IDs are globally
///         unique. Applying a suggestion requires the Developer, Maintainer or Owner role; a lower role
///         surfaces as <c>GitLabForbiddenException</c>.
///     </para>
///     <para>
///         GitLab exposes suggestion IDs through the <c>suggestions</c> array on a discussion note, which
///         <see cref="GitLab.Client.Models.GitLabNote" /> does not model yet - so an ID has to come from
///         elsewhere (a webhook payload, or the merge request UI) until it does.
///     </para>
/// </summary>
public interface ISuggestionsClient
{
    /// <summary>Applies one suggestion, committing the change to the merge request's source branch.</summary>
    Task<GitLabSuggestion> ApplyAsync(long suggestionId, ApplySuggestionRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Applies several suggestions in a single commit. GitLab's OpenAPI document declares the response as
    ///     one suggestion object rather than an array, and this signature mirrors that declaration.
    /// </summary>
    Task<GitLabSuggestion> ApplyBatchAsync(ApplySuggestionBatchRequest request,
        CancellationToken cancellationToken = default);
}