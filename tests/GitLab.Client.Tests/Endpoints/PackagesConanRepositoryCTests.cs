using System.Net;
using System.Net.Http.Headers;

using GitLab.Client.Abstractions;
using GitLab.Client.Endpoints;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Endpoints;

/// <summary>
///     Part C: the instance-wide Conan package file upload
///     (<see cref="PackagesConanClient.UploadPackageFileAsync" />) - the one operation from this
///     scope's brief that was not already covered by an earlier part of the <c>Packages: Conan</c> tag.
/// </summary>
public sealed class PackagesConanRepositoryCTests
{
    private const string PackageName = "hello";
    private const string PackageVersion = "1.0";
    private const string PackageUsername = "my-group+my-project";
    private const string PackageChannel = "stable";
    private const string ConanPackageReference = "103f6067a947f366ef91fc1b7da351c588d1827f";
    private const string RecipeRevision = "0";
    private const string PackageRevision = "0";
    private const string FileName = "conan_package.tgz";

    private const string RecipeSegment = "hello/1.0/my-group%2Bmy-project/stable";

    private static readonly Uri BaseAddress = new("https://gitlab.example/api/v4/");

    private static readonly byte[] PackageBytes = [0x1F, 0x8B, 0x08, 0x00];

    private static PackagesConanClient CreateRepository(HttpMessageHandler handler, out HttpClient httpClient)
    {
        httpClient = new HttpClient(handler) { BaseAddress = BaseAddress };
        return new PackagesConanClient(new GitLabApiConnection(httpClient));
    }

    [Fact]
    public async Task UploadPackageFileAsync_PutsMultipartFormData_ToTheInstanceWidePackageFileRoute()
    {
        string? sentBody = null;
        MediaTypeHeaderValue? sentContentType = null;

        using StubHttpMessageHandler handler = new(request =>
        {
            sentContentType = request.Content?.Headers.ContentType;
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK);
        });
        PackagesConanClient repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            using MemoryStream content = new(PackageBytes);
            GitLabFileUpload file = new() { Content = content, FileName = FileName };

            await repository.UploadPackageFileAsync(PackageName, PackageVersion, PackageUsername, PackageChannel,
                RecipeRevision, ConanPackageReference, PackageRevision, FileName, file,
                TestContext.Current.CancellationToken);

            Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
            Assert.Equal(
                $"https://gitlab.example/api/v4/packages/conan/v1/files/{RecipeSegment}/{RecipeRevision}/package/{ConanPackageReference}/{PackageRevision}/{FileName}",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
            Assert.Equal("multipart/form-data", sentContentType?.MediaType);
            Assert.NotNull(sentBody);
            Assert.Contains("name=file", sentBody, StringComparison.Ordinal);

            // The stream is borrowed, never owned: it must still be usable after the call returns.
            Assert.True(content.CanRead);
        }
    }

    [Fact]
    public async Task UploadPackageFileAsync_EscapesEveryCoordinate()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK));
        PackagesConanClient repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            using MemoryStream content = new(PackageBytes);
            GitLabFileUpload file = new() { Content = content, FileName = FileName };

            await repository.UploadPackageFileAsync("weird name", "1.0+rc1", "user/with@chars", "chan nel",
                "rev/1", "ref@1", "prev/1", "file name.tgz", file, TestContext.Current.CancellationToken);

            string? path = handler.LastRequest?.RequestUri?.AbsolutePath;
            Assert.NotNull(path);
            Assert.Contains("weird%20name", path, StringComparison.Ordinal);
            Assert.Contains("user%2Fwith%40chars", path, StringComparison.Ordinal);
            Assert.Contains("chan%20nel", path, StringComparison.Ordinal);
            Assert.Contains("file%20name.tgz", path, StringComparison.Ordinal);
        }
    }

    [Fact]
    public async Task UploadPackageFileAsync_ThrowsOnNullFile()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK));
        PackagesConanClient repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => repository.UploadPackageFileAsync(PackageName,
                PackageVersion, PackageUsername, PackageChannel, RecipeRevision, ConanPackageReference,
                PackageRevision, FileName, null!, TestContext.Current.CancellationToken));
        }
    }
}