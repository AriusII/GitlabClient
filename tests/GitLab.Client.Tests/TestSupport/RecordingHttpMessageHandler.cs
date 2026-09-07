namespace GitLab.Client.Tests.TestSupport;

/// <summary>
///     An <see cref="HttpMessageHandler" /> test double that records every request it sees and can answer each
///     one differently, which <see cref="StubHttpMessageHandler" /> cannot: it keeps only the last request and
///     answers every request the same way. Needed for anything multi-request - pagination, redirects, retries.
/// </summary>
internal sealed class RecordingHttpMessageHandler(Func<HttpRequestMessage, int, HttpResponseMessage> respond)
    : HttpMessageHandler
{
    private readonly List<HttpRequestMessage> _requests = [];

    /// <summary>Every request seen so far, oldest first.</summary>
    public IReadOnlyList<HttpRequestMessage> Requests => _requests;

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        int requestIndex = _requests.Count;
        _requests.Add(request);
        return Task.FromResult(respond(request, requestIndex));
    }
}