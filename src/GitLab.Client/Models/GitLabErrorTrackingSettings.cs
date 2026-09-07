namespace GitLab.Client.Models;

/// <summary>
///     A project's Error Tracking settings, as returned by
///     <c>/projects/:id/error_tracking/settings</c> - whether a Sentry (or Sentry-compatible) project is
///     wired up, and whether GitLab's own integrated backend or an external Sentry instance handles it.
/// </summary>
public sealed record GitLabErrorTrackingSettings
{
    public bool? Active { get; init; }

    /// <summary>The name of the configured Sentry project.</summary>
    public string? ProjectName { get; init; }

    /// <summary>The externally reachable Sentry project URL, for linking out from GitLab's UI.</summary>
    public Uri? SentryExternalUrl { get; init; }

    /// <summary>The Sentry API base URL GitLab calls to fetch error data.</summary>
    public Uri? ApiUrl { get; init; }

    /// <summary>Whether GitLab's own integrated error tracking backend is used instead of external Sentry.</summary>
    public bool? Integrated { get; init; }
}