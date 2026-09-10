namespace GitLab.Client.Models;

/// <summary>One file's grouped result from a semantic code search.</summary>
public sealed record GitLabSemanticCodeSearchMatch
{
    public string? Path { get; init; }

    public string? BlobId { get; init; }

    public Uri? FileUrl { get; init; }

    /// <summary>Overall relevance score for the file, from 0.0 to 1.0.</summary>
    public double? Score { get; init; }

    public IReadOnlyList<GitLabSemanticCodeSnippetRange>? SnippetRanges { get; init; }
}