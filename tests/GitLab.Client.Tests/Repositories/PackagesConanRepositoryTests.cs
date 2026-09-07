using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

using GitLab.Client.Abstractions;
using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Repositories;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Repositories;

public sealed class PackagesConanRepositoryTests
{
    // Coordinates chosen to prove escaping: '+' is how GitLab encodes a namespaced project path inside
    // package_username (my-group+my-project), and it is not in Uri.EscapeDataString's unreserved set, so
    // it round-trips through this suite as a live proof that .Escaped(...) - not .Literal(...) - built
    // the route.
    private const string PackageName = "hello";
    private const string PackageVersion = "1.0";
    private const string PackageUsername = "my-group+my-project";
    private const string PackageChannel = "stable";
    private const string ConanPackageReference = "103f6067a947f366ef91fc1b7da351c588d1827f";
    private const string RecipeRevision = "0";
    private const string PackageRevision = "0";
    private const string FileName = "conanfile.py";

    private const string RecipeSegment = "hello/1.0/my-group%2Bmy-project/stable";

    private static readonly Uri BaseAddress = new("https://gitlab.example/api/v4/");

    private static PackagesConanRepository CreateRepository(HttpMessageHandler handler, out HttpClient httpClient)
    {
        httpClient = new HttpClient(handler) { BaseAddress = BaseAddress };
        return new PackagesConanRepository(new GitLabApiConnection(httpClient));
    }

