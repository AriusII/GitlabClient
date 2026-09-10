namespace GitLab.Client.Models;

/// <summary>
///     The answer to <c>GET /namespaces/:id/exists</c> - whether a candidate namespace path is still
///     available, and if not, alternatives GitLab suggests instead.
/// </summary>
public sealed record GitLabNamespaceExistence
{
    public required bool Exists { get; init; }

    /// <summary>Alternative paths GitLab suggests when <see cref="Exists" /> is <see langword="true" />.</summary>
    public IReadOnlyList<string>? Suggests { get; init; }
}