using System.Net;
using System.Text;

using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Tests.TestSupport;

using GitLabJsonContext = GitLab.Client.Serialization.GitLabJsonContext;

namespace GitLab.Client.Tests.Infrastructure;

public sealed class GitLabApiConnectionTests
{
    private static readonly Uri GitLabBaseAddress = new("https://gitlab.example/api/v4/");

    private static readonly Uri ProjectsRoute = new("projects", UriKind.Relative);

    [Fact]
    public async Task GetPagedAsync_FollowsARelativeNextLink()
    {
        using RecordingHttpMessageHandler handler = new(static (_, index) => index switch
        {
            0 => Page(1, "<projects?page=2>; rel=\"next\""),
            _ => Page(2, null)
        });

        using HttpClient httpClient = new(handler) { BaseAddress = GitLabBaseAddress };
        GitLabApiConnection connection = new(httpClient);

        List<GitLabProject> projects = await CollectAsync(connection);

        Assert.Equal([1L, 2L], projects.Select(static project => project.Id));
        Assert.Equal(2, handler.Requests.Count);
    }

    [Fact]
    public async Task GetPagedAsync_FollowsAnAbsoluteNextLink_OnTheConfiguredInstance()
    {
        using RecordingHttpMessageHandler handler = new(static (_, index) => index switch
        {
            0 => Page(1, "<https://gitlab.example/api/v4/projects?page=2>; rel=\"next\""),
            _ => Page(2, null)
        });

        using HttpClient httpClient = new(handler) { BaseAddress = GitLabBaseAddress };
        GitLabApiConnection connection = new(httpClient);

        List<GitLabProject> projects = await CollectAsync(connection);

        Assert.Equal([1L, 2L], projects.Select(static project => project.Id));
        Assert.Equal("https://gitlab.example/api/v4/projects?page=2", handler.Requests[1].RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task GetPagedAsync_RefusesToFollowANextLinkPointingAtAnotherHost()
    {
        // The credential-leak path: the Link header is entirely server-controlled, and whatever we request
        // next gets authenticated as the configured instance.
        using RecordingHttpMessageHandler handler = new(static (_, index) => index switch
        {
            0 => Page(1, "<https://evil.example/api/v4/projects?page=2>; rel=\"next\""),
            _ => Page(2, null)
        });

        using HttpClient httpClient = new(handler) { BaseAddress = GitLabBaseAddress };
        GitLabApiConnection connection = new(httpClient);

        List<GitLabProject> projects = [];
        GitLabApiException exception = await Assert.ThrowsAsync<GitLabApiException>(async () =>
        {
            await foreach (GitLabProject project in connection.GetPagedAsync(ProjectsRoute,
                                   GitLabJsonContext.Default.GitLabProjectArray,
                                   TestContext.Current.CancellationToken)
                               .ConfigureAwait(false))
            {
                projects.Add(project);
            }
        });

        Assert.Contains("evil.example", exception.Message, StringComparison.Ordinal);
        Assert.Single(handler.Requests);
        Assert.Single(projects);
    }

    [Fact]
    public async Task GetPagedAsync_RefusesToFollowANetworkPathReferencePointingAtAnotherHost()
    {
        // "//host/path" is not Uri.IsAbsoluteUri, but HttpClient resolves it as a cross-host HTTPS URL.
        // Treating it as merely relative would send the configured token to the attacker-controlled host.
        using RecordingHttpMessageHandler handler = new(static (_, index) => index switch
        {
            0 => Page(1, "<//evil.example/api/v4/projects?page=2>; rel=\"next\""),
            _ => Page(2, null)
        });

        using HttpClient httpClient = new(handler) { BaseAddress = GitLabBaseAddress };
        GitLabApiConnection connection = new(httpClient);

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabApiException>(async () =>
        {
            await foreach (GitLabProject _ in connection.GetPagedAsync(ProjectsRoute,
                                   GitLabJsonContext.Default.GitLabProjectArray,
                                   TestContext.Current.CancellationToken)
                               .ConfigureAwait(false))
            {
            }
        });

        Assert.Contains("evil.example", exception.Message, StringComparison.Ordinal);
        Assert.Single(handler.Requests);
    }

    [Fact]
    public async Task GetAsync_BoundsTheRetainedErrorBodyBeforeCreatingTheException()
    {
        string proxyErrorPage = new('x', GitLabApiExceptionFactory.MaxRetainedResponseBodyLength * 2);
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.BadGateway)
        {
            Content = new StringContent(proxyErrorPage, Encoding.UTF8, "text/html")
        });
        using HttpClient httpClient = new(handler) { BaseAddress = GitLabBaseAddress };
        GitLabApiConnection connection = new(httpClient);

        GitLabServerException exception = await Assert.ThrowsAsync<GitLabServerException>(() =>
            connection.GetAsync(new Uri("projects/1", UriKind.Relative), GitLabJsonContext.Default.GitLabProject,
                TestContext.Current.CancellationToken));

        Assert.NotNull(exception.ResponseBody);
        Assert.True(exception.ResponseBody.Length <= GitLabApiExceptionFactory.MaxRetainedResponseBodyLength);
        Assert.Contains("response body truncated", exception.ResponseBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetAsync_ReadsOnlyABoundedPrefixOfALargeErrorBody()
    {
        byte[] payload =
            Encoding.UTF8.GetBytes(new string('x', GitLabApiExceptionFactory.MaxRetainedResponseBodyLength * 128));
        using CountingReadStream source = new(payload);
        using StubHttpMessageHandler handler = new(_ =>
            new HttpResponseMessage(HttpStatusCode.BadGateway) { Content = new StreamContent(source) });
        using HttpClient httpClient = new(handler) { BaseAddress = GitLabBaseAddress };
        GitLabApiConnection connection = new(httpClient);

        await Assert.ThrowsAsync<GitLabServerException>(() =>
            connection.GetAsync(new Uri("projects/1", UriKind.Relative), GitLabJsonContext.Default.GitLabProject,
                TestContext.Current.CancellationToken));

        // HttpContent/StreamReader can prefetch a buffer, but a proxy error page must never be read in its
        // entirety. The payload is deliberately 128 times larger than the retained diagnostic prefix.
        Assert.InRange(source.BytesRead, 1, GitLabApiExceptionFactory.MaxRetainedResponseBodyLength * 2);
    }

    [Fact]
    public async Task GetAsync_TimesOutTheBodyRead_WhenTheServerStallsAfterTheHeaders()
    {
        // HttpClient.Timeout stops applying once the headers arrive under ResponseHeadersRead, so without the
        // linked token source this call would hang forever.
        using StubHttpMessageHandler handler = new(static _ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StreamContent(new StallingStream())
        });

        using HttpClient httpClient = new(handler)
        {
            BaseAddress = GitLabBaseAddress, Timeout = TimeSpan.FromMilliseconds(250)
        };
        GitLabApiConnection connection = new(httpClient);

        TaskCanceledException exception = await Assert.ThrowsAsync<TaskCanceledException>(() =>
            connection.GetAsync(new Uri("projects/1", UriKind.Relative), GitLabJsonContext.Default.GitLabProject,
                TestContext.Current.CancellationToken));

        Assert.IsType<TimeoutException>(exception.InnerException);
    }

    [Fact]
    public async Task GetAsync_ReportsCallerCancellation_RatherThanRewritingItAsATimeout()
    {
        using StubHttpMessageHandler handler = new(static _ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StreamContent(new StallingStream())
        });

        using HttpClient httpClient = new(handler)
        {
            BaseAddress = GitLabBaseAddress, Timeout = Timeout.InfiniteTimeSpan
        };
        GitLabApiConnection connection = new(httpClient);

        using CancellationTokenSource cancellation = new(TimeSpan.FromMilliseconds(250));

        OperationCanceledException exception = await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            connection.GetAsync(new Uri("projects/1", UriKind.Relative), GitLabJsonContext.Default.GitLabProject,
                cancellation.Token));

