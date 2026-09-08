using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Models;

/// <summary>Filters for a group's SSH-key credentials inventory (<c>GET /groups/:id/manage/ssh_keys</c>).</summary>
[GitLabQuery]
public readonly record struct GroupManagedSshKeyListOptions
{
    /// <summary>Return only keys created before this instant.</summary>
    public DateTimeOffset? CreatedBefore { get; init; }

    /// <summary>Return only keys created after this instant.</summary>
    public DateTimeOffset? CreatedAfter { get; init; }

    /// <summary>Return only keys expiring before this instant.</summary>
    public DateTimeOffset? ExpiresBefore { get; init; }

    /// <summary>Return only keys expiring after this instant.</summary>
    public DateTimeOffset? ExpiresAfter { get; init; }

    public int? PerPage { get; init; }
}