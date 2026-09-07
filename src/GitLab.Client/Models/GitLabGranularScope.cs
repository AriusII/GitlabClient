namespace GitLab.Client.Models;

/// <summary>
///     One granular-permission entry on a token, as returned nested inside
///     <see cref="GitLabAccessToken" /> and <see cref="GitLabPersonalAccessToken" />. Granular scopes
///     narrow a token to a named set of permissions on a single project or group instead of the
///     coarse <c>api</c>/<c>read_api</c> scopes; only tokens whose <c>granular</c> flag is set carry them.
/// </summary>
public sealed record GitLabGranularScope
{
    /// <summary>The scope of resources the permissions apply to - for example <c>personal_projects</c>.</summary>
    public string? Access { get; init; }

    /// <summary>The permissions granted within <see cref="Access" /> - for example <c>read_job</c>.</summary>
    public IReadOnlyList<string>? Permissions { get; init; }

    public long? ProjectId { get; init; }

    public long? GroupId { get; init; }
}