using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace GitLab.Client.Infrastructure.Http;

/// <summary>
///     Retries transient GitLab failures - 429 (rate limited), 502, 503 and 504 - honouring the server's
///     <c>Retry-After</c> header when present and falling back to a small bounded exponential backoff
///     otherwise.
/// </summary>
/// <remarks>
///     An <see cref="HttpRequestMessage" /> can only be sent once - <see cref="HttpClient" /> throws on a
///     second <c>SendAsync</c> with the same instance - so every retry attempt is a fresh message built from
///     a pristine snapshot of the original (see <see cref="CreateRetryRequest" />). Two things make that
///     snapshot non-trivial:
///     <para>
///         Headers: this handler is registered OUTERMOST, ahead of <see cref="GitLabAuthenticationHandler" />
///         and <see cref="GitLabRateLimitHandler" />, precisely so every retry re-enters those handlers and
///         gets freshly re-authenticated. But <see cref="DelegatingHandler" />s mutate the same
///         <see cref="HttpRequestMessage" /> instance in place rather than cloning it, so by the time the
///         first attempt's response comes back, <c>request.Headers</c> already carries the
///         <c>PRIVATE-TOKEN</c>/<c>Authorization</c> header <see cref="GitLabAuthenticationHandler" /> just
///         stamped onto it. Cloning from <c>request.Headers</c> at that point - as opposed to a snapshot taken
///         before the first send - would carry that header into the retry, which then gets a SECOND one
///         stamped on top when it passes through the auth handler again. Headers are therefore captured once,
///         before the first <c>base.SendAsync</c>/<c>base.Send</c> call, and every retry clone is built from
///         that pristine snapshot.
///     </para>
///     <para>
///         Content: some content (see <see cref="BorrowedStreamContent" />, used by the multipart file-upload
///         paths in <see cref="GitLabApiConnection" />) wraps a caller-owned stream this handler has no way to
///         rewind, and re-sending it after it has already been read once would silently ship a truncated or
///         empty body. Rather than try to detect "already consumed" after the fact, this handler only retries
///         when the content is either absent or a type it knows is backed by an in-memory buffer
///         (<see cref="CanReuseContent" />) - and even then, it copies that buffer into a brand new
///         <see cref="ByteArrayContent" /> for every attempt (see <see cref="CreateRetryContent" />) rather
///         than handing the same <see cref="HttpContent" /> instance to two different
///         <see cref="HttpRequestMessage" />s: disposing one request disposes its content, and two requests
///         sharing one content instance means whichever is disposed first poisons the other. Anything that
///         cannot be safely reproduced is let through unretried on the first transient response: a slow retry
///         is recoverable, a corrupted upload is not.
///     </para>
/// </remarks>
internal sealed class GitLabRetryHandler : DelegatingHandler
{
    /// <summary>Retries beyond the first attempt, so this many retries + 1 initial send = 4 attempts total.</summary>
    private const int MaxRetryAttempts = 3;

    /// <summary>Doubled on every attempt when there is no server-supplied <c>Retry-After</c> to honour.</summary>
    private const double BackoffMultiplier = 2d;

    private static readonly TimeSpan InitialBackoffDelay = TimeSpan.FromMilliseconds(250);

    private static readonly IReadOnlyList<KeyValuePair<string, IEnumerable<string>>> EmptyHeaders = [];

    /// <summary>
    ///     Caps the computed exponential backoff only - a server-supplied <c>Retry-After</c> is honoured as
    ///     given, uncapped, because it is the more authoritative signal than anything this handler could guess.
    /// </summary>
    private static readonly TimeSpan MaxBackoffDelay = TimeSpan.FromSeconds(4);

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (!IsRetryable(request.Method))
        {
            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }

        IReadOnlyList<KeyValuePair<string, IEnumerable<string>>> originalHeaders = SnapshotHeaders(request);

        HttpResponseMessage response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

