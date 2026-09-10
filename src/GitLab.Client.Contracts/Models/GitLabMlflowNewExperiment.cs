namespace GitLab.Client.Models;

/// <summary>
///     What <c>experiments/create</c> answers with: the ID of the experiment that was created, and
///     nothing else. Fetch the rest with <c>experiments/get</c>.
/// </summary>
public sealed record GitLabMlflowNewExperiment
{
    /// <summary>The new experiment's ID, relative to the project.</summary>
    public required string ExperimentId { get; init; }
}