namespace GitLab.Client.Models.Responses;

/// <summary>
///     What <c>runs/update</c> answers with. MLflow names the member <c>run_info</c> here rather than
///     <c>info</c> as it does on a full run, which is why this is its own type and not a
///     <see cref="GitLabMlflowRun" />.
/// </summary>
public sealed record GitLabMlflowUpdateRunResponse
{
    /// <summary>The run's identity, timings and status after the update.</summary>
    public GitLabMlflowRunInfo? RunInfo { get; init; }
}