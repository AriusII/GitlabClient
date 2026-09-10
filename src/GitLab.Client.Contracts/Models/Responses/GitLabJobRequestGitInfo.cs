namespace GitLab.Client.Models.Responses;

/// <summary>Repository revision metadata included in a runner job-request response.</summary>
public sealed record GitLabJobRequestGitInfo
{
    public Uri? RepoUrl { get; init; }
    public string? Ref { get; init; }
    public string? Sha { get; init; }
    public string? BeforeSha { get; init; }
    public string? RefType { get; init; }
    public string? Refspecs { get; init; }
    public string? Depth { get; init; }
    public string? RepoObjectFormat { get; init; }
    public string? Protected { get; init; }
}