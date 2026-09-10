namespace GitLab.Client.Models;

/// <summary>The <c>APIEntitiesNamespaceBasic</c> projection embedded in a project.</summary>
public sealed record GitLabProjectNamespace
{
    public long? Id { get; init; }

    public string? Name { get; init; }

    public string? Path { get; init; }

    public string? Kind { get; init; }

    public string? FullPath { get; init; }

    public long? ParentId { get; init; }

    public Uri? AvatarUrl { get; init; }

    public Uri? WebUrl { get; init; }
}