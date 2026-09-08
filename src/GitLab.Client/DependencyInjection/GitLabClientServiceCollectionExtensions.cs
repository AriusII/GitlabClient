using System.Net;

using GitLab.Client.Abstractions;
using GitLab.Client.Controllers;
using GitLab.Client.DependencyInjection;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Infrastructure.RateLimiting;

using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

// Deliberately NOT GitLab.Client.DependencyInjection: every registration extension in the ecosystem
// (AddHttpClient, AddDbContext, AddOpenTelemetry) lives here, so it shows up on `services.` with the
// using a consumer already has. IDE0130 is a :suggestion in .editorconfig, so this does not break the
// warnings-as-errors build.
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>Registers the GitLab REST API client and everything it needs on an <see cref="IServiceCollection" />.</summary>
public static partial class GitLabClientServiceCollectionExtensions
{
    /// <summary>
    ///     Registers <see cref="IGitLabClient" /> and every resource client behind it, backed by a named
    ///     <see cref="HttpClient" /> that is taken from <see cref="IHttpClientFactory" /> per operation.
    /// </summary>
    /// <returns>
    ///     The <see cref="IHttpClientBuilder" /> for the underlying client, so callers can layer their own
    ///     handlers or resilience policies onto it.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when <c>AddGitLabClient</c> has already been called on this collection. Every resource
    ///     registration uses <c>TryAdd</c>, but options and HTTP-client configuration delegates accumulate, so a
    ///     second call would otherwise become a silent last-writer-wins across two different configurations.
    /// </exception>
    public static IHttpClientBuilder AddGitLabClient(this IServiceCollection services,
        Action<GitLabClientOptions> configureOptions)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureOptions);

        ThrowIfAlreadyRegistered(services);

        services.AddOptions<GitLabClientOptions>()
            .Configure(configureOptions)
            // Surfaces a missing or malformed token at host startup rather than on the first API call. Only a
            // host runs the IStartupValidator this registers; GitLabApiConnection's constructor also touches
            // IOptionsMonitor.CurrentValue, which covers a bare BuildServiceProvider() with no host.
            .ValidateOnStart();

        services.TryAddEnumerable(ServiceDescriptor
            .Singleton<IValidateOptions<GitLabClientOptions>, GitLabClientOptionsValidator>());

        // One tracker instance, two views onto it: consumers only read, the pipeline only writes.
        services.TryAddSingleton<GitLabRateLimitTracker>();
        services.TryAddSingleton<IGitLabRateLimitTracker>(static provider =>
            provider.GetRequiredService<GitLabRateLimitTracker>());
        services.TryAddSingleton<IGitLabRateLimitWriter>(static provider =>
            provider.GetRequiredService<GitLabRateLimitTracker>());

        services.TryAddTransient<GitLabRetryHandler>();
        services.TryAddTransient<GitLabAuthenticationHandler>();
        services.TryAddTransient<GitLabRateLimitHandler>();

        services.TryAddSingleton<IGitLabApiConnection, GitLabApiConnection>();

        AddResourceClients(services);

        services.TryAddSingleton<IGitLabClient, GitLabClient>();

        return services
            // A NAMED client, not a typed one. IHttpClientFactory registers typed clients as transient, and
            // every service above is a singleton, so a typed client would be captured by the singleton graph
            // for the life of the process - pinning one handler chain, one DNS answer and one snapshot of the
            // options. GitLabApiConnection asks the factory for this client per operation instead, which
            // restores handler rotation and makes BaseAddress/Timeout/UserAgent honour options reloads.
            .AddHttpClient(GitLabClientDefaults.HttpClientName, static (provider, client) =>
            {
                GitLabClientOptions options =
                    provider.GetRequiredService<IOptionsMonitor<GitLabClientOptions>>().CurrentValue;
                client.BaseAddress = options.BaseAddress;
                client.Timeout = options.Timeout;

                // HTTP/2 where the server offers it over ALPN, falling back transparently to 1.1 on older
                // self-managed instances.
                client.DefaultRequestVersion = HttpVersion.Version20;
                client.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrLower;

                client.DefaultRequestHeaders.UserAgent.ParseAdd(options.UserAgent);
                client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
            })
            .UseSocketsHttpHandler(static (handler, _) =>
            {
                // SocketsHttpHandler defaults to DecompressionMethods.None, so without this every GitLab list
                // page crosses the wire as uncompressed JSON. GitLab payloads are verbose, highly repetitive
                // JSON and compress roughly 5-10x: this is the largest measurable win in the transport layer
                // and it costs one line.
                handler.AutomaticDecompression = DecompressionMethods.All;

                // Redirects are followed by the PRIMARY handler, below every DelegatingHandler, so
                // GitLabAuthenticationHandler never sees the redirected request and cannot withhold the
                // credential from it. Only Authorization is cleared automatically across a redirect -
                // PRIVATE-TOKEN and JOB-TOKEN are custom headers and would be replayed verbatim to whatever
                // host the 3xx names. The GitLab JSON endpoints this library wraps do not redirect, so
                // surfacing a 3xx as a GitLabApiException is both safer and more informative.
                handler.AllowAutoRedirect = false;
            })
            // GitLabRetryHandler must be added FIRST so it ends up OUTERMOST in the pipeline (registration
            // order = wrapping order: the first handler added wraps every handler added after it, down to the
            // primary handler). That placement is what makes a retry re-run the rest of the pipeline: when
            // this handler resends a request, that call goes back down through base.SendAsync, so it passes
            // through GitLabAuthenticationHandler again (re-stamping the token onto the freshly-built retry
            // request) and through GitLabRateLimitHandler again (recording that attempt's own RateLimit-*
            // response headers). Registering it after either of those would let every retry after the first
            // one skip authentication and rate-limit bookkeeping.
            .AddHttpMessageHandler<GitLabRetryHandler>()
            .AddHttpMessageHandler<GitLabAuthenticationHandler>()
            .AddHttpMessageHandler<GitLabRateLimitHandler>();
    }

    private static void ThrowIfAlreadyRegistered(IServiceCollection services)
    {
        foreach (ServiceDescriptor descriptor in services)
        {
            if (descriptor.ServiceType == typeof(IGitLabClient))
            {
                throw new InvalidOperationException(
                    "AddGitLabClient has already been called on this IServiceCollection. Call it once, and use " +
                    "services.Configure<GitLabClientOptions>(...) to layer additional configuration on top.");
            }
        }
    }

    /// <summary>
    ///     Registers the Repository -&gt; Service -&gt; Controller triple for every resource whose Repository
    ///     interface carries <c>[GenerateClientLayers]</c>. Implemented by <c>GitLabClientWiringGenerator</c>
    ///     as explicit <c>TryAddSingleton</c> calls with both type arguments closed at compile time; see
    ///     <c>obj/Generated</c> for the emitted body.
    ///     <para>
    ///         The <c>private</c> modifier is load-bearing. An extended partial method that carries an
    ///         accessibility modifier MUST have an implementing part, so a generator that fails to run is a
    ///         CS8795 build error rather than a silently empty method and an unresolvable service at runtime.
    ///     </para>
    /// </summary>
    private static partial void AddResourceClients(IServiceCollection services);
}