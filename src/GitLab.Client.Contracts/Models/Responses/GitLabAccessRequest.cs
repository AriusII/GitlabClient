namespace GitLab.Client.Models.Responses;

/// <summary>
///     A pending request to join a project or group, as returned by the GitLab Access Requests API
///     (<c>/projects/:id/access_requests</c>, <c>/groups/:id/access_requests</c>).
///     <para>
///         Deliberately not <see cref="GitLabUser" />: this entity adds <c>created_at</c> and <c>requested_at</c>. The
///         members
///         the two share keep identical names so they read the same at a call site.
///     </para>
/// </summary>
public sealed record GitLabAccessRequest
{
    public long? Id { get; init; }

    public string? Username { get; init; }

    public string? Name { get; init; }

    public string? State { get; init; }

    public bool? Locked { get; init; }

    public string? PublicEmail { get; init; }

    public Uri? AvatarUrl { get; init; }

    /// <summary>Instance-relative avatar path (<c>/uploads/-/system/user/avatar/1/avatar.png</c>), not an absolute URL.</summary>
    public string? AvatarPath { get; init; }

    public IReadOnlyList<GitLabCustomAttribute>? CustomAttributes { get; init; }

    public Uri? WebUrl { get; init; }

    /// <summary>When GitLab created the access request.</summary>
    public DateTimeOffset? CreatedAt { get; init; }

    /// <summary>
    ///     When the user asked to join. GitLab sends an ISO-8601 timestamp even though the spec types it as a plain
    ///     string.
    /// </summary>
    public DateTimeOffset? RequestedAt { get; init; }
}