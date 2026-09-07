using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Models;

/// <summary>Filters for <c>GET /namespaces</c>.</summary>
[GitLabQuery]
public sealed record NamespaceListOptions
{
    /// <summary>Free-text filter, matched against the namespace name by default.</summary>
    public string? Search { get; init; }

    /// <summary>Returns only namespaces the current user owns.</summary>
    public bool? OwnedOnly { get; init; }

    /// <summary>Restricts the answer to top-level namespaces, excluding every subgroup.</summary>
    public bool? TopLevelOnly { get; init; }

    /// <summary>When set, <see cref="Search" /> is matched against the namespace's full path instead.</summary>
    public bool? FullPathSearch { get; init; }

    public int? Page { get; init; }

    public int? PerPage { get; init; }

    /// <summary>The name of a hosted plan requested by the customer, as GitLab.com's billing flow sets it.</summary>
    public string? RequestedHostedPlan { get; init; }
}