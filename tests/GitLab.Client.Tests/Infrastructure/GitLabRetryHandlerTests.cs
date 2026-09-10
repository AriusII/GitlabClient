using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

using GitLab.Client.Configuration;
using GitLab.Client.Tests.TestSupport;

using Microsoft.Extensions.DependencyInjection;

namespace GitLab.Client.Tests.Infrastructure;

/// <summary>
///     Exercises the real DI-built handler chain with only the primary handler swapped out - see
///     <c>GitLabAuthenticationHandlerTests</c> for why. That also proves the registration order in
///     <c>AddGitLabClient</c>: <c>GitLabRetryHandler</c> has to actually be the outermost handler for these to
///     pass, since a misordered pipeline would let the primary handler's canned responses reach the caller
///     without ever being retried.
/// </summary>
public sealed class GitLabRetryHandlerTests : IDisposable
{
    private const string TestToken = "glpat-test-token";

    private static readonly Uri GitLabBaseAddress = new("https://gitlab.example/api/v4/");

    private readonly List<ServiceProvider> _providers = [];

    public void Dispose()
    {
        foreach (ServiceProvider provider in _providers)
        {
            provider.Dispose();
        }
    }

    [Fact]
    public async Task SendAsync_RetriesOn429_UpToTheBound_ThenSurfacesTheFinalResponse()
    {
        using RecordingHttpMessageHandler handler = new(static (_, _) =>
            TransientFailure(HttpStatusCode.TooManyRequests));
        using HttpClient client = CreateClient(handler);

        using HttpResponseMessage response = await client.GetAsync(
            new Uri("projects/1", UriKind.Relative), TestContext.Current.CancellationToken);

        // 3 retries + the initial attempt = 4 total sends, matching GitLabRetryHandler.MaxRetryAttempts.
        Assert.Equal(4, handler.Requests.Count);
        Assert.Equal(HttpStatusCode.TooManyRequests, response.StatusCode);
    }

    [Theory]
    [InlineData(HttpStatusCode.OK)]
    [InlineData(HttpStatusCode.NotFound)]
    [InlineData(HttpStatusCode.Unauthorized)]
    public async Task SendAsync_DoesNotRetry_OnSuccessOrNonTransientErrors(HttpStatusCode statusCode)
    {
        using RecordingHttpMessageHandler handler = new((_, _) =>
            new HttpResponseMessage(statusCode) { Content = new StringContent("{}") });
        using HttpClient client = CreateClient(handler);

        using HttpResponseMessage response = await client.GetAsync(
            new Uri("projects/1", UriKind.Relative), TestContext.Current.CancellationToken);

        Assert.Single(handler.Requests);
        Assert.Equal(statusCode, response.StatusCode);
    }

