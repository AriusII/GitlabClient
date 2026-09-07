using System.Net;
using System.Net.Http.Headers;
using System.Text;

using GitLab.Client.Abstractions;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Repositories;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Repositories;

public sealed class PackagesComposerRepositoryTests
{
    private static readonly Uri BaseAddress = new("https://gitlab.example/api/v4/");

    [Fact]
    public async Task GetRepositoryUrlTemplatesForGroupAsync_UsesTheSingularGroupRoot()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("""{"packages":{"type":"vcs","url":"x"}}""", Encoding.UTF8,
                "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        PackagesComposerRepository repository = new(connection);

        // Composer's group-level routes live under "/group/:id", not "/groups/:id" like every other
        // group resource in this library - this is GitLab's own route, confirmed against the spec.
        using GitLabFileResponse response =
            await repository.GetRepositoryUrlTemplatesForGroupAsync(9, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/group/9/-/packages/composer/packages",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ListAllForGroupAsync_EscapesTheShaSegment()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{}", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        PackagesComposerRepository repository = new(connection);

        using GitLabFileResponse response = await repository.ListAllForGroupAsync(9,
            "673594f85a55fe3c0eb45df7bd2fa9d95a1601ab", TestContext.Current.CancellationToken);

        Assert.Equal(
            "https://gitlab.example/api/v4/group/9/-/packages/composer/p/673594f85a55fe3c0eb45df7bd2fa9d95a1601ab",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetPackageVersionsV2ForGroupAsync_BuildsTheP2Route()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{}", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        PackagesComposerRepository repository = new(connection);

        using GitLabFileResponse response = await repository.GetPackageVersionsV2ForGroupAsync(9,
            "my-composer-package", TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/group/9/-/packages/composer/p2/my-composer-package",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetPackageVersionsForGroupAsync_EscapesASlashBearingPackageName()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{}", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        PackagesComposerRepository repository = new(connection);

        using GitLabFileResponse response =
            await repository.GetPackageVersionsForGroupAsync(9, "vendor/my-package",
                TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/group/9/-/packages/composer/vendor%2Fmy-package",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CreateAsync_PostsTheBranchAndTag()
    {
        string? sentBody = null;

        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created);
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        PackagesComposerRepository repository = new(connection);

        await repository.CreateAsync(7, new ComposerPackageCreateRequest { Tag = "v1.0.0" },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/7/packages/composer",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.NotNull(sentBody);
        Assert.Contains("\"tag\":\"v1.0.0\"", sentBody, StringComparison.Ordinal);
        Assert.DoesNotContain("branch", sentBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task CreateAsync_DefaultsToAnEmptyBody_WhenNoRequestIsGiven()
    {
        string? sentBody = null;

        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created);
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        PackagesComposerRepository repository = new(connection);

        await repository.CreateAsync(7, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("{}", sentBody);
    }

    [Fact]
    public async Task DownloadArchiveAsync_BuildsTheArchiveRoute_WithTheShaQueryParameter()
    {
        using StubHttpMessageHandler handler = new(_ =>
        {
            HttpResponseMessage response = new(HttpStatusCode.OK) { Content = new ByteArrayContent([0x50, 0x4B]) };
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/zip");
            return response;
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        PackagesComposerRepository repository = new(connection);

        using GitLabFileResponse response = await repository.DownloadArchiveAsync(7, "my-composer-package",
            "673594f85a55fe3c0eb45df7bd2fa9d95a1601ab", TestContext.Current.CancellationToken);

        Assert.Equal(
            "https://gitlab.example/api/v4/projects/7/packages/composer/archives/my-composer-package"
            + "?sha=673594f85a55fe3c0eb45df7bd2fa9d95a1601ab",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("application/zip", response.ContentType);
    }
}