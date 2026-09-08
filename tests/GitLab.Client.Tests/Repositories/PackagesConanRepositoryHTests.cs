using System.Net;
using System.Net.Http.Headers;

using GitLab.Client.Abstractions;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Repositories;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Repositories;

/// <summary>
///     Covers the recipe-revision file upload added in part H of the Conan scope split - the one
///     operation from that brief not already implemented by an earlier round. See
///     <c>PackagesConanRepositoryTests.cs</c> for the rest of the resource's coverage.
/// </summary>
public sealed class PackagesConanRepositoryHTests
{
    private const string PackageName = "hello";
    private const string PackageVersion = "1.0";
    private const string PackageUsername = "my-group+my-project";
    private const string PackageChannel = "stable";
    private const string RecipeRevision = "0";

    private const string RecipeSegment = "hello/1.0/my-group%2Bmy-project/stable";

    private static readonly Uri BaseAddress = new("https://gitlab.example/api/v4/");

    private static readonly byte[] RecipeFileBytes = "print('conanfile')"u8.ToArray();

    private static PackagesConanRepository CreateRepository(HttpMessageHandler handler, out HttpClient httpClient)
    {
        httpClient = new HttpClient(handler) { BaseAddress = BaseAddress };
        return new PackagesConanRepository(new GitLabApiConnection(httpClient));
    }

    [Fact]
    public async Task UploadRecipeRevisionFileAsync_PutsMultipartFormData_ToTheFilesRoute()
    {
        string? sentBody = null;
        MediaTypeHeaderValue? sentContentType = null;

        using StubHttpMessageHandler handler = new(request =>
        {
            sentContentType = request.Content?.Headers.ContentType;
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK);
        });

        PackagesConanRepository repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            using MemoryStream content = new(RecipeFileBytes);
            GitLabFileUpload file = new() { Content = content, FileName = "conanfile.py" };

            await repository.UploadRecipeRevisionFileAsync(1, PackageName, PackageVersion, PackageUsername,
                PackageChannel, RecipeRevision, "conanfile.py", file, TestContext.Current.CancellationToken);

            Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
            Assert.Equal(
                $"https://gitlab.example/api/v4/projects/1/packages/conan/v2/conans/{RecipeSegment}/revisions/{RecipeRevision}/files/conanfile.py",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
            Assert.Equal("multipart/form-data", sentContentType?.MediaType);
            Assert.NotNull(sentBody);
            Assert.Contains("name=file", sentBody, StringComparison.Ordinal);

            // The stream is borrowed, never owned: it must still be usable after the call returns.
            Assert.True(content.CanRead);
        }
    }

    [Fact]
    public async Task UploadRecipeRevisionFileAsync_EscapesTheRevisionAndFileName()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK));
        PackagesConanRepository repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            using MemoryStream content = new(RecipeFileBytes);
            GitLabFileUpload file = new() { Content = content, FileName = "conanmanifest.txt" };

            await repository.UploadRecipeRevisionFileAsync(1, PackageName, PackageVersion, PackageUsername,
                PackageChannel, "0/build", "conan manifest.txt", file, TestContext.Current.CancellationToken);

            Assert.Equal(
                $"https://gitlab.example/api/v4/projects/1/packages/conan/v2/conans/{RecipeSegment}/revisions/0%2Fbuild/files/conan%20manifest.txt",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
        }
    }

    [Fact]
    public async Task UploadRecipeRevisionFileAsync_ThrowsOnNullFile()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK));
        PackagesConanRepository repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => repository.UploadRecipeRevisionFileAsync(1,
                PackageName, PackageVersion, PackageUsername, PackageChannel, RecipeRevision, "conanfile.py",
                null!, TestContext.Current.CancellationToken));
        }
    }
}