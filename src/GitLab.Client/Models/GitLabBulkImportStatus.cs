using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     The lifecycle of a direct-transfer migration and of each of its entities
///     (<c>/bulk_imports</c>, <c>/bulk_imports/:id/entities</c>).
/// </summary>
/// <remarks>
///     <see cref="Canceled" /> is deliberately present even though the spec's response schema omits it:
///     <c>POST /bulk_imports/:id/cancel</c> exists and puts a migration into exactly that state, and a
///     response enum that could not name it would turn a successful cancel into a deserialization
///     failure. It is also the sixth value the <c>status</c> query filter accepts.
/// </remarks>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabBulkImportStatus>))]
public enum GitLabBulkImportStatus
{
    [JsonStringEnumMemberName("created")] Created,

    [JsonStringEnumMemberName("started")] Started,

    [JsonStringEnumMemberName("finished")] Finished,

    [JsonStringEnumMemberName("timeout")] Timeout,

    [JsonStringEnumMemberName("failed")] Failed,

    [JsonStringEnumMemberName("canceled")] Canceled
}