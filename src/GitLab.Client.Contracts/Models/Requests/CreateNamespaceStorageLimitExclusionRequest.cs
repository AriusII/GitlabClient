namespace GitLab.Client.Models.Requests;

/// <summary>The body of <c>POST /namespaces/:id/storage/limit_exclusion</c>.</summary>
public sealed record CreateNamespaceStorageLimitExclusionRequest
{
    /// <summary>Why the namespace should be excluded from storage-limit enforcement.</summary>
    public required string Reason { get; init; }
}