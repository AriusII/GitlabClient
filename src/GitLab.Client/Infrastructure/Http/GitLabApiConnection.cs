using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

using GitLab.Client.Abstractions;
using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.DependencyInjection;
using GitLab.Client.Infrastructure.Pagination;

using Microsoft.Extensions.Options;

namespace GitLab.Client.Infrastructure.Http;

internal sealed class GitLabApiConnection : IGitLabApiConnection
{
    private readonly Func<HttpClient> _httpClientProvider;

    /// <summary>
    ///     The DI path. Takes <see cref="IHttpClientFactory" /> rather than an <see cref="HttpClient" /> because
    ///     this type is a singleton: holding a factory-produced client would capture one handler chain - and so
    ///     one DNS answer and one snapshot of the options - for the life of the process.
    /// </summary>
    public GitLabApiConnection(IHttpClientFactory httpClientFactory, IOptionsMonitor<GitLabClientOptions> options)
    {
        ArgumentNullException.ThrowIfNull(httpClientFactory);
        ArgumentNullException.ThrowIfNull(options);

        // Fail fast: reading CurrentValue runs GitLabClientOptionsValidator while the container graph is being
        // constructed, so a missing or malformed token surfaces there instead of from inside a DI factory on
        // the first API call. This is what covers a bare `new ServiceCollection().BuildServiceProvider()`,
        // where ValidateOnStart's IStartupValidator is never invoked because there is no host.
        _ = options.CurrentValue;

        _httpClientProvider = () => httpClientFactory.CreateClient(GitLabClientDefaults.HttpClientName);
    }

    /// <summary>
    ///     Direct-construction path, used by the transport-level tests that build their own
    ///     <see cref="HttpMessageHandler" />. Not reachable from DI: the container only considers public
    ///     constructors, so the factory overload above is always the one it picks.
    /// </summary>
    internal GitLabApiConnection(HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(httpClient);

        _httpClientProvider = () => httpClient;
    }

    public async Task<TResponse> GetAsync<TResponse>(
        Uri requestUri,
        JsonTypeInfo<TResponse> responseTypeInfo,
        CancellationToken cancellationToken = default)
    {
        HttpClient httpClient = CreateClient();
        using CancellationTokenSource operation = CreateOperationTimeout(httpClient, cancellationToken);

        using HttpResponseMessage response = await httpClient
            .GetAsync(requestUri, HttpCompletionOption.ResponseHeadersRead, operation.Token)
            .ConfigureAwait(false);
        await EnsureSuccessAsync(response, HttpMethod.Get, requestUri, operation.Token).ConfigureAwait(false);

        return await ReadBodyAsync(response, responseTypeInfo, httpClient.Timeout, HttpMethod.Get, requestUri,
                operation.Token, cancellationToken)
            .ConfigureAwait(false);
    }

    public async IAsyncEnumerable<TItem> GetPagedAsync<TItem>(
        Uri requestUri,
        JsonTypeInfo<TItem[]> pageTypeInfo,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        HttpClient httpClient = CreateClient();
        Uri? next = requestUri;

        while (next is not null)
        {
            // Per page rather than per enumeration: walking a large group legitimately takes far longer than
            // any single request should.
            using CancellationTokenSource operation = CreateOperationTimeout(httpClient, cancellationToken);

            using HttpResponseMessage response = await httpClient
                .GetAsync(next, HttpCompletionOption.ResponseHeadersRead, operation.Token)
                .ConfigureAwait(false);
            await EnsureSuccessAsync(response, HttpMethod.Get, next, operation.Token).ConfigureAwait(false);

            TItem[]? page = await ReadOptionalBodyAsync(response, pageTypeInfo, httpClient.Timeout, operation.Token,
                cancellationToken).ConfigureAwait(false);

            if (page is not null)
            {
                foreach (TItem item in page)
                {
                    yield return item;
                }
            }

            string? nextPageUrl = GitLabLinkHeader.GetNextPageUrl(response.Headers);
            next = nextPageUrl is null ? null : ResolveNextPage(nextPageUrl, httpClient.BaseAddress);
        }
    }

