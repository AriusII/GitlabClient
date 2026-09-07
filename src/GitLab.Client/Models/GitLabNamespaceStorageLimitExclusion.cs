namespace GitLab.Client.Models;

/// <summary>
///     A namespace excluded from GitLab's storage-limit enforcement
///     (<c>GET /namespaces/storage/limit_exclusions</c>,
///     <c>POST /namespaces/:id/storage/limit_exclusion</c>).
/// </summary>
public sealed record GitLabNamespaceStorageLimitExclusion
{
    public required long Id { get; init; }

    public long? NamespaceId { get; init; }

    public string? NamespaceName { get; init; }

    /// <summary>Why the namespace was excluded, as recorded when the exclusion was created.</summary>
    public string? Reason { get; init; }
}