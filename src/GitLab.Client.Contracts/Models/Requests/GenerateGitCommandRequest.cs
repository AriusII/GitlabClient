namespace GitLab.Client.Models.Requests;

/// <summary>Body of <c>POST /ai/llm/git_command</c>.</summary>
public sealed record GenerateGitCommandRequest
{
    /// <summary>What the user wants to do, in plain language - "undo my last two commits but keep the changes".</summary>
    public required string Prompt { get; init; }
}