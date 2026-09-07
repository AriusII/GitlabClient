using GitLab.Client.Models;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps the GitLab "Experiments" API area (<c>/experiments</c>) - the A/B experiments running on the
///     instance, and each caller's cached variant assignment.
///     <para>
///         GitLab documents this as an internal-use API: it answers <c>401</c>/<c>403</c> for anonymous
///         or unauthenticated callers, and every operation here requires an authenticated request.
///     </para>
/// </summary>
public interface IExperimentsClient
{
    /// <summary>Streams every experiment configured on the instance (<c>GET /experiments</c>).</summary>
    IAsyncEnumerable<GitLabExperiment> ListAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets the currently cached variant assignment for an experiment and context
    ///     (<c>GET /experiments/:experiment_name/assignments</c>).
    /// </summary>
    /// <param name="experimentName">The experiment's key, as it appears in <see cref="GitLabExperiment.Key" />.</param>
    /// <param name="context">
    ///     Context parameters identifying who/what to look the assignment up for - "user", "namespace",
    ///     "project" mapped to the relevant GitLab ID, as text. Pass every key the experiment declares in
    ///     its <see cref="GitLabExperiment.Context" /> keys; defaults to the current user when omitted.
    /// </param>
    /// <param name="cancellationToken">Cancels the request.</param>
    Task<GitLabExperimentAssignment> GetAssignmentAsync(string experimentName,
        IReadOnlyDictionary<string, string>? context = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Forces a specific variant assignment for an experiment and context
    ///     (<c>POST /experiments/:experiment_name/assignments</c>). The assignment is cached in Redis and
    ///     persists until overwritten or cleared.
    /// </summary>
    Task<GitLabExperimentAssignment> ForceAssignmentAsync(string experimentName,
        ForceExperimentAssignmentRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Clears the cached variant assignment for an experiment and context
    ///     (<c>DELETE /experiments/:experiment_name/assignments</c>), returning the actor to normal
    ///     rollout assignment.
    /// </summary>
    /// <param name="experimentName">The experiment's key.</param>
    /// <param name="context">The same context that was assigned - see <see cref="GetAssignmentAsync" />.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    Task ClearAssignmentAsync(string experimentName, IReadOnlyDictionary<string, string>? context = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Deletes every cached variant assignment for an experiment
    ///     (<c>DELETE /experiments/:name/cache</c>). Useful once an experiment's code has been removed and
    ///     its stale cache entries are no longer needed.
    /// </summary>
    Task DeleteCacheAsync(string experimentName, CancellationToken cancellationToken = default);
}