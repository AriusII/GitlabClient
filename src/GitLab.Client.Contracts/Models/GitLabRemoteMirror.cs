using System.Diagnostics.CodeAnalysis;

namespace GitLab.Client.Models;

/// <summary>A push mirror configured on a project, as returned by the GitLab Remote Mirrors API.</summary>
public sealed record GitLabRemoteMirror
{
    public required long Id { get; init; }

    public bool? Enabled { get; init; }

    /// <summary>
    ///     The mirror's remote URL. Deliberately a <see cref="string" /> rather than a <see cref="Uri" />:
    ///     GitLab scrubs credentials out of every response, returning literally
    ///     <c>https://*****:*****@example.com/gitlab/example.git</c>, and that userinfo is not reliably
    ///     parseable - round-tripping it through <see cref="Uri" /> risks a <see cref="UriFormatException" />
    ///     on deserialization of a perfectly normal payload.
    /// </summary>
    [SuppressMessage("Design", "CA1056",
        Justification =
            "GitLab scrubs the credentials in this value to '*****:*****', which Uri cannot be relied on to parse.")]
    public string? Url { get; init; }

    /// <summary>Free-text status - <c>none</c>, <c>scheduled</c>, <c>started</c>, <c>finished</c>, <c>failed</c>.</summary>
    public string? UpdateStatus { get; init; }

    public DateTimeOffset? LastUpdateAt { get; init; }

    public DateTimeOffset? LastUpdateStartedAt { get; init; }

    public DateTimeOffset? LastSuccessfulUpdateAt { get; init; }

    public string? LastError { get; init; }

    public bool? OnlyProtectedBranches { get; init; }

    public bool? KeepDivergentRefs { get; init; }

    /// <summary><c>ssh_public_key</c> or <c>password</c>.</summary>
    public string? AuthMethod { get; init; }

    public IReadOnlyList<GitLabMirrorHostKey>? HostKeys { get; init; }

    public string? MirrorBranchRegex { get; init; }
}