namespace GitLab.Client.Models;

/// <summary>
///     An MLflow model version, as exposed by the MLflow-compatible half of GitLab's MLOps API
///     (<c>/projects/:id/ml/mlflow/api/2.0/mlflow/model-versions/*</c>). GitLab maps it onto a version of
///     a model in its own model registry.
/// </summary>
public sealed record GitLabMlflowModelVersion
{
    /// <summary>The registered model's name.</summary>
    public required string Name { get; init; }

    /// <summary>The version. MLflow types this as a string on the response, even where it reads numeric.</summary>
    public required string Version { get; init; }

    /// <summary>When the version was created, as a Unix timestamp in milliseconds.</summary>
    public long? CreationTimestamp { get; init; }

    /// <summary>When the version was last changed, as a Unix timestamp in milliseconds.</summary>
    public long? LastUpdatedTimestamp { get; init; }

    /// <summary>The MLflow user ID. GitLab does not populate this from its own user model.</summary>
    public string? UserId { get; init; }

    /// <summary>MLflow's stage vocabulary - <c>development</c> and friends. Unenumerated in the spec.</summary>
    public string? CurrentStage { get; init; }

    /// <summary>The version's description.</summary>
    public string? Description { get; init; }

    /// <summary>Where the model artifacts came from.</summary>
    public string? Source { get; init; }

    /// <summary>The candidate this version was promoted from, when it was promoted from one.</summary>
    public string? RunId { get; init; }

    /// <summary>
    ///     MLflow's readiness vocabulary - <c>READY</c> and friends. Kept a bare string because the spec
    ///     leaves it unenumerated.
    /// </summary>
    public string? Status { get; init; }

    /// <summary>Detail accompanying <see cref="Status" />.</summary>
    public string? StatusMessage { get; init; }

    /// <summary>Tags stored against the version.</summary>
    public IReadOnlyList<GitLabMlflowKeyValue>? Tags { get; init; }

    /// <summary>MLflow's link back to the run that produced the version.</summary>
    public string? RunLink { get; init; }

    /// <summary>The aliases pointing at this version.</summary>
    public IReadOnlyList<string>? Aliases { get; init; }
}