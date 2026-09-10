namespace GitLab.Client.Models;

/// <summary>
///     One entry in <c>duo_namespace_access_rules</c>, granting named GitLab Duo features through a
///     namespace.
/// </summary>
public sealed record GitLabDuoNamespaceAccessRule
{
    public GitLabDuoNamespaceAccessRuleThroughNamespace? ThroughNamespace { get; init; }

    /// <summary>GitLab Duo features that the namespace may access.</summary>
    public required IReadOnlyList<string>? Features { get; init; }
}