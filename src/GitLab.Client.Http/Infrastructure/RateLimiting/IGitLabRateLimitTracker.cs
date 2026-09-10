namespace GitLab.Client.Infrastructure.RateLimiting;

/// <summary>Exposes the most recently observed GitLab rate-limit state, updated after every response.</summary>
/// <remarks>
///     Read-only on purpose: writing is the pipeline's internal job, and a consumer overwriting the library's
///     own observation of its quota would be a bug with no upside. Note also that
///     GitLab applies rate limits per endpoint category, so this is genuinely "the last response we saw" and
///     not an authoritative global counter - under concurrency, two calls to different endpoints overwrite
///     each other.
/// </remarks>
public interface IGitLabRateLimitTracker
{
    /// <summary>The snapshot taken from the most recent response, or <c>null</c> before the first one.</summary>
    GitLabRateLimitSnapshot? Current { get; }
}