        for (int attempt = 1;
             attempt <= MaxRetryAttempts && IsTransient(response.StatusCode);
             attempt++)
        {
            if (!CanReuseContent(request.Content))
            {
                break;
            }

            HttpRequestMessage retryRequest;
            try
            {
                HttpContent? retryContent =
                    await CreateRetryContentAsync(request.Content, cancellationToken).ConfigureAwait(false);
                retryRequest = CreateRetryRequest(request, originalHeaders, retryContent);
            }
            catch
            {
                response.Dispose();
                throw;
            }

            TimeSpan delay = GetRetryDelay(response, attempt);
            response.Dispose();

            if (delay > TimeSpan.Zero)
            {
                await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
            }

            try
            {
                response = await base.SendAsync(retryRequest, cancellationToken).ConfigureAwait(false);
                // The clone belongs to this handler. Preserve the caller's original request for diagnostics
                // before disposing the temporary request and its cloned content deterministically.
                response.RequestMessage = request;
            }
            finally
            {
                retryRequest.Dispose();
            }
        }

        return response;
    }

    protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (!IsRetryable(request.Method))
        {
            return base.Send(request, cancellationToken);
        }

        IReadOnlyList<KeyValuePair<string, IEnumerable<string>>> originalHeaders = SnapshotHeaders(request);

        HttpResponseMessage response = base.Send(request, cancellationToken);

        for (int attempt = 1;
             attempt <= MaxRetryAttempts && IsTransient(response.StatusCode);
             attempt++)
        {
            if (!CanReuseContent(request.Content))
            {
                break;
            }

            HttpRequestMessage retryRequest;
            try
            {
                HttpContent? retryContent = CreateRetryContent(request.Content);
                retryRequest = CreateRetryRequest(request, originalHeaders, retryContent);
            }
            catch
            {
                response.Dispose();
                throw;
            }

            TimeSpan delay = GetRetryDelay(response, attempt);
            response.Dispose();

            if (delay > TimeSpan.Zero)
            {
                // No synchronous, cancellable equivalent of Task.Delay; waiting on the token's own handle
                // blocks for the delay while still unblocking early - and throwing - if the caller cancels.
                cancellationToken.WaitHandle.WaitOne(delay);
                cancellationToken.ThrowIfCancellationRequested();
            }

            try
            {
                response = base.Send(retryRequest, cancellationToken);
                response.RequestMessage = request;
            }
            finally
            {
                retryRequest.Dispose();
            }
        }

        return response;
    }

    private static IReadOnlyList<KeyValuePair<string, IEnumerable<string>>> SnapshotHeaders(HttpRequestMessage request)
    {
        List<KeyValuePair<string, IEnumerable<string>>>? snapshot = null;

        foreach (KeyValuePair<string, IEnumerable<string>> header in request.Headers)
        {
            (snapshot ??= []).Add(new KeyValuePair<string, IEnumerable<string>>(header.Key, [.. header.Value]));
        }

        return snapshot ?? EmptyHeaders;
    }

    private static bool IsTransient(HttpStatusCode statusCode)
    {
        return statusCode is HttpStatusCode.TooManyRequests
            or HttpStatusCode.BadGateway
            or HttpStatusCode.ServiceUnavailable
            or HttpStatusCode.GatewayTimeout;
    }

    /// <summary>
    ///     A response can be lost after the server has already applied a POST, PUT, PATCH, or DELETE. Retrying
    ///     any of those automatically risks duplicating an externally visible mutation, so the default client
    ///     retries only methods defined as safe by HTTP semantics. Callers needing write retries can compose
    ///     an idempotency-aware policy above this library for the particular GitLab endpoint they use.
    /// </summary>
    private static bool IsRetryable(HttpMethod method)
    {
        return method == HttpMethod.Get || method == HttpMethod.Head || method == HttpMethod.Options;
    }

    private static HttpRequestMessage CreateRetryRequest(
        HttpRequestMessage originalRequest,
        IReadOnlyList<KeyValuePair<string, IEnumerable<string>>> originalHeaders,
        HttpContent? content)
    {
        HttpRequestMessage clone = new(originalRequest.Method, originalRequest.RequestUri)
        {
            Version = originalRequest.Version, VersionPolicy = originalRequest.VersionPolicy, Content = content
        };

        foreach (KeyValuePair<string, IEnumerable<string>> header in originalHeaders)
        {
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        return clone;
    }

    /// <summary>
    ///     True when <paramref name="content" /> is <see langword="null" />, or is a content type known to be
    ///     backed by an in-memory buffer - <see cref="ByteArrayContent" /> (and its derivatives
    ///     <see cref="StringContent" /> and <see cref="FormUrlEncodedContent" />) and <see cref="JsonContent" />,
    ///     the only content types this library's write paths construct outside of the multipart file upload.
    ///     <see cref="MultipartFormDataContent" /> (file uploads, wrapping <see cref="BorrowedStreamContent" />
    ///     over a caller-owned stream) and any other/custom <see cref="HttpContent" /> deliberately fail this
    ///     check: this handler cannot prove a stream inside them is still at its original position after the
    ///     first send.
    /// </summary>
    private static bool CanReuseContent(HttpContent? content)
    {
        return content is null or ByteArrayContent or JsonContent;
    }

    /// <summary>
    ///     Builds a brand new <see cref="ByteArrayContent" /> from <paramref name="content" />'s buffer for
    ///     every retry attempt, rather than handing the same <see cref="HttpContent" /> instance to more than
    ///     one <see cref="HttpRequestMessage" /> - see the type-level remarks for why that sharing is unsafe.
    /// </summary>
    private static async Task<ByteArrayContent?> CreateRetryContentAsync(HttpContent? content,
        CancellationToken cancellationToken)
    {
        if (content is null)
        {
            return null;
        }

        byte[] buffer = await content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
        return CloneWithHeaders(buffer, content);
    }

    /// <summary>Synchronous counterpart of <see cref="CreateRetryContentAsync" /> for the <see cref="Send" /> path.</summary>
    private static ByteArrayContent? CreateRetryContent(HttpContent? content)
    {
        if (content is null)
        {
            return null;
        }

        // Every type CanReuseContent accepts is backed by an in-memory buffer, so this never touches the
        // network and completes synchronously - safe to block on despite the async signature.
        byte[] buffer = content.ReadAsByteArrayAsync().GetAwaiter().GetResult();
        return CloneWithHeaders(buffer, content);
    }

    private static ByteArrayContent CloneWithHeaders(byte[] buffer, HttpContent source)
    {
        ByteArrayContent clone = new(buffer);

        foreach (KeyValuePair<string, IEnumerable<string>> header in source.Headers)
        {
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        return clone;
    }

    private static TimeSpan GetRetryDelay(HttpResponseMessage response, int attempt)
    {
        TimeSpan? retryAfter = GetRetryAfter(response);
        if (retryAfter is { } serverDelay)
        {
            return serverDelay;
        }

        TimeSpan backoff = InitialBackoffDelay * Math.Pow(BackoffMultiplier, attempt - 1);
        return backoff < MaxBackoffDelay ? backoff : MaxBackoffDelay;
    }

    /// <summary>
    ///     <c>Retry-After</c> is either delta-seconds or an HTTP-date (RFC 9110 10.2.3); the typed
    ///     <see cref="HttpResponseHeaders.RetryAfter" /> parses both into <see cref="RetryConditionHeaderValue" />,
    ///     which is worth the small allocation here - unlike <c>GitLabRateLimitSnapshot</c>, which hand-parses
    ///     the header because it runs on every single response, this only runs on the rare transient one.
    /// </summary>
    private static TimeSpan? GetRetryAfter(HttpResponseMessage response)
    {
        RetryConditionHeaderValue? retryAfter = response.Headers.RetryAfter;
        if (retryAfter is null)
        {
            return null;
        }

        if (retryAfter.Delta is { } delta)
        {
            return delta > TimeSpan.Zero ? delta : TimeSpan.Zero;
        }

        if (retryAfter.Date is not { } date)
        {
            return null;
        }

        TimeSpan delay = date - DateTimeOffset.UtcNow;
        return delay > TimeSpan.Zero ? delay : TimeSpan.Zero;
    }
}