namespace GitLab.Client.Models;

/// <summary>
///     The envelope MLflow wraps a page of experiments in - the response of <c>experiments/list</c> and
///     <c>experiments/search</c>.
/// </summary>
/// <remarks>
///     These two endpoints are paginated by MLflow's own <c>page_token</c> body field rather than by
///     GitLab's <c>Link</c> header, so they cannot be streamed through <c>GetPagedAsync</c> and are
///     returned one envelope at a time.
/// </remarks>
public sealed record GitLabMlflowExperimentList
{
    /// <summary>The experiments in this page.</summary>
    public IReadOnlyList<GitLabMlflowExperiment>? Experiments { get; init; }
}