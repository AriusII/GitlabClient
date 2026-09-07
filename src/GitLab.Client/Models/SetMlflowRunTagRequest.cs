namespace GitLab.Client.Models;

/// <summary>Body of <c>POST /projects/:id/ml/mlflow/api/2.0/mlflow/runs/set-tag</c>.</summary>
public sealed record SetMlflowRunTagRequest
{
    /// <summary>The candidate's UUID.</summary>
    public required string RunId { get; init; }

    /// <summary>The tag name.</summary>
    public required string Key { get; init; }

    /// <summary>The tag value.</summary>
    public required string Value { get; init; }
}