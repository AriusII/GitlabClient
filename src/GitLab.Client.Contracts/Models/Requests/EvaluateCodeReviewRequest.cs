namespace GitLab.Client.Models.Requests;

/// <summary>
///     Body of <c>POST /duo_code_review/evaluations</c>. Every member is required: the endpoint evaluates
///     a review in isolation and reads nothing from GitLab itself, so the whole merge request has to be
///     described in the request.
/// </summary>
public sealed record EvaluateCodeReviewRequest
{
    /// <summary>The raw unified diff to review.</summary>
    public required string Diffs { get; init; }

    /// <summary>The merge request's title.</summary>
    public required string MrTitle { get; init; }

    /// <summary>The merge request's description.</summary>
    public required string MrDescription { get; init; }

    /// <summary>
    ///     The full content of each touched file, keyed by file path, so the reviewer can see beyond the
    ///     diff hunks.
    /// </summary>
    public required IReadOnlyDictionary<string, string> FilesContent { get; init; }
}