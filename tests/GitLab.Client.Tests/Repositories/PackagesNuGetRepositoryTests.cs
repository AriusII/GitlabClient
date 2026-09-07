using System.Net;
using System.Text;

using GitLab.Client.Abstractions;
using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Repositories;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Repositories;

public sealed class PackagesNuGetRepositoryTests
{
    private const string ServiceIndexJson = """
                                            {
                                              "version": "1.3.0.17",
                                              "resources": [
                                                {
                                                  "@id": "https://gitlab.example.com/api/v4/projects/1/packages/nuget/query",
                                                  "@type": "SearchQueryService",
                                                  "comment": "Filter and search for packages by keyword."
                                                }
                                              ]
                                            }
                                            """;

    private const string PackageMetadataJson = """
                                                 {
                                                   "@id": "https://gitlab.example.com/api/v4/projects/1/packages/nuget/metadata/MyNuGetPkg/1.3.0.17.json",
                                                   "packageContent": "https://gitlab.example.com/api/v4/projects/1/packages/nuget/download/MyNuGetPkg/1.3.0.17/helloworld.1.3.0.17.nupkg",
                                                   "catalogEntry": {
                                                     "@id": "https://gitlab.example.com/api/v4/projects/1/packages/nuget/metadata/MyNuGetPkg/1.3.0.17.json",
                                                     "id": "MyNuGetPkg",
                                                     "version": "1.3.0.17",
                                                     "tags": "tag#1 tag#2",
                                                     "authors": "Authors",
                                                     "description": "Description",
                                                     "published": "2023-05-08T17:23:25Z",
                                                     "dependencyGroups": [
                                                       {
                                                         "targetFramework": "net8.0",
                                                         "dependencies": [
                                                           { "id": "Dependency", "range": "2.0.0" }
                                                         ]
                                                       }
                                                     ]
                                                   }
                                                 }
                                               """;

    private static readonly Uri BaseAddress = new("https://gitlab.example/api/v4/");

    private static readonly byte[] FileBytes = "<xml/>"u8.ToArray();

    private static (HttpClient httpClient, StubHttpMessageHandler handler) CreateClient(
        Func<HttpRequestMessage, HttpResponseMessage> respond)
    {
        StubHttpMessageHandler handler = new(respond);
        HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        return (httpClient, handler);
    }

    // Group scope.

    [Fact]
    public async Task GetServiceIndexForGroupAsync_BuildsTheDashRoute_AndDeserializesResources()
    {
        (HttpClient httpClient, StubHttpMessageHandler handler) = CreateClient(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(ServiceIndexJson, Encoding.UTF8, "application/json")
            });

        GitLabApiConnection connection = new(httpClient);
        PackagesNuGetRepository repository = new(connection);

        GitLabNugetServiceIndex index =
            await repository.GetServiceIndexForGroupAsync(9970, TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/groups/9970/-/packages/nuget/index",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("1.3.0.17", index.Version);
        Assert.NotNull(index.Resources);
        Assert.Single(index.Resources!);
    }

    [Fact]
    public async Task GetPackageMetadataForGroupAsync_EscapesTheNamespacedGroupPath_AndThePackageName()
    {
        const string MetadataJson = """{"count":1,"items":[]}""";

        (HttpClient httpClient, StubHttpMessageHandler handler) = CreateClient(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(MetadataJson, Encoding.UTF8, "application/json")
            });

        GitLabApiConnection connection = new(httpClient);
        PackagesNuGetRepository repository = new(connection);

        GitLabNugetPackagesMetadata metadata = await repository.GetPackageMetadataForGroupAsync(
            "parent-group/subgroup", "My NuGet Pkg", TestContext.Current.CancellationToken);

        Assert.Equal(
            "https://gitlab.example/api/v4/groups/parent-group%2Fsubgroup/-/packages/nuget/metadata/"
            + "My%20NuGet%20Pkg/index",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(1, metadata.Count);
    }

