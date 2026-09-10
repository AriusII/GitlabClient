using System.Net;
using System.Text;

using GitLab.Client.Abstractions;
using GitLab.Client.Endpoints;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Endpoints;

/// <summary>
///     Supplementary coverage for <see cref="PackagesDebianClient" />'s project-scoped APT metadata
///     routes that <see cref="PackagesDebianEndpointTests" /> does not already exercise by name -
///     <c>Release</c>, <c>Release.gpg</c>, and the <c>debian-installer</c>/<c>source</c> by-hash lookups.
///     The route-building helpers themselves (<c>BinaryRoute</c>, <c>ByHashRoute</c>, etc.) are already
///     proven by the sibling tests for <c>InRelease</c> and the binary-index-by-hash routes; these tests
///     close the remaining gaps in the "Packages: Debian" scope's operation list rather than duplicate
///     what is already proven.
/// </summary>
public sealed class PackagesDebianRepositoryBTests
{
    private const string Sha256Hash = "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855";
    private static readonly Uri BaseAddress = new("https://gitlab.example/api/v4/");

    [Fact]
    public async Task GetReleaseForProjectAsync_StreamsTheUnsignedReleaseFile()
    {
        const string ReleaseBody = "Origin: Grep\nCodename: sid\n";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(ReleaseBody, Encoding.UTF8, "text/plain")
        });
        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        PackagesDebianClient repository = new(connection);

        GitLabFileResponse response =
            await repository.GetReleaseForProjectAsync(7, "sid", TestContext.Current.CancellationToken);

        await using (response.ConfigureAwait(true))
        {
            Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
            Assert.Equal("https://gitlab.example/api/v4/projects/7/packages/debian/dists/sid/Release",
                handler.LastRequest?.RequestUri?.AbsoluteUri);

            using StreamReader reader = new(response.Content);
            Assert.Equal(ReleaseBody, await reader.ReadToEndAsync(TestContext.Current.CancellationToken));
        }
    }

    [Fact]
    public async Task GetReleaseSignatureForProjectAsync_BuildsTheReleaseGpgRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("signature-bytes", Encoding.UTF8, "application/pgp-signature")
        });
        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        PackagesDebianClient repository = new(connection);

        GitLabFileResponse response = await repository.GetReleaseSignatureForProjectAsync(7, "sid",
            TestContext.Current.CancellationToken);

        await using (response.ConfigureAwait(true))
        {
            Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
            Assert.Equal("https://gitlab.example/api/v4/projects/7/packages/debian/dists/sid/Release.gpg",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
            Assert.Equal("application/pgp-signature", response.ContentType);
        }
    }

    [Fact]
    public async Task GetInstallerBinaryPackagesIndexByHashForProjectAsync_AppendsTheSha256Segment()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(string.Empty, Encoding.UTF8, "text/plain")
        });
        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        PackagesDebianClient repository = new(connection);

        GitLabFileResponse response = await repository.GetInstallerBinaryPackagesIndexByHashForProjectAsync(7,
            "sid", "main", "amd64", Sha256Hash, TestContext.Current.CancellationToken);

        await using (response.ConfigureAwait(true))
        {
            Assert.Equal(
                "https://gitlab.example/api/v4/projects/7/packages/debian/dists/sid/main/debian-installer/"
                + "binary-amd64/by-hash/SHA256/" + Sha256Hash,
                handler.LastRequest?.RequestUri?.AbsoluteUri);
        }
    }

    [Fact]
    public async Task GetSourcePackagesIndexByHashForProjectAsync_AppendsTheSha256Segment()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(string.Empty, Encoding.UTF8, "text/plain")
        });
        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        PackagesDebianClient repository = new(connection);

        GitLabFileResponse response = await repository.GetSourcePackagesIndexByHashForProjectAsync(7, "sid",
            "main", Sha256Hash, TestContext.Current.CancellationToken);

        await using (response.ConfigureAwait(true))
        {
            Assert.Equal(
                "https://gitlab.example/api/v4/projects/7/packages/debian/dists/sid/main/source/by-hash/SHA256/"
                + Sha256Hash,
                handler.LastRequest?.RequestUri?.AbsoluteUri);
        }
    }
}