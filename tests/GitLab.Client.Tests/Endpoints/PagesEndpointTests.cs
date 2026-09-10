using System.Net;
using System.Text;

using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Domain;
using GitLab.Client.Endpoints;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Endpoints;

public sealed class PagesEndpointTests
{
    /// <summary>
    ///     A custom domain is caller-supplied free text, and its dots must reach GitLab intact rather than
    ///     being mistaken for path structure.
    /// </summary>
    private const string CustomDomain = "pages.example.com";

    private const string DomainJson = """
                                      {
                                        "domain": "pages.example.com",
                                        "url": "https://pages.example.com",
                                        "verified": true,
                                        "verification_code": "1234567890abcdef",
                                        "enabled_until": "2026-04-12T14:32:00.000Z",
                                        "auto_ssl_enabled": false,
                                        "certificate": {
                                          "subject": "/CN=pages.example.com",
                                          "expired": false,
                                          "certificate": "-----BEGIN CERTIFICATE-----\nMIIFsA...\n-----END CERTIFICATE-----",
                                          "certificate_text": "Certificate:\n    Data:\n"
                                        }
                                      }
                                      """;

    private const string DomainSummaryJson = """
                                             {
                                               "domain": "pages.example.com",
                                               "url": "https://pages.example.com",
                                               "project_id": 1337,
                                               "verified": true,
                                               "verification_code": "1234567890abcdef",
                                               "enabled_until": "2026-04-12T14:32:00.000Z",
                                               "auto_ssl_enabled": false,
                                               "certificate_expiration": {
                                                 "expired": false,
                                                 "expiration": "2026-04-12T14:32:00.000Z"
                                               }
                                             }
                                             """;

    private const string SettingsJson = """
                                        {
                                          "url": "https://group.example.io/project",
                                          "is_unique_domain_enabled": true,
                                          "force_https": true,
                                          "primary_domain": "pages.example.com",
                                          "deployments": [
                                            {
                                              "created_at": "2026-01-05T18:04:04.577Z",
                                              "url": "https://group.example.io/project",
                                              "path_prefix": "",
                                              "root_directory": null
                                            },
                                            {
                                              "created_at": "2026-01-06T09:11:00.000Z",
                                              "url": "https://group.example.io/project/mr-42",
                                              "path_prefix": "mr-42",
                                              "root_directory": "public"
                                            }
                                          ]
                                        }
                                        """;

    private static readonly Uri BaseAddress = new("https://gitlab.example/api/v4/");