    [Fact]
    public async Task GetPackageVersionMetadataForGroupAsync_BuildsTheRoute_AndDeserializesTheCatalogEntry()
    {
        (HttpClient httpClient, StubHttpMessageHandler handler) = CreateClient(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(PackageMetadataJson, Encoding.UTF8, "application/json")
            });

        GitLabApiConnection connection = new(httpClient);
        PackagesNuGetRepository repository = new(connection);

        GitLabNugetPackageMetadata metadata = await repository.GetPackageVersionMetadataForGroupAsync(9970,
            "MyNuGetPkg", "1.3.0.17", TestContext.Current.CancellationToken);

        Assert.Equal(
            "https://gitlab.example/api/v4/groups/9970/-/packages/nuget/metadata/MyNuGetPkg/1.3.0.17",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        Assert.NotNull(metadata.CatalogEntry);
        Assert.Equal("MyNuGetPkg", metadata.CatalogEntry!.Id);
        Assert.Equal("tag#1 tag#2", metadata.CatalogEntry.Tags);
        Assert.Equal(new DateTimeOffset(2023, 5, 8, 17, 23, 25, TimeSpan.Zero), metadata.CatalogEntry.Published);
        Assert.NotNull(metadata.CatalogEntry.DependencyGroups);
        Assert.Equal("net8.0", metadata.CatalogEntry.DependencyGroups![0].TargetFramework);
        Assert.Equal("Dependency", metadata.CatalogEntry.DependencyGroups[0].Dependencies?[0].Id);
        Assert.Equal("2.0.0", metadata.CatalogEntry.DependencyGroups[0].Dependencies?[0].Range);
    }

    [Fact]
    public async Task SearchForGroupAsync_BuildsTheQueryString_AndDeserializesResults()
    {
        const string ResultsJson = """
                                   {
                                     "totalHits": 1,
                                     "data": [
                                       {
                                         "id": "MyNuGetPkg",
                                         "version": "1.3.0.17",
                                         "totalDownloads": 5,
                                         "verified": true,
                                         "versions": { "version": "1.3.0.17", "downloads": 5 }
                                       }
                                     ]
                                   }
                                   """;

        (HttpClient httpClient, StubHttpMessageHandler handler) = CreateClient(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(ResultsJson, Encoding.UTF8, "application/json")
            });

        GitLabApiConnection connection = new(httpClient);
        PackagesNuGetRepository repository = new(connection);

        NugetSearchOptions options = new() { Q = "MyNuGetPkg", Skip = 0, Take = 20, Prerelease = true };

        GitLabNugetSearchResults results =
            await repository.SearchForGroupAsync(9970, options, TestContext.Current.CancellationToken);

        Assert.Equal(
            "https://gitlab.example/api/v4/groups/9970/-/packages/nuget/query?"
            + "q=MyNuGetPkg&skip=0&take=20&prerelease=true",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        Assert.Equal(1, results.TotalHits);
        GitLabNugetSearchResult result = Assert.Single(results.Data!);
        Assert.Equal("MyNuGetPkg", result.Id);
        Assert.True(result.Verified);
        Assert.Equal(5, result.Versions?.Downloads);
    }

    [Fact]
    public async Task DownloadSymbolFileForGroupAsync_BuildsTheThreeSegmentRoute()
    {
        (HttpClient httpClient, StubHttpMessageHandler handler) = CreateClient(_ =>
            new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(FileBytes) });

        GitLabApiConnection connection = new(httpClient);
        PackagesNuGetRepository repository = new(connection);

        using GitLabFileResponse file = await repository.DownloadSymbolFileForGroupAsync(9970, "mynugetpkg.pdb",
            "k813f89485474661234z7109cve5709eFFFFFFFF", "mynugetpkg.pdb", TestContext.Current.CancellationToken);

        Assert.Equal(
            "https://gitlab.example/api/v4/groups/9970/-/packages/nuget/symbolfiles/mynugetpkg.pdb/"
            + "k813f89485474661234z7109cve5709eFFFFFFFF/mynugetpkg.pdb",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(HttpStatusCode.OK, file.StatusCode);
    }

