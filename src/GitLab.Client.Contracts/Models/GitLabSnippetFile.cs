namespace GitLab.Client.Models;

/// <summary>One file inside a snippet repository, as embedded in <see cref="GitLabSnippet.Files" />.</summary>
public sealed record GitLabSnippetFile
{
    /// <summary>The file's path inside the snippet repository, such as <c>src/App.cs</c>.</summary>
    public string? Path { get; init; }

    /// <summary>Where this file's raw content is served from.</summary>
    public Uri? RawUrl { get; init; }
}