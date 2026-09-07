namespace GitLab.Client.Models;

/// <summary>Body of <c>POST /projects/:id/ml/mlflow/api/2.0/mlflow/model-versions/create</c>.</summary>
public sealed record CreateMlflowModelVersionRequest
{
    /// <summary>The registered model to add the version to. GitLab requires it despite the spec's wording.</summary>
    public required string Name { get; init; }

    /// <summary>A description for the new version.</summary>
    public string? Description { get; init; }

    /// <summary>
    ///     Tags to store against the version. The spec leaves the element type open; MLflow sends
    ///     <c>{ "key": ..., "value": ... }</c> pairs.
    /// </summary>
    public IReadOnlyList<GitLabMlflowKeyValue>? Tags { get; init; }

    /// <summary>The candidate to promote to a model version.</summary>
    public string? RunId { get; init; }
}