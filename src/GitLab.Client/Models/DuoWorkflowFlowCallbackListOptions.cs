using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Models;

/// <summary>Paging for <c>GET /ai/duo_workflows/flow_callbacks</c>.</summary>
[GitLabQuery]
public sealed record DuoWorkflowFlowCallbackListOptions
{
    public int? Page { get; init; }

    public int? PerPage { get; init; }
}