    private static StubHttpMessageHandler JsonHandler(string json)
    {
        return new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });
    }

    private static StubHttpMessageHandler EmptyHandler(HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        return new StubHttpMessageHandler(_ => new HttpResponseMessage(statusCode));
    }

    // ----- Recipes & packages (v1) -----

    [Fact]
    public async Task SearchAsync_BuildsInstanceRoute_WithQueryAndDeserializesJsonElement()
    {
        using StubHttpMessageHandler handler = JsonHandler("""{"results":["hello/1.0@my-group+my-project/stable"]}""");
        PackagesConanRepository repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            JsonElement result =
                await repository.SearchAsync("Hello*", true, TestContext.Current.CancellationToken);

            Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
            string? uri = handler.LastRequest?.RequestUri?.AbsoluteUri;
            Assert.StartsWith("https://gitlab.example/api/v4/packages/conan/v1/conans/search?", uri,
                StringComparison.Ordinal);
            Assert.Contains("q=Hello%2A", uri, StringComparison.Ordinal);
            Assert.Contains("ignorecase=true", uri, StringComparison.Ordinal);
            Assert.Equal("hello/1.0@my-group+my-project/stable", result.GetProperty("results")[0].GetString());
        }
    }

    [Fact]
    public async Task SearchForProjectAsync_BuildsProjectRoute()
    {
        using StubHttpMessageHandler handler = JsonHandler("""{"results":[]}""");
        PackagesConanRepository repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            await repository.SearchForProjectAsync(1, "Hello*",
                cancellationToken: TestContext.Current.CancellationToken);

            string? uri = handler.LastRequest?.RequestUri?.AbsoluteUri;
            Assert.StartsWith("https://gitlab.example/api/v4/projects/1/packages/conan/v1/conans/search?", uri,
                StringComparison.Ordinal);
            Assert.Contains("q=Hello%2A", uri, StringComparison.Ordinal);
            Assert.DoesNotContain("ignorecase", uri, StringComparison.Ordinal);
        }
    }

    [Fact]
    public async Task DeleteRecipeAsync_EscapesEveryCoordinate_AndSendsDelete()
    {
        using StubHttpMessageHandler handler = EmptyHandler();
        PackagesConanRepository repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            await repository.DeleteRecipeAsync(PackageName, PackageVersion, PackageUsername, PackageChannel,
                TestContext.Current.CancellationToken);

            Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
            Assert.Equal($"https://gitlab.example/api/v4/packages/conan/v1/conans/{RecipeSegment}",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
        }
    }

    [Fact]
    public async Task DeleteRecipeForProjectAsync_BuildsProjectRoute()
    {
        using StubHttpMessageHandler handler = EmptyHandler();
        PackagesConanRepository repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            await repository.DeleteRecipeForProjectAsync(42, PackageName, PackageVersion, PackageUsername,
                PackageChannel, TestContext.Current.CancellationToken);

            Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
            Assert.Equal(
                $"https://gitlab.example/api/v4/projects/42/packages/conan/v1/conans/{RecipeSegment}",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
        }
    }

    [Fact]
    public async Task GetRecipeSnapshotAsync_DeserializesTheSnapshot()
    {
        using StubHttpMessageHandler handler =
            JsonHandler("""{"recipe_snapshot":{"conanfile.py":"44e6d3"}}""");
        PackagesConanRepository repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            GitLabConanRecipeSnapshot snapshot = await repository.GetRecipeSnapshotAsync(PackageName,
                PackageVersion, PackageUsername, PackageChannel, TestContext.Current.CancellationToken);

            Assert.Equal($"https://gitlab.example/api/v4/packages/conan/v1/conans/{RecipeSegment}",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
            Assert.Equal("44e6d3", snapshot.RecipeSnapshot?.GetProperty("conanfile.py").GetString());
        }
    }

    [Fact]
    public async Task GetRecipeSnapshotForProjectAsync_BuildsProjectRoute()
    {
        using StubHttpMessageHandler handler = JsonHandler("""{"recipe_snapshot":{}}""");
        PackagesConanRepository repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            await repository.GetRecipeSnapshotForProjectAsync(7, PackageName, PackageVersion, PackageUsername,
                PackageChannel, TestContext.Current.CancellationToken);

            Assert.Equal($"https://gitlab.example/api/v4/projects/7/packages/conan/v1/conans/{RecipeSegment}",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
        }
    }

    [Fact]
    public async Task GetRecipeManifestAsync_HitsTheDigestRoute_AndDeserializesRecipeUrls()
    {
        using StubHttpMessageHandler handler =
            JsonHandler("""{"recipe_urls":{"conanfile.py":"https://gitlab.example/x/conanfile.py"}}""");
        PackagesConanRepository repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            GitLabConanRecipeUrls urls = await repository.GetRecipeManifestAsync(PackageName, PackageVersion,
                PackageUsername, PackageChannel, TestContext.Current.CancellationToken);

            Assert.Equal($"https://gitlab.example/api/v4/packages/conan/v1/conans/{RecipeSegment}/digest",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
            Assert.Equal("https://gitlab.example/x/conanfile.py",
                urls.RecipeUrls?.GetProperty("conanfile.py").GetString());
        }
    }

    [Fact]
    public async Task GetRecipeManifestForProjectAsync_BuildsProjectRoute()
    {
        using StubHttpMessageHandler handler = JsonHandler("""{"recipe_urls":{}}""");
        PackagesConanRepository repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            await repository.GetRecipeManifestForProjectAsync(1, PackageName, PackageVersion, PackageUsername,
                PackageChannel, TestContext.Current.CancellationToken);

            Assert.Equal(
                $"https://gitlab.example/api/v4/projects/1/packages/conan/v1/conans/{RecipeSegment}/digest",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
        }
    }

    [Fact]
    public async Task GetRecipeDownloadUrlsAsync_HitsTheDownloadUrlsRoute_UsingTheSameDtoAsManifest()
    {
        using StubHttpMessageHandler handler =
            JsonHandler("""{"recipe_urls":{"conanmanifest.txt":"https://gitlab.example/x/conanmanifest.txt"}}""");
        PackagesConanRepository repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            GitLabConanRecipeUrls urls = await repository.GetRecipeDownloadUrlsAsync(PackageName, PackageVersion,
                PackageUsername, PackageChannel, TestContext.Current.CancellationToken);

            Assert.Equal(
                $"https://gitlab.example/api/v4/packages/conan/v1/conans/{RecipeSegment}/download_urls",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
            Assert.NotNull(urls.RecipeUrls);
        }
    }

    [Fact]
    public async Task GetRecipeDownloadUrlsForProjectAsync_BuildsProjectRoute()
    {
        using StubHttpMessageHandler handler = JsonHandler("""{"recipe_urls":{}}""");
        PackagesConanRepository repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            await repository.GetRecipeDownloadUrlsForProjectAsync(1, PackageName, PackageVersion, PackageUsername,
                PackageChannel, TestContext.Current.CancellationToken);

            Assert.Equal(
                $"https://gitlab.example/api/v4/projects/1/packages/conan/v1/conans/{RecipeSegment}/download_urls",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
        }
    }

    [Fact]
    public async Task GetRecipeUploadUrlsAsync_PostsWithNoBody_AndDeserializesUploadUrls()
    {
        bool hadContent = true;
        using StubHttpMessageHandler handler = new(request =>
        {
            hadContent = request.Content is not null;
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    """{"upload_urls":{"conanfile.py":"https://gitlab.example/u/conanfile.py"}}""",
                    Encoding.UTF8, "application/json")
            };
        });
        PackagesConanRepository repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            GitLabConanUploadUrls urls = await repository.GetRecipeUploadUrlsAsync(PackageName, PackageVersion,
                PackageUsername, PackageChannel, TestContext.Current.CancellationToken);

            Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
            Assert.False(hadContent);
            Assert.Equal(
                $"https://gitlab.example/api/v4/packages/conan/v1/conans/{RecipeSegment}/upload_urls",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
            Assert.Equal("https://gitlab.example/u/conanfile.py",
                urls.UploadUrls?.GetProperty("conanfile.py").GetString());
        }
    }

    [Fact]
    public async Task GetRecipeUploadUrlsForProjectAsync_BuildsProjectRoute()
    {
        using StubHttpMessageHandler handler = JsonHandler("""{"upload_urls":{}}""");
        PackagesConanRepository repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            await repository.GetRecipeUploadUrlsForProjectAsync(1, PackageName, PackageVersion, PackageUsername,
                PackageChannel, TestContext.Current.CancellationToken);

            Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
            Assert.Equal(
                $"https://gitlab.example/api/v4/projects/1/packages/conan/v1/conans/{RecipeSegment}/upload_urls",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
        }
    }

    [Fact]
    public async Task GetPackageSnapshotAsync_EscapesTheConanPackageReference_AndDeserializesTheSnapshot()
    {
        using StubHttpMessageHandler handler =
            JsonHandler("""{"package_snapshot":{"conaninfo.txt":"abc123"}}""");
        PackagesConanRepository repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            GitLabConanPackageSnapshot snapshot = await repository.GetPackageSnapshotAsync(PackageName,
                PackageVersion, PackageUsername, PackageChannel, ConanPackageReference,
                TestContext.Current.CancellationToken);

            Assert.Equal(
                $"https://gitlab.example/api/v4/packages/conan/v1/conans/{RecipeSegment}/packages/{ConanPackageReference}",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
            Assert.Equal("abc123", snapshot.PackageSnapshot?.GetProperty("conaninfo.txt").GetString());
        }
    }

    [Fact]
    public async Task GetPackageSnapshotForProjectAsync_BuildsProjectRoute()
    {
        using StubHttpMessageHandler handler = JsonHandler("""{"package_snapshot":{}}""");
        PackagesConanRepository repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            await repository.GetPackageSnapshotForProjectAsync(1, PackageName, PackageVersion, PackageUsername,
                PackageChannel, ConanPackageReference, TestContext.Current.CancellationToken);

            Assert.Equal(
                $"https://gitlab.example/api/v4/projects/1/packages/conan/v1/conans/{RecipeSegment}/packages/{ConanPackageReference}",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
        }
    }

    [Fact]
    public async Task GetPackageManifestAsync_HitsTheDigestRoute()
    {
        using StubHttpMessageHandler handler =
            JsonHandler("""{"package_urls":{"conaninfo.txt":"https://x/conaninfo.txt"}}""");
        PackagesConanRepository repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            GitLabConanPackageUrls urls = await repository.GetPackageManifestAsync(PackageName, PackageVersion,
                PackageUsername, PackageChannel, ConanPackageReference, TestContext.Current.CancellationToken);

            Assert.Equal(
                $"https://gitlab.example/api/v4/packages/conan/v1/conans/{RecipeSegment}/packages/{ConanPackageReference}/digest",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
            Assert.NotNull(urls.PackageUrls);
        }
    }

    [Fact]
    public async Task GetPackageManifestForProjectAsync_BuildsProjectRoute()
    {
        using StubHttpMessageHandler handler = JsonHandler("""{"package_urls":{}}""");
        PackagesConanRepository repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            await repository.GetPackageManifestForProjectAsync(1, PackageName, PackageVersion, PackageUsername,
                PackageChannel, ConanPackageReference, TestContext.Current.CancellationToken);

            Assert.Equal(
                $"https://gitlab.example/api/v4/projects/1/packages/conan/v1/conans/{RecipeSegment}/packages/{ConanPackageReference}/digest",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
        }
    }

    [Fact]
    public async Task GetPackageDownloadUrlsAsync_HitsTheDownloadUrlsRoute()
    {
        using StubHttpMessageHandler handler = JsonHandler("""{"package_urls":{}}""");
        PackagesConanRepository repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            await repository.GetPackageDownloadUrlsAsync(PackageName, PackageVersion, PackageUsername,
                PackageChannel, ConanPackageReference, TestContext.Current.CancellationToken);

            Assert.Equal(
                $"https://gitlab.example/api/v4/packages/conan/v1/conans/{RecipeSegment}/packages/{ConanPackageReference}/download_urls",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
        }
    }

    [Fact]
    public async Task GetPackageDownloadUrlsForProjectAsync_BuildsProjectRoute()
    {
        using StubHttpMessageHandler handler = JsonHandler("""{"package_urls":{}}""");
        PackagesConanRepository repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            await repository.GetPackageDownloadUrlsForProjectAsync(1, PackageName, PackageVersion,
                PackageUsername, PackageChannel, ConanPackageReference, TestContext.Current.CancellationToken);

            Assert.Equal(
                $"https://gitlab.example/api/v4/projects/1/packages/conan/v1/conans/{RecipeSegment}/packages/{ConanPackageReference}/download_urls",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
        }
    }

    [Fact]
    public async Task GetPackageUploadUrlsAsync_PostsWithNoBody()
    {
        bool hadContent = true;
        using StubHttpMessageHandler handler = new(request =>
        {
            hadContent = request.Content is not null;
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"upload_urls":{}}""", Encoding.UTF8, "application/json")
            };
        });
        PackagesConanRepository repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            await repository.GetPackageUploadUrlsAsync(PackageName, PackageVersion, PackageUsername,
                PackageChannel, ConanPackageReference, TestContext.Current.CancellationToken);

            Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
            Assert.False(hadContent);
            Assert.Equal(
                $"https://gitlab.example/api/v4/packages/conan/v1/conans/{RecipeSegment}/packages/{ConanPackageReference}/upload_urls",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
        }
    }

    [Fact]
    public async Task GetPackageUploadUrlsForProjectAsync_BuildsProjectRoute()
    {
        using StubHttpMessageHandler handler = JsonHandler("""{"upload_urls":{}}""");
        PackagesConanRepository repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            await repository.GetPackageUploadUrlsForProjectAsync(1, PackageName, PackageVersion, PackageUsername,
                PackageChannel, ConanPackageReference, TestContext.Current.CancellationToken);

            Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
            Assert.Equal(
                $"https://gitlab.example/api/v4/projects/1/packages/conan/v1/conans/{RecipeSegment}/packages/{ConanPackageReference}/upload_urls",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
        }
    }

    [Fact]
    public async Task SearchPackageReferencesAsync_HitsTheSearchSubResource_AndReturnsJsonElement()
    {
        using StubHttpMessageHandler handler =
            JsonHandler($$$"""{"{{{ConanPackageReference}}}":{"recipe_hash":"abc"}}""");
        PackagesConanRepository repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            JsonElement result = await repository.SearchPackageReferencesAsync(PackageName, PackageVersion,
                PackageUsername, PackageChannel, TestContext.Current.CancellationToken);

            Assert.Equal($"https://gitlab.example/api/v4/packages/conan/v1/conans/{RecipeSegment}/search",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
            Assert.Equal("abc", result.GetProperty(ConanPackageReference).GetProperty("recipe_hash").GetString());
        }
    }

    [Fact]
    public async Task SearchPackageReferencesForProjectAsync_BuildsProjectRoute()
    {
        using StubHttpMessageHandler handler = JsonHandler("{}");
        PackagesConanRepository repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            await repository.SearchPackageReferencesForProjectAsync(1, PackageName, PackageVersion,
                PackageUsername, PackageChannel, TestContext.Current.CancellationToken);

            Assert.Equal(
                $"https://gitlab.example/api/v4/projects/1/packages/conan/v1/conans/{RecipeSegment}/search",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
        }
    }

    // ----- Files (v1) -----

    [Fact]
    public async Task DownloadRecipeFileAsync_StreamsTheRawBody()
    {
        byte[] bytes = "print('conanfile')"u8.ToArray();
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(bytes)
            {
                Headers = { ContentType = new MediaTypeHeaderValue("text/x-python") }
            }
        });
        PackagesConanRepository repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            using GitLabFileResponse file = await repository.DownloadRecipeFileAsync(PackageName, PackageVersion,
                PackageUsername, PackageChannel, RecipeRevision, FileName, TestContext.Current.CancellationToken);

            Assert.Equal(
                $"https://gitlab.example/api/v4/packages/conan/v1/files/{RecipeSegment}/{RecipeRevision}/export/{FileName}",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
            Assert.Equal("text/x-python", file.ContentType);

            using MemoryStream copy = new();
            await file.Content.CopyToAsync(copy, TestContext.Current.CancellationToken);
            Assert.Equal(bytes, copy.ToArray());
        }
    }

    [Fact]
    public async Task DownloadRecipeFileForProjectAsync_BuildsProjectRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent([1, 2, 3])
        });
        PackagesConanRepository repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            using GitLabFileResponse file = await repository.DownloadRecipeFileForProjectAsync(1, PackageName,
                PackageVersion, PackageUsername, PackageChannel, RecipeRevision, FileName,
                TestContext.Current.CancellationToken);

            Assert.Equal(
                $"https://gitlab.example/api/v4/projects/1/packages/conan/v1/files/{RecipeSegment}/{RecipeRevision}/export/{FileName}",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
            Assert.Equal(HttpStatusCode.OK, file.StatusCode);
        }
    }

    [Fact]
    public async Task AuthorizeRecipeFileUploadAsync_PutsWithNoBody_AndAcceptsAnEmptyResponse()
    {
        bool hadContent = true;
        using StubHttpMessageHandler handler = new(request =>
        {
            hadContent = request.Content is not null;
            return new HttpResponseMessage(HttpStatusCode.OK);
        });
        PackagesConanRepository repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            await repository.AuthorizeRecipeFileUploadAsync(PackageName, PackageVersion, PackageUsername,
                PackageChannel, RecipeRevision, FileName, TestContext.Current.CancellationToken);

            Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
            Assert.False(hadContent);
            Assert.Equal(
                $"https://gitlab.example/api/v4/packages/conan/v1/files/{RecipeSegment}/{RecipeRevision}/export/{FileName}/authorize",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
        }
    }

    [Fact]
    public async Task AuthorizeRecipeFileUploadForProjectAsync_BuildsProjectRoute()
    {
        using StubHttpMessageHandler handler = EmptyHandler();
        PackagesConanRepository repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            await repository.AuthorizeRecipeFileUploadForProjectAsync(1, PackageName, PackageVersion,
                PackageUsername, PackageChannel, RecipeRevision, FileName, TestContext.Current.CancellationToken);

            Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
            Assert.Equal(
                $"https://gitlab.example/api/v4/projects/1/packages/conan/v1/files/{RecipeSegment}/{RecipeRevision}/export/{FileName}/authorize",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
        }
    }

    [Fact]
    public async Task DownloadPackageFileAsync_StreamsTheRawBody()
    {
        byte[] bytes = [0xDE, 0xAD, 0xBE, 0xEF];
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(bytes)
            {
                Headers = { ContentType = new MediaTypeHeaderValue("application/gzip") }
            }
        });
        PackagesConanRepository repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            using GitLabFileResponse file = await repository.DownloadPackageFileAsync(PackageName, PackageVersion,
                PackageUsername, PackageChannel, RecipeRevision, ConanPackageReference, PackageRevision,
                "conan_package.tgz", TestContext.Current.CancellationToken);

            Assert.Equal(
                $"https://gitlab.example/api/v4/packages/conan/v1/files/{RecipeSegment}/{RecipeRevision}/package/{ConanPackageReference}/{PackageRevision}/conan_package.tgz",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
            Assert.Equal("application/gzip", file.ContentType);
        }
    }

    [Fact]
    public async Task DownloadPackageFileForProjectAsync_BuildsProjectRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent([1])
        });
        PackagesConanRepository repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            using GitLabFileResponse file = await repository.DownloadPackageFileForProjectAsync(1, PackageName,
                PackageVersion, PackageUsername, PackageChannel, RecipeRevision, ConanPackageReference,
                PackageRevision, FileName, TestContext.Current.CancellationToken);

            Assert.Equal(
                $"https://gitlab.example/api/v4/projects/1/packages/conan/v1/files/{RecipeSegment}/{RecipeRevision}/package/{ConanPackageReference}/{PackageRevision}/{FileName}",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
            _ = file;
        }
    }

    [Fact]
    public async Task AuthorizePackageFileUploadAsync_PutsToTheAuthorizeRoute()
    {
        using StubHttpMessageHandler handler = EmptyHandler();
        PackagesConanRepository repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            await repository.AuthorizePackageFileUploadAsync(PackageName, PackageVersion, PackageUsername,
                PackageChannel, RecipeRevision, ConanPackageReference, PackageRevision, FileName,
                TestContext.Current.CancellationToken);

            Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
            Assert.Equal(
                $"https://gitlab.example/api/v4/packages/conan/v1/files/{RecipeSegment}/{RecipeRevision}/package/{ConanPackageReference}/{PackageRevision}/{FileName}/authorize",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
        }
    }

    [Fact]
    public async Task AuthorizePackageFileUploadForProjectAsync_BuildsProjectRoute()
    {
        using StubHttpMessageHandler handler = EmptyHandler();
        PackagesConanRepository repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            await repository.AuthorizePackageFileUploadForProjectAsync(1, PackageName, PackageVersion,
                PackageUsername, PackageChannel, RecipeRevision, ConanPackageReference, PackageRevision, FileName,
                TestContext.Current.CancellationToken);

            Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
            Assert.Equal(
                $"https://gitlab.example/api/v4/projects/1/packages/conan/v1/files/{RecipeSegment}/{RecipeRevision}/package/{ConanPackageReference}/{PackageRevision}/{FileName}/authorize",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
        }
    }

    // ----- Instance & credentials (v1) -----

    [Fact]
    public async Task PingAsync_GetsThePingRoute_AndAcceptsAnEmptyBody()
    {
        using StubHttpMessageHandler handler = EmptyHandler();
        PackagesConanRepository repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            await repository.PingAsync(TestContext.Current.CancellationToken);

            Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
            Assert.Equal("https://gitlab.example/api/v4/packages/conan/v1/ping",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
        }
    }

    [Fact]
    public async Task PingForProjectAsync_BuildsProjectRoute()
    {
        using StubHttpMessageHandler handler = EmptyHandler();
        PackagesConanRepository repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            await repository.PingForProjectAsync(9, TestContext.Current.CancellationToken);

            Assert.Equal("https://gitlab.example/api/v4/projects/9/packages/conan/v1/ping",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
        }
    }

    [Fact]
    public async Task AuthenticateAsync_ReadsTheTokenBodyAsRawBytes()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("eyJhbGciOiJIUzI1NiJ9.token", Encoding.UTF8, "text/plain")
        });
        PackagesConanRepository repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            using GitLabFileResponse token = await repository.AuthenticateAsync(TestContext.Current.CancellationToken);

            Assert.Equal("https://gitlab.example/api/v4/packages/conan/v1/users/authenticate",
                handler.LastRequest?.RequestUri?.AbsoluteUri);

            using StreamReader reader = new(token.Content, Encoding.UTF8);
            string text = await reader.ReadToEndAsync(TestContext.Current.CancellationToken);
            Assert.Equal("eyJhbGciOiJIUzI1NiJ9.token", text);
        }
    }

    [Fact]
    public async Task AuthenticateForProjectAsync_BuildsProjectRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("token", Encoding.UTF8, "text/plain")
        });
        PackagesConanRepository repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            using GitLabFileResponse token =
                await repository.AuthenticateForProjectAsync(1, TestContext.Current.CancellationToken);

            Assert.Equal("https://gitlab.example/api/v4/projects/1/packages/conan/v1/users/authenticate",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
            _ = token;
        }
    }

    [Fact]
    public async Task CheckCredentialsAsync_GetsTheCheckCredentialsRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("my-group+my-project", Encoding.UTF8, "text/plain")
        });
        PackagesConanRepository repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            using GitLabFileResponse response =
                await repository.CheckCredentialsAsync(TestContext.Current.CancellationToken);

            Assert.Equal("https://gitlab.example/api/v4/packages/conan/v1/users/check_credentials",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
            _ = response;
        }
    }

    [Fact]
    public async Task CheckCredentialsForProjectAsync_BuildsProjectRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("ok", Encoding.UTF8, "text/plain")
        });
        PackagesConanRepository repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            using GitLabFileResponse response =
                await repository.CheckCredentialsForProjectAsync(5, TestContext.Current.CancellationToken);

            Assert.Equal("https://gitlab.example/api/v4/projects/5/packages/conan/v1/users/check_credentials",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
            _ = response;
        }
    }

    // ----- Revisions (v2, project-scoped only) -----

    [Fact]
    public async Task SearchForProjectV2Async_BuildsTheV2Route_WithQuery()
    {
        using StubHttpMessageHandler handler = JsonHandler("""{"results":[]}""");
        PackagesConanRepository repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            await repository.SearchForProjectV2Async(1, "hello", false, TestContext.Current.CancellationToken);

            string? uri = handler.LastRequest?.RequestUri?.AbsoluteUri;
            Assert.StartsWith("https://gitlab.example/api/v4/projects/1/packages/conan/v2/conans/search?", uri,
                StringComparison.Ordinal);
            Assert.Contains("q=hello", uri, StringComparison.Ordinal);
            Assert.Contains("ignorecase=false", uri, StringComparison.Ordinal);
        }
    }

    [Fact]
    public async Task GetLatestRecipeRevisionAsync_DeserializesRevisionAndTime()
    {
        using StubHttpMessageHandler handler =
            JsonHandler("""{"revision":"3244231d…","time":"2020-01-01T00:00:00.000Z"}""");
        PackagesConanRepository repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            GitLabConanRevisionInfo info = await repository.GetLatestRecipeRevisionAsync(1, PackageName,
                PackageVersion, PackageUsername, PackageChannel, TestContext.Current.CancellationToken);

            Assert.Equal(
                $"https://gitlab.example/api/v4/projects/1/packages/conan/v2/conans/{RecipeSegment}/latest",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
            Assert.Equal("2020-01-01T00:00:00.000Z", info.Time);
        }
    }

    [Fact]
    public async Task GetRecipeRevisionsAsync_DeserializesReferenceAndRevisions()
    {
        using StubHttpMessageHandler handler = JsonHandler(
            """{"reference":"hello/1.0@my-group+my-project/stable","revisions":[{"revision":"0","time":"2020-01-01T00:00:00Z"}]}""");
        PackagesConanRepository repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            GitLabConanRecipeRevisions revisions = await repository.GetRecipeRevisionsAsync(1, PackageName,
                PackageVersion, PackageUsername, PackageChannel, TestContext.Current.CancellationToken);

            Assert.Equal(
                $"https://gitlab.example/api/v4/projects/1/packages/conan/v2/conans/{RecipeSegment}/revisions",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
            Assert.Equal("hello/1.0@my-group+my-project/stable", revisions.Reference);
            Assert.Equal("0", Assert.Single(revisions.Revisions!).GetProperty("revision").GetString());
        }
    }

    [Fact]
    public async Task DeleteRecipeRevisionAsync_EscapesTheRevision_AndSendsDelete()
    {
        using StubHttpMessageHandler handler = EmptyHandler();
        PackagesConanRepository repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            await repository.DeleteRecipeRevisionAsync(1, PackageName, PackageVersion, PackageUsername,
                PackageChannel, "0/build", TestContext.Current.CancellationToken);

            Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
            Assert.Equal(
                $"https://gitlab.example/api/v4/projects/1/packages/conan/v2/conans/{RecipeSegment}/revisions/0%2Fbuild",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
        }
    }

    [Fact]
    public async Task GetRecipeRevisionFilesAsync_DeserializesTheFileListing()
    {
        using StubHttpMessageHandler handler = JsonHandler("""{"files":{"conanfile.py":{}}}""");
        PackagesConanRepository repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            GitLabConanFileList files = await repository.GetRecipeRevisionFilesAsync(1, PackageName,
                PackageVersion, PackageUsername, PackageChannel, RecipeRevision,
                TestContext.Current.CancellationToken);

            Assert.Equal(
                $"https://gitlab.example/api/v4/projects/1/packages/conan/v2/conans/{RecipeSegment}/revisions/{RecipeRevision}/files",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
            Assert.True(files.Files!.Value.TryGetProperty("conanfile.py", out _));
        }
    }

    [Fact]
    public async Task DownloadRecipeRevisionFileAsync_StreamsTheRawBody()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent([9, 9])
        });
        PackagesConanRepository repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            using GitLabFileResponse file = await repository.DownloadRecipeRevisionFileAsync(1, PackageName,
                PackageVersion, PackageUsername, PackageChannel, RecipeRevision, FileName,
                TestContext.Current.CancellationToken);

            Assert.Equal(
                $"https://gitlab.example/api/v4/projects/1/packages/conan/v2/conans/{RecipeSegment}/revisions/{RecipeRevision}/files/{FileName}",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
            _ = file;
        }
    }

    [Fact]
    public async Task AuthorizeRecipeRevisionFileUploadAsync_PutsToTheAuthorizeRoute()
    {
        using StubHttpMessageHandler handler = EmptyHandler();
        PackagesConanRepository repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            await repository.AuthorizeRecipeRevisionFileUploadAsync(1, PackageName, PackageVersion,
                PackageUsername, PackageChannel, RecipeRevision, FileName, TestContext.Current.CancellationToken);

            Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
            Assert.Equal(
                $"https://gitlab.example/api/v4/projects/1/packages/conan/v2/conans/{RecipeSegment}/revisions/{RecipeRevision}/files/{FileName}/authorize",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
        }
    }

    [Fact]
    public async Task GetLatestPackageRevisionAsync_EscapesTheConanPackageReference()
    {
        using StubHttpMessageHandler handler = JsonHandler("""{"revision":"0","time":"2020-01-01T00:00:00Z"}""");
        PackagesConanRepository repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            GitLabConanRevisionInfo info = await repository.GetLatestPackageRevisionAsync(1, PackageName,
                PackageVersion, PackageUsername, PackageChannel, RecipeRevision, ConanPackageReference,
                TestContext.Current.CancellationToken);

            Assert.Equal(
                $"https://gitlab.example/api/v4/projects/1/packages/conan/v2/conans/{RecipeSegment}/revisions/{RecipeRevision}/packages/{ConanPackageReference}/latest",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
            Assert.Equal("0", info.Revision);
        }
    }

    [Fact]
    public async Task GetPackageRevisionsAsync_DeserializesTheCamelCasePackageReferenceField()
    {
        using StubHttpMessageHandler handler = JsonHandler(
            $$"""{"packageReference":"{{ConanPackageReference}}","revisions":[{"revision":"0","time":"2020-01-01T00:00:00Z"}]}""");
        PackagesConanRepository repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            GitLabConanPackageRevisions revisions = await repository.GetPackageRevisionsAsync(1, PackageName,
                PackageVersion, PackageUsername, PackageChannel, RecipeRevision, ConanPackageReference,
                TestContext.Current.CancellationToken);

            Assert.Equal(
                $"https://gitlab.example/api/v4/projects/1/packages/conan/v2/conans/{RecipeSegment}/revisions/{RecipeRevision}/packages/{ConanPackageReference}/revisions",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
            Assert.Equal(ConanPackageReference, revisions.PackageReference);
            Assert.Single(revisions.Revisions!);
        }
    }

    [Fact]
    public async Task DeletePackageRevisionAsync_BuildsTheFullCoordinateChain()
    {
        using StubHttpMessageHandler handler = EmptyHandler();
        PackagesConanRepository repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            await repository.DeletePackageRevisionAsync(1, PackageName, PackageVersion, PackageUsername,
                PackageChannel, RecipeRevision, ConanPackageReference, PackageRevision,
                TestContext.Current.CancellationToken);

            Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
            Assert.Equal(
                $"https://gitlab.example/api/v4/projects/1/packages/conan/v2/conans/{RecipeSegment}/revisions/{RecipeRevision}/packages/{ConanPackageReference}/revisions/{PackageRevision}",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
        }
    }

    [Fact]
    public async Task GetPackageRevisionFilesAsync_DeserializesTheFileListing()
    {
        using StubHttpMessageHandler handler = JsonHandler("""{"files":{"conaninfo.txt":{}}}""");
        PackagesConanRepository repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            await repository.GetPackageRevisionFilesAsync(1, PackageName, PackageVersion, PackageUsername,
                PackageChannel, RecipeRevision, ConanPackageReference, PackageRevision,
                TestContext.Current.CancellationToken);

            Assert.Equal(
                $"https://gitlab.example/api/v4/projects/1/packages/conan/v2/conans/{RecipeSegment}/revisions/{RecipeRevision}/packages/{ConanPackageReference}/revisions/{PackageRevision}/files",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
        }
    }

    [Fact]
    public async Task DownloadPackageRevisionFileAsync_StreamsTheRawBody()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent([7])
        });
        PackagesConanRepository repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            using GitLabFileResponse file = await repository.DownloadPackageRevisionFileAsync(1, PackageName,
                PackageVersion, PackageUsername, PackageChannel, RecipeRevision, ConanPackageReference,
                PackageRevision, "conan_package.tgz", TestContext.Current.CancellationToken);

            Assert.Equal(
                $"https://gitlab.example/api/v4/projects/1/packages/conan/v2/conans/{RecipeSegment}/revisions/{RecipeRevision}/packages/{ConanPackageReference}/revisions/{PackageRevision}/files/conan_package.tgz",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
            _ = file;
        }
    }

    [Fact]
    public async Task AuthorizePackageRevisionFileUploadAsync_PutsToTheAuthorizeRoute()
    {
        using StubHttpMessageHandler handler = EmptyHandler();
        PackagesConanRepository repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            await repository.AuthorizePackageRevisionFileUploadAsync(1, PackageName, PackageVersion,
                PackageUsername, PackageChannel, RecipeRevision, ConanPackageReference, PackageRevision, FileName,
                TestContext.Current.CancellationToken);

            Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
            Assert.Equal(
                $"https://gitlab.example/api/v4/projects/1/packages/conan/v2/conans/{RecipeSegment}/revisions/{RecipeRevision}/packages/{ConanPackageReference}/revisions/{PackageRevision}/files/{FileName}/authorize",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
        }
    }

    [Fact]
    public async Task SearchPackageReferencesByRecipeRevisionAsync_HitsTheRevisionScopedSearchRoute()
    {
        using StubHttpMessageHandler handler = JsonHandler("{}");
        PackagesConanRepository repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            await repository.SearchPackageReferencesByRecipeRevisionAsync(1, PackageName, PackageVersion,
                PackageUsername, PackageChannel, RecipeRevision, TestContext.Current.CancellationToken);

            Assert.Equal(
                $"https://gitlab.example/api/v4/projects/1/packages/conan/v2/conans/{RecipeSegment}/revisions/{RecipeRevision}/search",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
        }
    }

    [Fact]
    public async Task SearchPackageReferencesForProjectV2Async_HitsTheUnpinnedSearchRoute()
    {
        using StubHttpMessageHandler handler = JsonHandler("{}");
        PackagesConanRepository repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            await repository.SearchPackageReferencesForProjectV2Async(1, PackageName, PackageVersion,
                PackageUsername, PackageChannel, TestContext.Current.CancellationToken);

            Assert.Equal(
                $"https://gitlab.example/api/v4/projects/1/packages/conan/v2/conans/{RecipeSegment}/search",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
        }
    }

    [Fact]
    public async Task AuthenticateForProjectV2Async_BuildsTheV2UsersRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("token", Encoding.UTF8, "text/plain")
        });
        PackagesConanRepository repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            using GitLabFileResponse token =
                await repository.AuthenticateForProjectV2Async(1, TestContext.Current.CancellationToken);

            Assert.Equal("https://gitlab.example/api/v4/projects/1/packages/conan/v2/users/authenticate",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
            _ = token;
        }
    }

    [Fact]
    public async Task CheckCredentialsForProjectV2Async_BuildsTheV2UsersRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("ok", Encoding.UTF8, "text/plain")
        });
        PackagesConanRepository repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            using GitLabFileResponse response =
                await repository.CheckCredentialsForProjectV2Async(1, TestContext.Current.CancellationToken);

            Assert.Equal("https://gitlab.example/api/v4/projects/1/packages/conan/v2/users/check_credentials",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
            _ = response;
        }
    }

    // ----- Escaping & error mapping -----

    [Fact]
    public async Task GetRecipeSnapshotForProjectAsync_EscapesSlashAndAtInCoordinates()
    {
        using StubHttpMessageHandler handler = JsonHandler("""{"recipe_snapshot":{}}""");
        PackagesConanRepository repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            await repository.GetRecipeSnapshotForProjectAsync(1, "zlib", "1.2.11", "conan/stable",
                "stable@1", TestContext.Current.CancellationToken);

            // '/' and '@' are both legal inside a Conan coordinate and both must stay percent-encoded so
            // each coordinate remains exactly one path segment.
            Assert.Equal(
                "https://gitlab.example/api/v4/projects/1/packages/conan/v1/conans/"
                + "zlib/1.2.11/conan%2Fstable/stable%401",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
        }
    }

    [Fact]
    public async Task GetRecipeSnapshotAsync_OnMissingRecipe_ThrowsGitLabNotFoundException()
    {
        const string Json = """{ "message": "404 Not found" }""";
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });
        PackagesConanRepository repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            GitLabApiException exception = await Assert.ThrowsAsync<GitLabNotFoundException>(() =>
                repository.GetRecipeSnapshotAsync(PackageName, PackageVersion, PackageUsername, PackageChannel,
                    TestContext.Current.CancellationToken));

            Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        }
    }

    [Fact]
    public async Task SearchAsync_OnBadRequest_ThrowsGitLabValidationException()
    {
        const string Json = """{ "message": "q is empty" }""";
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });
        PackagesConanRepository repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            GitLabValidationException exception = await Assert.ThrowsAsync<GitLabValidationException>(() =>
                repository.SearchAsync("*", cancellationToken: TestContext.Current.CancellationToken));

            Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);
        }
    }
}