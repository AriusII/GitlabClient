using System.Net;
using System.Text;

using GitLab.Client.Abstractions;
using GitLab.Client.Endpoints;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Endpoints;

/// <summary>
///     Coverage for the group-scoped Debian APT metadata endpoints that make up the
///     "Packages: Debian" tag's slice A: the unsigned Release file, its detached signature, the
///     binary package indexes (plain and by-hash), and the installer (udeb) binary package index.
///     <see cref="PackagesDebianClient" /> already implemented all six of these routes (see
///     <c>GetInReleaseForGroupAsync</c>, covered by <see cref="PackagesDebianEndpointTests" />)
///     before this slice began; this file closes the remaining coverage gap rather than pinning
///     down brand-new behavior.
/// </summary>
public sealed class PackagesDebianRepositoryATests
{
    private static readonly Uri BaseAddress = new("https://gitlab.example/api/v4/");

    [Fact]
    public async Task GetReleaseForGroupAsync_BuildsTheDashPrefixedPackagesRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("Origin: Grep\n", Encoding.UTF8, "text/plain")
        });
        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        PackagesDebianClient repository = new(connection);

        GitLabFileResponse response =
            await repository.GetReleaseForGroupAsync(42, "sid", TestContext.Current.CancellationToken);

        await using (response.ConfigureAwait(true))
        {
            Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
            Assert.Equal("https://gitlab.example/api/v4/groups/42/-/packages/debian/dists/sid/Release",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
            Assert.Equal("text/plain", response.ContentType);
        }
    }

    [Fact]
    public async Task GetReleaseSignatureForGroupAsync_AppendsTheGpgLiteralSegment()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(string.Empty, Encoding.UTF8, "application/pgp-signature")
        });
        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        PackagesDebianClient repository = new(connection);

        GitLabFileResponse response =
            await repository.GetReleaseSignatureForGroupAsync(42, "sid", TestContext.Current.CancellationToken);

        await using (response.ConfigureAwait(true))
        {
            Assert.Equal("https://gitlab.example/api/v4/groups/42/-/packages/debian/dists/sid/Release.gpg",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
        }
    }

    [Fact]
    public async Task GetBinaryPackagesIndexForGroupAsync_FoldsBinaryDashArchitectureIntoOneSegment()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(string.Empty, Encoding.UTF8, "text/plain")
        });
        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        PackagesDebianClient repository = new(connection);

        GitLabFileResponse response = await repository.GetBinaryPackagesIndexForGroupAsync(42, "sid", "main",
            "amd64", TestContext.Current.CancellationToken);

        await using (response.ConfigureAwait(true))
        {
            Assert.Equal(
                "https://gitlab.example/api/v4/groups/42/-/packages/debian/dists/sid/main/binary-amd64/Packages",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
        }
    }

    [Fact]
    public async Task GetBinaryPackagesIndexByHashForGroupAsync_AppendsByHashSha256Segments()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(string.Empty, Encoding.UTF8, "text/plain")
        });
        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        PackagesDebianClient repository = new(connection);

        const string FileSha256 = "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855";

        GitLabFileResponse response = await repository.GetBinaryPackagesIndexByHashForGroupAsync(42, "sid", "main",
            "amd64", FileSha256, TestContext.Current.CancellationToken);

        await using (response.ConfigureAwait(true))
        {
            Assert.Equal(
                "https://gitlab.example/api/v4/groups/42/-/packages/debian/dists/sid/main/binary-amd64/by-hash/"
                + "SHA256/" + FileSha256,
                handler.LastRequest?.RequestUri?.AbsoluteUri);
        }
    }

    [Fact]
    public async Task GetInstallerBinaryPackagesIndexForGroupAsync_InsertsTheDebianInstallerLiteralSegment()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(string.Empty, Encoding.UTF8, "text/plain")
        });
        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        PackagesDebianClient repository = new(connection);

        GitLabFileResponse response = await repository.GetInstallerBinaryPackagesIndexForGroupAsync(42, "sid",
            "main", "amd64", TestContext.Current.CancellationToken);

        await using (response.ConfigureAwait(true))
        {
            Assert.Equal(
                "https://gitlab.example/api/v4/groups/42/-/packages/debian/dists/sid/main/debian-installer/"
                + "binary-amd64/Packages",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
        }
    }
}