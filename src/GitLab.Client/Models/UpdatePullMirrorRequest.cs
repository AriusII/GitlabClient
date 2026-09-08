using System.Diagnostics.CodeAnalysis;

namespace GitLab.Client.Models;

/// <summary>
///     Request body for <c>PUT /projects/:id/mirror/pull</c> (introduced in GitLab 17.5). Every member is
///     optional: unset ones are omitted from the payload rather than sent as null, so a partial update
///     cannot clear a field it never mentioned.
/// </summary>
public sealed record UpdatePullMirrorRequest
{
    /// <summary>Enables pull mirroring on the project when set to <c>true</c>.</summary>
    public bool? Enabled { get; init; }

    /// <summary>
    ///     The URL of the upstream project to pull mirror. Deliberately a <see cref="string" /> - see
    ///     <see cref="GitLabPullMirror.Url" />.
    /// </summary>
    [SuppressMessage("Design", "CA1056",
        Justification =
            "Symmetric with GitLabPullMirror.Url, whose scrubbed '*****:*****' userinfo Uri cannot be relied on to parse.")]
    public string? Url { get; init; }

    /// <summary>The username used to authenticate against the upstream repository.</summary>
    public string? AuthUser { get; init; }

    /// <summary>
    ///     The password, or a personal access token with the <c>api</c> scope, used to authenticate against
    ///     the upstream repository.
    /// </summary>
    public string? AuthPassword { get; init; }

    /// <summary>Whether pull mirroring triggers CI/CD pipelines.</summary>
    public bool? MirrorTriggerBuilds { get; init; }

    /// <summary>Mutually exclusive with <see cref="MirrorBranchRegex" />.</summary>
    public bool? OnlyMirrorProtectedBranches { get; init; }

    /// <summary>Whether the pull mirror overwrites diverged branches.</summary>
    public bool? MirrorOverwritesDivergedBranches { get; init; }

    /// <summary>
    ///     Only mirror branches with names that match this regex. Mutually exclusive with
    ///     <see cref="OnlyProtectedBranches" />.
    /// </summary>
    public string? MirrorBranchRegex { get; init; }

    /// <summary>
    ///     Mutually exclusive with <see cref="MirrorBranchRegex" />. Typed as <see cref="string" /> because
    ///     the spec declares it that way despite the boolean-sounding name - followed here rather than
    ///     "corrected", since GitLab's Grape endpoint is the actual authority on what it accepts.
    /// </summary>
    public string? OnlyProtectedBranches { get; init; }
}