namespace GitLab.Client.Models;

/// <summary>
///     A client key used to authenticate error submissions into GitLab's integrated error tracking, as
///     returned by <c>/projects/:id/error_tracking/client_keys</c>.
///     <para>
///         Unlike an access token, GitLab's list, create and delete responses all carry the same shape -
///         including <see cref="PublicKey" /> and <see cref="SentryDsn" /> - so there is no narrower
///         "list" projection to split this into; the key material is not a one-time secret here.
///     </para>
/// </summary>
public sealed record GitLabErrorTrackingClientKey
{
    public required long Id { get; init; }

    public bool? Active { get; init; }

    /// <summary>The generated public key, for example <c>glet_aa77551d849c083f76d0bc545ed053a3</c>.</summary>
    public string? PublicKey { get; init; }

    /// <summary>The full DSN error-tracking clients submit events to, embedding <see cref="PublicKey" />.</summary>
    public Uri? SentryDsn { get; init; }
}