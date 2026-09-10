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
        response.Headers.TryAddWithoutValidation("RateLimit-Name", "throttle_authenticated_api");
        response.Headers.TryAddWithoutValidation("RateLimit-Observed", "1");
        response.Headers.TryAddWithoutValidation("RateLimit-Remaining", "599");
        response.Headers.TryAddWithoutValidation("RateLimit-Reset", "1700000000");
        response.Headers.TryAddWithoutValidation("RateLimit-ResetTime", "Tue, 14 Nov 2023 22:13:20 GMT");
        response.Headers.TryAddWithoutValidation("Retry-After", "30");

        GitLabRateLimitSnapshot snapshot = GitLabRateLimitSnapshot.FromHeaders(response.Headers);

        Assert.Equal(600, snapshot.Limit);
        Assert.Equal("throttle_authenticated_api", snapshot.Name);
        Assert.Equal(1, snapshot.Observed);
        Assert.Equal(599, snapshot.Remaining);
        Assert.Equal(DateTimeOffset.FromUnixTimeSeconds(1700000000), snapshot.ResetsAt);
        Assert.Equal(DateTimeOffset.FromUnixTimeSeconds(1700000000), snapshot.ResetTime);
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
        Assert.Null(snapshot.Name);
        Assert.Null(snapshot.Observed);
        Assert.Null(snapshot.Remaining);
        Assert.Null(snapshot.ResetsAt);
        Assert.Null(snapshot.ResetTime);
        Assert.Null(snapshot.RetryAfter);
    }

    [Fact]
    public void FromHeaders_IgnoresUnparseableValues()
    {
        using HttpResponseMessage response = new();
        response.Headers.TryAddWithoutValidation("RateLimit-Limit", "not-a-number");

        Assert.Null(GitLabRateLimitSnapshot.FromHeaders(response.Headers).Limit);
    }

    [Fact]
    public void FromHeaders_RejectsNegativeCountersAndIgnoresMalformedObservations()
    {
        using HttpResponseMessage response = new();
        response.Headers.TryAddWithoutValidation("RateLimit-Limit", "-1");
        response.Headers.TryAddWithoutValidation("RateLimit-Remaining", "-2");
        response.Headers.TryAddWithoutValidation("RateLimit-Observed", "-3");
        response.Headers.TryAddWithoutValidation("RateLimit-Name", " ");
        response.Headers.TryAddWithoutValidation("RateLimit-ResetTime", "not-a-date");

        GitLabRateLimitSnapshot snapshot = GitLabRateLimitSnapshot.FromHeaders(response.Headers);

        Assert.Null(snapshot.Limit);
        Assert.Null(snapshot.Remaining);
        Assert.Null(snapshot.Observed);
        Assert.Null(snapshot.Name);
        Assert.Null(snapshot.ResetTime);
    }

    [Fact]
    public void FromHeaders_UsesTheFirstParseableRateLimitValue()
    {
        using HttpResponseMessage response = new();
        response.Headers.TryAddWithoutValidation("RateLimit-Observed", ["unparseable", "42"]);
        response.Headers.TryAddWithoutValidation("RateLimit-ResetTime", ["invalid", "Tue, 14 Nov 2023 22:13:20 GMT"]);

        GitLabRateLimitSnapshot snapshot = GitLabRateLimitSnapshot.FromHeaders(response.Headers);

        Assert.Equal(42, snapshot.Observed);
        Assert.Equal(DateTimeOffset.FromUnixTimeSeconds(1700000000), snapshot.ResetTime);
    }

    [Fact]
    public void FromHeaders_IgnoresOutOfRangeResetAndNegativeRetryAfterValues()
    {
        using HttpResponseMessage response = new();
        response.Headers.TryAddWithoutValidation("RateLimit-Reset",
            long.MaxValue.ToString(CultureInfo.InvariantCulture));
        response.Headers.TryAddWithoutValidation("Retry-After", "-1");

        GitLabRateLimitSnapshot snapshot = GitLabRateLimitSnapshot.FromHeaders(response.Headers);

        Assert.Null(snapshot.ResetsAt);
        Assert.Null(snapshot.RetryAfter);
    }
}