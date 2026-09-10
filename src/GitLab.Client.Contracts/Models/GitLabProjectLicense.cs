namespace GitLab.Client.Models;

/// <summary>The license projection embedded in a project response.</summary>
public sealed record GitLabProjectLicense
{
    public string? Key { get; init; }

    public string? Name { get; init; }

    public string? Nickname { get; init; }

    public Uri? HtmlUrl { get; init; }

    public Uri? SourceUrl { get; init; }
}