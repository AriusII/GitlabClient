using System.Net;
using System.Net.Http.Headers;
using System.Text;

using GitLab.Client.Abstractions;
using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Repositories;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Repositories;

/// <summary>
///     Part D coverage for <see cref="PackagesDebianRepository" />: <c>UploadPackageFileAsync</c>, the
///     actual <c>PUT /projects/:id/packages/debian/:file_name</c> upload GitLab answers with no body -
///     the paired operation to <c>AuthorizePackageUploadAsync</c>, which the sibling
///     <see cref="PackagesDebianRepositoryTests" /> already covers.
/// </summary>
public sealed class PackagesDebianRepositoryDTests
{
    private static readonly Uri BaseAddress = new("https://gitlab.example/api/v4/");

    private static readonly byte[] FileBytes = [0x21, 0x3C, 0x61, 0x72, 0x63, 0x68, 0x3E];

    private static (HttpClient httpClient, StubHttpMessageHandler handler) CreateClient(
        Func<HttpRequestMessage, HttpResponseMessage> respond)
    {
        StubHttpMessageHandler handler = new(respond);
        HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        return (httpClient, handler);
    }

    [Fact]
    public async Task UploadPackageFileAsync_SendsAMultipartPutToTheFileRoute()
    {
        MediaTypeHeaderValue? sentContentType = null;
        string? sentBody = null;

        (HttpClient httpClient, StubHttpMessageHandler handler) = CreateClient(request =>
        {
            sentContentType = request.Content?.Headers.ContentType;
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();

            return new HttpResponseMessage(HttpStatusCode.Created);
        });

        GitLabApiConnection connection = new(httpClient);
        PackagesDebianRepository repository = new(connection);

        using MemoryStream content = new(FileBytes);
        GitLabFileUpload file = new() { Content = content, FileName = "mypkg_1.0.0_amd64.deb" };

        await repository.UploadPackageFileAsync(1, "mypkg_1.0.0_amd64.deb", file,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/packages/debian/mypkg_1.0.0_amd64.deb",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("multipart/form-data", sentContentType?.MediaType);
        Assert.NotNull(sentBody);

        string unquoted = sentBody.Replace("\"", string.Empty, StringComparison.Ordinal);
        Assert.Contains("name=file", unquoted, StringComparison.Ordinal);
        Assert.Contains("mypkg_1.0.0_amd64.deb", unquoted, StringComparison.Ordinal);
    }

    [Fact]
    public async Task UploadPackageFileAsync_EscapesAFileNameWithAReservedCharacter()
    {
        (HttpClient httpClient, StubHttpMessageHandler handler) =
            CreateClient(_ => new HttpResponseMessage(HttpStatusCode.Created));

        GitLabApiConnection connection = new(httpClient);
        PackagesDebianRepository repository = new(connection);

        using MemoryStream content = new(FileBytes);
        GitLabFileUpload file = new() { Content = content, FileName = "mypkg_1.0.0+dfsg-1_amd64.deb" };

        await repository.UploadPackageFileAsync(9, "mypkg_1.0.0+dfsg-1_amd64.deb", file,
            TestContext.Current.CancellationToken);

        Assert.Equal(
            "https://gitlab.example/api/v4/projects/9/packages/debian/mypkg_1.0.0%2Bdfsg-1_amd64.deb",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task UploadPackageFileAsync_ThrowsWhenFileIsNull()
    {
        (HttpClient httpClient, StubHttpMessageHandler _) =
            CreateClient(_ => new HttpResponseMessage(HttpStatusCode.Created));

        GitLabApiConnection connection = new(httpClient);
        PackagesDebianRepository repository = new(connection);

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            repository.UploadPackageFileAsync(1, "mypkg_1.0.0_amd64.deb", null!,
                TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task UploadPackageFileAsync_MapsA400ToTheTypedValidationException()
    {
        (HttpClient httpClient, StubHttpMessageHandler _) = CreateClient(_ =>
            new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent("""{"message":"Bad Request"}""", Encoding.UTF8, "application/json")
            });

        GitLabApiConnection connection = new(httpClient);
        PackagesDebianRepository repository = new(connection);

        using MemoryStream content = new(FileBytes);
        GitLabFileUpload file = new() { Content = content, FileName = "mypkg_1.0.0_amd64.deb" };

        GitLabValidationException exception = await Assert.ThrowsAsync<GitLabValidationException>(() =>
            repository.UploadPackageFileAsync(1, "mypkg_1.0.0_amd64.deb", file,
                TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);
    }
}