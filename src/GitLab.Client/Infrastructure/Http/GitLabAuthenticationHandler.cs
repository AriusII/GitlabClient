using System.Net.Http.Headers;

using GitLab.Client.DependencyInjection;

using Microsoft.Extensions.Options;

namespace GitLab.Client.Infrastructure.Http;

internal sealed class GitLabAuthenticationHandler(IOptionsMonitor<GitLabClientOptions> options) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        Authenticate(request);
        return base.SendAsync(request, cancellationToken);
    }

    protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Authenticate(request);
        return base.Send(request, cancellationToken);
    }

    private void Authenticate(HttpRequestMessage request)
    {
        GitLabClientOptions current = options.CurrentValue;

        if (!IsConfiguredInstance(request.RequestUri, current.BaseAddress))
        {
            // Never hand GitLab credentials to a host the caller did not configure. Not every request URI on
            // this pipeline comes from the route builder: GetPagedAsync follows an absolute URL the server
            // chose, and IGitLabApiConnection is a public escape hatch that accepts any Uri. Silently sending
            // no credential (rather than throwing) keeps this a guard rather than a new failure mode - the
            // request simply comes back 401 from wherever it was pointed.
            return;
        }

        switch (current.AuthenticationMode)
        {
            case GitLabAuthenticationMode.PersonalAccessToken:
                request.Headers.TryAddWithoutValidation("PRIVATE-TOKEN", current.AccessToken);
                break;
            case GitLabAuthenticationMode.JobToken:
                request.Headers.TryAddWithoutValidation("JOB-TOKEN", current.AccessToken);
                break;
            case GitLabAuthenticationMode.OAuthBearer:
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", current.AccessToken);
                break;
            default:
                throw new InvalidOperationException($"Unsupported authentication mode '{current.AuthenticationMode}'.");
        }
    }

    /// <summary>
    ///     <see cref="HttpClient" /> resolves a relative request URI against its <c>BaseAddress</c> before the
    ///     handler chain runs, so <paramref name="requestUri" /> is already absolute by the time we see it.
    ///     Scheme, host and port must all match: an http-to-https hop is a different origin and must not carry
    ///     the token either.
    /// </summary>
    private static bool IsConfiguredInstance(Uri? requestUri, Uri? baseAddress)
    {
        return requestUri is not null
               && baseAddress is not null
               && requestUri.IsAbsoluteUri
               && baseAddress.IsAbsoluteUri
               && Uri.Compare(requestUri, baseAddress, UriComponents.SchemeAndServer, UriFormat.UriEscaped,
                   StringComparison.OrdinalIgnoreCase) == 0;
    }
}