    public async Task<TResponse> PostAsync<TRequest, TResponse>(
        Uri requestUri,
        TRequest request,
        JsonTypeInfo<TRequest> requestTypeInfo,
        JsonTypeInfo<TResponse> responseTypeInfo,
        CancellationToken cancellationToken = default)
    {
        HttpClient httpClient = CreateClient();
        using CancellationTokenSource operation = CreateOperationTimeout(httpClient, cancellationToken);

        // Sent through an explicit HttpRequestMessage because HttpClient.PostAsync implies
        // ResponseContentRead, which copies the whole response body into an intermediate MemoryStream before
        // the deserializer can see it. HttpRequestMessage.Dispose disposes its Content, so the request body
        // needs no separate `using`.
        using HttpRequestMessage message = new(HttpMethod.Post, requestUri)
        {
            Content = JsonContent.Create(request, requestTypeInfo)
        };

        using HttpResponseMessage response = await httpClient
            .SendAsync(message, HttpCompletionOption.ResponseHeadersRead, operation.Token)
            .ConfigureAwait(false);
        await EnsureSuccessAsync(response, HttpMethod.Post, requestUri, operation.Token).ConfigureAwait(false);

        return await ReadBodyAsync(response, responseTypeInfo, httpClient.Timeout, HttpMethod.Post, requestUri,
                operation.Token, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<TResponse> PostAsync<TResponse>(
        Uri requestUri,
        JsonTypeInfo<TResponse> responseTypeInfo,
        CancellationToken cancellationToken = default)
    {
        HttpClient httpClient = CreateClient();
        using CancellationTokenSource operation = CreateOperationTimeout(httpClient, cancellationToken);

        using HttpRequestMessage message = new(HttpMethod.Post, requestUri);

        using HttpResponseMessage response = await httpClient
            .SendAsync(message, HttpCompletionOption.ResponseHeadersRead, operation.Token)
            .ConfigureAwait(false);
        await EnsureSuccessAsync(response, HttpMethod.Post, requestUri, operation.Token).ConfigureAwait(false);

        return await ReadBodyAsync(response, responseTypeInfo, httpClient.Timeout, HttpMethod.Post, requestUri,
                operation.Token, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task PostAsync(Uri requestUri, CancellationToken cancellationToken = default)
    {
        HttpClient httpClient = CreateClient();
        using CancellationTokenSource operation = CreateOperationTimeout(httpClient, cancellationToken);

        using HttpRequestMessage message = new(HttpMethod.Post, requestUri);

        using HttpResponseMessage response = await httpClient
            .SendAsync(message, HttpCompletionOption.ResponseHeadersRead, operation.Token)
            .ConfigureAwait(false);
        await EnsureSuccessAsync(response, HttpMethod.Post, requestUri, operation.Token).ConfigureAwait(false);
    }

    public async Task<TResponse> PutAsync<TRequest, TResponse>(
        Uri requestUri,
        TRequest request,
        JsonTypeInfo<TRequest> requestTypeInfo,
        JsonTypeInfo<TResponse> responseTypeInfo,
        CancellationToken cancellationToken = default)
    {
        HttpClient httpClient = CreateClient();
        using CancellationTokenSource operation = CreateOperationTimeout(httpClient, cancellationToken);

        using HttpRequestMessage message = new(HttpMethod.Put, requestUri)
        {
            Content = JsonContent.Create(request, requestTypeInfo)
        };

        using HttpResponseMessage response = await httpClient
            .SendAsync(message, HttpCompletionOption.ResponseHeadersRead, operation.Token)
            .ConfigureAwait(false);
        await EnsureSuccessAsync(response, HttpMethod.Put, requestUri, operation.Token).ConfigureAwait(false);

        return await ReadBodyAsync(response, responseTypeInfo, httpClient.Timeout, HttpMethod.Put, requestUri,
                operation.Token, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task PostAsync<TRequest>(
        Uri requestUri,
        TRequest request,
        JsonTypeInfo<TRequest> requestTypeInfo,
        CancellationToken cancellationToken = default)
    {
        HttpClient httpClient = CreateClient();
        using CancellationTokenSource operation = CreateOperationTimeout(httpClient, cancellationToken);

        using HttpRequestMessage message = new(HttpMethod.Post, requestUri)
        {
            Content = JsonContent.Create(request, requestTypeInfo)
        };

        using HttpResponseMessage response = await httpClient
            .SendAsync(message, HttpCompletionOption.ResponseHeadersRead, operation.Token)
            .ConfigureAwait(false);
        await EnsureSuccessAsync(response, HttpMethod.Post, requestUri, operation.Token).ConfigureAwait(false);
    }

    public async Task PutAsync(Uri requestUri, CancellationToken cancellationToken = default)
    {
        HttpClient httpClient = CreateClient();
        using CancellationTokenSource operation = CreateOperationTimeout(httpClient, cancellationToken);

        using HttpRequestMessage message = new(HttpMethod.Put, requestUri);

        using HttpResponseMessage response = await httpClient
            .SendAsync(message, HttpCompletionOption.ResponseHeadersRead, operation.Token)
            .ConfigureAwait(false);
        await EnsureSuccessAsync(response, HttpMethod.Put, requestUri, operation.Token).ConfigureAwait(false);
    }

    public async Task PutAsync<TRequest>(
        Uri requestUri,
        TRequest request,
        JsonTypeInfo<TRequest> requestTypeInfo,
        CancellationToken cancellationToken = default)
    {
        HttpClient httpClient = CreateClient();
        using CancellationTokenSource operation = CreateOperationTimeout(httpClient, cancellationToken);

        using HttpRequestMessage message = new(HttpMethod.Put, requestUri)
        {
            Content = JsonContent.Create(request, requestTypeInfo)
        };

        using HttpResponseMessage response = await httpClient
            .SendAsync(message, HttpCompletionOption.ResponseHeadersRead, operation.Token)
            .ConfigureAwait(false);
        await EnsureSuccessAsync(response, HttpMethod.Put, requestUri, operation.Token).ConfigureAwait(false);
    }

    public async Task<TResponse> PutAsync<TResponse>(
        Uri requestUri,
        JsonTypeInfo<TResponse> responseTypeInfo,
        CancellationToken cancellationToken = default)
    {
        HttpClient httpClient = CreateClient();
        using CancellationTokenSource operation = CreateOperationTimeout(httpClient, cancellationToken);

        using HttpRequestMessage message = new(HttpMethod.Put, requestUri);

        using HttpResponseMessage response = await httpClient
            .SendAsync(message, HttpCompletionOption.ResponseHeadersRead, operation.Token)
            .ConfigureAwait(false);
        await EnsureSuccessAsync(response, HttpMethod.Put, requestUri, operation.Token).ConfigureAwait(false);

        return await ReadBodyAsync(response, responseTypeInfo, httpClient.Timeout, HttpMethod.Put, requestUri,
                operation.Token, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task DeleteAsync(Uri requestUri, CancellationToken cancellationToken = default)
    {
        HttpClient httpClient = CreateClient();
        using CancellationTokenSource operation = CreateOperationTimeout(httpClient, cancellationToken);

        using HttpRequestMessage message = new(HttpMethod.Delete, requestUri);

        using HttpResponseMessage response = await httpClient
            .SendAsync(message, HttpCompletionOption.ResponseHeadersRead, operation.Token)
            .ConfigureAwait(false);
        await EnsureSuccessAsync(response, HttpMethod.Delete, requestUri, operation.Token).ConfigureAwait(false);
    }

    public async Task<TResponse> DeleteAsync<TResponse>(
        Uri requestUri,
        JsonTypeInfo<TResponse> responseTypeInfo,
        CancellationToken cancellationToken = default)
    {
        HttpClient httpClient = CreateClient();
        using CancellationTokenSource operation = CreateOperationTimeout(httpClient, cancellationToken);

        using HttpRequestMessage message = new(HttpMethod.Delete, requestUri);

        using HttpResponseMessage response = await httpClient
            .SendAsync(message, HttpCompletionOption.ResponseHeadersRead, operation.Token)
            .ConfigureAwait(false);
        await EnsureSuccessAsync(response, HttpMethod.Delete, requestUri, operation.Token).ConfigureAwait(false);

        return await ReadBodyAsync(response, responseTypeInfo, httpClient.Timeout, HttpMethod.Delete, requestUri,
                operation.Token, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task GetAsync(Uri requestUri, CancellationToken cancellationToken = default)
    {
        HttpClient httpClient = CreateClient();
        using CancellationTokenSource operation = CreateOperationTimeout(httpClient, cancellationToken);

        using HttpRequestMessage message = new(HttpMethod.Get, requestUri);

        using HttpResponseMessage response = await httpClient
            .SendAsync(message, HttpCompletionOption.ResponseHeadersRead, operation.Token)
            .ConfigureAwait(false);
        await EnsureSuccessAsync(response, HttpMethod.Get, requestUri, operation.Token).ConfigureAwait(false);
    }

    public async Task<GitLabFileResponse> GetFileAsync(Uri requestUri, CancellationToken cancellationToken = default)
    {
        HttpClient httpClient = CreateClient();

        // Neither of these is scoped with `using`: on the success path both are handed to the returned
        // GitLabFileResponse, which owns them for as long as the caller holds the body open. Disposing them
        // here - as every JSON path above does - would tear the stream down before the caller read a byte.
        CancellationTokenSource? operation = CreateOperationTimeout(httpClient, cancellationToken);
        HttpResponseMessage? response = null;

        try
        {
            response = await httpClient
                .GetAsync(requestUri, HttpCompletionOption.ResponseHeadersRead, operation.Token)
                .ConfigureAwait(false);
            await EnsureSuccessAsync(response, HttpMethod.Get, requestUri, operation.Token).ConfigureAwait(false);

            // The operation budget exists to bound getting to the headers. Past that point the body is the
            // caller's to read at their own pace - a repository archive or a job artifact legitimately takes
            // longer than one request timeout - so stand the countdown down rather than aborting a healthy
            // download mid-stream. The source stays linked to the caller's token, so their cancellation
            // still aborts it.
            operation.CancelAfter(Timeout.InfiniteTimeSpan);

            Stream content = await response.Content.ReadAsStreamAsync(operation.Token).ConfigureAwait(false);
            GitLabFileResponse file = GitLabFileResponse.Create(response, content, operation);

            // Ownership has moved; stop the finally block from disposing what the caller now holds.
            response = null;
            operation = null;

            return file;
        }
        finally
        {
            response?.Dispose();
            operation?.Dispose();
        }
    }

    public Task<TResponse> PostFileAsync<TResponse>(
        Uri requestUri,
        GitLabFileUpload file,
        IReadOnlyDictionary<string, string>? formFields,
        JsonTypeInfo<TResponse> responseTypeInfo,
        CancellationToken cancellationToken = default)
    {
        return SendFileAsync(HttpMethod.Post, requestUri, file, formFields, responseTypeInfo, cancellationToken);
    }

    public Task<TResponse> PutFileAsync<TResponse>(
        Uri requestUri,
        GitLabFileUpload file,
        IReadOnlyDictionary<string, string>? formFields,
        JsonTypeInfo<TResponse> responseTypeInfo,
        CancellationToken cancellationToken = default)
    {
        return SendFileAsync(HttpMethod.Put, requestUri, file, formFields, responseTypeInfo, cancellationToken);
    }

    public async Task PostFileAsync(
        Uri requestUri,
        GitLabFileUpload file,
        IReadOnlyDictionary<string, string>? formFields,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(file);

        HttpClient httpClient = CreateClient();
        using CancellationTokenSource operation = CreateOperationTimeout(httpClient, cancellationToken);

        // As in SendFileAsync: the request message owns the multipart content and disposes every part with
        // itself, which is why the file part is a BorrowedStreamContent and the caller's stream survives.
        using HttpRequestMessage message = new(HttpMethod.Post, requestUri)
        {
            Content = BuildMultipartContent(file, formFields)
        };

        using HttpResponseMessage response = await httpClient
            .SendAsync(message, HttpCompletionOption.ResponseHeadersRead, operation.Token)
            .ConfigureAwait(false);
        await EnsureSuccessAsync(response, HttpMethod.Post, requestUri, operation.Token).ConfigureAwait(false);
    }

    public async Task<GitLabHeadResponse> HeadAsync(Uri requestUri, CancellationToken cancellationToken = default)
    {
        HttpClient httpClient = CreateClient();
        using CancellationTokenSource operation = CreateOperationTimeout(httpClient, cancellationToken);

        using HttpRequestMessage message = new(HttpMethod.Head, requestUri);

        using HttpResponseMessage response = await httpClient
            .SendAsync(message, HttpCompletionOption.ResponseHeadersRead, operation.Token)
            .ConfigureAwait(false);

        // 404 is the ANSWER on these routes, not a failure - they exist to be asked "is there a branch
        // called this?". Every other non-success status still maps to its typed exception, so an expired
        // token (401) or a private project (403) can never be misread as "it does not exist".
        if (response.StatusCode != HttpStatusCode.NotFound)
        {
            await EnsureSuccessAsync(response, HttpMethod.Head, requestUri, operation.Token).ConfigureAwait(false);
        }

        return GitLabHeadResponse.FromResponse(response);
    }

    public async Task<TResponse> PatchAsync<TRequest, TResponse>(
        Uri requestUri,
        TRequest request,
        JsonTypeInfo<TRequest> requestTypeInfo,
        JsonTypeInfo<TResponse> responseTypeInfo,
        CancellationToken cancellationToken = default)
    {
        HttpClient httpClient = CreateClient();
        using CancellationTokenSource operation = CreateOperationTimeout(httpClient, cancellationToken);

        using HttpRequestMessage message = new(HttpMethod.Patch, requestUri)
        {
            Content = JsonContent.Create(request, requestTypeInfo)
        };

        using HttpResponseMessage response = await httpClient
            .SendAsync(message, HttpCompletionOption.ResponseHeadersRead, operation.Token)
            .ConfigureAwait(false);
        await EnsureSuccessAsync(response, HttpMethod.Patch, requestUri, operation.Token).ConfigureAwait(false);

        return await ReadBodyAsync(response, responseTypeInfo, httpClient.Timeout, HttpMethod.Patch, requestUri,
                operation.Token, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task PatchAsync<TRequest>(
        Uri requestUri,
        TRequest request,
        JsonTypeInfo<TRequest> requestTypeInfo,
        CancellationToken cancellationToken = default)
    {
        HttpClient httpClient = CreateClient();
        using CancellationTokenSource operation = CreateOperationTimeout(httpClient, cancellationToken);

        using HttpRequestMessage message = new(HttpMethod.Patch, requestUri)
        {
            Content = JsonContent.Create(request, requestTypeInfo)
        };

        using HttpResponseMessage response = await httpClient
            .SendAsync(message, HttpCompletionOption.ResponseHeadersRead, operation.Token)
            .ConfigureAwait(false);
        await EnsureSuccessAsync(response, HttpMethod.Patch, requestUri, operation.Token).ConfigureAwait(false);
    }

    public async Task PatchAsync(Uri requestUri, CancellationToken cancellationToken = default)
    {
        HttpClient httpClient = CreateClient();
        using CancellationTokenSource operation = CreateOperationTimeout(httpClient, cancellationToken);

        using HttpRequestMessage message = new(HttpMethod.Patch, requestUri);

        using HttpResponseMessage response = await httpClient
            .SendAsync(message, HttpCompletionOption.ResponseHeadersRead, operation.Token)
            .ConfigureAwait(false);
        await EnsureSuccessAsync(response, HttpMethod.Patch, requestUri, operation.Token).ConfigureAwait(false);
    }

    private async Task<TResponse> SendFileAsync<TResponse>(
        HttpMethod method,
        Uri requestUri,
        GitLabFileUpload file,
        IReadOnlyDictionary<string, string>? formFields,
        JsonTypeInfo<TResponse> responseTypeInfo,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(file);

        HttpClient httpClient = CreateClient();
        using CancellationTokenSource operation = CreateOperationTimeout(httpClient, cancellationToken);

        // HttpRequestMessage.Dispose disposes its Content, which disposes every part of the multipart body -
        // hence BorrowedStreamContent for the file itself, so the caller's stream survives the call.
        using HttpRequestMessage message = new(method, requestUri)
        {
            Content = BuildMultipartContent(file, formFields)
        };

        using HttpResponseMessage response = await httpClient
            .SendAsync(message, HttpCompletionOption.ResponseHeadersRead, operation.Token)
            .ConfigureAwait(false);
        await EnsureSuccessAsync(response, method, requestUri, operation.Token).ConfigureAwait(false);

        return await ReadBodyAsync(response, responseTypeInfo, httpClient.Timeout, method, requestUri,
                operation.Token, cancellationToken)
            .ConfigureAwait(false);
    }

    [SuppressMessage(
        "Reliability",
        "CA2000:Dispose objects before losing scope",
        Justification =
            "MultipartFormDataContent takes ownership of every part added to it and disposes them with itself; " +
            "it is in turn owned by the HttpRequestMessage in SendFileAsync, which is scoped with `using`. " +
            "The one part that must NOT dispose what it wraps is BorrowedStreamContent, by design.")]
    private static MultipartFormDataContent BuildMultipartContent(
        GitLabFileUpload file,
        IReadOnlyDictionary<string, string>? formFields)
    {
        MultipartFormDataContent content = new();

        // Fields first, file last: GitLab's Grape parameter coercion does not care about part order, but a
        // server streaming the request can start acting on the metadata before the bytes arrive.
        if (formFields is not null)
        {
            foreach (KeyValuePair<string, string> field in formFields)
            {
                content.Add(new StringContent(field.Value), field.Key);
            }
        }

        BorrowedStreamContent fileContent = new(file.Content);

        if (!string.IsNullOrWhiteSpace(file.ContentType))
        {
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
        }

        content.Add(fileContent, file.FieldName, file.FileName);

        return content;
    }

    /// <summary>
    ///     Deliberately not disposed by the caller: <see cref="IHttpClientFactory" /> owns the pooled handler
    ///     chain, and disposing the client would tear down the response still being streamed under
    ///     <see cref="HttpCompletionOption.ResponseHeadersRead" />. Asking for a client per operation - rather
    ///     than holding one - is what keeps handler rotation and options reloads working from a singleton.
    /// </summary>
    private HttpClient CreateClient()
    {
        return _httpClientProvider();
    }

    /// <summary>
    ///     <see cref="HttpClient.Timeout" /> stops applying once the response headers arrive, so under
    ///     <see cref="HttpCompletionOption.ResponseHeadersRead" /> it does not bound the body read at all: a
    ///     server that sends headers and then stalls would hang the caller forever with no exception and no
    ///     log line. This linked source re-applies the same budget across the whole operation.
    /// </summary>
    private static CancellationTokenSource CreateOperationTimeout(HttpClient httpClient,
        CancellationToken cancellationToken)
    {
        CancellationTokenSource source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        if (httpClient.Timeout != Timeout.InfiniteTimeSpan)
        {
            source.CancelAfter(httpClient.Timeout);
        }

        return source;
    }

    private static async Task<TResponse> ReadBodyAsync<TResponse>(
        HttpResponseMessage response,
        JsonTypeInfo<TResponse> responseTypeInfo,
        TimeSpan timeout,
        HttpMethod requestMethod,
        Uri requestUri,
        CancellationToken operationToken,
        CancellationToken callerToken)
    {
        TResponse? result = await ReadOptionalBodyAsync(response, responseTypeInfo, timeout, operationToken,
            callerToken).ConfigureAwait(false);

        return result ?? throw GitLabApiExceptionFactory.CreateForEmptyBody(response, requestMethod, requestUri);
    }

    private static async Task<TResponse?> ReadOptionalBodyAsync<TResponse>(
        HttpResponseMessage response,
        JsonTypeInfo<TResponse> responseTypeInfo,
        TimeSpan timeout,
        CancellationToken operationToken,
        CancellationToken callerToken)
    {
        try
        {
            Stream stream = await response.Content.ReadAsStreamAsync(operationToken).ConfigureAwait(false);
            return await JsonSerializer.DeserializeAsync(stream, responseTypeInfo, operationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (!callerToken.IsCancellationRequested)
        {
            // Our own budget elapsed rather than the caller cancelling, so mirror what HttpClient itself
            // throws when its Timeout expires instead of reporting someone else's cancellation.
            throw new TaskCanceledException(
                string.Create(CultureInfo.InvariantCulture,
                    $"The GitLab response body was not read within the configured timeout of {timeout}."),
                new TimeoutException());
        }
    }

    /// <summary>
    ///     GitLab advertises ABSOLUTE next-page URLs, and the authentication handler attaches the caller's
    ///     credential to whatever this pipeline is asked to request. A <c>Link</c> header pointing off-instance
    ///     would therefore both leak the token and silently enumerate someone else's data, so refuse it loudly.
    ///     The mundane version of the same failure is far more likely: a self-managed instance behind a proxy
    ///     advertising an <c>external_url</c> the client cannot reach, where a clear message beats a DNS error
    ///     on page 2.
    /// </summary>
    private static Uri ResolveNextPage(string nextPageUrl, Uri? baseAddress)
    {
        Uri candidate = new(nextPageUrl, UriKind.RelativeOrAbsolute);

        if (candidate.IsAbsoluteUri
            && baseAddress is not null
            && Uri.Compare(candidate, baseAddress, UriComponents.SchemeAndServer, UriFormat.UriEscaped,
                StringComparison.OrdinalIgnoreCase) != 0)
        {
            throw new GitLabApiException(
                $"The pagination Link header pointed at '{candidate.GetLeftPart(UriPartial.Authority)}', which is " +
                $"not the configured GitLab instance '{baseAddress.GetLeftPart(UriPartial.Authority)}'.");
        }

        return candidate;
    }

    /// <summary>
    ///     Not <c>async</c> on purpose: the success path is synchronous, so it returns a completed
    ///     <see cref="ValueTask" /> without building a state machine. Only the failure path - which is about to
    ///     throw anyway - pays for one.
    /// </summary>
    private static ValueTask EnsureSuccessAsync(
        HttpResponseMessage response,
        HttpMethod requestMethod,
        Uri requestUri,
        CancellationToken cancellationToken)
    {
        return response.IsSuccessStatusCode
            ? ValueTask.CompletedTask
            : ThrowMappedAsync(response, requestMethod, requestUri, cancellationToken);
    }

    private static async ValueTask ThrowMappedAsync(
        HttpResponseMessage response,
        HttpMethod requestMethod,
        Uri requestUri,
        CancellationToken cancellationToken)
    {
        string body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        throw GitLabApiExceptionFactory.Create(response, requestMethod, requestUri, body);
    }
}