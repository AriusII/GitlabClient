using System.Net;
using System.Net.Http.Headers;
using System.Text;

using GitLab.Client.Abstractions;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Repositories;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Repositories;

/// <summary>
///     Part B of the Conan resource's scope: the instance-wide recipe file upload
///     (<c>PUT .../files/.../export/:file_name</c>), the one operation the earlier pass of this resource
///     could not express (see <c>PackagesConanRepository.Files.cs</c>'s remarks) because it needed a
///     no-content multipart <c>PUT</c> the transport did not yet have. Every other operation assigned to
///     this scope (package download URLs, package/recipe upload-URL listings, package-reference search,
///     and downloading the recipe file) was already covered by <c>PackagesConanRepositoryTests</c>.
/// </summary>
public sealed class PackagesConanRepositoryBTests
{
    private const string PackageName = "hello";
    private const string PackageVersion = "1.0";
    private const string PackageUsername = "my-group+my-project";
    private const string PackageChannel = "stable";
    private const string RecipeRevision = "0";
    private const string FileName = "conanfile.py";

    private const string RecipeSegment = "hello/1.0/my-group%2Bmy-project/stable";

    private static readonly Uri BaseAddress = new("https://gitlab.example/api/v4/");

    private static PackagesConanRepository CreateRepository(HttpMessageHandler handler, out HttpClient httpClient)
    {
        httpClient = new HttpClient(handler) { BaseAddress = BaseAddress };
        return new PackagesConanRepository(new GitLabApiConnection(httpClient));
    }

    [Fact]
    public async Task UploadRecipeFileAsync_PutsTheFileAsMultipart_ToTheExportRoute()
    {
        MediaTypeHeaderValue? sentContentType = null;
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentContentType = request.Content?.Headers.ContentType;
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK);
        });
        PackagesConanRepository repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            using MemoryStream content = new(Encoding.UTF8.GetBytes("class HelloConan(ConanFile): pass"));
            GitLabFileUpload upload = new() { Content = content, FileName = FileName, ContentType = "text/x-python" };

            await repository.UploadRecipeFileAsync(PackageName, PackageVersion, PackageUsername, PackageChannel,
                RecipeRevision, FileName, upload, TestContext.Current.CancellationToken);

            Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
            Assert.Equal(
                $"https://gitlab.example/api/v4/packages/conan/v1/files/{RecipeSegment}/{RecipeRevision}/export/{FileName}",
                handler.LastRequest?.RequestUri?.AbsoluteUri);

            Assert.Equal("multipart/form-data", sentContentType?.MediaType);
            Assert.NotNull(sentBody);
            Assert.Contains("name=file", sentBody, StringComparison.Ordinal);
            Assert.Contains(FileName, sentBody, StringComparison.Ordinal);
            Assert.Contains("class HelloConan(ConanFile): pass", sentBody, StringComparison.Ordinal);

            // The stream is borrowed, never owned: it must still be usable after the call returns.
            Assert.True(content.CanRead);
        }
    }

    [Fact]
    public async Task UploadRecipeFileAsync_EscapesEveryCoordinate()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK));
        PackagesConanRepository repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            using MemoryStream content = new([1, 2, 3]);
            GitLabFileUpload upload = new() { Content = content, FileName = "conanmanifest.txt" };

            await repository.UploadRecipeFileAsync("zlib", "1.2.11", "conan/stable", "stable@1", "0/build",
                "conanmanifest.txt", upload, TestContext.Current.CancellationToken);

            // '/' and '@' are both legal inside a Conan coordinate or revision and must stay percent-encoded
            // so each one remains exactly one path segment - proof that .Escaped(...), not .Literal(...),
            // built every one of them.
            Assert.Equal(
                "https://gitlab.example/api/v4/packages/conan/v1/files/"
                + "zlib/1.2.11/conan%2Fstable/stable%401/0%2Fbuild/export/conanmanifest.txt",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
        }
    }

    [Fact]
    public async Task UploadRecipeFileAsync_ThrowsOnNullFile()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK));
        PackagesConanRepository repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                repository.UploadRecipeFileAsync(PackageName, PackageVersion, PackageUsername, PackageChannel,
                    RecipeRevision, FileName, null!, TestContext.Current.CancellationToken));
        }
    }
}