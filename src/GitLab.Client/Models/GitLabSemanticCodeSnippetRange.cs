namespace GitLab.Client.Models;

/// <summary>One merged line range within a semantic code search match.</summary>
public sealed record GitLabSemanticCodeSnippetRange
{
    public int? StartLine { get; init; }

    public int? EndLine { get; init; }

    public string? Content { get; init; }

    /// <summary>Relevance score for this specific range, from 0.0 to 1.0.</summary>
    public double? Score { get; init; }
}