namespace GitLab.Client.Models;

/// <summary>
///     A namespace enabled for ActiveContext code indexing, and where it currently sits in the pending
///     to ready indexing lifecycle (<c>PUT /admin/active_context/code/enabled_namespaces</c>).
/// </summary>
/// <remarks>
///     Instance-administrator only. <see cref="State" /> is deliberately a bare string rather than the
///     <see cref="GitLabActiveContextNamespaceState" /> enum this same value is sent as on the request:
///     the response schema does not enumerate its vocabulary, so a future state value must not turn a
///     healthy response into a deserialization failure.
/// </remarks>
public sealed record GitLabActiveContextCodeEnabledNamespace
{
    public long? Id { get; init; }

    public long? NamespaceId { get; init; }

    public long? ConnectionId { get; init; }

    /// <summary>Typically "pending" or "ready", but see the remarks on why this is not the request enum.</summary>
    public string? State { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }

    public DateTimeOffset? UpdatedAt { get; init; }
}