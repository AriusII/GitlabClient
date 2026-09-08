using System.Net;
using System.Net.Http.Headers;

using GitLab.Client.Abstractions;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Repositories;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Repositories;

/// <summary>
///     Part F: the two Conan recipe/package file upload operations added in
///     <c>PackagesConanRepository.F.cs</c> - the multipart <c>PUT</c> endpoints
///     <c>PackagesConanRepository.Files.cs</c> originally left unimplemented before the transport grew a
///     no-content <see cref="IGitLabApiConnection.PutFileAsync" /> overload. Coordinates mirror
///     <see cref="PackagesConanRepositoryTests" /> exactly, including the '+' in
///     <c>package_username</c> that proves <c>.Escaped(...)</c> built the route.
/// </summary>
public sealed class PackagesConanRepositoryFTests
{
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

    private static readonly byte[] FileBytes = [0xDE, 0xAD, 0xBE, 0xEF];

    private static PackagesConanRepository CreateRepository(HttpMessageHandler handler, out HttpClient httpClient)
    {
        httpClient = new HttpClient(handler) { BaseAddress = BaseAddress };
        return new PackagesConanRepository(new GitLabApiConnection(httpClient));
    }

    [Fact]
    public async Task UploadRecipeFileForProjectAsync_PutsMultipartFormData_ToTheExportRoute()
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
        using (MemoryStream content = new(FileBytes))
        {
            GitLabFileUpload upload = new() { Content = content, FileName = FileName };

            await repository.UploadRecipeFileForProjectAsync(1, PackageName, PackageVersion, PackageUsername,
                PackageChannel, RecipeRevision, FileName, upload, TestContext.Current.CancellationToken);

            Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
            Assert.Equal(
                $"https://gitlab.example/api/v4/projects/1/packages/conan/v1/files/{RecipeSegment}/{RecipeRevision}/export/{FileName}",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
            Assert.Equal("multipart/form-data", sentContentType?.MediaType);
            Assert.NotNull(sentBody);
            Assert.Contains("name=file", sentBody, StringComparison.Ordinal);

            // The stream is borrowed, never owned: it must still be usable after the call returns.
            Assert.True(content.CanRead);
        }
    }

    [Fact]
    public async Task UploadPackageFileForProjectAsync_PutsMultipartFormData_ToThePackageRoute()
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
        using (MemoryStream content = new(FileBytes))
        {
            GitLabFileUpload upload = new() { Content = content, FileName = FileName };

            await repository.UploadPackageFileForProjectAsync(1, PackageName, PackageVersion, PackageUsername,
                PackageChannel, RecipeRevision, ConanPackageReference, PackageRevision, FileName, upload,
                TestContext.Current.CancellationToken);

            Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
            Assert.Equal(
                $"https://gitlab.example/api/v4/projects/1/packages/conan/v1/files/{RecipeSegment}/{RecipeRevision}/package/{ConanPackageReference}/{PackageRevision}/{FileName}",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
            Assert.Equal("multipart/form-data", sentContentType?.MediaType);
            Assert.NotNull(sentBody);
            Assert.Contains("name=file", sentBody, StringComparison.Ordinal);

            // The stream is borrowed, never owned: it must still be usable after the call returns.
            Assert.True(content.CanRead);
        }
    }

    [Fact]
    public async Task UploadRecipeFileForProjectAsync_ThrowsArgumentNullException_WhenFileIsNull()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK));
        PackagesConanRepository repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                repository.UploadRecipeFileForProjectAsync(1, PackageName, PackageVersion, PackageUsername,
                    PackageChannel, RecipeRevision, FileName, null!, TestContext.Current.CancellationToken));
        }
    }

    [Fact]
    public async Task UploadPackageFileForProjectAsync_ThrowsArgumentNullException_WhenFileIsNull()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK));
        PackagesConanRepository repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                repository.UploadPackageFileForProjectAsync(1, PackageName, PackageVersion, PackageUsername,
                    PackageChannel, RecipeRevision, ConanPackageReference, PackageRevision, FileName, null!,
                    TestContext.Current.CancellationToken));
        }
    }
}