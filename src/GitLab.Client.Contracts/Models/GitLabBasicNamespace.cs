using System.Diagnostics.CodeAnalysis;

namespace GitLab.Client.Models;

/// <summary>The <c>APIEntitiesNamespaceBasic</c> projection embedded by a basic project.</summary>
public sealed record GitLabBasicNamespace
{
    public long? Id { get; init; }

    public string? Name { get; init; }

    public string? Path { get; init; }

    public string? Kind { get; init; }

    public string? FullPath { get; init; }

    public long? ParentId { get; init; }

    /// <summary>The namespace avatar location as the unconstrained string returned by GitLab.</summary>
    [SuppressMessage("Design", "CA1056",
        Justification =
            "APIEntitiesNamespaceBasic declares avatar_url as string without a URI format; it can be an "
            + "instance-relative value and must not be rejected by Uri parsing.")]
    public string? AvatarUrl { get; init; }

    /// <summary>The namespace web location as the unconstrained string returned by GitLab.</summary>
    [SuppressMessage("Design", "CA1056",
        Justification =
            "APIEntitiesNamespaceBasic declares web_url as string without a URI format; preserving the "
            + "wire value avoids inventing an absolute-URI guarantee absent from GitLab 19.4.")]
    public string? WebUrl { get; init; }
}