    [Fact]
    public async Task GetV2ServiceIndexForGroupAsync_StreamsTheRawBody()
    {
        (HttpClient httpClient, StubHttpMessageHandler handler) = CreateClient(_ =>
            new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(FileBytes) });

        GitLabApiConnection connection = new(httpClient);
        PackagesNuGetRepository repository = new(connection);

        using GitLabFileResponse file =
            await repository.GetV2ServiceIndexForGroupAsync(9970, TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/groups/9970/-/packages/nuget/v2",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(HttpStatusCode.OK, file.StatusCode);
    }

    [Fact]
    public async Task GetV2MetadataForGroupAsync_BuildsTheDollarMetadataRoute()
    {
        (HttpClient httpClient, StubHttpMessageHandler handler) = CreateClient(_ =>
            new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(FileBytes) });

        GitLabApiConnection connection = new(httpClient);
        PackagesNuGetRepository repository = new(connection);

        using GitLabFileResponse file =
            await repository.GetV2MetadataForGroupAsync(9970, TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/groups/9970/-/packages/nuget/v2/$metadata",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(HttpStatusCode.OK, file.StatusCode);
    }

    // Project scope.

    [Fact]
    public async Task AuthorizePackageUploadAsync_PutsToTheAuthorizeRoute_WithNoBody()
    {
        (HttpClient httpClient, StubHttpMessageHandler handler) =
            CreateClient(_ => new HttpResponseMessage(HttpStatusCode.OK));

        GitLabApiConnection connection = new(httpClient);
        PackagesNuGetRepository repository = new(connection);

        await repository.AuthorizePackageUploadAsync(1, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/packages/nuget/authorize",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Null(handler.LastRequest?.Content);
    }

    [Fact]
    public async Task GetPackageVersionsAsync_DeserializesTheVersionList()
    {
        const string VersionsJson = """{"versions":["1.0.0","1.3.0.17"]}""";

        (HttpClient httpClient, StubHttpMessageHandler handler) = CreateClient(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(VersionsJson, Encoding.UTF8, "application/json")
            });

        GitLabApiConnection connection = new(httpClient);
        PackagesNuGetRepository repository = new(connection);

        GitLabNugetPackagesVersions versions =
            await repository.GetPackageVersionsAsync(1, "MyNuGetPkg", TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/1/packages/nuget/download/MyNuGetPkg/index",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(["1.0.0", "1.3.0.17"], versions.Versions);
    }

    [Fact]
    public async Task DownloadPackageContentAsync_BuildsTheThreeSegmentRoute()
    {
        (HttpClient httpClient, StubHttpMessageHandler handler) = CreateClient(_ =>
            new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(FileBytes) });

        GitLabApiConnection connection = new(httpClient);
        PackagesNuGetRepository repository = new(connection);

        using GitLabFileResponse file = await repository.DownloadPackageContentAsync(1, "MyNuGetPkg", "1.3.0.17",
            "mynugetpkg.1.3.0.17.nupkg", TestContext.Current.CancellationToken);

        Assert.Equal(
            "https://gitlab.example/api/v4/projects/1/packages/nuget/download/MyNuGetPkg/1.3.0.17/"
            + "mynugetpkg.1.3.0.17.nupkg",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(HttpStatusCode.OK, file.StatusCode);
    }

    [Fact]
    public async Task GetServiceIndexAsync_BuildsTheProjectRoute_AndDeserializesTheIndex()
    {
        (HttpClient httpClient, StubHttpMessageHandler handler) = CreateClient(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(ServiceIndexJson, Encoding.UTF8, "application/json")
            });

        GitLabApiConnection connection = new(httpClient);
        PackagesNuGetRepository repository = new(connection);

        GitLabNugetServiceIndex index =
            await repository.GetServiceIndexAsync(1, TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/1/packages/nuget/index",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("1.3.0.17", index.Version);
    }

    [Fact]
    public async Task GetPackageMetadataAsync_BuildsTheIndexRoute()
    {
        const string MetadataJson = """{"count":2,"items":[]}""";

        (HttpClient httpClient, StubHttpMessageHandler handler) = CreateClient(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(MetadataJson, Encoding.UTF8, "application/json")
            });

        GitLabApiConnection connection = new(httpClient);
        PackagesNuGetRepository repository = new(connection);

        GitLabNugetPackagesMetadata metadata =
            await repository.GetPackageMetadataAsync(1, "MyNuGetPkg", TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/1/packages/nuget/metadata/MyNuGetPkg/index",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(2, metadata.Count);
    }

    [Fact]
    public async Task GetPackageVersionMetadataAsync_BuildsTheVersionRoute()
    {
        (HttpClient httpClient, StubHttpMessageHandler handler) = CreateClient(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(PackageMetadataJson, Encoding.UTF8, "application/json")
            });

        GitLabApiConnection connection = new(httpClient);
        PackagesNuGetRepository repository = new(connection);

        GitLabNugetPackageMetadata metadata = await repository.GetPackageVersionMetadataAsync(1, "MyNuGetPkg",
            "1.3.0.17", TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/1/packages/nuget/metadata/MyNuGetPkg/1.3.0.17",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.NotNull(metadata.AtId);
        Assert.NotNull(metadata.PackageContent);
    }

    [Fact]
    public async Task SearchAsync_OmitsUnsetParameters()
    {
        const string ResultsJson = """{"totalHits":0,"data":[]}""";

        (HttpClient httpClient, StubHttpMessageHandler handler) = CreateClient(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(ResultsJson, Encoding.UTF8, "application/json")
            });

        GitLabApiConnection connection = new(httpClient);
        PackagesNuGetRepository repository = new(connection);

        await repository.SearchAsync(1, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/1/packages/nuget/query",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task DownloadSymbolFileAsync_BuildsTheThreeSegmentRoute()
    {
        (HttpClient httpClient, StubHttpMessageHandler handler) = CreateClient(_ =>
            new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(FileBytes) });

        GitLabApiConnection connection = new(httpClient);
        PackagesNuGetRepository repository = new(connection);

        using GitLabFileResponse file = await repository.DownloadSymbolFileAsync(1, "mynugetpkg.pdb",
            "k813f89485474661234z7109cve5709eFFFFFFFF", "mynugetpkg.pdb", TestContext.Current.CancellationToken);

        Assert.Equal(
            "https://gitlab.example/api/v4/projects/1/packages/nuget/symbolfiles/mynugetpkg.pdb/"
            + "k813f89485474661234z7109cve5709eFFFFFFFF/mynugetpkg.pdb",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(HttpStatusCode.OK, file.StatusCode);
    }

    [Fact]
    public async Task AuthorizeSymbolPackageUploadAsync_PutsToTheAuthorizeRoute()
    {
        (HttpClient httpClient, StubHttpMessageHandler handler) =
            CreateClient(_ => new HttpResponseMessage(HttpStatusCode.OK));

        GitLabApiConnection connection = new(httpClient);
        PackagesNuGetRepository repository = new(connection);

        await repository.AuthorizeSymbolPackageUploadAsync(1, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/packages/nuget/symbolpackage/authorize",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task GetV2ServiceIndexAsync_BuildsTheProjectV2Route()
    {
        (HttpClient httpClient, StubHttpMessageHandler handler) = CreateClient(_ =>
            new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(FileBytes) });

        GitLabApiConnection connection = new(httpClient);
        PackagesNuGetRepository repository = new(connection);

        using GitLabFileResponse file =
            await repository.GetV2ServiceIndexAsync(1, TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/1/packages/nuget/v2",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(HttpStatusCode.OK, file.StatusCode);
    }

    [Fact]
    public async Task GetV2MetadataAsync_BuildsTheProjectDollarMetadataRoute()
    {
        (HttpClient httpClient, StubHttpMessageHandler handler) = CreateClient(_ =>
            new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(FileBytes) });

        GitLabApiConnection connection = new(httpClient);
        PackagesNuGetRepository repository = new(connection);

        using GitLabFileResponse file =
            await repository.GetV2MetadataAsync(1, TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/1/packages/nuget/v2/$metadata",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(HttpStatusCode.OK, file.StatusCode);
    }

    [Fact]
    public async Task AuthorizePackageV2UploadAsync_PutsToTheV2AuthorizeRoute()
    {
        (HttpClient httpClient, StubHttpMessageHandler handler) =
            CreateClient(_ => new HttpResponseMessage(HttpStatusCode.OK));

        GitLabApiConnection connection = new(httpClient);
        PackagesNuGetRepository repository = new(connection);

        await repository.AuthorizePackageV2UploadAsync(1, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/packages/nuget/v2/authorize",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task DeletePackageAsync_SendsDeleteToTheNameAndVersionRoute()
    {
        (HttpClient httpClient, StubHttpMessageHandler handler) =
            CreateClient(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        GitLabApiConnection connection = new(httpClient);
        PackagesNuGetRepository repository = new(connection);

        await repository.DeletePackageAsync(1, "MyNuGetPkg", "1.3.0.17", TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/packages/nuget/MyNuGetPkg/1.3.0.17",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task FindPackagesByIdAsync_BuildsTheLiteralParenSegment_WithAnIdQueryParameter()
    {
        (HttpClient httpClient, StubHttpMessageHandler handler) = CreateClient(_ =>
            new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(FileBytes) });

        GitLabApiConnection connection = new(httpClient);
        PackagesNuGetRepository repository = new(connection);

        using GitLabFileResponse file =
            await repository.FindPackagesByIdAsync(1, "MyNuGetPkg", TestContext.Current.CancellationToken);

        Assert.Equal(
            "https://gitlab.example/api/v4/projects/1/packages/nuget/v2/FindPackagesById()?id=MyNuGetPkg",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(HttpStatusCode.OK, file.StatusCode);
    }

    [Fact]
    public async Task EnumeratePackagesAsync_BuildsTheLiteralParenSegment_WithAFilterQueryParameter()
    {
        (HttpClient httpClient, StubHttpMessageHandler handler) = CreateClient(_ =>
            new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(FileBytes) });

        GitLabApiConnection connection = new(httpClient);
        PackagesNuGetRepository repository = new(connection);

        using GitLabFileResponse file = await repository.EnumeratePackagesAsync(1,
            "substringof('NuGetPkg',Id)", TestContext.Current.CancellationToken);

        Assert.Equal(
            "https://gitlab.example/api/v4/projects/1/packages/nuget/v2/Packages()?"
            + "$filter=substringof%28%27NuGetPkg%27%2CId%29",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(HttpStatusCode.OK, file.StatusCode);
    }

    [Fact]
    public async Task EnumeratePackagesAsync_OmitsTheFilterParameter_WhenNotProvided()
    {
        (HttpClient httpClient, StubHttpMessageHandler handler) = CreateClient(_ =>
            new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(FileBytes) });

        GitLabApiConnection connection = new(httpClient);
        PackagesNuGetRepository repository = new(connection);

        using GitLabFileResponse file =
            await repository.EnumeratePackagesAsync(1, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/1/packages/nuget/v2/Packages()",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(HttpStatusCode.OK, file.StatusCode);
    }

    [Fact]
    public async Task GetServiceIndexAsync_MapsA404ToTheTypedNotFoundException()
    {
        (HttpClient httpClient, StubHttpMessageHandler _) = CreateClient(_ =>
            new HttpResponseMessage(HttpStatusCode.NotFound)
            {
                Content = new StringContent("""{"message":"404 Not Found"}""", Encoding.UTF8, "application/json")
            });

        GitLabApiConnection connection = new(httpClient);
        PackagesNuGetRepository repository = new(connection);

        GitLabNotFoundException exception = await Assert.ThrowsAsync<GitLabNotFoundException>(() =>
            repository.GetServiceIndexAsync(1, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
    }
}