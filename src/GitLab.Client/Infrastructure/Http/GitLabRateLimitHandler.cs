using GitLab.Client.Infrastructure.RateLimiting;

namespace GitLab.Client.Infrastructure.Http;

internal sealed class GitLabRateLimitHandler(IGitLabRateLimitWriter tracker) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        HttpResponseMessage response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        tracker.Update(GitLabRateLimitSnapshot.FromHeaders(response.Headers));
        return response;
    }

    protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        HttpResponseMessage response = base.Send(request, cancellationToken);
        tracker.Update(GitLabRateLimitSnapshot.FromHeaders(response.Headers));
        return response;
    }
}