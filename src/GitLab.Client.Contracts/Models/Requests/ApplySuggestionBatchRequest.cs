namespace GitLab.Client.Models.Requests;

/// <summary>Request body for <c>PUT /suggestions/batch_apply</c>, which applies several suggestions in one commit.</summary>
public sealed record ApplySuggestionBatchRequest
{
    /// <summary>The suggestion IDs to apply together.</summary>
    public required IReadOnlyList<long> Ids { get; init; }

    /// <summary>
    ///     Custom commit message for the single commit that applies all of them. Left unset, GitLab uses the
    ///     project's default suggestion commit message.
    /// </summary>
    public string? CommitMessage { get; init; }
}