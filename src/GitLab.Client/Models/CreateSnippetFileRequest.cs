namespace GitLab.Client.Models;

/// <summary>One file of a multi-file snippet being created. Both members are required by GitLab.</summary>
public sealed record CreateSnippetFileRequest
{
    /// <summary>The file's path inside the snippet repository, such as <c>src/App.cs</c>.</summary>
    public required string FilePath { get; init; }

    /// <summary>The file's content.</summary>
    public required string Content { get; init; }
}