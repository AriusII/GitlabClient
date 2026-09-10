namespace GitLab.Client.Models.Requests;

/// <summary>Request body for <c>PUT /suggestions/:id/apply</c>. Nothing is required.</summary>
public sealed record ApplySuggestionRequest
{
    /// <summary>
    ///     Custom commit message for the commit that applies the suggestion. Left unset, GitLab uses the
    ///     project's default suggestion commit message.
    /// </summary>
    public string? CommitMessage { get; init; }
}