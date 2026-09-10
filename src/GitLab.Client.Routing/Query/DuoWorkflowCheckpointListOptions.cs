using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Query;

/// <summary>Filters for <c>GET /ai/duo_workflows/workflows/:id/checkpoints</c>.</summary>
[GitLabQuery]
public readonly record struct DuoWorkflowCheckpointListOptions
{
    /// <summary>Return checkpoints zlib-compressed and base64-encoded rather than inline.</summary>
    public bool? AcceptCompressed { get; init; }

    /// <summary>
    ///     Return only checkpoints in this LangGraph checkpoint namespace - blank for the flow's own
    ///     top-level lineage. Left unset, GitLab returns every lineage unfiltered. At most 4096 characters.
    /// </summary>
    public string? CheckpointNs { get; init; }
}