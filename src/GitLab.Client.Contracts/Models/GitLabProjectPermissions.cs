namespace GitLab.Client.Models;

/// <summary>The direct and inherited permissions attached to a project response.</summary>
public sealed record GitLabProjectPermissions
{
    public GitLabProjectAccess? ProjectAccess { get; init; }

    public GitLabProjectGroupAccess? GroupAccess { get; init; }
}