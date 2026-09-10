using GitLab.Client.Models;
using GitLab.Client.Models.Requests;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps the GitLab "Rollouts" API area (<c>/rollouts/:id</c>): the webhook a CD orchestrator (a
///     Starlark workflow run by AutoFlow) calls back into to report flow-graph progress for a CD
///     rollout.
/// </summary>
public interface IRolloutsClient
{
    /// <summary>
    ///     Ingests one flow-graph progress event for the rollout <paramref name="id" />
    ///     (<c>POST /rollouts/:id</c>) and returns the rollout as it now stands.
    /// </summary>
    Task<GitLabCdRollout> IngestEventAsync(long id, IngestRolloutEventRequest request,
        CancellationToken cancellationToken = default);
}