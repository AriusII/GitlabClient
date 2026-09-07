namespace GitLab.Client.Tests.TestSupport;

/// <summary>An <see cref="HttpMessageHandler" /> test double, per this repo's rule to mock GitLab at the transport level.</summary>
internal sealed class StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
{
    public HttpRequestMessage? LastRequest { get; private set; }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        LastRequest = request;

        HttpResponseMessage response = respond(request);

        // Real handlers (SocketsHttpHandler) set this; a bare HttpMessageHandler does not. The client's error
        // mapping reads it to report the absolute URI that failed, so mirror the real behaviour here.
        response.RequestMessage ??= request;

        return Task.FromResult(response);
    }
}