    [Fact]
    public async Task GetSettingsAsync_BuildsThePagesRoute_AndDeserializesEveryDeployment()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(SettingsJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        PagesClient repository = new(connection);

        GitLabPagesSettings settings = await repository.GetSettingsAsync(7, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/7/pages",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        Assert.Equal("https://group.example.io/project", settings.Url?.AbsoluteUri);
        Assert.True(settings.IsUniqueDomainEnabled);
        Assert.True(settings.ForceHttps);
        Assert.Equal("pages.example.com", settings.PrimaryDomain);

        Assert.NotNull(settings.Deployments);
        Assert.Equal(2, settings.Deployments!.Count);
        Assert.Equal(string.Empty, settings.Deployments[0].PathPrefix);
        Assert.Null(settings.Deployments[0].RootDirectory);
        Assert.Equal("mr-42", settings.Deployments[1].PathPrefix);
        Assert.Equal("public", settings.Deployments[1].RootDirectory);
        Assert.Equal("https://group.example.io/project/mr-42", settings.Deployments[1].Url?.AbsoluteUri);
    }

    [Fact]
    public async Task GetSettingsAsync_EncodesANamespacedProjectPath()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(SettingsJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        PagesClient repository = new(connection);

        await repository.GetSettingsAsync(ProjectId.FromPath("group/subgroup/project"),
            TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/group%2Fsubgroup%2Fproject/pages",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task UpdateSettingsAsync_PatchesThePagesRoute_AndOmitsUnsetMembers()
    {
        string? sentBody = null;

        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(SettingsJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        PagesClient repository = new(connection);

        GitLabPagesSettings settings = await repository.UpdateSettingsAsync(
            7,
            new UpdatePagesSettingsRequest { PagesHttpsOnly = true, PagesPrimaryDomain = CustomDomain },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Patch, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/7/pages",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"pages_https_only":true,"pages_primary_domain":"pages.example.com"}""", sentBody);
        Assert.True(settings.ForceHttps);
    }

    [Fact]
    public async Task UnpublishAsync_DeletesThePagesRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        PagesClient repository = new(connection);

        await repository.UnpublishAsync(7, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/7/pages",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    /// <summary>
    ///     <c>GET /projects/:id/pages_access</c> answers <c>200</c> with no body at all - the status code is
    ///     the whole answer - so it must not go through the deserializing GET, which reports an empty body
    ///     as a failure.
    /// </summary>
    [Fact]
    public async Task CheckAccessAsync_GetsThePagesAccessRoute_AndAcceptsAnEmptyBody()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK));

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        PagesClient repository = new(connection);

        await repository.CheckAccessAsync(7, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/7/pages_access",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task CheckAccessAsync_MapsA403ToTheTypedForbiddenException()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Forbidden)
        {
            Content = new StringContent("""{"message":"403 Forbidden"}""", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        PagesClient repository = new(connection);

        await Assert.ThrowsAsync<GitLabForbiddenException>(() =>
            repository.CheckAccessAsync(7, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task ListAllDomainsAsync_BuildsTheInstanceRoute_AndDeserializesTheCertificateExpiry()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent($"[{DomainSummaryJson}]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        PagesClient repository = new(connection);

        List<GitLabPagesDomainSummary> domains = [];
        await foreach (GitLabPagesDomainSummary summary in
                       repository.ListAllDomainsAsync(cancellationToken: TestContext.Current.CancellationToken))
        {
            domains.Add(summary);
        }

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/pages/domains",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabPagesDomainSummary only = Assert.Single(domains);
        Assert.Equal(CustomDomain, only.Domain);
        Assert.Equal(1337, only.ProjectId);
        Assert.True(only.Verified);
        Assert.False(only.AutoSslEnabled);
        Assert.Equal("1234567890abcdef", only.VerificationCode);
        Assert.NotNull(only.CertificateExpiration);
        Assert.False(only.CertificateExpiration!.Expired);
        Assert.Equal(2026, only.CertificateExpiration.Expiration?.Year);
    }

    [Fact]
    public async Task ListAllDomainsAsync_SendsTheDomainFilterAsAQueryParameter()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        PagesClient repository = new(connection);

        await foreach (GitLabPagesDomainSummary _ in
                       repository.ListAllDomainsAsync(CustomDomain, TestContext.Current.CancellationToken))
        {
            // Draining the sequence is what issues the request.
        }

        Assert.Equal("https://gitlab.example/api/v4/pages/domains?domain=pages.example.com",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task ListDomainsAsync_BuildsTheProjectDomainsRoute_AndDeserializesTheCertificate()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent($"[{DomainJson}]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        PagesClient repository = new(connection);

        List<GitLabPagesDomain> domains = [];
        await foreach (GitLabPagesDomain domain in
                       repository.ListDomainsAsync(7, TestContext.Current.CancellationToken))
        {
            domains.Add(domain);
        }

        Assert.Equal("https://gitlab.example/api/v4/projects/7/pages/domains",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabPagesDomain only = Assert.Single(domains);
        Assert.Equal(CustomDomain, only.Domain);
        Assert.Equal("https://pages.example.com/", only.Url?.AbsoluteUri);
        Assert.True(only.Verified);
        Assert.Equal(2026, only.EnabledUntil?.Year);
        Assert.NotNull(only.Certificate);
        Assert.Equal("/CN=pages.example.com", only.Certificate!.Subject);
        Assert.False(only.Certificate.Expired);
        Assert.StartsWith("-----BEGIN CERTIFICATE-----", only.Certificate.Certificate, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ListDomainsAsync_EncodesANamespacedProjectPath()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        PagesClient repository = new(connection);

        await foreach (GitLabPagesDomain _ in repository.ListDomainsAsync(
                           ProjectId.FromPath("group/subgroup/project"), TestContext.Current.CancellationToken))
        {
            // Draining the sequence is what issues the request.
        }

        Assert.Equal("https://gitlab.example/api/v4/projects/group%2Fsubgroup%2Fproject/pages/domains",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    /// <summary>
    ///     The two encoding rules that share one route: the custom domain's dots must survive verbatim,
    ///     while the namespaced project path's slashes must be percent-encoded.
    /// </summary>
    [Fact]
    public async Task GetDomainAsync_RoundTripsTheDottedDomain_AndEncodesTheNamespacedProjectPath()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(DomainJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        PagesClient repository = new(connection);

        GitLabPagesDomain domain = await repository.GetDomainAsync(ProjectId.FromPath("group/subgroup/project"),
            CustomDomain, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/projects/group%2Fsubgroup%2Fproject/pages/domains/pages.example.com",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(CustomDomain, domain.Domain);
    }

    [Fact]
    public async Task GetDomainAsync_EncodesAWildcardDomain()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(DomainJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        PagesClient repository = new(connection);

        await repository.GetDomainAsync(7, "*.example.com", TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/7/pages/domains/%2A.example.com",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task CreateDomainAsync_PostsTheCertificateAndKey_AndReadsBackOnlyThePublicHalf()
    {
        string? sentBody = null;

        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();

            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(DomainJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        PagesClient repository = new(connection);

        GitLabPagesDomain created = await repository.CreateDomainAsync(
            7,
            new CreatePagesDomainRequest
            {
                Domain = CustomDomain,
                Certificate = "-----BEGIN CERTIFICATE-----",
                Key = "-----BEGIN PRIVATE KEY-----"
            },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/7/pages/domains",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(
            """{"domain":"pages.example.com","certificate":"-----BEGIN CERTIFICATE-----","key":"-----BEGIN PRIVATE KEY-----"}""",
            sentBody);

        // The private key travels outbound only: nothing on the response surface can carry it back.
        Assert.NotNull(created.Certificate);
        Assert.DoesNotContain("PRIVATE KEY", created.Certificate!.Certificate, StringComparison.Ordinal);
    }

    [Fact]
    public async Task CreateDomainAsync_WithAutoSsl_SendsNoCertificateMaterial()
    {
        string? sentBody = null;

        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();

            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(DomainJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        PagesClient repository = new(connection);

        await repository.CreateDomainAsync(
            7,
            new CreatePagesDomainRequest { Domain = CustomDomain, AutoSslEnabled = true },
            TestContext.Current.CancellationToken);

        Assert.Equal("""{"domain":"pages.example.com","auto_ssl_enabled":true}""", sentBody);
    }

    [Fact]
    public async Task UpdateDomainAsync_PutsToTheDomainRoute_AndOmitsUnsetMembers()
    {
        string? sentBody = null;

        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(DomainJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        PagesClient repository = new(connection);

        await repository.UpdateDomainAsync(
            7,
            CustomDomain,
            new UpdatePagesDomainRequest { AutoSslEnabled = true },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/7/pages/domains/pages.example.com",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"auto_ssl_enabled":true}""", sentBody);
    }

    [Fact]
    public async Task DeleteDomainAsync_DeletesTheDomainRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        PagesClient repository = new(connection);

        await repository.DeleteDomainAsync(7, CustomDomain, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/7/pages/domains/pages.example.com",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    /// <summary>
    ///     Verification is a body-less PUT that answers with the domain, so it must send no payload and
    ///     still deserialize the response.
    /// </summary>
    [Fact]
    public async Task VerifyDomainAsync_PutsToTheVerifyRoute_WithNoBody()
    {
        bool hadContent = true;

        using StubHttpMessageHandler handler = new(request =>
        {
            hadContent = request.Content is not null;

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(DomainJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        PagesClient repository = new(connection);

        GitLabPagesDomain verified =
            await repository.VerifyDomainAsync(7, CustomDomain, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/7/pages/domains/pages.example.com/verify",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.False(hadContent);
        Assert.True(verified.Verified);
    }

    [Fact]
    public async Task GetDomainAsync_MapsA404ToTheTypedNotFoundException()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent("""{"message":"404 Domain Not Found"}""", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        PagesClient repository = new(connection);

        GitLabNotFoundException exception = await Assert.ThrowsAsync<GitLabNotFoundException>(() =>
            repository.GetDomainAsync(7, CustomDomain, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
    }
}