namespace GitLab.Client.Models;

/// <summary>
///     A registered model in the GitLab model registry
///     (<c>/projects/:id/ml/mlflow/api/2.0/mlflow/registered-models</c>), in MLflow's
///     <c>RegisteredModel</c> shape.
/// </summary>
public sealed record GitLabMlModel
{
    /// <summary>
    ///     The model's name, unique within the project. This is the identifier every other operation on
    ///     this resource takes, and it is caller-chosen free text that may contain dots and slashes.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>When the model was created, as a Unix timestamp in milliseconds.</summary>
    public long? CreationTimestamp { get; init; }

    /// <summary>When the model was last modified, as a Unix timestamp in milliseconds.</summary>
    public long? LastUpdatedTimestamp { get; init; }

    /// <summary>The model's description, as free text.</summary>
    public string? Description { get; init; }

    /// <summary>The MLflow user identifier, which GitLab reports as a string rather than a numeric id.</summary>
    public string? UserId { get; init; }

    /// <summary>User-defined key/value metadata attached to the model.</summary>
    public IReadOnlyList<GitLabMlModelTag>? Tags { get; init; }

    /// <summary>The most recent versions of this model, when GitLab embeds them in the response.</summary>
    public IReadOnlyList<GitLabMlModelVersion>? LatestVersions { get; init; }
}