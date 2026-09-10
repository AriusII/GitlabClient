using GitLab.Client.Batching;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Creates bounded, client-side asynchronous plans that compose independently typed GitLab REST and GraphQL
///     calls.
/// </summary>
/// <remarks>
///     This facade coordinates already typed SDK operations; it does not add a generic GitLab REST batch endpoint,
///     merge HTTP requests, cache DTOs, or change the transport's permissive rate-limit observation. Use the typed
///     GitLab endpoint-specific batch operations when GitLab exposes one, then use a plan to fan out independent
///     calls and pass their results to <see cref="IMappersClient" /> explicitly.
/// </remarks>
public interface IBatchesClient
{
    /// <summary>Creates a single-use plan of deferred operations with bounded asynchronous concurrency.</summary>
    /// <param name="options">The per-plan concurrency and failure policy, or <see langword="null" /> for defaults.</param>
    /// <returns>An empty plan to which REST and GraphQL operations can be added before it is executed once.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     <paramref name="options" /> specifies an unsupported concurrency or failure-mode value.
    /// </exception>
    GitLabBatchPlan Create(GitLabBatchOptions? options = null);
}