using System.Diagnostics.CodeAnalysis;

namespace GitLab.Client.Models;

/// <summary>The <c>APIEntitiesUserBasic</c> projection embedded by a board response.</summary>
public sealed record GitLabBasicUser
{
    public long? Id { get; init; }

    public string? Username { get; init; }

    public string? PublicEmail { get; init; }

    public string? Name { get; init; }

    public string? State { get; init; }

    public bool? Locked { get; init; }

    /// <summary>The user's avatar location as the unconstrained string returned by GitLab.</summary>
    [SuppressMessage("Design", "CA1056",
        Justification =
            "APIEntitiesUserBasic declares avatar_url as string without a URI format; GitLab does not "
            + "guarantee an absolute URI for every deployment.")]
    public string? AvatarUrl { get; init; }

    public string? AvatarPath { get; init; }

    public IReadOnlyList<GitLabBasicCustomAttributeEntry>? CustomAttributes { get; init; }

    /// <summary>The user's web location as the unconstrained string returned by GitLab.</summary>
    [SuppressMessage("Design", "CA1056",
        Justification =
            "APIEntitiesUserBasic declares web_url as string without a URI format; preserving the exact "
            + "wire contract avoids rejecting a relative URL.")]
    public string? WebUrl { get; init; }
}