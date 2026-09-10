using System.Net;
using System.Text;

using GitLab.Client.Abstractions;
using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Configuration;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Infrastructure.RateLimiting;
using GitLab.Client.Models;

using Microsoft.Extensions.DependencyInjection;

using GitLabJsonContext = GitLab.Client.Serialization.GitLabJsonContext;

namespace GitLab.Client.Tests.Infrastructure.Http;

public sealed class GitLabTransportRegressionTests
{
    private static readonly Uri BaseAddress = new("https://gitlab.example/api/v4/");

    [Fact]
    public async Task SendAsync_CancellationInterruptsTheRetryAfterDelay_BeforeAnotherAttempt()
    {
        using SequencedHttpMessageHandler primary = new(static (_, _) => RetryAfterResponse());
        using GitLabRetryHandler retry = new() { InnerHandler = primary };
        using HttpClient client = new(retry) { BaseAddress = BaseAddress };
        using CancellationTokenSource cancellation = CancellationTokenSource.CreateLinkedTokenSource(
            TestContext.Current.CancellationToken);

        Task<HttpResponseMessage> send = client.GetAsync(new Uri("projects/1", UriKind.Relative), cancellation.Token);

        await primary.FirstRequestStarted.Task.WaitAsync(TestContext.Current.CancellationToken);
        await cancellation.CancelAsync().ConfigureAwait(true);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await send.ConfigureAwait(true))
            .ConfigureAwait(true);
        Assert.Equal(1, primary.SendCount);
    }

    [Fact]
    public async Task SendAsync_DisposesTheDiscardedTransientResponseBeforeReturningTheRetryResult()
    {
        using TrackingContent transientContent = new("transient"u8.ToArray());
        using SequencedHttpMessageHandler primary = new((_, attempt) => attempt == 0
            ? new HttpResponseMessage(HttpStatusCode.ServiceUnavailable) { Content = transientContent }
            : new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{}") });
        using GitLabRetryHandler retry = new() { InnerHandler = primary };
        using HttpClient client = new(retry) { BaseAddress = BaseAddress };

        using HttpResponseMessage response = await client.GetAsync(
            new Uri("projects/1", UriKind.Relative), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(2, primary.SendCount);
        Assert.True(transientContent.IsDisposed);
    }

    [Theory]
    [InlineData("PUT")]
    [InlineData("PATCH")]
    [InlineData("DELETE")]
    public async Task SendAsync_DoesNotRetryTransientWriteRequests(string method)
    {
        using SequencedHttpMessageHandler primary = new(static (_, _) =>
            new HttpResponseMessage(HttpStatusCode.ServiceUnavailable) { Content = new StringContent("{}") });
        using GitLabRetryHandler retry = new() { InnerHandler = primary };
        using HttpClient client = new(retry) { BaseAddress = BaseAddress };
        using HttpRequestMessage request = new(new HttpMethod(method), new Uri("projects/1", UriKind.Relative));

        using HttpResponseMessage response = await client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        Assert.Equal(1, primary.SendCount);
    }

    [Fact]
    public async Task GetAsync_StopsReadingAnOversizedErrorEntityAfterTheDiagnosticPrefix()
    {
        byte[] payload = Encoding.UTF8.GetBytes(new string('x',
            GitLabApiExceptionFactory.MaxRetainedResponseBodyLength * 128));
        using CountingReadStream source = new(payload);
        using StaticHttpMessageHandler primary = new(() =>
            new HttpResponseMessage(HttpStatusCode.BadGateway) { Content = new StreamContent(source) });
        using HttpClient client = new(primary) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(client);

        GitLabServerException exception = await Assert.ThrowsAsync<GitLabServerException>(() => connection.GetAsync(
            new Uri("projects/1", UriKind.Relative), GitLabJsonContext.Default.GitLabProject,
            TestContext.Current.CancellationToken)).ConfigureAwait(true);

        Assert.NotNull(exception.ResponseBody);
        Assert.Contains("response body truncated", exception.ResponseBody, StringComparison.Ordinal);
        Assert.InRange(source.BytesRead, 1,
            GitLabApiExceptionFactory.MaxRetainedResponseBodyLength + 1024);
        Assert.True(source.BytesRead < payload.Length);
    }

    [Fact]
    public async Task PostFileAsync_AfterSlowUpload_UsesTheOriginalTimeoutForAStalledResponseBody()
    {
        // The body clock must start only after the large upload has reached response headers. The upload itself
        // is deliberately unbounded, but an accepted upload must not leave JSON deserialization waiting forever
        // when a proxy sends successful headers and then stops producing its response entity.
        using NeverCompletingReadStream stalledResponseBody = new();
        using StaticHttpMessageHandler primary = new(() => new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = new StreamContent(stalledResponseBody)
        });
        using HttpClient client = new(primary) { BaseAddress = BaseAddress, Timeout = TimeSpan.FromMilliseconds(100) };
        GitLabApiConnection connection = new(client);
        using MemoryStream fileContent = new("file"u8.ToArray());
        GitLabFileUpload upload = new() { Content = fileContent, FileName = "project.tar.gz" };

        Task<GitLabProject> request = connection.PostFileAsync(
            new Uri("projects/import", UriKind.Relative),
            upload,
            null,
            GitLabJsonContext.Default.GitLabProject,
            TestContext.Current.CancellationToken);

        TaskCanceledException exception = await Assert.ThrowsAsync<TaskCanceledException>(async () =>
                await request.WaitAsync(TimeSpan.FromSeconds(2), TestContext.Current.CancellationToken)
                    .ConfigureAwait(true))
            .ConfigureAwait(true);

        Assert.IsType<TimeoutException>(exception.InnerException);
    }

    [Fact]
    public async Task SendAsync_RateLimitTrackerReflectsTheLastRetryAttempt()
    {
        using SequencedHttpMessageHandler primary = new(static (_, attempt) => attempt == 0
            ? RateLimitedResponse(HttpStatusCode.ServiceUnavailable, 7)
            : RateLimitedResponse(HttpStatusCode.OK, 6));

        ServiceCollection services = new();
        services.AddGitLabClient(options =>
            {
                options.AccessToken = "glpat-test-token";
                options.BaseAddress = BaseAddress;
            })
            .ConfigurePrimaryHttpMessageHandler(() => primary);

        using ServiceProvider provider = services.BuildServiceProvider();
        IGitLabRateLimitTracker tracker = provider.GetRequiredService<IGitLabRateLimitTracker>();
        using HttpClient client = provider.GetRequiredService<IHttpClientFactory>()
            .CreateClient(GitLabClientDefaults.HttpClientName);

        using HttpResponseMessage response = await client.GetAsync(
            new Uri("projects/1", UriKind.Relative), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(2, primary.SendCount);
        Assert.NotNull(tracker.Current);
        Assert.Equal(600, tracker.Current.Value.Limit);
        Assert.Equal(6, tracker.Current.Value.Remaining);
    }

    private static HttpResponseMessage RetryAfterResponse()
    {
        HttpResponseMessage response = new(HttpStatusCode.ServiceUnavailable) { Content = new StringContent("{}") };
        response.Headers.TryAddWithoutValidation("Retry-After", "60");
        return response;
    }

    private static HttpResponseMessage RateLimitedResponse(HttpStatusCode statusCode, int remaining)
    {
        HttpResponseMessage response = new(statusCode) { Content = new StringContent("{}") };
        response.Headers.TryAddWithoutValidation("Retry-After", "0");
        response.Headers.TryAddWithoutValidation("RateLimit-Limit", "600");
        response.Headers.TryAddWithoutValidation("RateLimit-Remaining", remaining.ToString());
        return response;
    }

    private sealed class SequencedHttpMessageHandler(Func<HttpRequestMessage, int, HttpResponseMessage> respond)
        : HttpMessageHandler
    {
        private int _sendCount;

        public TaskCompletionSource FirstRequestStarted { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public int SendCount => Volatile.Read(ref _sendCount);

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            int attempt = Interlocked.Increment(ref _sendCount) - 1;
            FirstRequestStarted.TrySetResult();

            HttpResponseMessage response = respond(request, attempt);
            response.RequestMessage ??= request;
            return Task.FromResult(response);
        }
    }

    private sealed class StaticHttpMessageHandler(Func<HttpResponseMessage> respond) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            HttpResponseMessage response = respond();
            response.RequestMessage ??= request;
            return Task.FromResult(response);
        }
    }

    private sealed class TrackingContent(byte[] content) : ByteArrayContent(content)
    {
        public bool IsDisposed { get; private set; }

        protected override void Dispose(bool disposing)
        {
            IsDisposed |= disposing;
            base.Dispose(disposing);
        }
    }

    private sealed class CountingReadStream(byte[] content) : MemoryStream(content, false)
    {
        public int BytesRead { get; private set; }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int read = base.Read(buffer, offset, count);
            BytesRead += read;
            return read;
        }
    }

    private sealed class NeverCompletingReadStream : Stream
    {
        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => throw new NotSupportedException();

        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override void Flush()
        {
        }

        public override Task FlushAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override ValueTask<int> ReadAsync(Memory<byte> buffer,
            CancellationToken cancellationToken = default)
        {
            return WaitForCancellationAsync(cancellationToken);
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count,
            CancellationToken cancellationToken)
        {
            return WaitForCancellationAsync(cancellationToken).AsTask();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        private static async ValueTask<int> WaitForCancellationAsync(CancellationToken cancellationToken)
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken).ConfigureAwait(false);
            return 0;
        }
    }
}