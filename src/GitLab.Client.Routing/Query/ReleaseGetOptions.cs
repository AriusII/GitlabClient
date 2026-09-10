using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Query;

/// <summary>Options for retrieving one project release.</summary>
[GitLabQuery]
public readonly record struct ReleaseGetOptions
{
    /// <summary>Includes the Markdown-rendered description in the response.</summary>
    public bool? IncludeHtmlDescription { get; init; }
}