namespace GitLab.Client.Models;

/// <summary>
///     Request body for <c>POST /projects/:id/ml/mlflow/api/2.0/mlflow/registered-models/create</c>.
/// </summary>
public sealed record CreateMlModelRequest
{
    /// <summary>The name to register the model under. Must be unique within the project.</summary>
    public required string Name { get; init; }

    /// <summary>An optional description for the registered model.</summary>
    public string? Description { get; init; }

    /// <summary>
    ///     Additional user-defined metadata for the model. The spec declares this array without an item
    ///     schema; it is typed as MLflow's key/value tag here because that is the shape GitLab echoes
    ///     back on <see cref="GitLabMlModel.Tags" />.
    /// </summary>
    public IReadOnlyList<GitLabMlModelTag>? Tags { get; init; }
}