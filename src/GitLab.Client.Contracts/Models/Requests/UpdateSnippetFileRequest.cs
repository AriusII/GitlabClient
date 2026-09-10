namespace GitLab.Client.Models.Requests;

/// <summary>
///     One entry of the <c>files</c> array on <see cref="UpdateSnippetRequest" />: an action plus the
///     paths and content it needs. Which members matter depends on <see cref="Action" /> -
///     <see cref="GitLabSnippetFileAction.Move" /> is the only one that reads
///     <see cref="PreviousPath" />, and <see cref="GitLabSnippetFileAction.Delete" /> ignores
///     <see cref="Content" />.
/// </summary>
public sealed record UpdateSnippetFileRequest
{
    /// <summary>What to do with the file. The only member GitLab always requires.</summary>
    public required GitLabSnippetFileAction Action { get; init; }

    /// <summary>The file's path inside the snippet repository - the destination path for a move.</summary>
    public string? FilePath { get; init; }

    /// <summary>The path the file is being moved from. Only read for <see cref="GitLabSnippetFileAction.Move" />.</summary>
    public string? PreviousPath { get; init; }

    /// <summary>The file's new content.</summary>
    public string? Content { get; init; }
}