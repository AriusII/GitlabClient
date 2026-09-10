using System.Globalization;
using System.Net.Http.Headers;

namespace GitLab.Client.Infrastructure.RateLimiting;

/// <summary>A point-in-time read of GitLab's <c>RateLimit-*</c> response headers.</summary>
public readonly record struct GitLabRateLimitSnapshot(
    int? Limit,
    int? Remaining,
    DateTimeOffset? ResetsAt,
    TimeSpan? RetryAfter)
{
    /// <summary>The GitLab throttle that produced this observation, from <c>RateLimit-Name</c>.</summary>
    public string? Name { get; init; }

    /// <summary>The number of requests GitLab observed in the current window, from <c>RateLimit-Observed</c>.</summary>
    public int? Observed { get; init; }

    /// <summary>
    ///     The HTTP-date at which GitLab resets the quota, from <c>RateLimit-ResetTime</c>. Unlike
    ///     <see cref="ResetsAt" />, this header is generally emitted only with a throttled response.
    /// </summary>
    public DateTimeOffset? ResetTime { get; init; }

    /// <summary>
    ///     Reads the snapshot off a response. Internal because it takes a <c>System.Net.Http.Headers</c> type,
    ///     and this package keeps <see cref="HttpClient" /> plumbing out of its public surface.
    /// </summary>
    internal static GitLabRateLimitSnapshot FromHeaders(HttpResponseHeaders headers)
    {
        ArgumentNullException.ThrowIfNull(headers);

        // This runs after EVERY response. The NonValidated view is a struct over the raw header store whose
        // TryGetValues hands back a struct with a struct enumerator, so each lookup avoids the string[] plus
        // boxed array enumerator that TryGetValues(out IEnumerable<string>) allocates. It also skips typed-header
        // parsing, which is exactly right for headers this code parses itself.
        HttpHeadersNonValidated raw = headers.NonValidated;

        return new GitLabRateLimitSnapshot(
            ParseNonNegativeInt32(raw, "RateLimit-Limit"),
            ParseNonNegativeInt32(raw, "RateLimit-Remaining"),
            ParseUnixSeconds(raw, "RateLimit-Reset"),
            ParseRetryAfter(raw))
        {
            Name = ParseNonEmptyString(raw, "RateLimit-Name"),
            Observed = ParseNonNegativeInt32(raw, "RateLimit-Observed"),
            ResetTime = ParseHttpDate(raw, "RateLimit-ResetTime")
        };
    }

    private static int? ParseNonNegativeInt32(HttpHeadersNonValidated headers, string headerName)
    {
        if (!headers.TryGetValues(headerName, out HeaderStringValues values))
        {
            return null;
        }

        foreach (string value in values)
        {
            if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int result) && result >= 0)
            {
                return result;
            }
        }

        return null;
    }

    private static string? ParseNonEmptyString(HttpHeadersNonValidated headers, string headerName)
    {
        if (!headers.TryGetValues(headerName, out HeaderStringValues values))
        {
            return null;
        }

        foreach (string value in values)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        return null;
    }

    private static DateTimeOffset? ParseUnixSeconds(HttpHeadersNonValidated headers, string headerName)
    {
        if (!headers.TryGetValues(headerName, out HeaderStringValues values))
        {
            return null;
        }

        foreach (string value in values)
        {
            if (!long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out long seconds))
            {
                continue;
            }

            // Response headers are untrusted input. FromUnixTimeSeconds throws outside the .NET
            // DateTimeOffset range, so a malformed proxy/server quota header must remain an absent
            // observation rather than turning an otherwise valid API response into a client failure.
            try
            {
                return DateTimeOffset.FromUnixTimeSeconds(seconds);
            }
            catch (ArgumentOutOfRangeException)
            {
                return null;
            }
        }

        return null;
    }

    private static DateTimeOffset? ParseHttpDate(HttpHeadersNonValidated headers, string headerName)
    {
        if (!headers.TryGetValues(headerName, out HeaderStringValues values))
        {
            return null;
        }

        foreach (string value in values)
        {
            if (DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture,
                    DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                    out DateTimeOffset result))
            {
                return result;
            }
        }

        return null;
    }

    /// <summary>
    ///     <c>Retry-After</c> is either delta-seconds or an HTTP-date (RFC 9110 10.2.3). The typed
    ///     <see cref="HttpResponseHeaders.RetryAfter" /> property allocates, and reports a null <c>Delta</c>
    ///     for the date form - so a 429 carrying a date looked exactly like a 429 carrying nothing.
    /// </summary>
    private static TimeSpan? ParseRetryAfter(HttpHeadersNonValidated headers)
    {
        if (!headers.TryGetValues("Retry-After", out HeaderStringValues values))
        {
            return null;
        }

        foreach (string value in values)
        {
            if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int seconds))
            {
                return seconds >= 0 ? TimeSpan.FromSeconds(seconds) : null;
            }

            if (!DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out DateTimeOffset retryAt))
            {
                continue;
            }

            TimeSpan delta = retryAt - DateTimeOffset.UtcNow;
            return delta > TimeSpan.Zero ? delta : TimeSpan.Zero;
        }

        return null;
    }
}