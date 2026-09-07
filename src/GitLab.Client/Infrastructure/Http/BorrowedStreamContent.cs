using System.Net;

namespace GitLab.Client.Infrastructure.Http;

/// <summary>
///     <see cref="HttpContent" /> over a stream the caller still owns.
/// </summary>
/// <remarks>
///     <see cref="StreamContent" /> disposes the stream it was handed, and
///     <see cref="HttpRequestMessage.Dispose()" /> disposes the request's content, so the obvious spelling of a
///     multipart upload closes a stream the caller passed in and may want to rewind and send again. This one
///     reads the stream and leaves it exactly where it found it, which is the contract
///     <see cref="Abstractions.GitLabFileUpload" /> documents.
/// </remarks>
internal sealed class BorrowedStreamContent(Stream content) : HttpContent
{
    protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context)
    {
        return content.CopyToAsync(stream);
    }

    protected override Task SerializeToStreamAsync(
        Stream stream,
        TransportContext? context,
        CancellationToken cancellationToken)
    {
        return content.CopyToAsync(stream, cancellationToken);
    }

    protected override void SerializeToStream(Stream stream, TransportContext? context,
        CancellationToken cancellationToken)
    {
        content.CopyTo(stream);
    }

    protected override bool TryComputeLength(out long length)
    {
        // A seekable stream lets the request carry a real Content-Length, which keeps the upload off
        // chunked transfer encoding - some reverse proxies in front of self-managed instances reject it.
        if (content.CanSeek)
        {
            length = content.Length - content.Position;
            return true;
        }

        length = 0;
        return false;
    }
}