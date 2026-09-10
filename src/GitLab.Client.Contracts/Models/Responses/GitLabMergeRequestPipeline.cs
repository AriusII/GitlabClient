using System.Text.Json;

namespace GitLab.Client.Models.Responses;

/// <summary>
///     A pipeline embedded in a merge request. GitLab uses the basic pipeline shape for <c>pipeline</c>
///     and the richer shape for <c>head_pipeline</c>; nullable richer members make one DTO accurately
///     represent both without coupling the merge request response to the Pipelines resource contract.
/// </summary>
public sealed record GitLabMergeRequestPipeline
{
    public long? Id { get; init; }

    public long? Iid { get; init; }

    public long? ProjectId { get; init; }

    public string? Sha { get; init; }

    public string? Ref { get; init; }

    /// <summary>Free-form pipeline state, for example <c>success</c>, <c>pending</c> or <c>failed</c>.</summary>
    public string? Status { get; init; }

    public string? Source { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }

    public DateTimeOffset? UpdatedAt { get; init; }

    public Uri? WebUrl { get; init; }

    /// <summary>The commit the ref pointed to before this pipeline was created.</summary>
    public string? BeforeSha { get; init; }

    public bool? Tag { get; init; }

    public string? YamlErrors { get; init; }

    public GitLabUser? User { get; init; }

    public DateTimeOffset? StartedAt { get; init; }

    public DateTimeOffset? FinishedAt { get; init; }

    public DateTimeOffset? CommittedAt { get; init; }

    /// <summary>Seconds spent running, as GitLab's integer wire value.</summary>
    public int? Duration { get; init; }

    /// <summary>Seconds spent queued, as GitLab's integer wire value.</summary>
    public int? QueuedDuration { get; init; }

    /// <summary>Test coverage percentage, represented as an OpenAPI <c>number</c>/<c>float</c>.</summary>
    public float? Coverage { get; init; }

    public GitLabMergeRequestPipelineDetailedStatus? DetailedStatus { get; init; }

    public bool? Archived { get; init; }
}

/// <summary>
///     GitLab's rendered status presentation for an embedded merge request pipeline
///     (<c>DetailedStatusEntity</c> in the OpenAPI schema).
/// </summary>
public sealed record GitLabMergeRequestPipelineDetailedStatus
{
    public string? Icon { get; init; }

    public string? Text { get; init; }

    public string? Label { get; init; }

    public string? Group { get; init; }

    public string? Tooltip { get; init; }

    public bool? HasDetails { get; init; }

    /// <summary>Instance-relative path to GitLab's status details.</summary>
    public string? DetailsPath { get; init; }

    /// <summary>
    ///     The schema declares this as an object but gives no fixed property contract. Preserve it as JSON
    ///     rather than inventing a closed DTO from an illustrative example.
    /// </summary>
    public JsonElement? Illustration { get; init; }

    /// <summary>Instance-relative path to the status favicon.</summary>
    public string? Favicon { get; init; }

    public GitLabMergeRequestPipelineDetailedStatusAction? Action { get; init; }
}

/// <summary>An action GitLab can render alongside a pipeline detailed status.</summary>
public sealed record GitLabMergeRequestPipelineDetailedStatusAction
{
    public string? Icon { get; init; }

    public string? Title { get; init; }

    /// <summary>Instance-relative GitLab path receiving this action.</summary>
    public string? Path { get; init; }

    /// <summary>HTTP method GitLab expects for this action, such as <c>post</c>.</summary>
    public string? Method { get; init; }

    public string? ButtonTitle { get; init; }

    public string? ConfirmationMessage { get; init; }
}