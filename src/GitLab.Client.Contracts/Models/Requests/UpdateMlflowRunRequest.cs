namespace GitLab.Client.Models.Requests;

/// <summary>Body of <c>POST /projects/:id/ml/mlflow/api/2.0/mlflow/runs/update</c>.</summary>
public sealed record UpdateMlflowRunRequest
{
    /// <summary>The candidate's UUID.</summary>
    public required string RunId { get; init; }

    /// <summary>The status to move the run to. This is the one field in the area the spec enumerates.</summary>
    public GitLabMlflowRunStatus? Status { get; init; }

    /// <summary>When the run ended, as a Unix timestamp in milliseconds.</summary>
    public long? EndTime { get; init; }
}