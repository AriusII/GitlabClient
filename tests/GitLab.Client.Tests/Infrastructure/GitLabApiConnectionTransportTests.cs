using System.Net;
using System.Net.Http.Headers;
using System.Text;

using GitLab.Client.Abstractions;
using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Infrastructure.Serialization;
using GitLab.Client.Models;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Infrastructure;

/// <summary>
///     The non-JSON half of the transport: raw/binary downloads, multipart uploads, <c>HEAD</c> probes and
///     <c>PATCH</c>. Mocked at the <see cref="HttpMessageHandler" /> level like every other transport test,
///     so what is actually verified is the bytes on the wire.
/// </summary>
public sealed class GitLabApiConnectionTransportTests
{
    private const string ProjectJson = """
                                       {
                                         "id": 42,
                                         "name": "Example",
                                         "path_with_namespace": "group/example",
                                         "visibility": "public",
                                         "web_url": "https://gitlab.example/group/example"
                                       }
                                       """;

    private static readonly Uri BaseAddress = new("https://gitlab.example/api/v4/");

    private static readonly Uri ArchiveRoute = new("projects/42/repository/archive", UriKind.Relative);

    private static readonly Uri UploadsRoute = new("projects/42/uploads", UriKind.Relative);

    private static readonly Uri BranchProbeRoute = new("projects/42/repository/branches/main", UriKind.Relative);

    private static readonly Uri UserStatusRoute = new("user/status", UriKind.Relative);

    private static readonly byte[] ArchiveBytes = [0x1F, 0x8B, 0x08, 0x00, 0x11, 0x22, 0x33, 0x44];

    [Fact]
    public async Task GetFileAsync_StreamsTheBody_AndKeepsItAliveAfterTheCallReturns()
    {
        // The point of returning GitLabFileResponse rather than a bare Stream: the response and the linked
        // CancellationTokenSource must outlive the method, or the stream the caller holds is already dead.
        TrackingStream body = new(ArchiveBytes);

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StreamContent(body)
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);

        GitLabFileResponse file =
            await connection.GetFileAsync(ArchiveRoute, TestContext.Current.CancellationToken);

        // Deliberately after the call returned: this is the assertion the design exists for.
        Assert.False(body.IsDisposed);

        using MemoryStream copy = new();
        await file.Content.CopyToAsync(copy, TestContext.Current.CancellationToken);

        Assert.Equal(ArchiveBytes, copy.ToArray());

        await file.DisposeAsync();

