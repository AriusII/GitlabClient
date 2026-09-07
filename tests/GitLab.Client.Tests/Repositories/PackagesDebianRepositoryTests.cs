using System.Net;
using System.Text;

using GitLab.Client.Abstractions;
using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Repositories;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Repositories;

public sealed class PackagesDebianRepositoryTests
{
    /// <summary>A codename containing a dot - the character the Debian protocol tests must prove round-trips.</summary>
    private const string DottedCodename = "bullseye-security.1";

    private const string DistributionJson = """
                                            {
                                              "id": 1,
                                              "codename": "sid",
                                              "suite": "unstable",
                                              "origin": "Grep",
                                              "label": "grep.be",
                                              "version": "12",
                                              "description": "My description",
                                              "valid_time_duration_seconds": 604800,
                                              "components": ["main"],
                                              "architectures": ["amd64"]
                                            }
                                            """;

    private static readonly Uri BaseAddress = new("https://gitlab.example/api/v4/");

    // ---- Distributions (project scope) ----

    [Fact]
    public async Task ListDistributionsForProjectAsync_BuildsTheRoute_AndDeserializesTheEntry()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent($"[{DistributionJson}]", Encoding.UTF8, "application/json")
        });
        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        PackagesDebianRepository repository = new(connection);

        List<GitLabDebianDistribution> distributions = [];
        await foreach (GitLabDebianDistribution distribution in repository.ListDistributionsForProjectAsync(
                           7,
                           new DebianDistributionListOptions { Suite = "unstable", PerPage = 20 },
                           TestContext.Current.CancellationToken))
        {
            distributions.Add(distribution);
        }

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/projects/7/debian_distributions?per_page=20&suite=unstable",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabDebianDistribution only = Assert.Single(distributions);
        Assert.Equal(1, only.Id);
        Assert.Equal("sid", only.Codename);
        Assert.Equal("grep.be", only.Label);
        Assert.Equal(604800, only.ValidTimeDurationSeconds);
        Assert.Equal(["main"], only.Components);
        Assert.Equal(["amd64"], only.Architectures);
    }

    [Fact]
    public async Task CreateDistributionForProjectAsync_PostsOnlyTheSuppliedFields()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(DistributionJson, Encoding.UTF8, "application/json")
            };
        });
        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        PackagesDebianRepository repository = new(connection);

        GitLabDebianDistribution created = await repository.CreateDistributionForProjectAsync(
            7,
            new CreateDebianDistributionRequest { Codename = "sid", Components = ["main"] },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/7/debian_distributions",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"codename":"sid","components":["main"]}""", sentBody);
        Assert.Equal("sid", created.Codename);
    }

    [Fact]
    public async Task GetDistributionForProjectAsync_RoundTripsADottedCodename()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(DistributionJson, Encoding.UTF8, "application/json")
        });
        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        PackagesDebianRepository repository = new(connection);

        await repository.GetDistributionForProjectAsync(7, DottedCodename, TestContext.Current.CancellationToken);

        // The dot is unreserved and stays literal; the whole name must still land as ONE path segment.
        Assert.Equal(
            "https://gitlab.example/api/v4/projects/7/debian_distributions/bullseye-security.1",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task UpdateDistributionForProjectAsync_PutsTheChangedFields()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(DistributionJson, Encoding.UTF8, "application/json")
            };
        });
        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        PackagesDebianRepository repository = new(connection);

        await repository.UpdateDistributionForProjectAsync(
            7,
            DottedCodename,
            new UpdateDebianDistributionRequest { Description = "Updated" },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/projects/7/debian_distributions/bullseye-security.1",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"description":"Updated"}""", sentBody);
    }

    [Fact]
    public async Task DeleteDistributionForProjectAsync_SendsTheMatchFiltersAsQueryParameters()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Accepted));
        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        PackagesDebianRepository repository = new(connection);

        await repository.DeleteDistributionForProjectAsync(
            7,
            "sid",
            new DeleteDebianDistributionOptions { Suite = "unstable", Components = ["main", "contrib"] },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/projects/7/debian_distributions/sid?suite=unstable&components=main,contrib",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task GetDistributionKeyForProjectAsync_BuildsTheKeyAscRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(DistributionJson, Encoding.UTF8, "application/json")
        });
        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        PackagesDebianRepository repository = new(connection);

        GitLabDebianDistribution key =
            await repository.GetDistributionKeyForProjectAsync(7, "sid", TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/7/debian_distributions/sid/key.asc",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("sid", key.Codename);
    }

    [Fact]
    public async Task GetDistributionForProjectAsync_OnMissingDistribution_ThrowsGitLabNotFoundException()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent("""{"message":"404 Not found"}""", Encoding.UTF8, "application/json")
        });
        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        PackagesDebianRepository repository = new(connection);

        await Assert.ThrowsAsync<GitLabNotFoundException>(() =>
            repository.GetDistributionForProjectAsync(7, "missing", TestContext.Current.CancellationToken));
    }

    // ---- Distributions (group scope) ----

    [Fact]
    public async Task ListDistributionsForGroupAsync_BuildsTheDashPrefixedGroupRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        });
        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        PackagesDebianRepository repository = new(connection);

        await foreach (GitLabDebianDistribution _ in
                       repository.ListDistributionsForGroupAsync(42,
                           cancellationToken: TestContext.Current.CancellationToken))
        {
            Assert.Fail("The stubbed response is an empty page.");
        }

        Assert.Equal("https://gitlab.example/api/v4/groups/42/-/debian_distributions",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task GetDistributionForGroupAsync_EncodesANamespacedGroupPath()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(DistributionJson, Encoding.UTF8, "application/json")
        });
        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        PackagesDebianRepository repository = new(connection);

        await repository.GetDistributionForGroupAsync("parent-group/subgroup", "sid",
            TestContext.Current.CancellationToken);

        Assert.Equal(
            "https://gitlab.example/api/v4/groups/parent-group%2Fsubgroup/-/debian_distributions/sid",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task DeleteDistributionForGroupAsync_SendsDeleteToTheDashPrefixedRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Accepted));
        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        PackagesDebianRepository repository = new(connection);

        await repository.DeleteDistributionForGroupAsync(42, "sid",
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/42/-/debian_distributions/sid",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    // ---- APT metadata tree (project scope) ----

    [Fact]
    public async Task GetInReleaseForProjectAsync_StreamsTheRawReleaseFile_WithoutTouchingTheSerializer()
    {
        const string ReleaseBody = "Origin: Grep\nCodename: sid\n";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(ReleaseBody, Encoding.UTF8, "text/plain")
        });
        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        PackagesDebianRepository repository = new(connection);

        GitLabFileResponse response =
            await repository.GetInReleaseForProjectAsync(7, "sid", TestContext.Current.CancellationToken);

        await using (response.ConfigureAwait(true))
        {
            Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
            Assert.Equal("https://gitlab.example/api/v4/projects/7/packages/debian/dists/sid/InRelease",
                handler.LastRequest?.RequestUri?.AbsoluteUri);

            using StreamReader reader = new(response.Content);
            Assert.Equal(ReleaseBody, await reader.ReadToEndAsync(TestContext.Current.CancellationToken));
        }
    }

    [Fact]
    public async Task GetBinaryPackagesIndexForProjectAsync_FoldsTheArchitectureIntoOneBinaryDashSegment()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(string.Empty, Encoding.UTF8, "text/plain")
        });
        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        PackagesDebianRepository repository = new(connection);

        GitLabFileResponse response = await repository.GetBinaryPackagesIndexForProjectAsync(7, "sid", "main",
            "amd64", TestContext.Current.CancellationToken);

        await using (response.ConfigureAwait(true))
        {
            // "binary-{architecture}" is ONE path segment in GitLab's route template, not "binary-" then
            // "amd64" as two - a naive Literal("binary-").Escaped(architecture) call would double it up.
            Assert.Equal(
                "https://gitlab.example/api/v4/projects/7/packages/debian/dists/sid/main/binary-amd64/Packages",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
        }
    }

    [Fact]
    public async Task GetBinaryPackagesIndexByHashForProjectAsync_AppendsTheSha256Segment()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(string.Empty, Encoding.UTF8, "text/plain")
        });
        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        PackagesDebianRepository repository = new(connection);

        const string Hash = "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855";

        GitLabFileResponse response = await repository.GetBinaryPackagesIndexByHashForProjectAsync(7, "sid",
            "main", "amd64", Hash, TestContext.Current.CancellationToken);

        await using (response.ConfigureAwait(true))
        {
            Assert.Equal(
                "https://gitlab.example/api/v4/projects/7/packages/debian/dists/sid/main/binary-amd64/by-hash/SHA256/"
                + Hash,
                handler.LastRequest?.RequestUri?.AbsoluteUri);
        }
    }

    [Fact]
    public async Task GetInstallerBinaryPackagesIndexForProjectAsync_InsertsTheDebianInstallerSegment()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(string.Empty, Encoding.UTF8, "text/plain")
        });
        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        PackagesDebianRepository repository = new(connection);

        GitLabFileResponse response = await repository.GetInstallerBinaryPackagesIndexForProjectAsync(7, "sid",
            "main", "amd64", TestContext.Current.CancellationToken);

        await using (response.ConfigureAwait(true))
        {
            Assert.Equal(
                "https://gitlab.example/api/v4/projects/7/packages/debian/dists/sid/main/debian-installer/"
                + "binary-amd64/Packages",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
        }
    }

    [Fact]
    public async Task GetSourcePackagesIndexForProjectAsync_BuildsTheSourceSourcesRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(string.Empty, Encoding.UTF8, "text/plain")
        });
        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        PackagesDebianRepository repository = new(connection);

        GitLabFileResponse response = await repository.GetSourcePackagesIndexForProjectAsync(7, "sid", "main",
            TestContext.Current.CancellationToken);

        await using (response.ConfigureAwait(true))
        {
            Assert.Equal(
                "https://gitlab.example/api/v4/projects/7/packages/debian/dists/sid/main/source/Sources",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
        }
    }

    [Fact]
    public async Task DownloadPackageFileForProjectAsync_RoundTripsAMultiDotFileName()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("binary", Encoding.UTF8, "application/octet-stream")
        });
        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        PackagesDebianRepository repository = new(connection);

        const string FileName = "example_1.0.0~alpha2_amd64.deb";

        GitLabFileResponse response = await repository.DownloadPackageFileForProjectAsync(7, "sid", "e",
            "example", "1.0.0~alpha2", FileName, TestContext.Current.CancellationToken);

        await using (response.ConfigureAwait(true))
        {
            Assert.Equal(
                "https://gitlab.example/api/v4/projects/7/packages/debian/pool/sid/e/example/1.0.0~alpha2/"
                + FileName,
                handler.LastRequest?.RequestUri?.AbsoluteUri);
            Assert.Equal("application/octet-stream", response.ContentType);
        }
    }

    // ---- APT metadata tree (group scope) ----

    [Fact]
    public async Task GetInReleaseForGroupAsync_BuildsTheDashPrefixedPackagesRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(string.Empty, Encoding.UTF8, "text/plain")
        });
        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        PackagesDebianRepository repository = new(connection);

        GitLabFileResponse response =
            await repository.GetInReleaseForGroupAsync(42, "sid", TestContext.Current.CancellationToken);

        await using (response.ConfigureAwait(true))
        {
            Assert.Equal("https://gitlab.example/api/v4/groups/42/-/packages/debian/dists/sid/InRelease",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
        }
    }

    [Fact]
    public async Task DownloadPackageFileForGroupAsync_IncludesTheOwningProjectIdSegment()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("binary", Encoding.UTF8, "application/octet-stream")
        });
        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        PackagesDebianRepository repository = new(connection);

        GitLabFileResponse response = await repository.DownloadPackageFileForGroupAsync(42, "sid", 7, "e",
            "example", "1.0.0", "example_1.0.0_amd64.deb", TestContext.Current.CancellationToken);

        await using (response.ConfigureAwait(true))
        {
            Assert.Equal(
                "https://gitlab.example/api/v4/groups/42/-/packages/debian/pool/sid/7/e/example/1.0.0/"
                + "example_1.0.0_amd64.deb",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
        }
    }

    // ---- Package upload (project scope) ----

    [Fact]
    public async Task AuthorizePackageUploadAsync_PutsTheJsonBody_AndEscapesTheFileName()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK);
        });
        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        PackagesDebianRepository repository = new(connection);

        const string FileName = "example_1.0.0~alpha2_amd64.deb";

        await repository.AuthorizePackageUploadAsync(
            7,
            FileName,
            new AuthorizeDebianPackageUploadRequest { Component = "main", Distribution = "sid" },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/projects/7/packages/debian/" + FileName + "/authorize",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"component":"main","distribution":"sid"}""", sentBody);
    }
}