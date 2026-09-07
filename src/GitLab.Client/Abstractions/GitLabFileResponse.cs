using System.Net;
using System.Net.Http.Headers;

namespace GitLab.Client.Abstractions;

/// <summary>
///     A GitLab response whose body is bytes rather than JSON - a repository archive, a raw file blob, a
///     merge request's raw diffs, a job artifact or a job trace log.
/// </summary>
/// <remarks>
///     <para>
///         This type deliberately OWNS the underlying HTTP response and the per-operation
///         <see cref="CancellationTokenSource" /> that produced it, which is why the transport returns it
///         instead of a bare <see cref="Stream" />. Every call runs under a linked token source because
///         <see cref="HttpClient.Timeout" /> stops applying once the response headers arrive; if that source
///         and the response were disposed when the method returned, the stream handed back to the caller
///         would already be dead. Disposing this object is what releases all three, in the only order that
///         is safe: body first, then the response, then the token source.
///     </para>
///     <para>
///         Always dispose it - <c>await using</c> for the asynchronous path - and do so before the process
///         exits, or the pooled connection the body is streaming over is never returned.
///     </para>
/// </remarks>
public sealed class GitLabFileResponse : IAsyncDisposable, IDisposable
{
    private readonly CancellationTokenSource? _operation;
    private readonly HttpResponseMessage? _response;
    private bool _disposed;

    /// <summary>
    ///     Creates a response over a caller-supplied stream. Exists so consumers can build a test double for
    ///     <see cref="IGitLabApiConnection" /> without reaching for <c>System.Net.Http</c>; the client's own
    ///     download path uses the internal overload that also carries the HTTP response it must keep alive.
    /// </summary>
    /// <param name="content">The body. Disposed when this instance is disposed.</param>
    /// <param name="contentType">The media type GitLab reported, without parameters.</param>
    /// <param name="contentLength">The body length in bytes, when GitLab reported one.</param>
    /// <param name="fileName">The file name from <c>Content-Disposition</c>, when GitLab sent one.</param>
    public GitLabFileResponse(Stream content, string? contentType, long? contentLength, string? fileName)
        : this(content, contentType, contentLength, fileName, HttpStatusCode.OK, null, null)
    {
    }

    private GitLabFileResponse(
        Stream content,
        string? contentType,
        long? contentLength,
        string? fileName,
        HttpStatusCode statusCode,
        HttpResponseMessage? response,
        CancellationTokenSource? operation)
    {
        ArgumentNullException.ThrowIfNull(content);

        Content = content;
        ContentType = contentType;
        ContentLength = contentLength;
        FileName = fileName;
        StatusCode = statusCode;
        _response = response;
        _operation = operation;
    }

    /// <summary>
    ///     The response body, streamed rather than buffered: it is read straight off the connection and never
    ///     passes through the JSON serializer. Valid until this instance is disposed.
    /// </summary>
    public Stream Content { get; }

    /// <summary>The media type GitLab reported, without parameters (<c>application/gzip</c>, <c>text/plain</c>).</summary>
    public string? ContentType { get; }

    /// <summary>
    ///     The body length in bytes when GitLab reported one. Null for a chunked response, which is what
    ///     GitLab sends for an archive it is generating on the fly.
    /// </summary>
    public long? ContentLength { get; }

    /// <summary>
    ///     The file name parsed out of the <c>Content-Disposition</c> header, when present, preferring the
    ///     RFC 5987 <c>filename*</c> form. Any directory component is stripped: the value is entirely
    ///     server-controlled, and callers routinely use it to name a file on disk.
    /// </summary>
    public string? FileName { get; }

    /// <summary>The success status code GitLab answered with.</summary>
    public HttpStatusCode StatusCode { get; }

    /// <summary>Releases the body, the HTTP response and the per-operation cancellation source.</summary>
    /// <returns>A task that completes once the body has been released.</returns>
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        // Body first: closing it is what returns the pooled connection. The token source goes last,
        // because tearing it down first would drop the abort registration the body read still relies on.
        await Content.DisposeAsync().ConfigureAwait(false);
        _response?.Dispose();
        _operation?.Dispose();

        GC.SuppressFinalize(this);
    }

    /// <summary>Releases the body, the HTTP response and the per-operation cancellation source.</summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        Content.Dispose();
        _response?.Dispose();
        _operation?.Dispose();

        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     The transport's own entry point: takes ownership of <paramref name="response" /> and
    ///     <paramref name="operation" /> so both outlive the call that produced the stream.
    /// </summary>
    internal static GitLabFileResponse Create(
        HttpResponseMessage response,
        Stream content,
        CancellationTokenSource operation)
    {
        HttpContentHeaders headers = response.Content.Headers;

        return new GitLabFileResponse(
            content,
            headers.ContentType?.MediaType,
            headers.ContentLength,
            ReadFileName(headers.ContentDisposition),
            response.StatusCode,
            response,
            operation);
    }

    private static string? ReadFileName(ContentDispositionHeaderValue? contentDisposition)
    {
        if (contentDisposition is null)
        {
            return null;
        }

        // filename* wins: it is the RFC 5987 form, already percent-decoded by the header parser, and the
        // only one that can carry a non-ASCII name. GitLab sends both for archives.
        string? fileName = contentDisposition.FileNameStar ?? Unquote(contentDisposition.FileName);

        if (string.IsNullOrWhiteSpace(fileName))
        {
            return null;
        }

        string stripped = StripDirectory(fileName);

        return stripped.Length == 0 ? null : stripped;
    }

    private static string? Unquote(string? value)
    {
        return value is { Length: >= 2 } && value[0] == '"' && value[^1] == '"'
            ? value[1..^1]
            : value;
    }

    /// <summary>
    ///     Keeps only the last path component. Deliberately not <see cref="Path.GetFileName(string)" />,
    ///     whose separator set is platform-dependent: a backslash is a legal file-name character on Linux, so
    ///     the same header would be sanitized on Windows and not on Linux.
    /// </summary>
    private static string StripDirectory(string fileName)
    {
        int lastSeparator = fileName.LastIndexOfAny(['/', '\\']);

        return lastSeparator < 0 ? fileName : fileName[(lastSeparator + 1)..];
    }
}