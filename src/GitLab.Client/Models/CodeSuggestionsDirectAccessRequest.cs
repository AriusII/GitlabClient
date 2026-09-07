namespace GitLab.Client.Models;

/// <summary>
///     Body of <c>POST /code_suggestions/direct_access</c>. The endpoint accepts an empty body, so this is
///     optional at the call site.
/// </summary>
public sealed record CodeSuggestionsDirectAccessRequest
{
    /// <summary>The full path of the project the IDE is working in (<c>namespace/project</c>).</summary>
    public string? ProjectPath { get; init; }
}