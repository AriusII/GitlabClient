using System.Net;
using System.Text;
using System.Text.Json.Serialization;

using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;

using GitLabJsonContext = GitLab.Client.Serialization.GitLabJsonContext;

namespace GitLab.Client.Tests.Infrastructure.Http;

/// <summary>
///     Pins the non-file form transports to the representations declared by GitLab's OpenAPI contract. The
///     handler reads the content before <see cref="HttpRequestMessage" /> is disposed, so each assertion sees
///     the actual bytes sent through the HTTP pipeline.
/// </summary>
public sealed class GitLabFormTransportTests
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

    private static readonly Uri UsersRoute = new("users", UriKind.Relative);

    private static readonly Uri UserRoute = new("users/42", UriKind.Relative);

    private static readonly Uri BulkImportsRoute = new("bulk_imports", UriKind.Relative);

    [Fact]
    public async Task PostMultipartFormAsync_WithResponse_SendsWireNamedFieldsAndDeserializesTheResponse()
    {
        using CapturingHttpMessageHandler handler = new(() => JsonResponse(HttpStatusCode.Created, ProjectJson));
        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);

        IReadOnlyDictionary<string, string> fields = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["name"] = "Ada Lovelace", ["preferences[public_profile]"] = "true"
        };

        GitLabProject project = await connection.PostMultipartFormAsync(UsersRoute, fields,
            GitLabJsonContext.Default.GitLabProject, TestContext.Current.CancellationToken);

        Assert.Equal(42, project.Id);
        Assert.Equal(HttpMethod.Post, handler.Method);
        Assert.StartsWith("multipart/form-data", handler.RequestContentType, StringComparison.Ordinal);
        AssertMultipartField(handler.RequestBody, "name");
        Assert.Contains("Ada Lovelace", handler.RequestBody, StringComparison.Ordinal);
        AssertMultipartField(handler.RequestBody, "preferences[public_profile]");
        Assert.Contains("true", handler.RequestBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task PostMultipartFormAsync_WithNoResponse_SucceedsWithoutAttemptingToDeserializeAnEmptyBody()
    {
        using CapturingHttpMessageHandler handler = new(static () => new HttpResponseMessage(HttpStatusCode.Accepted));
        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);

        await connection.PostMultipartFormAsync(UsersRoute,
            new Dictionary<string, string> { ["username"] = "ada" }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.Method);
        Assert.StartsWith("multipart/form-data", handler.RequestContentType, StringComparison.Ordinal);
        AssertMultipartField(handler.RequestBody, "username");
    }

    [Fact]
    public async Task PutMultipartFormAsync_WithResponse_UsesPutAndDeserializesTheResponse()
    {
        using CapturingHttpMessageHandler handler = new(() => JsonResponse(HttpStatusCode.OK, ProjectJson));
        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);

        GitLabProject project = await connection.PutMultipartFormAsync(UserRoute,
            new Dictionary<string, string> { ["name"] = "Ada Lovelace" }, GitLabJsonContext.Default.GitLabProject,
            TestContext.Current.CancellationToken);

        Assert.Equal(42, project.Id);
        Assert.Equal(HttpMethod.Put, handler.Method);
        Assert.StartsWith("multipart/form-data", handler.RequestContentType, StringComparison.Ordinal);
        AssertMultipartField(handler.RequestBody, "name");
    }

    [Fact]
    public async Task PutMultipartFormAsync_WithNoResponse_SucceedsWithoutAttemptingToDeserializeAnEmptyBody()
    {
        using CapturingHttpMessageHandler handler = new(static () => new HttpResponseMessage(HttpStatusCode.NoContent));
        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);

        await connection.PutMultipartFormAsync(UserRoute,
            new Dictionary<string, string> { ["private_profile"] = "false" },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.Method);
        Assert.StartsWith("multipart/form-data", handler.RequestContentType, StringComparison.Ordinal);
        AssertMultipartField(handler.RequestBody, "private_profile");
    }

    [Fact]
    public async Task PostUrlEncodedFormAsync_EncodesBulkImportFieldsAndDeserializesTheResponse()
    {
        using CapturingHttpMessageHandler handler = new(() => JsonResponse(HttpStatusCode.Created,
            """{"id":51,"status":"created"}"""));
        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);

        CreateBulkImportRequest request = new()
        {
            Configuration = new BulkImportConfiguration
            {
                Url = new Uri("https://source.example/api/v4"), AccessToken = "secret value"
            },
            Entities =
            [
                new BulkImportEntityRequest
                {
                    SourceType = GitLabBulkImportEntitySourceType.GroupEntity,
                    SourceFullPath = "engineering/platform",
                    DestinationNamespace = "destination"
                }
            ]
        };

        GitLabBulkImport import = await connection.PostUrlEncodedFormAsync(BulkImportsRoute, request,
            GitLabJsonContext.Default.CreateBulkImportRequest, GitLabJsonContext.Default.GitLabBulkImport,
            TestContext.Current.CancellationToken);

        Assert.Equal(51, import.Id);
        Assert.Equal(HttpMethod.Post, handler.Method);
        Assert.Equal("application/x-www-form-urlencoded", handler.RequestContentType);
        Assert.Contains("configuration%5Burl%5D=https%3A%2F%2Fsource.example%2Fapi%2Fv4", handler.RequestBody,
            StringComparison.Ordinal);
        Assert.Contains("configuration%5Baccess_token%5D=secret+value", handler.RequestBody,
            StringComparison.Ordinal);
        Assert.Contains("entities%5B0%5D%5Bsource_type%5D=group_entity", handler.RequestBody,
            StringComparison.Ordinal);
        Assert.Contains("entities%5B0%5D%5Bsource_full_path%5D=engineering%2Fplatform", handler.RequestBody,
            StringComparison.Ordinal);
        Assert.Contains("entities%5B0%5D%5Bdestination_namespace%5D=destination", handler.RequestBody,
            StringComparison.Ordinal);
    }

    [Fact]
    public async Task PostMultipartFormAsync_WithSourceGeneratedRequestMetadata_FlattensNestedValuesDeterministically()
    {
        using CapturingHttpMessageHandler handler = new(() => JsonResponse(HttpStatusCode.Created, ProjectJson));
        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);

        FormTransportRequest request = new()
        {
            Mode = FormTransportMode.ReadyForReview,
            OccurredAt = new DateTimeOffset(2026, 9, 10, 14, 30, 0, TimeSpan.FromHours(2)),
            Tags = ["first", "second"],
            Settings = new FormTransportSettings { Enabled = true, Limit = 5 },
            Omitted = null
        };

        await connection.PostMultipartFormAsync(UsersRoute, request,
            FormTransportJsonContext.Default.FormTransportRequest,
            GitLabJsonContext.Default.GitLabProject, TestContext.Current.CancellationToken);

        string normalizedBody = NormalizeMultipartBody(handler.RequestBody);

        Assert.Contains("Content-Disposition: form-data; name=mode", normalizedBody, StringComparison.Ordinal);
        Assert.Contains("ready-for-review", handler.RequestBody, StringComparison.Ordinal);
        Assert.Contains("Content-Disposition: form-data; name=occurred_at", normalizedBody,
            StringComparison.Ordinal);
        Assert.Contains("2026-09-10T14:30:00+02:00", handler.RequestBody, StringComparison.Ordinal);
        Assert.Contains("Content-Disposition: form-data; name=tags[0]", normalizedBody, StringComparison.Ordinal);
        Assert.Contains("Content-Disposition: form-data; name=tags[1]", normalizedBody, StringComparison.Ordinal);
        Assert.Contains("Content-Disposition: form-data; name=settings[enabled]", normalizedBody,
            StringComparison.Ordinal);
        Assert.Contains("Content-Disposition: form-data; name=settings[limit]", normalizedBody,
            StringComparison.Ordinal);
        Assert.DoesNotContain("omitted", handler.RequestBody, StringComparison.Ordinal);

        Assert.True(
            IndexOfMultipartField(normalizedBody, "mode") < IndexOfMultipartField(normalizedBody, "occurred_at"));
        Assert.True(IndexOfMultipartField(normalizedBody, "occurred_at")
                    < IndexOfMultipartField(normalizedBody, "settings[enabled]"));
        Assert.True(IndexOfMultipartField(normalizedBody, "settings[enabled]")
                    < IndexOfMultipartField(normalizedBody, "settings[limit]"));
        Assert.True(IndexOfMultipartField(normalizedBody, "settings[limit]")
                    < IndexOfMultipartField(normalizedBody, "tags[0]"));
        Assert.True(IndexOfMultipartField(normalizedBody, "tags[0]") <
                    IndexOfMultipartField(normalizedBody, "tags[1]"));
    }

    [Fact]
    public async Task PostMultipartFormAsync_MapsANonSuccessResponseToTheTypedException()
    {
        using CapturingHttpMessageHandler handler = new(() => JsonResponse(HttpStatusCode.Forbidden,
            """{"message":"403 Forbidden"}"""));
        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);

        await Assert.ThrowsAsync<GitLabForbiddenException>(() => connection.PostMultipartFormAsync(UsersRoute,
            new Dictionary<string, string> { ["username"] = "ada" }, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task PostMultipartFormAsync_HonorsCallerCancellation()
    {
        using AwaitCancellationHttpMessageHandler handler = new();
        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        using CancellationTokenSource cancellation = CancellationTokenSource.CreateLinkedTokenSource(
            TestContext.Current.CancellationToken);

        Task request = connection.PostMultipartFormAsync(UsersRoute,
            new Dictionary<string, string> { ["username"] = "ada" }, cancellation.Token);

        await handler.RequestStarted.Task.WaitAsync(TestContext.Current.CancellationToken);
        await cancellation.CancelAsync().ConfigureAwait(true);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => request);
    }

    private static HttpResponseMessage JsonResponse(HttpStatusCode statusCode, string body)
    {
        return new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        };
    }

    /// <summary>
    ///     MIME permits a parameter value that is a token to be emitted either with or without quotes.
    ///     <see cref="MultipartFormDataContent" /> currently omits quotes for simple names, while field names
    ///     containing GitLab's bracket syntax can be rendered differently by framework versions. Strip only
    ///     parameter quotes, then assert the complete Content-Disposition field header rather than an incidental
    ///     presentation choice.
    /// </summary>
    private static void AssertMultipartField(string requestBody, string name)
    {
        string normalizedBody = NormalizeMultipartBody(requestBody);
        Assert.Contains($"Content-Disposition: form-data; name={name}", normalizedBody, StringComparison.Ordinal);
    }

    private static int IndexOfMultipartField(string normalizedBody, string name)
    {
        int index = normalizedBody.IndexOf($"Content-Disposition: form-data; name={name}", StringComparison.Ordinal);
        Assert.True(index >= 0, $"Expected a multipart field named '{name}'.");
        return index;
    }

    private static string NormalizeMultipartBody(string requestBody)
    {
        return requestBody.Replace("\"", string.Empty, StringComparison.Ordinal);
    }

    private sealed class CapturingHttpMessageHandler(Func<HttpResponseMessage> respond) : HttpMessageHandler
    {
        public HttpMethod? Method { get; private set; }

        public string? RequestContentType { get; private set; }

        public string RequestBody { get; private set; } = string.Empty;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Method = request.Method;
            RequestContentType = request.Content?.Headers.ContentType?.MediaType;
            RequestBody = request.Content is null
                ? string.Empty
                : await request.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

            HttpResponseMessage response = respond();
            response.RequestMessage ??= request;
            return response;
        }
    }

    private sealed class AwaitCancellationHttpMessageHandler : HttpMessageHandler
    {
        public TaskCompletionSource RequestStarted { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            RequestStarted.TrySetResult();
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken).ConfigureAwait(false);
            throw new InvalidOperationException("The infinite delay completed without cancellation.");
        }
    }
}

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(FormTransportRequest))]
internal sealed partial class FormTransportJsonContext : JsonSerializerContext;

internal sealed record FormTransportRequest
{
    public required FormTransportMode Mode { get; init; }

    public required DateTimeOffset OccurredAt { get; init; }

    public required IReadOnlyList<string> Tags { get; init; }

    public required FormTransportSettings Settings { get; init; }

    public string? Omitted { get; init; }
}

internal sealed record FormTransportSettings
{
    public required bool Enabled { get; init; }

    public required int Limit { get; init; }
}

[JsonConverter(typeof(JsonStringEnumConverter<FormTransportMode>))]
internal enum FormTransportMode
{
    [JsonStringEnumMemberName("ready-for-review")]
    ReadyForReview
}