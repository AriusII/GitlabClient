namespace GitLab.Client.Models;

/// <summary>Body of <c>POST /code_suggestions/enabled</c>.</summary>
public sealed record CodeSuggestionsEnabledRequest
{
    /// <summary>The full path of the project to check (<c>namespace/project</c>). Required.</summary>
    public required string ProjectPath { get; init; }
}