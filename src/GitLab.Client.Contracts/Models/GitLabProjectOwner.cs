namespace GitLab.Client.Models;

/// <summary>The basic user projection returned as a project's owner.</summary>
public sealed record GitLabProjectOwner
{
    public long? Id { get; init; }

    public string? Username { get; init; }

    public string? PublicEmail { get; init; }

    public string? Name { get; init; }

    public string? State { get; init; }

    public bool? Locked { get; init; }

    public Uri? AvatarUrl { get; init; }

    public string? AvatarPath { get; init; }

    public IReadOnlyList<GitLabProjectCustomAttributeEntry>? CustomAttributes { get; init; }

    public Uri? WebUrl { get; init; }
}