using System.Net;
using System.Text;

using GitLab.Client.Abstractions;
using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Infrastructure.Serialization;
using GitLab.Client.Models;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Infrastructure;

/// <summary>
///     The overloads that exist because GitLab pairs verbs and bodies in ways the obvious four methods do
///     not cover: a PUT that carries a body but answers 204, a body-less PUT that answers with the updated
///     resource, a DELETE that echoes what it deleted, a GET whose answer is the status code, and a
///     multipart upload with an empty response. Each one was added because a resource had to skip an
///     endpoint without it, so the tests pin the exact bytes on the wire.
/// </summary>
public sealed class GitLabApiConnectionEmptyBodyTests
{
    private const string LabelJson = """
                                     {
                                       "id": 5,
                                       "name": "bug",
                                       "color": "#d9534f"
                                     }
                                     """;

    private static readonly Uri BaseAddress = new("https://gitlab.example/api/v4/");

    private static readonly Uri UrlVariableRoute =
        new("projects/42/hooks/7/url_variables/token", UriKind.Relative);

    private static readonly Uri PromoteRoute = new("projects/42/labels/bug/promote", UriKind.Relative);

    private static readonly Uri IssueLinkRoute = new("projects/42/issues/3/links/9", UriKind.Relative);

    private static readonly Uri PagesAccessRoute = new("projects/42/pages_access", UriKind.Relative);

    private static readonly Uri TerraformStateRoute = new("projects/42/terraform/state/prod", UriKind.Relative);

    [Fact]
    public async Task PutAsync_WithABodyAndNoResponse_SendsTheBody_AndDoesNotTryToDeserializeTheEmptyAnswer()
    {
        // The body has to be captured inside the handler: HttpRequestMessage.Dispose disposes its Content,
        // and the connection scopes the request with `using`, so reading it afterwards is already too late.
        using RecordingHandler handler = new(() => new HttpResponseMessage(HttpStatusCode.NoContent));
        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);

        CreateLabelRequest request = new() { Name = "bug", Color = "#d9534f" };

        await connection.PutAsync(UrlVariableRoute, request, GitLabJsonContext.Default.CreateLabelRequest,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/42/hooks/7/url_variables/token",
            handler.RequestUri?.AbsoluteUri);
        Assert.Contains("\"name\":\"bug\"", handler.RequestBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task PutAsync_WithNoBodyButAResponse_SendsNoContent_AndDeserializesTheResource()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(LabelJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);

        GitLabLabel label = await connection.PutAsync(PromoteRoute, GitLabJsonContext.Default.GitLabLabel,
            TestContext.Current.CancellationToken);

        Assert.Equal("bug", label.Name);
        Assert.NotNull(handler.LastRequest);
        Assert.Equal(HttpMethod.Put, handler.LastRequest.Method);

        // The point of this overload: promoting a label declares no request body, so sending "{}" would be
        // inventing a payload the endpoint does not document.
        Assert.Null(handler.LastRequest.Content);
    }

    [Fact]
    public async Task DeleteAsync_WithAResponse_ReturnsTheDeletedResource()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(LabelJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);

        GitLabLabel deleted = await connection.DeleteAsync(IssueLinkRoute, GitLabJsonContext.Default.GitLabLabel,
            TestContext.Current.CancellationToken);

