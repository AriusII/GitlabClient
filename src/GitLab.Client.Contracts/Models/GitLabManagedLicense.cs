namespace GitLab.Client.Models;

/// <summary>
///     A software licence policy on a project (<c>/projects/:id/managed_licenses</c>) - one licence name
///     plus the verdict the project has recorded for it.
/// </summary>
/// <remarks>
///     Despite sharing the "Licenses" tag with the instance <see cref="GitLabLicense" />, this is an
///     unrelated resource: it belongs to licence compliance scanning, not to activating GitLab.
/// </remarks>
public sealed record GitLabManagedLicense
{
    public required long Id { get; init; }

    /// <summary>The licence name, such as <c>MIT</c> or <c>Apache-2.0</c>.</summary>
    public string? Name { get; init; }

    /// <summary>
    ///     The recorded verdict. Deliberately a bare string rather than
    ///     <see cref="GitLabManagedLicenseApprovalStatus" />: the spec enumerates the vocabulary on the
    ///     request body only, and GitLab still echoes the older spellings <c>approved</c> and
    ///     <c>blacklisted</c> for policies created before the rename to <c>allowed</c>/<c>denied</c>. A
    ///     value GitLab adds later must not turn a healthy response into a JSON exception.
    /// </summary>
    public string? ApprovalStatus { get; init; }
}