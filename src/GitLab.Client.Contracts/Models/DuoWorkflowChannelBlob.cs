namespace GitLab.Client.Models;

/// <summary>
///     One per-channel blob of a checkpoint, for the incremental checkpoint storage path of
///     <c>POST /ai/duo_workflows/workflows/:id/checkpoints</c>.
/// </summary>
public sealed record DuoWorkflowChannelBlob
{
    /// <summary>The channel name. At most 255 characters.</summary>
    public required string Channel { get; init; }

    /// <summary>The channel version. At most 255 characters.</summary>
    public required string Version { get; init; }

    /// <summary>
    ///     Blob serialization type. GitLab's read path decodes zlib-compressed JSON, so producers must
    ///     send <c>json</c>.
    /// </summary>
    public required string WriteType { get; init; }

    /// <summary>Whether this blob appends to the channel or replaces it.</summary>
    public required GitLabDuoWorkflowChannelBlobStepAction StepAction { get; init; }

    /// <summary>The blob bytes, base64-encoded. GitLab caps this at 1398104 characters.</summary>
    public required string Data { get; init; }
}