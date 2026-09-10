namespace GitLab.Client.Models;

/// <summary>
///     Namespace metadata carried by <see cref="GitLabDuoNamespaceAccessRule.ThroughNamespace" />.
/// </summary>
public sealed record GitLabDuoNamespaceAccessRuleThroughNamespace
{
    public required long? Id { get; init; }

    public string? Name { get; init; }

    public string? FullPath { get; init; }
}