namespace GitLab.Client.Models.Requests;

/// <summary>
///     The body sent with
///     <c>POST /projects/:id/ml/mlflow/api/2.0/mlflow/registered-models/get-latest-versions</c>.
/// </summary>
/// <remarks>
///     MLflow expresses this read as a <c>POST</c> carrying the model name in the body rather than as a
///     <c>GET</c> with a query parameter. The record is internal because it is an artefact of that
///     protocol quirk, not part of the public request vocabulary - the client method takes the name
///     directly.
/// </remarks>
internal sealed record MlModelLatestVersionsRequest
{
    /// <summary>The registered model's unique name, in reference to the project.</summary>
    public required string Name { get; init; }
}