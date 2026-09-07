namespace GitLab.Client.Models;

/// <summary>Body of <c>POST /code_suggestions/completions</c>.</summary>
public sealed record GenerateCodeCompletionRequest
{
    /// <summary>The buffer the cursor is in. The only member GitLab requires.</summary>
    public required CodeSuggestionCurrentFile CurrentFile { get; init; }

    /// <summary>Whether to finish the current statement or write a new block. Inferred server-side when omitted.</summary>
    public CodeSuggestionIntent? Intent { get; init; }

    /// <summary>What triggered a generation request. Only meaningful with <see cref="CodeSuggestionIntent.Generation" />.</summary>
    public CodeSuggestionGenerationType? GenerationType { get; init; }

    /// <summary>
    ///     Ask the AI Gateway to stream the completion. Leave this unset: a streamed response is a plain
    ///     text body rather than the JSON this client deserializes, so it cannot be consumed here.
    /// </summary>
    public bool? Stream { get; init; }

    /// <summary>
    ///     The full path of the project the file belongs to (<c>namespace/project</c>), which scopes the request's
    ///     entitlement check.
    /// </summary>
    public string? ProjectPath { get; init; }

    /// <summary>Extra instructions from the user, prepended to the prompt. At most 600,000 characters.</summary>
    public string? UserInstruction { get; init; }

    /// <summary>Related files or excerpts to widen the model's view beyond the open buffer.</summary>
    public IReadOnlyList<CodeSuggestionContextPart>? Context { get; init; }
}