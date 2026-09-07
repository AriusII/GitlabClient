using System.Globalization;

using GitLab.Client.Infrastructure.RateLimiting;

namespace GitLab.Client.Tests.Infrastructure;

public sealed class GitLabRateLimitSnapshotTests
{
    [Fact]
    public void FromHeaders_ReadsGitLabsRateLimitHeaders()
    {
        using HttpResponseMessage response = new();
        response.Headers.TryAddWithoutValidation("RateLimit-Limit", "600");
        response.Headers.TryAddWithoutValidation("RateLimit-Remaining", "599");
        response.Headers.TryAddWithoutValidation("RateLimit-Reset", "1700000000");
        response.Headers.TryAddWithoutValidation("Retry-After", "30");

        GitLabRateLimitSnapshot snapshot = GitLabRateLimitSnapshot.FromHeaders(response.Headers);

        Assert.Equal(600, snapshot.Limit);
        Assert.Equal(599, snapshot.Remaining);
        Assert.Equal(DateTimeOffset.FromUnixTimeSeconds(1700000000), snapshot.ResetsAt);
        Assert.Equal(TimeSpan.FromSeconds(30), snapshot.RetryAfter);
    }

    [Fact]
    public void FromHeaders_ReadsTheHttpDateFormOfRetryAfter()
    {
        // Legal per RFC 9110 10.2.3, and HttpResponseHeaders.RetryAfter.Delta reports null for it, so the
        // typed accessor silently dropped the one number a caller hitting a 429 actually needs.
        using HttpResponseMessage response = new();
        response.Headers.TryAddWithoutValidation(
            "Retry-After",
            DateTimeOffset.UtcNow.AddMinutes(5).ToString("R", CultureInfo.InvariantCulture));

        GitLabRateLimitSnapshot snapshot = GitLabRateLimitSnapshot.FromHeaders(response.Headers);

        Assert.NotNull(snapshot.RetryAfter);
        Assert.InRange(snapshot.RetryAfter.Value, TimeSpan.FromMinutes(4), TimeSpan.FromMinutes(5));
    }

    [Fact]
    public void FromHeaders_YieldsNulls_WhenNoRateLimitHeadersArePresent()
    {
        using HttpResponseMessage response = new();

        GitLabRateLimitSnapshot snapshot = GitLabRateLimitSnapshot.FromHeaders(response.Headers);

        Assert.Null(snapshot.Limit);
        Assert.Null(snapshot.Remaining);
        Assert.Null(snapshot.ResetsAt);
        Assert.Null(snapshot.RetryAfter);
    }

    [Fact]
    public void FromHeaders_IgnoresUnparseableValues()
    {
        using HttpResponseMessage response = new();
        response.Headers.TryAddWithoutValidation("RateLimit-Limit", "not-a-number");

        Assert.Null(GitLabRateLimitSnapshot.FromHeaders(response.Headers).Limit);
    }
}