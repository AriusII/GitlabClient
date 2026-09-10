using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>The status a client may move an MLflow run to with <c>POST .../mlflow/runs/update</c>.</summary>
/// <remarks>
///     This is the one place the spec enumerates the vocabulary. The matching <em>response</em> field,
///     <see cref="GitLabMlflowRunInfo.Status" />, is a bare string on purpose so that a status GitLab or
///     MLflow adds later cannot turn a healthy response into a deserialization failure.
/// </remarks>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabMlflowRunStatus>))]
public enum GitLabMlflowRunStatus
{
    /// <summary>The run is executing.</summary>
    [JsonStringEnumMemberName("RUNNING")] Running,

    /// <summary>The run is queued but has not started.</summary>
    [JsonStringEnumMemberName("SCHEDULED")]
    Scheduled,

    /// <summary>The run completed successfully.</summary>
    [JsonStringEnumMemberName("FINISHED")] Finished,

    /// <summary>The run ended in an error.</summary>
    [JsonStringEnumMemberName("FAILED")] Failed,

    /// <summary>The run was terminated by a client.</summary>
    [JsonStringEnumMemberName("KILLED")] Killed
}