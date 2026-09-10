namespace GitLab.Client.Models.Responses;

/// <summary>
///     The envelope MLflow wraps a single experiment in - the response of <c>experiments/get</c> and
///     <c>experiments/get-by-name</c>.
/// </summary>
/// <remarks>
///     Kept as an envelope rather than unwrapped because it is MLflow's documented response shape, and
///     because MLflow has added sibling fields to these envelopes before.
/// </remarks>
public sealed record GitLabMlflowExperimentResponse
{
    /// <summary>The experiment, when GitLab found one.</summary>
    public GitLabMlflowExperiment? Experiment { get; init; }
}