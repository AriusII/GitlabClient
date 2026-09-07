using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for Experiments, sitting between the public
///     <c>IExperimentsClient</c> controller and <c>IExperimentsRepository</c>'s raw GitLab access.
///     Mirrors the repository's method shapes 1:1 today (its implementation is generated); this is the
///     seam where request validation, caching, or cross-resource composition would go once this
///     resource needs more than pass-through.
/// </summary>
internal interface IExperimentsService
{
    IAsyncEnumerable<GitLabExperiment> ListAsync(CancellationToken cancellationToken = default);

    Task<GitLabExperimentAssignment> GetAssignmentAsync(string experimentName,
        IReadOnlyDictionary<string, string>? context = null, CancellationToken cancellationToken = default);

    Task<GitLabExperimentAssignment> ForceAssignmentAsync(string experimentName,
        ForceExperimentAssignmentRequest request, CancellationToken cancellationToken = default);

    Task ClearAssignmentAsync(string experimentName, IReadOnlyDictionary<string, string>? context = null,
        CancellationToken cancellationToken = default);

    Task DeleteCacheAsync(string experimentName, CancellationToken cancellationToken = default);
}