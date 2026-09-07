using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>How far along one batch of a batched project relations export is.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabProjectRelationExportBatchState>))]
public enum GitLabProjectRelationExportBatchState
{
    /// <summary>GitLab is writing this batch.</summary>
    [JsonStringEnumMemberName("started")] Started,

    /// <summary>The batch can be downloaded by its <see cref="GitLabProjectRelationExportBatch.BatchNumber" />.</summary>
    [JsonStringEnumMemberName("finished")] Finished,

    /// <summary>The batch failed; see <see cref="GitLabProjectRelationExportBatch.Error" />.</summary>
    [JsonStringEnumMemberName("failed")] Failed
}