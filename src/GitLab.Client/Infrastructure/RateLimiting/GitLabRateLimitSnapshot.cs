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
    /// <summary>
    ///     Reads the snapshot off a response. Internal because it takes a <c>System.Net.Http.Headers</c> type,
    ///     and this package keeps <see cref="HttpClient" /> plumbing out of its public surface.
    /// </summary>
    internal static GitLabRateLimitSnapshot FromHeaders(HttpResponseHeaders headers)
    {
        ArgumentNullException.ThrowIfNull(headers);

        // This runs after EVERY response. The NonValidated view is a struct over the raw header store whose
        // TryGetValues hands back a struct with a struct enumerator, so each lookup avoids the string[] plus
        // boxed array enumerator that TryGetValues(out IEnumerable<string>) allocates - six allocations per
        // response, purely to read three integers. It also skips typed-header parsing, which is exactly right
        // for headers this code parses itself.
        HttpHeadersNonValidated raw = headers.NonValidated;

        return new GitLabRateLimitSnapshot(
            ParseInt32(raw, "RateLimit-Limit"),
            ParseInt32(raw, "RateLimit-Remaining"),
            ParseUnixSeconds(raw, "RateLimit-Reset"),
            ParseRetryAfter(raw));
    }

    private static int? ParseInt32(HttpHeadersNonValidated headers, string headerName)
    {
        if (!headers.TryGetValues(headerName, out HeaderStringValues values))
        {
            return null;
        }

        foreach (string value in values)
        {
            if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int result))
            {
                return result;
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
            if (long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out long seconds))
            {
                return DateTimeOffset.FromUnixTimeSeconds(seconds);
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
                return TimeSpan.FromSeconds(seconds);
            }

            if (DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out DateTimeOffset retryAt))
            {
                TimeSpan delta = retryAt - DateTimeOffset.UtcNow;
                return delta > TimeSpan.Zero ? delta : TimeSpan.Zero;
            }
        }

        return null;
    }
}