        Assert.Equal(5, deleted.Id);
        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
    }

    [Fact]
    public async Task DeleteAsync_WithAResponse_StillThrowsTheTypedException_OnAFailureStatus()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Forbidden)
        {
            Content = new StringContent("""{"message":"403 Forbidden"}""", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);

        await Assert.ThrowsAsync<GitLabForbiddenException>(() =>
            connection.DeleteAsync(IssueLinkRoute, GitLabJsonContext.Default.GitLabLabel,
                TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task GetAsync_WithNoResponseBody_SucceedsOnTheStatusCodeAlone()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK));
        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);

        await connection.GetAsync(PagesAccessRoute, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/42/pages_access",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task GetAsync_WithNoResponseBody_StillThrowsTheTypedException_OnAFailureStatus()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent("""{"message":"404 Project Not Found"}""", Encoding.UTF8,
                "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);

        await Assert.ThrowsAsync<GitLabNotFoundException>(() =>
            connection.GetAsync(PagesAccessRoute, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task PostFileAsync_WithNoResponseBody_SendsTheMultipartParts_AndLeavesTheCallersStreamOpen()
    {
        using RecordingHandler handler = new(() => new HttpResponseMessage(HttpStatusCode.OK));
        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);

        using MemoryStream content = new("{\"version\":4}"u8.ToArray(), false);

        GitLabFileUpload upload = new()
        {
            Content = content, FileName = "terraform.tfstate", ContentType = "application/json"
        };

        await connection.PostFileAsync(TerraformStateRoute, upload,
            new Dictionary<string, string> { ["lock_id"] = "abc" }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.Method);
        Assert.StartsWith("multipart/form-data", handler.RequestContentType, StringComparison.Ordinal);
        Assert.Contains("name=lock_id", handler.RequestBody, StringComparison.Ordinal);
        Assert.Contains("filename=terraform.tfstate", handler.RequestBody, StringComparison.Ordinal);
        Assert.Contains("{\"version\":4}", handler.RequestBody, StringComparison.Ordinal);

        // The upload stream is borrowed, never owned: disposing the request must not close what the caller
        // may still want to rewind and retry with.
        Assert.True(content.CanRead);
    }

    [Fact]
    public async Task PutFileAsync_WithNoResponseBody_SendsTheMultipartParts_AndLeavesTheCallersStreamOpen()
    {
        using RecordingHandler handler = new(() => new HttpResponseMessage(HttpStatusCode.OK));
        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);

        using MemoryStream content = new([0x1f, 0x8b], false);

        GitLabFileUpload upload = new() { Content = content, FileName = "package-1.0.0.tar.gz" };

        await connection.PutFileAsync(TerraformStateRoute, upload, null, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.Method);
        Assert.StartsWith("multipart/form-data", handler.RequestContentType, StringComparison.Ordinal);
        Assert.Contains("filename=package-1.0.0.tar.gz", handler.RequestBody, StringComparison.Ordinal);

        // The upload stream is borrowed, never owned: disposing the request must not close what the caller
        // may still want to rewind and retry with.
        Assert.True(content.CanRead);
    }

    [Fact]
    public async Task PutFileAsync_WithNoResponseBody_StillThrowsTheTypedException_OnAFailureStatus()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Conflict)
        {
            Content = new StringContent("""{"message":"already exists"}""", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);

        using MemoryStream content = new([0x1f, 0x8b], false);
        GitLabFileUpload upload = new() { Content = content, FileName = "package-1.0.0.tar.gz" };

        await Assert.ThrowsAsync<GitLabConflictException>(() =>
            connection.PutFileAsync(TerraformStateRoute, upload, null, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task GetRedirectAsync_ReturnsTheLocationHeader_WithoutThrowingOnA302()
    {
        using StubHttpMessageHandler handler = new(_ =>
        {
            HttpResponseMessage response = new(HttpStatusCode.Found);
            response.Headers.Location = new Uri("https://upstream.example/files/pkg-1.0.0.tar.gz");
            return response;
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);

        GitLabRedirectResponse redirect =
            await connection.GetRedirectAsync(PagesAccessRoute, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Found, redirect.StatusCode);
        Assert.Equal(new Uri("https://upstream.example/files/pkg-1.0.0.tar.gz"), redirect.Location);
    }

    [Fact]
    public async Task GetRedirectAsync_WithNoResponseBody_StillThrowsTheTypedException_OnAFailureStatus()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent("""{"message":"404 Not Found"}""", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);

        await Assert.ThrowsAsync<GitLabNotFoundException>(() =>
            connection.GetRedirectAsync(PagesAccessRoute, TestContext.Current.CancellationToken));
    }

    /// <summary>
    ///     Captures the outgoing request while it is still alive. The connection scopes every
    ///     <see cref="HttpRequestMessage" /> with <c>using</c>, and disposing it disposes its content, so a
    ///     test that reads the body after the call has already lost it.
    /// </summary>
    private sealed class RecordingHandler(Func<HttpResponseMessage> respond) : HttpMessageHandler
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
}