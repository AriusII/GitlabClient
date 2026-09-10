using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>What triggered a Duo flow session (<c>source</c> on <c>POST /ai/duo_workflows/workflows</c>).</summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabDuoWorkflowSource>))]
public enum GitLabDuoWorkflowSource
{
    /// <summary>A merge request with conflicting changes.</summary>
    [JsonStringEnumMemberName("merge_request_code_conflict")]
    MergeRequestCodeConflict,

    /// <summary>A dependency bump merge request.</summary>
    [JsonStringEnumMemberName("merge_request_dependency_bump")]
    MergeRequestDependencyBump,

    /// <summary>A failing pipeline on a merge request.</summary>
    [JsonStringEnumMemberName("merge_request_fix_pipeline")]
    MergeRequestFixPipeline,

    /// <summary>An unresolved discussion on a merge request.</summary>
    [JsonStringEnumMemberName("merge_request_resolve_discussion")]
    MergeRequestResolveDiscussion,

    /// <summary>A work item being turned into a merge request.</summary>
    [JsonStringEnumMemberName("work_item_to_merge_request")]
    WorkItemToMergeRequest,

    /// <summary>A failing pipeline outside a merge request.</summary>
    [JsonStringEnumMemberName("fix_pipeline")]
    FixPipeline,

    /// <summary>A CI pipeline being converted from another platform.</summary>
    [JsonStringEnumMemberName("convert_platform_ci_pipeline")]
    ConvertPlatformCiPipeline
}