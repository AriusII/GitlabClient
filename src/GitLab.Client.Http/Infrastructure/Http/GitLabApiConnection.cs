using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

using GitLab.Client.Abstractions;
using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Configuration;
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

            TItem[]? page;
            string? nextPageUrl;
            Uri? responseUri;

            // The complete page is deserialized before yielding its first item. Dispose the response before
            // the consumer's iterator suspension point so a slow consumer never pins an HTTP connection from
            // the factory pool for an entire page.
            using (HttpResponseMessage response = await httpClient
                       .GetAsync(next, HttpCompletionOption.ResponseHeadersRead, operation.Token)
                       .ConfigureAwait(false))
            {
                await EnsureSuccessAsync(response, HttpMethod.Get, next, operation.Token).ConfigureAwait(false);
                page = await ReadOptionalBodyAsync(response, pageTypeInfo, httpClient.Timeout, operation.Token,
                    cancellationToken).ConfigureAwait(false);
                nextPageUrl = GitLabLinkHeader.GetNextPageUrl(response.Headers);
                responseUri = response.RequestMessage?.RequestUri ?? next;
            }

            if (page is not null)
            {
                foreach (TItem item in page)
                {
                    yield return item;
                }
            }

            // Resolve after yielding the already-read page. This preserves normal async-enumerable
            // semantics (the consumer can use page one) while the response itself has already been disposed
            // before the first yield above.
            next = nextPageUrl is null
                ? null
                : ResolveNextPage(nextPageUrl, responseUri, httpClient.BaseAddress);
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
        using HttpRequestMessage message = new(HttpMethod.Post, requestUri);
        message.Content = JsonContent.Create(request, requestTypeInfo);

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

        using HttpRequestMessage message = new(HttpMethod.Put, requestUri);
        message.Content = JsonContent.Create(request, requestTypeInfo);

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

        using HttpRequestMessage message = new(HttpMethod.Post, requestUri);
        message.Content = JsonContent.Create(request, requestTypeInfo);

        using HttpResponseMessage response = await httpClient
            .SendAsync(message, HttpCompletionOption.ResponseHeadersRead, operation.Token)
            .ConfigureAwait(false);
        await EnsureSuccessAsync(response, HttpMethod.Post, requestUri, operation.Token).ConfigureAwait(false);
    }

    public Task<TResponse> PostMultipartFormAsync<TResponse>(
        Uri requestUri,
        IReadOnlyDictionary<string, string> formFields,
        JsonTypeInfo<TResponse> responseTypeInfo,
        CancellationToken cancellationToken = default)
    {
        return SendFormAsync(HttpMethod.Post, requestUri, BuildMultipartFormContent(formFields), responseTypeInfo,
            cancellationToken);
    }

    public Task PostMultipartFormAsync(
        Uri requestUri,
        IReadOnlyDictionary<string, string> formFields,
        CancellationToken cancellationToken = default)
    {
        return SendFormAsync(HttpMethod.Post, requestUri, BuildMultipartFormContent(formFields), cancellationToken);
    }

    public Task<TResponse> PutMultipartFormAsync<TResponse>(
        Uri requestUri,
        IReadOnlyDictionary<string, string> formFields,
        JsonTypeInfo<TResponse> responseTypeInfo,
        CancellationToken cancellationToken = default)
    {
        return SendFormAsync(HttpMethod.Put, requestUri, BuildMultipartFormContent(formFields), responseTypeInfo,
            cancellationToken);
    }

    public Task PutMultipartFormAsync(
        Uri requestUri,
        IReadOnlyDictionary<string, string> formFields,
        CancellationToken cancellationToken = default)
    {
        return SendFormAsync(HttpMethod.Put, requestUri, BuildMultipartFormContent(formFields), cancellationToken);
    }

    public Task<TResponse> PostUrlEncodedFormAsync<TResponse>(
        Uri requestUri,
        IReadOnlyDictionary<string, string> formFields,
        JsonTypeInfo<TResponse> responseTypeInfo,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(formFields);

        return SendFormAsync(HttpMethod.Post, requestUri, new FormUrlEncodedContent(formFields), responseTypeInfo,
            cancellationToken);
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

        using HttpRequestMessage message = new(HttpMethod.Put, requestUri);
        message.Content = JsonContent.Create(request, requestTypeInfo);

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

        // Unlike every other verb, an upload's slow, size-dependent phase is the SEND, which happens INSIDE
        // this SendAsync call rather than after it returns - the mirror image of GetFileAsync's post-header
        // download, which happens after its SendAsync call returns. That is exactly why GetFileAsync's trick
        // of standing its own CancelAfter down does not transfer here as-is: HttpClient enforces its Timeout
        // by wrapping whatever token SendAsync is given in one more linked source and arming that source's
        // own CancelAfter(Timeout) internally (see the exception's "canceled due to the configured
        // HttpClient.Timeout" wording, which only HttpClient's own wrapper produces) - so a fixed budget still
        // bounds the send even when our own token carries none. CreateClient() hands back a fresh, not-yet-
        // used HttpClient per call (see CreateClient's own remarks), so disarming that budget on THIS instance
        // - rather than the token we pass it - is what actually frees the send from it, the same way
        // GetFileAsync frees the body read. The linked operation token still carries the caller's own
        // cancellation.
        // The send has no fixed budget; after headers, reapply the client-configured budget to error handling.
        TimeSpan responseBodyTimeout = httpClient.Timeout;
        httpClient.Timeout = Timeout.InfiniteTimeSpan;
        using CancellationTokenSource operation = CreateUnboundedOperation(cancellationToken);

        // As in SendFileAsync: the request message owns the multipart content and disposes every part with
        // itself, which is why the file part is a BorrowedStreamContent and the caller's stream survives.
        using HttpRequestMessage message = new(HttpMethod.Post, requestUri);
        message.Content = BuildMultipartContent(file, formFields);

        using HttpResponseMessage response = await httpClient
            .SendAsync(message, HttpCompletionOption.ResponseHeadersRead, operation.Token)
            .ConfigureAwait(false);
        using CancellationTokenSource responseBodyOperation = CreateOperationTimeout(responseBodyTimeout,
            cancellationToken);
        await EnsureSuccessAsync(response, HttpMethod.Post, requestUri, responseBodyOperation.Token)
            .ConfigureAwait(false);
    }

    public async Task PutFileAsync(
        Uri requestUri,
        GitLabFileUpload file,
        IReadOnlyDictionary<string, string>? formFields,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(file);

        HttpClient httpClient = CreateClient();

        // See PostFileAsync above: the send, not a post-header receive, is this upload's payload-size-
        // dependent phase, and it happens inside this SendAsync call - the one span HttpClient always times
        // out itself regardless of the token it is given. Disarming Timeout on this call's own fresh
        // HttpClient instance is what actually frees the send from that fixed budget; the operation token
        // still carries the caller's own cancellation.
        // The send has no fixed budget; after headers, reapply the client-configured budget to error handling.
        TimeSpan responseBodyTimeout = httpClient.Timeout;
        httpClient.Timeout = Timeout.InfiniteTimeSpan;
        using CancellationTokenSource operation = CreateUnboundedOperation(cancellationToken);

        // As in SendFileAsync: the request message owns the multipart content and disposes every part with
        // itself, which is why the file part is a BorrowedStreamContent and the caller's stream survives.
        using HttpRequestMessage message = new(HttpMethod.Put, requestUri);
        message.Content = BuildMultipartContent(file, formFields);

        using HttpResponseMessage response = await httpClient
            .SendAsync(message, HttpCompletionOption.ResponseHeadersRead, operation.Token)
            .ConfigureAwait(false);
        using CancellationTokenSource responseBodyOperation = CreateOperationTimeout(responseBodyTimeout,
            cancellationToken);
        await EnsureSuccessAsync(response, HttpMethod.Put, requestUri, responseBodyOperation.Token)
            .ConfigureAwait(false);
    }

    public async Task<GitLabRedirectResponse> GetRedirectAsync(Uri requestUri,
        CancellationToken cancellationToken = default)
    {
        HttpClient httpClient = CreateClient();
        using CancellationTokenSource operation = CreateOperationTimeout(httpClient, cancellationToken);

        using HttpRequestMessage message = new(HttpMethod.Get, requestUri);

        using HttpResponseMessage response = await httpClient
            .SendAsync(message, HttpCompletionOption.ResponseHeadersRead, operation.Token)
            .ConfigureAwait(false);

        // A 3xx is the ANSWER on this route, not a failure - GitLab hides the real package location behind
        // a redirect instead of streaming it. Every other non-success status still maps to its typed
        // exception, exactly as HeadAsync does the equivalent carve-out for 404.
        bool isRedirect = (int)response.StatusCode is >= 300 and < 400;
        if (!isRedirect)
        {
            await EnsureSuccessAsync(response, HttpMethod.Get, requestUri, operation.Token).ConfigureAwait(false);
        }

        return GitLabRedirectResponse.FromResponse(response);
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

        using HttpRequestMessage message = new(HttpMethod.Patch, requestUri);
        message.Content = JsonContent.Create(request, requestTypeInfo);

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

        using HttpRequestMessage message = new(HttpMethod.Patch, requestUri);
        message.Content = JsonContent.Create(request, requestTypeInfo);

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

        // See PostFileAsync/PutFileAsync above: sending the (potentially large) multipart body is this
        // upload's payload-size-dependent phase, and it happens inside this SendAsync call, which HttpClient
        // always times out itself off its own Timeout regardless of the token it is given. Disarming Timeout
        // on this call's own fresh HttpClient instance is what actually frees the send from that fixed
        // budget - only the caller's own token can abort this call from here on.
        // The send has no fixed budget; after headers, reapply the client-configured budget to JSON reading.
        TimeSpan responseBodyTimeout = httpClient.Timeout;
        httpClient.Timeout = Timeout.InfiniteTimeSpan;
        using CancellationTokenSource operation = CreateUnboundedOperation(cancellationToken);

        // HttpRequestMessage.Dispose disposes its Content, which disposes every part of the multipart body -
        // hence BorrowedStreamContent for the file itself, so the caller's stream survives the call.
        using HttpRequestMessage message = new(method, requestUri);
        message.Content = BuildMultipartContent(file, formFields);

        using HttpResponseMessage response = await httpClient
            .SendAsync(message, HttpCompletionOption.ResponseHeadersRead, operation.Token)
            .ConfigureAwait(false);
        using CancellationTokenSource responseBodyOperation = CreateOperationTimeout(responseBodyTimeout,
            cancellationToken);
        await EnsureSuccessAsync(response, method, requestUri, responseBodyOperation.Token).ConfigureAwait(false);

        return await ReadBodyAsync(response, responseTypeInfo, responseBodyTimeout, method, requestUri,
                responseBodyOperation.Token, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    ///     Sends one of the small, all-text form bodies declared by the API specification. Unlike a file
    ///     upload, form fields are buffered in memory, so the regular request timeout remains appropriate for
    ///     both the send and the response body.
    /// </summary>
    private async Task<TResponse> SendFormAsync<TResponse>(
        HttpMethod method,
        Uri requestUri,
        HttpContent formContent,
        JsonTypeInfo<TResponse> responseTypeInfo,
        CancellationToken cancellationToken)
    {
        HttpClient httpClient = CreateClient();
        using CancellationTokenSource operation = CreateOperationTimeout(httpClient, cancellationToken);

        // The request owns the supplied content, including all multipart field parts. It deliberately uses
        // ResponseHeadersRead so the source-generated deserializer receives the response stream directly.
        using HttpRequestMessage message = new(method, requestUri);
        message.Content = formContent;

        using HttpResponseMessage response = await httpClient
            .SendAsync(message, HttpCompletionOption.ResponseHeadersRead, operation.Token)
            .ConfigureAwait(false);
        await EnsureSuccessAsync(response, method, requestUri, operation.Token).ConfigureAwait(false);

        return await ReadBodyAsync(response, responseTypeInfo, httpClient.Timeout, method, requestUri,
                operation.Token, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    ///     The no-content counterpart of <see cref="SendFormAsync{TResponse}" />. Keeping it separate means
    ///     a successful <c>204 No Content</c> never reaches the JSON deserializer.
    /// </summary>
    private async Task SendFormAsync(
        HttpMethod method,
        Uri requestUri,
        HttpContent formContent,
        CancellationToken cancellationToken)
    {
        HttpClient httpClient = CreateClient();
        using CancellationTokenSource operation = CreateOperationTimeout(httpClient, cancellationToken);

        using HttpRequestMessage message = new(method, requestUri);
        message.Content = formContent;

        using HttpResponseMessage response = await httpClient
            .SendAsync(message, HttpCompletionOption.ResponseHeadersRead, operation.Token)
            .ConfigureAwait(false);
        await EnsureSuccessAsync(response, method, requestUri, operation.Token).ConfigureAwait(false);
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

    [SuppressMessage(
        "Reliability",
        "CA2000:Dispose objects before losing scope",
        Justification =
            "MultipartFormDataContent takes ownership of its StringContent parts and is in turn owned by the " +
            "HttpRequestMessage created by SendFormAsync, which scopes it with `using`.")]
    private static MultipartFormDataContent BuildMultipartFormContent(IReadOnlyDictionary<string, string> formFields)
    {
        ArgumentNullException.ThrowIfNull(formFields);

        MultipartFormDataContent content = new();
        foreach (KeyValuePair<string, string> field in formFields)
        {
            content.Add(new StringContent(field.Value), field.Key);
        }

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
        ArgumentNullException.ThrowIfNull(httpClient);

        return CreateOperationTimeout(httpClient.Timeout, cancellationToken);
    }

    /// <summary>
    ///     Creates a caller-cancellable operation budget without registering against <paramref name="cancellationToken" />
    ///     when it can never be cancelled. Most SDK calls use the default token, so avoiding the linked-source
    ///     registration on that path removes unnecessary per-request work while preserving the timeout budget.
    /// </summary>
    private static CancellationTokenSource CreateOperationTimeout(TimeSpan timeout,
        CancellationToken cancellationToken)
    {
        CancellationTokenSource source = cancellationToken.CanBeCanceled
            ? CancellationTokenSource.CreateLinkedTokenSource(cancellationToken)
            : new CancellationTokenSource();

        if (timeout != Timeout.InfiniteTimeSpan)
        {
            source.CancelAfter(timeout);
        }

        return source;
    }

    /// <summary>
    ///     The upload-side counterpart of <see cref="GetFileAsync" /> standing its countdown down once headers
    ///     arrive: there, the caller-paced phase is the download, which happens after that method's own
    ///     <c>SendAsync</c> call returns, so lifting the self-imposed budget on the token passed to the
    ///     subsequent body read is enough. Here the caller-paced, payload-size-dependent phase is the send
    ///     itself, which happens INSIDE the <c>SendAsync</c> call - and <see cref="HttpClient" /> arms its own
    ///     <see cref="HttpClient.Timeout" />-based cancellation for that whole call by wrapping whatever token
    ///     it is given, regardless of what that token does or does not do. So unlike the download side, this
    ///     source alone cannot exempt the send from the fixed budget; each upload call site also sets its own
    ///     freshly obtained <see cref="HttpClient" />'s <see cref="HttpClient.Timeout" /> to
    ///     <see cref="Timeout.InfiniteTimeSpan" /> before calling this. What this source still does is exactly
    ///     what <see cref="CreateOperationTimeout(HttpClient,CancellationToken)" /> does for every JSON call: link to the
    ///     caller's own token,
    ///     so their cancellation still aborts the call - just without also re-applying a fixed budget on top.
    ///     Used by <see cref="SendFileAsync{TResponse}" /> and the non-generic
    ///     <see cref="PostFileAsync(Uri,GitLabFileUpload,IReadOnlyDictionary{string,string}?,CancellationToken)" />
    ///     / <see cref="PutFileAsync(Uri,GitLabFileUpload,IReadOnlyDictionary{string,string}?,CancellationToken)" />
    ///     overloads.
    /// </summary>
    private static CancellationTokenSource CreateUnboundedOperation(CancellationToken cancellationToken)
    {
        return cancellationToken.CanBeCanceled
            ? CancellationTokenSource.CreateLinkedTokenSource(cancellationToken)
            : new CancellationTokenSource();
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
    private static Uri ResolveNextPage(string nextPageUrl, Uri? responseUri, Uri? trustedBaseAddress)
    {
        // A real HttpClient response carries its absolute RequestUri. A synthetic handler used by a
        // consumer's tests may omit it, though, so fall back to the configured absolute base in that
        // case rather than trying to resolve a relative link against another relative URI.
        Uri? resolutionBase = responseUri is { IsAbsoluteUri: true } ? responseUri : trustedBaseAddress;
        return resolutionBase is null || trustedBaseAddress is null
                                      || !Uri.TryCreate(resolutionBase, nextPageUrl, out Uri? candidate)
            ? throw new GitLabApiException(
                "The pagination Link header did not contain a URI that can be resolved against the configured GitLab instance.")
            : Uri.Compare(candidate, trustedBaseAddress, UriComponents.SchemeAndServer, UriFormat.UriEscaped,
                StringComparison.OrdinalIgnoreCase) != 0
                ? throw new GitLabApiException(
                    $"The pagination Link header pointed at '{candidate.GetLeftPart(UriPartial.Authority)}', which is " +
                    $"not the configured GitLab instance '{trustedBaseAddress.GetLeftPart(UriPartial.Authority)}'.")
                : candidate;
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
        string body = await ReadErrorBodyAsync(response.Content, cancellationToken).ConfigureAwait(false);
        throw GitLabApiExceptionFactory.Create(response, requestMethod, requestUri, body);
    }

    /// <summary>
    ///     Reads only the diagnostic prefix of an error response.
    ///     <see cref="HttpContent.ReadAsStringAsync(CancellationToken)" /> buffers the entire entity before
    ///     returning, which lets a proxy-generated multi-megabyte HTML error page turn a failed API call into an
    ///     avoidable allocation spike. The exception factory only retains this bounded value, so read and
    ///     retention are capped together.
    /// </summary>
    private static async ValueTask<string> ReadErrorBodyAsync(HttpContent content, CancellationToken cancellationToken)
    {
        const string truncatedSuffix = "... [response body truncated]";
        // ArrayPool promises an array *at least* this large; it may hand back a larger bucket. Keep every
        // I/O bound independent of that implementation detail, otherwise a large rented array turns this
        // diagnostic-prefix reader back into an oversized read.
        const int readBufferSize = 1024;
        int retainedCharacterCount = GitLabApiExceptionFactory.MaxRetainedResponseBodyLength - truncatedSuffix.Length;
        Encoding encoding = GetErrorBodyEncoding(content);
        int characterBufferSize = encoding.GetMaxCharCount(readBufferSize);
        byte[] byteBuffer = ArrayPool<byte>.Shared.Rent(readBufferSize);
        char[] characterBuffer = ArrayPool<char>.Shared.Rent(characterBufferSize);

        try
        {
            StringBuilder body = new(Math.Min(retainedCharacterCount, readBufferSize));
            // StreamReader owns an independent byte buffer and can prefetch beyond the prefix retained below.
            // Decode each strictly bounded stream read ourselves instead, retaining decoder state for characters
            // that span a network-buffer boundary.
            Decoder decoder = encoding.GetDecoder();
            using Stream stream = await content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);

            while (body.Length <= retainedCharacterCount)
            {
                int bytesRead = await stream
                    .ReadAsync(byteBuffer.AsMemory(0, readBufferSize), cancellationToken)
                    .ConfigureAwait(false);

                int remaining = retainedCharacterCount + 1 - body.Length;
                decoder.Convert(
                    byteBuffer.AsSpan(0, bytesRead),
                    characterBuffer.AsSpan(0, Math.Min(characterBufferSize, remaining)),
                    bytesRead == 0,
                    out _,
                    out int charactersRead,
                    out _);

                body.Append(characterBuffer, 0, charactersRead);

                if (bytesRead == 0)
                {
                    return body.ToString();
                }
            }

            body.Length = retainedCharacterCount;
            body.Append(truncatedSuffix);
            return body.ToString();
        }
        finally
        {
            ArrayPool<char>.Shared.Return(characterBuffer);
            ArrayPool<byte>.Shared.Return(byteBuffer);
        }
    }

    private static Encoding GetErrorBodyEncoding(HttpContent content)
    {
        string? charset = content.Headers.ContentType?.CharSet;

        if (string.IsNullOrWhiteSpace(charset))
        {
            return Encoding.UTF8;
        }

        charset = charset.Trim();
        if (charset.Length > 1 && charset[0] == '"' && charset[^1] == '"')
        {
            charset = charset[1..^1];
        }

        try
        {
            return Encoding.GetEncoding(charset);
        }
        catch (ArgumentException)
        {
            return Encoding.UTF8;
        }
    }
}

/// <summary>
///     Strongly-typed form counterparts for the dictionary transport methods. This assembly exposes them to the
///     façade through <c>InternalsVisibleTo</c>, so endpoint implementations can reuse their request DTO and
///     source-generated <see cref="JsonTypeInfo{T}" /> rather than hand-maintaining form-field projections.
/// </summary>
internal static class GitLabApiConnectionFormExtensions
{
    extension(IGitLabApiConnection connection)
    {
        internal Task<TResponse> PostMultipartFormAsync<TRequest, TResponse>(Uri requestUri,
            TRequest request,
            JsonTypeInfo<TRequest> requestTypeInfo,
            JsonTypeInfo<TResponse> responseTypeInfo,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(connection);

            return connection.PostMultipartFormAsync(requestUri, GitLabFormFields.FromRequest(request, requestTypeInfo),
                responseTypeInfo, cancellationToken);
        }

        internal Task PostMultipartFormAsync<TRequest>(Uri requestUri,
            TRequest request,
            JsonTypeInfo<TRequest> requestTypeInfo,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(connection);

            return connection.PostMultipartFormAsync(requestUri, GitLabFormFields.FromRequest(request, requestTypeInfo),
                cancellationToken);
        }

        internal Task<TResponse> PutMultipartFormAsync<TRequest, TResponse>(Uri requestUri,
            TRequest request,
            JsonTypeInfo<TRequest> requestTypeInfo,
            JsonTypeInfo<TResponse> responseTypeInfo,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(connection);

            return connection.PutMultipartFormAsync(requestUri, GitLabFormFields.FromRequest(request, requestTypeInfo),
                responseTypeInfo, cancellationToken);
        }

        internal Task PutMultipartFormAsync<TRequest>(Uri requestUri,
            TRequest request,
            JsonTypeInfo<TRequest> requestTypeInfo,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(connection);

            return connection.PutMultipartFormAsync(requestUri, GitLabFormFields.FromRequest(request, requestTypeInfo),
                cancellationToken);
        }

        internal Task<TResponse> PostUrlEncodedFormAsync<TRequest, TResponse>(Uri requestUri,
            TRequest request,
            JsonTypeInfo<TRequest> requestTypeInfo,
            JsonTypeInfo<TResponse> responseTypeInfo,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(connection);

            return connection.PostUrlEncodedFormAsync(requestUri,
                GitLabFormFields.FromRequest(request, requestTypeInfo),
                responseTypeInfo, cancellationToken);
        }
    }
}

/// <summary>
///     Projects the JSON representation already defined by a source-generated context onto GitLab's bracketed
///     HTML-form convention. Serializing to <see cref="JsonElement" /> is intentional: it preserves each DTO's
///     JSON property names, enum wire values, dates and null-ignore policy without any reflection or separate
///     form-specific model metadata.
/// </summary>
internal static class GitLabFormFields
{
    /// <summary>
    ///     Produces an ordinally ordered, immutable-to-callers view of form fields. JSON nulls and empty
    ///     objects/arrays have no scalar form representation and are therefore omitted; a present scalar empty
    ///     string remains a field with an empty value.
    /// </summary>
    internal static IReadOnlyDictionary<string, string> FromRequest<TRequest>(TRequest request,
        JsonTypeInfo<TRequest> requestTypeInfo)
    {
        ArgumentNullException.ThrowIfNull(requestTypeInfo);

        JsonElement root = JsonSerializer.SerializeToElement(request, requestTypeInfo);
        if (root.ValueKind is not JsonValueKind.Object)
        {
            throw new ArgumentException("A GitLab form request must serialize to a JSON object.", nameof(request));
        }

        SortedDictionary<string, string> fields = new(StringComparer.Ordinal);
        AddObject(fields, root, null);
        return fields;
    }

    private static void AddObject(SortedDictionary<string, string> fields, JsonElement value, string? prefix)
    {
        foreach (JsonProperty property in value.EnumerateObject())
        {
            AddValue(fields, property.Value, AppendKey(prefix, property.Name));
        }
    }

    private static void AddValue(SortedDictionary<string, string> fields, JsonElement value, string key)
    {
        switch (value.ValueKind)
        {
            case JsonValueKind.Object:
                AddObject(fields, value, key);
                break;

            case JsonValueKind.Array:
                {
                    int index = 0;
                    foreach (JsonElement item in value.EnumerateArray())
                    {
                        AddValue(fields, item, AppendKey(key, index.ToString(CultureInfo.InvariantCulture)));
                        index++;
                    }

                    break;
                }

            case JsonValueKind.String:
                fields.Add(key, value.GetString() ?? string.Empty);
                break;

            case JsonValueKind.Number:
            case JsonValueKind.True:
            case JsonValueKind.False:
                fields.Add(key, value.GetRawText());
                break;

            case JsonValueKind.Null:
            case JsonValueKind.Undefined:
                break;

            default:
                throw new InvalidOperationException($"Unsupported JSON value kind '{value.ValueKind}'.");
        }
    }

    private static string AppendKey(string? prefix, string name)
    {
        return prefix is null ? name : string.Concat(prefix, "[", name, "]");
    }
}