        Assert.False(exception.InnerException is TimeoutException);
    }

    private static async Task<List<GitLabProject>> CollectAsync(GitLabApiConnection connection)
    {
        List<GitLabProject> projects = [];

        await foreach (GitLabProject project in connection.GetPagedAsync(ProjectsRoute,
                               GitLabJsonContext.Default.GitLabProjectArray, TestContext.Current.CancellationToken)
                           .ConfigureAwait(false))
        {
            projects.Add(project);
        }

        return projects;
    }

    private static HttpResponseMessage Page(long projectId, string? linkHeader)
    {
        string json = $$"""
                        [
                          {
                            "id": {{projectId}},
                            "name": "Project {{projectId}}",
                            "path_with_namespace": "group/project-{{projectId}}",
                            "visibility": "public",
                            "web_url": "https://gitlab.example/group/project-{{projectId}}"
                          }
                        ]
                        """;

        HttpResponseMessage response = new(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

        if (linkHeader is not null)
        {
            response.Headers.TryAddWithoutValidation("Link", linkHeader);
        }

        return response;
    }

    /// <summary>A response body that sends headers and then never produces a byte, the shape this guards against.</summary>
    private sealed class StallingStream : Stream
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

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override async ValueTask<int> ReadAsync(Memory<byte> buffer,
            CancellationToken cancellationToken = default)
        {
            await Task.Delay(Timeout.Infinite, cancellationToken).ConfigureAwait(false);
            return 0;
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

        public override async ValueTask<int> ReadAsync(Memory<byte> buffer,
            CancellationToken cancellationToken = default)
        {
            int read = await base.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
            BytesRead += read;
            return read;
        }
    }
}