    [Fact]
    public async Task SendAsync_RetriesOnBadGatewayAndServiceUnavailableAndGatewayTimeout()
    {
        foreach (HttpStatusCode statusCode in new[]
                 {
                     HttpStatusCode.BadGateway, HttpStatusCode.ServiceUnavailable, HttpStatusCode.GatewayTimeout
                 })
        {
            using RecordingHttpMessageHandler handler = new((_, requestIndex) =>
                requestIndex == 0 ? TransientFailure(statusCode) : Ok());
            using HttpClient client = CreateClient(handler);

            using HttpResponseMessage response = await client.GetAsync(
                new Uri("projects/1", UriKind.Relative), TestContext.Current.CancellationToken);

            Assert.Equal(2, handler.Requests.Count);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }

    [Fact]
    public async Task SendAsync_RecoversOnceTheServerStopsFailing()
    {
        using RecordingHttpMessageHandler handler = new(static (_, requestIndex) =>
            requestIndex < 2 ? TransientFailure(HttpStatusCode.ServiceUnavailable) : Ok());
        using HttpClient client = CreateClient(handler);

        using HttpResponseMessage response = await client.GetAsync(
            new Uri("projects/1", UriKind.Relative), TestContext.Current.CancellationToken);

        Assert.Equal(3, handler.Requests.Count);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task SendAsync_HonoursANumericRetryAfterHeader()
    {
        TimeSpan retryAfter = TimeSpan.FromSeconds(1);

        using RecordingHttpMessageHandler handler = new(static (_, requestIndex) =>
        {
            if (requestIndex > 0)
            {
                return Ok();
            }

            HttpResponseMessage response = TransientFailure(HttpStatusCode.TooManyRequests);
            response.Headers.RetryAfter = new RetryConditionHeaderValue(
                TimeSpan.FromSeconds(1));
            return response;
        });
        using HttpClient client = CreateClient(handler);

        Stopwatch stopwatch = Stopwatch.StartNew();
        using HttpResponseMessage response = await client.GetAsync(
            new Uri("projects/1", UriKind.Relative), TestContext.Current.CancellationToken);
        stopwatch.Stop();

        Assert.Equal(2, handler.Requests.Count);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        // Generous lower bound: proves the header's delay was actually awaited rather than the (much
        // smaller) default backoff, without making the test flaky under CI scheduling jitter.
        Assert.True(stopwatch.Elapsed >= retryAfter - TimeSpan.FromMilliseconds(150),
            $"Expected to honour the 1s Retry-After header; only waited {stopwatch.Elapsed}.");
    }

    [Fact]
    public async Task SendAsync_DoesNotRetry_WhenRequestContentCannotBeSafelyResent()
    {
        // StreamContent wraps a stream this handler cannot prove is still at position 0 after the first
        // send, unlike the ByteArrayContent/JsonContent bodies every other write path in this library uses -
        // see GitLabRetryHandler.TryReuseContent. A single transient response must therefore pass straight
        // through unretried.
        using RecordingHttpMessageHandler handler = new(static (_, _) =>
            TransientFailure(HttpStatusCode.ServiceUnavailable));
        using HttpClient client = CreateClient(handler);

        using MemoryStream body = new("{}"u8.ToArray());
        using StreamContent content = new(body);
        using HttpResponseMessage response = await client.PostAsync(
            new Uri("projects/1/hooks", UriKind.Relative), content, TestContext.Current.CancellationToken);

        Assert.Single(handler.Requests);
        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }

    [Fact]
    public async Task SendAsync_DoesNotRetryMutatingRequestsEvenWhenTheirContentIsReplayable()
    {
        // Replayable bytes are not enough to make a POST safe: the connection can fail after GitLab has
        // created the resource, so an automatic second send could duplicate a visible mutation.
        using RecordingHttpMessageHandler handler = new(static (_, requestIndex) =>
            requestIndex == 0 ? TransientFailure(HttpStatusCode.ServiceUnavailable) : Ok());
        using HttpClient client = CreateClient(handler);

        using StringContent content = new("{\"name\":\"hook\"}", Encoding.UTF8, "application/json");
        using HttpResponseMessage response = await client.PostAsync(
            new Uri("projects/1/hooks", UriKind.Relative), content, TestContext.Current.CancellationToken);

        Assert.Single(handler.Requests);
        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }

    [Fact]
    public async Task SendAsync_ReauthenticatesEveryRetryAttempt()
    {
        // Retries must pass back through GitLabAuthenticationHandler, which sits INSIDE (added after)
        // GitLabRetryHandler in the pipeline - proving the outermost-registration ordering documented on
        // AddGitLabClient actually takes effect, not just that the handler compiles correctly in isolation.
        using RecordingHttpMessageHandler handler = new(static (_, requestIndex) =>
            requestIndex == 0 ? TransientFailure(HttpStatusCode.TooManyRequests) : Ok());
        using HttpClient client = CreateClient(handler);

        using HttpResponseMessage response = await client.GetAsync(
            new Uri("projects/1", UriKind.Relative), TestContext.Current.CancellationToken);

        Assert.Equal(2, handler.Requests.Count);
        Assert.All(handler.Requests, r => Assert.Equal(TestToken, r.Headers.GetValues("PRIVATE-TOKEN").Single()));
    }

    private static HttpResponseMessage Ok()
    {
        return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{}") };
    }

    private static HttpResponseMessage TransientFailure(HttpStatusCode statusCode)
    {
        return new HttpResponseMessage(statusCode) { Content = new StringContent("{}") };
    }

    private HttpClient CreateClient(HttpMessageHandler primaryHandler)
    {
        ServiceCollection services = new();
        services.AddGitLabClient(options =>
            {
                options.AccessToken = TestToken;
                options.BaseAddress = GitLabBaseAddress;
            })
            .ConfigurePrimaryHttpMessageHandler(() => primaryHandler);

        ServiceProvider provider = services.BuildServiceProvider();
        _providers.Add(provider);

        return provider.GetRequiredService<IHttpClientFactory>().CreateClient(GitLabClientDefaults.HttpClientName);
    }
}