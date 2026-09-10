using System.Text.Json;

namespace GitLab.Client.Models.Requests;

/// <summary>Body of <c>POST /ai/duo_workflows/workflows/:id/checkpoints</c>.</summary>
public sealed record CreateDuoWorkflowCheckpointRequest
{
    /// <summary>The LangGraph <c>thread_ts</c> identifying this checkpoint.</summary>
    public required string ThreadTs { get; init; }

    /// <summary>
    ///     Checkpoint metadata. The spec types it as an untyped object, so it is carried as a raw
    ///     <see cref="JsonElement" />; GitLab requires it, and it must be a JSON object.
    /// </summary>
    public required JsonElement Metadata { get; init; }

    /// <summary>The <c>thread_ts</c> of the parent checkpoint, when this one continues another.</summary>
    public string? ParentTs { get; init; }

    /// <summary>
    ///     The LangGraph checkpoint namespace this checkpoint belongs to. Omit (or leave blank) for the
    ///     flow's own top-level lineage; set it to LangGraph's namespace string for one nested subgraph
    ///     invocation. At most 4096 characters.
    /// </summary>
    public string? CheckpointNs { get; init; }

    /// <summary>
    ///     The checkpoint content, as a raw <see cref="JsonElement" /> - the spec declares no shape for it.
    ///     Send either this or <see cref="CompressedCheckpoint" />.
    /// </summary>
    public JsonElement? Checkpoint { get; init; }

    /// <summary>The checkpoint content, zlib-compressed and base64-encoded.</summary>
    public string? CompressedCheckpoint { get; init; }

    /// <summary>JSON string of the model metadata.</summary>
    public string? ModelMetadataJson { get; init; }

    /// <summary>JSON string of the flow metadata.</summary>
    public string? FlowMetadataJson { get; init; }

    /// <summary>Thread grouping hint for blob reconstruction. GitLab defaults it to 0.</summary>
    public int? CurrentThread { get; init; }

    /// <summary>Live channel membership of the checkpoint.</summary>
    public IReadOnlyList<string>? ChannelKeys { get; init; }

    /// <summary>Per-channel blobs, for the incremental checkpoint storage path.</summary>
    public IReadOnlyList<DuoWorkflowChannelBlob>? ChannelBlobs { get; init; }
}