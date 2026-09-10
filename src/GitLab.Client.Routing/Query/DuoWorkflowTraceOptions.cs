using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Query;

/// <summary>Filters for <c>GET /ai/duo_workflows/workflows/:workflow_id/trace.jsonl</c>.</summary>
[GitLabQuery]
public readonly record struct DuoWorkflowTraceOptions
{
    /// <summary>
    ///     Include internal channels such as conversation history and handover. Restricted to the flow's
    ///     owner - anyone else gets a <c>403</c>.
    /// </summary>
    public bool? Full { get; init; }

    /// <summary>
    ///     Which thread to return: omit for the full cross-thread trace, <c>latest</c> for the newest
    ///     thread only, or a <c>current_thread</c> id for that thread. Only applies on the blob-read path.
    /// </summary>
    public string? Thread { get; init; }
}