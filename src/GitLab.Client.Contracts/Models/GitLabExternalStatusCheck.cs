namespace GitLab.Client.Models;

/// <summary>
///     An external status check service configured on a project
///     (<c>/projects/:id/external_status_checks</c>) - the third-party gate that must report back before a
///     merge request can merge.
/// </summary>
/// <remarks>
///     The shared secret used to sign GitLab's callbacks is write-only: it is sent on create and update and
///     never returned. <see cref="Hmac" /> is the only thing the API says about it.
/// </remarks>
public sealed record GitLabExternalStatusCheck
{
    public required long Id { get; init; }

    public required string Name { get; init; }

    public long? ProjectId { get; init; }

    /// <summary>The absolute URL GitLab calls when the check needs to run.</summary>
    public Uri? ExternalUrl { get; init; }

    /// <summary>The protected branches this check is scoped to; empty or absent means every branch.</summary>
    public IReadOnlyList<GitLabProtectedBranch>? ProtectedBranches { get; init; }

    /// <summary>Whether a shared secret is configured, so GitLab signs its callbacks with an HMAC.</summary>
    public bool? Hmac { get; init; }
}