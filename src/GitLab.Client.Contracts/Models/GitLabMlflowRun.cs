namespace GitLab.Client.Models;

/// <summary>
///     An MLflow run, which GitLab implements as a <em>candidate</em> of an ML experiment. Split into
///     <see cref="Info" /> (identity and lifecycle) and <see cref="Data" /> (what the client logged),
///     exactly as MLflow's own <c>Run</c> message is.
/// </summary>
public sealed record GitLabMlflowRun
{
    /// <summary>The run's identity, timings and status.</summary>
    public GitLabMlflowRunInfo? Info { get; init; }

    /// <summary>The metrics, parameters and tags logged against the run.</summary>
    public GitLabMlflowRunData? Data { get; init; }
}