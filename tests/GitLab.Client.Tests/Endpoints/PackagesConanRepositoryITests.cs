using System.Net;
using System.Net.Http.Headers;
using System.Text;

using GitLab.Client.Abstractions;
using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Endpoints;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Endpoints;

/// <summary>
///     Covers <c>PackagesConanClient.I.cs</c> - the "Packages: Conan" v2 package-revision file upload,
///     the one operation in this scope slice that was not already implemented by an earlier round (the
///     other five - delete/list/download/authorize a package revision and search its references - already
///     existed in <c>PackagesConanClient.V2.cs</c>; see that file's coverage in
///     <c>PackagesConanEndpointTests.cs</c>).
/// </summary>
public sealed class PackagesConanRepositoryITests
{
    // Same coordinates as PackagesConanEndpointTests: '+' proves package_username round-trips through
    // .Escaped(...), not .Literal(...).
    private const string PackageName = "hello";
    private const string PackageVersion = "1.0";
    private const string PackageUsername = "my-group+my-project";
    private const string PackageChannel = "stable";
    private const string ConanPackageReference = "103f6067a947f366ef91fc1b7da351c588d1827f";
    private const string RecipeRevision = "0";
    private const string PackageRevision = "0";

    private const string RecipeSegment = "hello/1.0/my-group%2Bmy-project/stable";

    private static readonly Uri BaseAddress = new("https://gitlab.example/api/v4/");

    private static PackagesConanClient CreateRepository(HttpMessageHandler handler, out HttpClient httpClient)
    {
        httpClient = new HttpClient(handler) { BaseAddress = BaseAddress };
        return new PackagesConanClient(new GitLabApiConnection(httpClient));
    }

    [Fact]
    public async Task UploadPackageRevisionFileAsync_PutsTheFileAsMultipart_ToTheFilesRoute()
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
            using MemoryStream content = new(Encoding.UTF8.GetBytes("binary-package-contents"));
            GitLabFileUpload upload = new()
            {
                Content = content, FileName = "conan_package.tgz", ContentType = "application/octet-stream"
            };

            await repository.UploadPackageRevisionFileAsync(1, PackageName, PackageVersion, PackageUsername,
                PackageChannel, RecipeRevision, ConanPackageReference, PackageRevision, "conan_package.tgz",
                upload, TestContext.Current.CancellationToken);

            Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
            Assert.Equal(
                $"https://gitlab.example/api/v4/projects/1/packages/conan/v2/conans/{RecipeSegment}/revisions/{RecipeRevision}/packages/{ConanPackageReference}/revisions/{PackageRevision}/files/conan_package.tgz",
                handler.LastRequest?.RequestUri?.AbsoluteUri);

            Assert.Equal("multipart/form-data", sentContentType?.MediaType);
            Assert.NotNull(sentBody);
            Assert.Contains("name=file", sentBody, StringComparison.Ordinal);
            Assert.Contains("conan_package.tgz", sentBody, StringComparison.Ordinal);
            Assert.Contains("binary-package-contents", sentBody, StringComparison.Ordinal);

            // The upload stream is borrowed, never owned, by the transport.
            Assert.True(content.CanRead);
        }
    }

    [Fact]
    public async Task UploadPackageRevisionFileAsync_EscapesASlashBearingFileName()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK));
        PackagesConanClient repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            using MemoryStream content = new([1, 2, 3]);
            GitLabFileUpload upload = new() { Content = content, FileName = "conaninfo.txt" };

            // Conan file names are caller-supplied free text; this proves the route used .Escaped(...) for
            // file_name rather than .Literal(...).
            await repository.UploadPackageRevisionFileAsync(1, PackageName, PackageVersion, PackageUsername,
                PackageChannel, RecipeRevision, ConanPackageReference, PackageRevision, "sub/conaninfo.txt",
                upload, TestContext.Current.CancellationToken);

            Assert.Equal(
                $"https://gitlab.example/api/v4/projects/1/packages/conan/v2/conans/{RecipeSegment}/revisions/{RecipeRevision}/packages/{ConanPackageReference}/revisions/{PackageRevision}/files/sub%2Fconaninfo.txt",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
        }
    }

    [Fact]
    public async Task UploadPackageRevisionFileAsync_ThrowsTheTypedException_OnAFailureStatus()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Conflict)
        {
            Content = new StringContent("""{"message":"already exists"}""", Encoding.UTF8, "application/json")
        });

        PackagesConanClient repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            using MemoryStream content = new([1, 2, 3]);
            GitLabFileUpload upload = new() { Content = content, FileName = "conan_package.tgz" };

            await Assert.ThrowsAsync<GitLabConflictException>(() =>
                repository.UploadPackageRevisionFileAsync(1, PackageName, PackageVersion, PackageUsername,
                    PackageChannel, RecipeRevision, ConanPackageReference, PackageRevision, "conan_package.tgz",
                    upload, TestContext.Current.CancellationToken));
        }
    }

    [Fact]
    public async Task UploadPackageRevisionFileAsync_ThrowsArgumentNullException_WhenFileIsNull()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK));
        PackagesConanClient repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                repository.UploadPackageRevisionFileAsync(1, PackageName, PackageVersion, PackageUsername,
                    PackageChannel, RecipeRevision, ConanPackageReference, PackageRevision, "conan_package.tgz",
                    null!, TestContext.Current.CancellationToken));
        }
    }
}