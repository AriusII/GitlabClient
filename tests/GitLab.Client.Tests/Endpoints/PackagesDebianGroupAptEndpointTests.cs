using System.Net;
using System.Text;

using GitLab.Client.Abstractions;
using GitLab.Client.Endpoints;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Endpoints;

/// <summary>
///     Supplementary coverage for <see cref="PackagesDebianClient" />'s group-scoped APT metadata
///     routes that neither <see cref="PackagesDebianEndpointTests" /> nor
///     <see cref="PackagesDebianRepositoryBTests" /> already exercise by name - the installer
///     (udeb) binary index by hash, the source index, and the source index by hash, all under the
///     dash-prefixed <c>groups/:id/-/packages/debian</c> tree. The equivalent project-scoped routes and
///     the group-scoped route-building helpers themselves (<c>InstallerBinaryRoute</c>,
///     <c>SourceRoute</c>, <c>ByHashRoute</c>) are already proven by the sibling test files; these tests
///     close the remaining "Packages: Debian" scope gaps for the group tree rather than duplicate what
///     is already proven.
/// </summary>
public sealed class PackagesDebianGroupAptEndpointTests
{
    private const string Sha256Hash = "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855";
    private static readonly Uri BaseAddress = new("https://gitlab.example/api/v4/");

    [Fact]
    public async Task GetInstallerBinaryPackagesIndexByHashForGroupAsync_AppendsTheSha256Segment()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(string.Empty, Encoding.UTF8, "text/plain")
        });
        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        PackagesDebianClient repository = new(connection);

        GitLabFileResponse response = await repository.GetInstallerBinaryPackagesIndexByHashForGroupAsync(42,
            "sid", "main", "amd64", Sha256Hash, TestContext.Current.CancellationToken);

        await using (response.ConfigureAwait(true))
        {
            Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
            Assert.Equal(
                "https://gitlab.example/api/v4/groups/42/-/packages/debian/dists/sid/main/debian-installer/"
                + "binary-amd64/by-hash/SHA256/" + Sha256Hash,
                handler.LastRequest?.RequestUri?.AbsoluteUri);
        }
    }

    [Fact]
    public async Task GetSourcePackagesIndexForGroupAsync_BuildsTheSourceSourcesRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(string.Empty, Encoding.UTF8, "text/plain")
        });
        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        PackagesDebianClient repository = new(connection);

        GitLabFileResponse response = await repository.GetSourcePackagesIndexForGroupAsync(42, "sid", "main",
            TestContext.Current.CancellationToken);

        await using (response.ConfigureAwait(true))
        {
            Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
            Assert.Equal(
                "https://gitlab.example/api/v4/groups/42/-/packages/debian/dists/sid/main/source/Sources",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
        }
    }

    [Fact]
    public async Task GetSourcePackagesIndexByHashForGroupAsync_AppendsTheSha256Segment()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(string.Empty, Encoding.UTF8, "text/plain")
        });
        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        PackagesDebianClient repository = new(connection);

        GitLabFileResponse response = await repository.GetSourcePackagesIndexByHashForGroupAsync(42, "sid",
            "main", Sha256Hash, TestContext.Current.CancellationToken);

        await using (response.ConfigureAwait(true))
        {
            Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
            Assert.Equal(
                "https://gitlab.example/api/v4/groups/42/-/packages/debian/dists/sid/main/source/by-hash/SHA256/"
                + Sha256Hash,
                handler.LastRequest?.RequestUri?.AbsoluteUri);
        }
    }
}