        Assert.True(body.IsDisposed);
    }

    [Fact]
    public async Task GetFileAsync_ReportsTheMediaTypeAndLength()
    {
        using StubHttpMessageHandler handler = new(_ =>
        {
            HttpResponseMessage response = new(HttpStatusCode.OK) { Content = new ByteArrayContent(ArchiveBytes) };

            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/gzip");
            return response;
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);

        using GitLabFileResponse file =
            await connection.GetFileAsync(ArchiveRoute, TestContext.Current.CancellationToken);

        Assert.Equal("application/gzip", file.ContentType);
        Assert.Equal(ArchiveBytes.Length, file.ContentLength);
        Assert.Equal(HttpStatusCode.OK, file.StatusCode);
        Assert.Null(file.FileName);
    }

    [Fact]
    public async Task GetFileAsync_ParsesTheQuotedContentDispositionFileName()
    {
        using GitLabFileResponse file =
            await DownloadWithDispositionAsync("attachment; filename=\"gitlab-master-archive.tar.gz\"");

        Assert.Equal("gitlab-master-archive.tar.gz", file.FileName);
    }

    [Fact]
    public async Task GetFileAsync_PrefersTheRfc5987FileName_AndStripsAnyDirectory()
    {
        // filename* is the only form that can carry a non-ASCII name, and it is entirely server-controlled -
        // hence the directory strip, since callers routinely use this value to name a file on disk.
        using GitLabFileResponse file = await DownloadWithDispositionAsync(
            "attachment; filename=\"fallback.txt\"; filename*=UTF-8''caf%C3%A9%2Frapport.txt");

        Assert.Equal("rapport.txt", file.FileName);
    }

    [Fact]
    public async Task GetFileAsync_MapsANonSuccessStatusOntoTheTypedException()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent("""{ "message": "404 Project Not Found" }""", Encoding.UTF8,
                "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);

        GitLabNotFoundException exception = await Assert.ThrowsAsync<GitLabNotFoundException>(() =>
            connection.GetFileAsync(ArchiveRoute, TestContext.Current.CancellationToken));

        Assert.Equal("404 Project Not Found", exception.Message);
        Assert.Equal(HttpMethod.Get, exception.RequestMethod);
    }

    [Fact]
    public async Task CancelAfter_WithAnInfiniteDelay_StandsDownAPendingTimeout()
    {
        // Load-bearing for GetFileAsync: once the headers are in, it stands the operation timeout down so a
        // large artifact download is not aborted mid-stream, while keeping the source linked to the caller's
        // token. If this framework behaviour ever changed, every big download would fail after ~100 seconds.
        using CancellationTokenSource source = new();
        source.CancelAfter(TimeSpan.FromMilliseconds(20));
        source.CancelAfter(Timeout.InfiniteTimeSpan);

        await Task.Delay(TimeSpan.FromMilliseconds(200), TestContext.Current.CancellationToken);

        Assert.False(source.IsCancellationRequested);
    }

    [Fact]
    public async Task PostFileAsync_SendsAWellFormedMultipartBody()
    {
        using CapturingHttpMessageHandler handler = new(() => new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = new StringContent(ProjectJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);

        using MemoryStream content = new("PNG-BYTES"u8.ToArray());

        GitLabFileUpload upload = new() { Content = content, FileName = "logo.png", ContentType = "image/png" };

        GitLabProject project = await connection.PostFileAsync(
            UploadsRoute,
            upload,
            new Dictionary<string, string>(StringComparer.Ordinal) { ["url"] = "https://gitlab.example/img" },
            GitLabJsonContext.Default.GitLabProject,
            TestContext.Current.CancellationToken);

        Assert.Equal(42, project.Id);
        Assert.Equal(HttpMethod.Post, handler.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/42/uploads", handler.RequestUri?.AbsoluteUri);
        string contentType = handler.RequestContentType ?? string.Empty;
        Assert.StartsWith("multipart/form-data", contentType, StringComparison.Ordinal);
        Assert.Contains("boundary=", contentType, StringComparison.Ordinal);

        string body = handler.RequestBody;
        Assert.Contains("name=url", body.Replace("\"", string.Empty, StringComparison.Ordinal),
            StringComparison.Ordinal);
        Assert.Contains("https://gitlab.example/img", body, StringComparison.Ordinal);
        Assert.Contains("name=file", body.Replace("\"", string.Empty, StringComparison.Ordinal),
            StringComparison.Ordinal);
        Assert.Contains("filename=logo.png", body.Replace("\"", string.Empty, StringComparison.Ordinal),
            StringComparison.Ordinal);
        Assert.Contains("Content-Type: image/png", body, StringComparison.Ordinal);
        Assert.Contains("PNG-BYTES", body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task PostFileAsync_LeavesTheCallersStreamOpen()
    {
        // GitLabFileUpload documents that the stream is borrowed, not owned. StreamContent would have closed
        // it, since HttpRequestMessage.Dispose disposes the whole multipart body.
        using CapturingHttpMessageHandler handler = new(() => new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = new StringContent(ProjectJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);

        TrackingStream content = new("PNG-BYTES"u8.ToArray());

        await connection.PostFileAsync(
            UploadsRoute,
            new GitLabFileUpload { Content = content, FileName = "logo.png" },
            null,
            GitLabJsonContext.Default.GitLabProject,
            TestContext.Current.CancellationToken);

        Assert.False(content.IsDisposed);
        Assert.True(content.CanRead);
    }

    [Fact]
    public async Task PutFileAsync_UsesThePutVerb()
    {
        using CapturingHttpMessageHandler handler = new(() => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(ProjectJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);

        using MemoryStream content = new("AVATAR"u8.ToArray());

        await connection.PutFileAsync(
            new Uri("projects/42", UriKind.Relative),
            new GitLabFileUpload { Content = content, FileName = "avatar.png", FieldName = "avatar" },
            null,
            GitLabJsonContext.Default.GitLabProject,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.Method);
        Assert.Contains("name=avatar", handler.RequestBody.Replace("\"", string.Empty, StringComparison.Ordinal),
            StringComparison.Ordinal);
    }

    [Fact]
    public async Task PostFileAsync_WithAResponse_CompletesEvenWhenSlowerThanTheFixedHttpClientTimeout()
    {
        // Regression test for the upload-side counterpart of GetFileAsync's post-header CancelAfter: sending
        // a large multipart body is the payload-size-dependent phase for an upload, so - like a big download
        // past the headers - it must survive outliving the fixed httpClient.Timeout many times over instead
        // of being spuriously cancelled. This exercises SendFileAsync<TResponse> via the public overload.
        using DelayingHttpMessageHandler handler = new(TimeSpan.FromMilliseconds(300),
            () => new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(ProjectJson, Encoding.UTF8, "application/json")
            });

        using HttpClient httpClient = new(handler)
        {
            BaseAddress = BaseAddress, Timeout = TimeSpan.FromMilliseconds(50)
        };
        GitLabApiConnection connection = new(httpClient);

        using MemoryStream content = new("PNG-BYTES"u8.ToArray());
        GitLabFileUpload upload = new() { Content = content, FileName = "logo.png" };

        GitLabProject project = await connection.PostFileAsync(
            UploadsRoute,
            upload,
            null,
            GitLabJsonContext.Default.GitLabProject,
            TestContext.Current.CancellationToken);

        Assert.Equal(42, project.Id);
    }

    [Fact]
    public async Task PostFileAsync_NonGeneric_CompletesEvenWhenSlowerThanTheFixedHttpClientTimeout()
    {
        // Same regression as above, for the non-generic overload's own CreateUnboundedOperation call site.
        using DelayingHttpMessageHandler handler = new(TimeSpan.FromMilliseconds(300),
            static () => new HttpResponseMessage(HttpStatusCode.Created));

        using HttpClient httpClient = new(handler)
        {
            BaseAddress = BaseAddress, Timeout = TimeSpan.FromMilliseconds(50)
        };
        GitLabApiConnection connection = new(httpClient);

        using MemoryStream content = new("PNG-BYTES"u8.ToArray());
        GitLabFileUpload upload = new() { Content = content, FileName = "logo.png" };

        // No exception is the assertion: the old CreateOperationTimeout-bound call would have thrown
        // TaskCanceledException here well before the handler's 300ms delay elapsed.
        await connection.PostFileAsync(UploadsRoute, upload, null, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task PutFileAsync_NonGeneric_CompletesEvenWhenSlowerThanTheFixedHttpClientTimeout()
    {
        // Same regression as above, for PutFileAsync's own CreateUnboundedOperation call site.
        using DelayingHttpMessageHandler handler = new(TimeSpan.FromMilliseconds(300),
            static () => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler)
        {
            BaseAddress = BaseAddress, Timeout = TimeSpan.FromMilliseconds(50)
        };
        GitLabApiConnection connection = new(httpClient);

        using MemoryStream content = new("AVATAR"u8.ToArray());

        await connection.PutFileAsync(
            new Uri("projects/42", UriKind.Relative),
            new GitLabFileUpload { Content = content, FileName = "avatar.png", FieldName = "avatar" },
            null,
            TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task PostFileAsync_StillHonorsTheCallersOwnCancellation()
    {
        // Removing the fixed httpClient.Timeout bound must not remove cancellation altogether: the operation
        // source stays linked to the caller's own token, exactly as GetFileAsync's post-header source does.
        using DelayingHttpMessageHandler handler = new(TimeSpan.FromSeconds(30),
            () => new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(ProjectJson, Encoding.UTF8, "application/json")
            });

        using HttpClient httpClient = new(handler)
        {
            BaseAddress = BaseAddress, Timeout = Timeout.InfiniteTimeSpan
        };
        GitLabApiConnection connection = new(httpClient);

        using MemoryStream content = new("PNG-BYTES"u8.ToArray());
        GitLabFileUpload upload = new() { Content = content, FileName = "logo.png" };

        using CancellationTokenSource cancellation = new(TimeSpan.FromMilliseconds(100));

        OperationCanceledException exception = await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            connection.PostFileAsync(UploadsRoute, upload, null, GitLabJsonContext.Default.GitLabProject,
                cancellation.Token));

        // Mirrors GitLabApiConnectionTests' GetAsync_ReportsCallerCancellation test: a genuine caller
        // cancellation must not be rewritten into the "our own budget elapsed" timeout shape.
        Assert.False(exception.InnerException is TimeoutException);
    }

    [Fact]
    public async Task HeadAsync_ReportsNotExists_OnA404_WithoutThrowing()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NotFound));

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);

        GitLabHeadResponse probe = await connection.HeadAsync(BranchProbeRoute,
            TestContext.Current.CancellationToken);

        Assert.False(probe.Exists);
        Assert.Equal(HttpStatusCode.NotFound, probe.StatusCode);
        Assert.Equal(HttpMethod.Head, handler.LastRequest?.Method);
    }

    [Fact]
    public async Task HeadAsync_ReportsExists_AndSurfacesTheGitLabMetadataHeaders()
    {
        using StubHttpMessageHandler handler = new(_ =>
        {
            HttpResponseMessage response = new(HttpStatusCode.OK);
            response.Headers.TryAddWithoutValidation("X-Gitlab-Blob-Id", "79f7bbd25901e8334750839545a9bd021f0e4c83");
            response.Headers.TryAddWithoutValidation("X-Gitlab-Size", "1024");
            return response;
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);

        GitLabHeadResponse probe = await connection.HeadAsync(BranchProbeRoute,
            TestContext.Current.CancellationToken);

        Assert.True(probe.Exists);
        Assert.Equal("1024", probe.GetHeaderValue("x-gitlab-size"));
        Assert.Equal("79f7bbd25901e8334750839545a9bd021f0e4c83", probe.GetHeaderValue("X-GitLab-Blob-Id"));
        Assert.Null(probe.GetHeaderValue("X-Gitlab-Absent"));
    }

    [Fact]
    public async Task HeadAsync_StillThrows_OnEveryOtherFailure()
    {
        // The whole point of special-casing 404: a 403 must never be reported as "it does not exist".
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Forbidden));

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);

        GitLabForbiddenException exception = await Assert.ThrowsAsync<GitLabForbiddenException>(() =>
            connection.HeadAsync(BranchProbeRoute, TestContext.Current.CancellationToken));

        Assert.Equal(HttpMethod.Head, exception.RequestMethod);
    }

    [Fact]
    public async Task PatchAsync_SendsTheBody_AndDeserializesTheResponse()
    {
        using CapturingHttpMessageHandler handler = new(() => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(ProjectJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);

        GitLabProject project = await connection.PatchAsync(
            UserStatusRoute,
            new UpdateInvitationRequest { AccessLevel = 30 },
            GitLabJsonContext.Default.UpdateInvitationRequest,
            GitLabJsonContext.Default.GitLabProject,
            TestContext.Current.CancellationToken);

        Assert.Equal(42, project.Id);
        Assert.Equal(HttpMethod.Patch, handler.Method);
        Assert.Equal("https://gitlab.example/api/v4/user/status", handler.RequestUri?.AbsoluteUri);
        Assert.Contains("\"access_level\":30", handler.RequestBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task PatchAsync_WithABody_AndNoResponse_DoesNotTouchTheDeserializer()
    {
        using CapturingHttpMessageHandler handler = new(() => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);

        await connection.PatchAsync(
            UserStatusRoute,
            new UpdateInvitationRequest { MemberRoleId = 7 },
            GitLabJsonContext.Default.UpdateInvitationRequest,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Patch, handler.Method);
        Assert.Contains("\"member_role_id\":7", handler.RequestBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task PatchAsync_BodyLess_SendsNoContent()
    {
        // PATCH /users/{id}/disable_two_factor is the shape this exists for: no payload, no response body.
        using CapturingHttpMessageHandler handler = new(() => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);

        await connection.PatchAsync(new Uri("users/7/disable_two_factor", UriKind.Relative),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Patch, handler.Method);
        Assert.Null(handler.RequestContentType);
    }

    [Fact]
    public async Task PatchAsync_MapsANonSuccessStatusOntoTheTypedException()
    {
        using CapturingHttpMessageHandler handler = new(() => new HttpResponseMessage(HttpStatusCode.Conflict)
        {
            Content = new StringContent("""{ "message": "already disabled" }""", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);

        GitLabConflictException exception = await Assert.ThrowsAsync<GitLabConflictException>(() =>
            connection.PatchAsync(UserStatusRoute, TestContext.Current.CancellationToken));

        Assert.Equal("already disabled", exception.Message);
        Assert.Equal(HttpMethod.Patch, exception.RequestMethod);
    }

    [Fact]
    public async Task ResponseBody_IsCappedWithAnElisionMarker()
    {
        // A proxy in front of a self-managed instance answering with a multi-megabyte HTML page would
        // otherwise be retained for the lifetime of the exception.
        string oversized = new('x', (16 * 1024) + 7);

        GitLabApiException exception = await CaptureAsync(HttpStatusCode.BadGateway, oversized);

        Assert.NotNull(exception.ResponseBody);
        Assert.StartsWith(new string('x', 512), exception.ResponseBody, StringComparison.Ordinal);
        Assert.Contains("more characters elided", exception.ResponseBody, StringComparison.Ordinal);
        Assert.True(exception.ResponseBody.Length < oversized.Length);
        Assert.Contains($"[{oversized.Length - (8 * 1024)} more characters elided]", exception.ResponseBody,
            StringComparison.Ordinal);

        // The message is built from the retained body, so it must not smuggle the full payload back in.
        Assert.True(exception.Message.Length < oversized.Length);
    }

    [Fact]
    public async Task ResponseBody_IsRetainedVerbatim_WhenItFitsUnderTheCap()
    {
        string body = """{ "message": "boom" }""";

        GitLabApiException exception = await CaptureAsync(HttpStatusCode.InternalServerError, body);

        Assert.Equal(body, exception.ResponseBody);
        Assert.Equal("boom", exception.Message);
    }

    [Fact]
    public async Task ErrorMessage_IsStillParsed_FromABodyLargerThanTheCap()
    {
        // Truncating before parsing would leave invalid JSON and lose the one useful sentence in the body.
        string body = $$"""{ "message": "boom", "padding": "{{new string('p', 12 * 1024)}}" }""";

        GitLabApiException exception = await CaptureAsync(HttpStatusCode.InternalServerError, body);

        Assert.Equal("boom", exception.Message);
        Assert.NotNull(exception.ResponseBody);
        Assert.True(exception.ResponseBody.Length < body.Length);
    }

    private static async Task<GitLabFileResponse> DownloadWithDispositionAsync(string contentDisposition)
    {
        using StubHttpMessageHandler handler = new(_ =>
        {
            HttpResponseMessage response = new(HttpStatusCode.OK) { Content = new ByteArrayContent(ArchiveBytes) };

            response.Content.Headers.TryAddWithoutValidation("Content-Disposition", contentDisposition);
            return response;
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);

        return await connection.GetFileAsync(ArchiveRoute, TestContext.Current.CancellationToken)
            .ConfigureAwait(false);
    }

    private static async Task<GitLabApiException> CaptureAsync(HttpStatusCode statusCode, string body)
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);

        return await Assert.ThrowsAnyAsync<GitLabApiException>(() =>
                connection.GetAsync(new Uri("projects/42", UriKind.Relative),
                    GitLabJsonContext.Default.GitLabProject, TestContext.Current.CancellationToken))
            .ConfigureAwait(false);
    }

    /// <summary>
    ///     Reads the request body before answering, which a synchronous
    ///     <see cref="StubHttpMessageHandler" /> cannot do: <see cref="HttpRequestMessage.Dispose()" />
    ///     disposes its content, so a multipart body is gone by the time the call returns.
    /// </summary>
    private sealed class CapturingHttpMessageHandler(Func<HttpResponseMessage> respond) : HttpMessageHandler
    {
        public HttpMethod? Method { get; private set; }

        public Uri? RequestUri { get; private set; }

        public string? RequestContentType { get; private set; }

        public string RequestBody { get; private set; } = string.Empty;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Method = request.Method;
            RequestUri = request.RequestUri;
            RequestContentType = request.Content?.Headers.ContentType?.ToString();

            if (request.Content is not null)
            {
                RequestBody = await request.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            }

            HttpResponseMessage response = respond();
            response.RequestMessage ??= request;
            return response;
        }
    }

    /// <summary>
    ///     Stands in for a slow-but-healthy upload: drains the multipart body the way a real socket write
    ///     would, then delays before answering. The delay is what a fixed httpClient.Timeout would have to
    ///     race against, so honoring it or not is exactly the behaviour under test in the timeout/cancellation
    ///     tests above.
    /// </summary>
    private sealed class DelayingHttpMessageHandler(TimeSpan delay, Func<HttpResponseMessage> respond)
        : HttpMessageHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (request.Content is not null)
            {
                await request.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            }

            await Task.Delay(delay, cancellationToken).ConfigureAwait(false);

            HttpResponseMessage response = respond();
            response.RequestMessage ??= request;
            return response;
        }
    }

    /// <summary>A stream that remembers whether anyone disposed it - the whole assertion in two tests.</summary>
    private sealed class TrackingStream(byte[] content) : MemoryStream(content, false)
    {
        public bool IsDisposed { get; private set; }

        public override ValueTask DisposeAsync()
        {
            IsDisposed = true;
            return base.DisposeAsync();
        }

        protected override void Dispose(bool disposing)
        {
            IsDisposed = true;
            base.Dispose(disposing);
        }
    }
}