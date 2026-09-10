using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     The lifecycle of one batch of a batched group relations export. Narrower than
///     <see cref="GitLabGroupRelationExportState" />: a batch only exists once the export has started, so
///     there is no <c>pending</c> member.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabGroupRelationExportBatchState>))]
public enum GitLabGroupRelationExportBatchState
{
    [JsonStringEnumMemberName("started")] Started,

    [JsonStringEnumMemberName("finished")] Finished,

    [JsonStringEnumMemberName("failed")] Failed
}