namespace GitLab.Client.Models;

/// <summary>
///     Request body for <c>PATCH /projects/:id/ml/mlflow/api/2.0/mlflow/registered-models/update</c>.
/// </summary>
/// <remarks>
///     <see cref="Name" /> identifies the model to update rather than renaming it - the endpoint exposes
///     no way to rename a registered model. <see cref="Description" /> is the only mutable field.
/// </remarks>
public sealed record UpdateMlModelRequest
{
    /// <summary>The name of the model to update, unique within the project.</summary>
    public required string Name { get; init; }

    /// <summary>The new description. Leave null to send nothing and keep the current one.</summary>
    public string? Description { get; init; }
}