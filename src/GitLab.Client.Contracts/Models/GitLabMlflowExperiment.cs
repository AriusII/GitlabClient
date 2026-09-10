namespace GitLab.Client.Models;

/// <summary>
///     An MLflow experiment. GitLab stores these as project-scoped ML experiments, so the identifiers
///     below are relative to the project, not global to the instance.
/// </summary>
public sealed record GitLabMlflowExperiment
{
    /// <summary>
    ///     The experiment ID, relative to the project. MLflow types this as a string even though GitLab's
    ///     underlying value is numeric - <c>POST .../runs/create</c> is the one place it is sent as an integer.
    /// </summary>
    public required string ExperimentId { get; init; }

    /// <summary>The experiment name, unique within the project.</summary>
    public string? Name { get; init; }

    /// <summary>MLflow's soft-delete marker - <c>active</c> or <c>deleted</c>. A bare string in the spec.</summary>
    public string? LifecycleStage { get; init; }

    /// <summary>Where MLflow believes the experiment's artifacts live.</summary>
    public string? ArtifactLocation { get; init; }

    /// <summary>Tags stored against the experiment.</summary>
    public IReadOnlyList<GitLabMlflowKeyValue>? Tags { get; init; }
}