namespace GitLab.Client.Infrastructure.RateLimiting;

/// <summary>
///     The write half of the rate-limit tracker, kept internal so the observation of our own quota cannot be
///     falsified from outside the library. <c>GitLabRateLimitTracker</c> implements this and
///     <see cref="IGitLabRateLimitTracker" />; both resolve to the same singleton.
/// </summary>
internal interface IGitLabRateLimitWriter
{
    void Update(GitLabRateLimitSnapshot snapshot);
}