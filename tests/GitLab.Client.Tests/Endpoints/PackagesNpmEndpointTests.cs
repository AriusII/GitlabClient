using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

using GitLab.Client.Abstractions;
using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Domain;
using GitLab.Client.Endpoints;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Endpoints;

public sealed class PackagesNpmEndpointTests
{
    private const string PackageJson = """
                                       {
                                         "name": "@scope/my-package",
                                         "versions": {
                                           "1.0.0": {
                                             "name": "@scope/my-package",
                                             "version": "1.0.0",
                                             "dist": {
                                               "shasum": "abc123",
                                               "tarball": "https://gitlab.example/api/v4/projects/1/packages/npm/@scope%2fmy-package/-/my-package-1.0.0.tgz"
                                             }
                                           }
                                         },
                                         "dist-tags": {
                                           "latest": "1.0.0"
                                         }
                                       }
                                       """;

    private const string DistTagsJson = """
                                        {
                                          "dist_tags": {
                                            "latest": "1.0.1",
                                            "beta": "1.1.0-beta.1"
                                          }
                                        }
                                        """;

    private static readonly Uri BaseAddress = new("https://gitlab.example/api/v4/");

    private static (HttpClient client, StubHttpMessageHandler handler) CreateClient(
        Func<HttpRequestMessage, HttpResponseMessage> respond)
    {
        StubHttpMessageHandler handler = new(respond);
        HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        return (httpClient, handler);
    }

    [Fact]
    public async Task GetPackageForProjectAsync_EncodesAScopedPackageNameAsOneSegment()
    {
        (HttpClient httpClient, StubHttpMessageHandler handler) = CreateClient(_ => new HttpResponseMessage(
            HttpStatusCode.OK) { Content = new StringContent(PackageJson, Encoding.UTF8, "application/json") });

        using HttpClient client = httpClient;
        GitLabApiConnection connection = new(client);
        PackagesNpmClient repository = new(connection);

        await repository.GetPackageForProjectAsync(7, "@scope/my-package", TestContext.Current.CancellationToken);

        // "@scope/my-package" contains a literal '/' that npm itself folds into one path segment by
        // percent-encoding it before ever sending the request - GitLabRouteBuilder.Escaped must do the
        // same thing here rather than treating it as two path segments.
        Assert.Equal("https://gitlab.example/api/v4/projects/7/packages/npm/%40scope%2Fmy-package",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task GetPackageForProjectAsync_DeserializesNameVersionsAndDistTags()
    {
        (HttpClient httpClient, _) = CreateClient(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(PackageJson, Encoding.UTF8, "application/json")
        });

        using HttpClient client = httpClient;
        GitLabApiConnection connection = new(client);
        PackagesNpmClient repository = new(connection);

        GitLabNpmPackage package =
            await repository.GetPackageForProjectAsync(7, "@scope/my-package", TestContext.Current.CancellationToken);

        Assert.Equal("@scope/my-package", package.Name);

        Assert.NotNull(package.Versions);
        JsonElement version = Assert.Single(package.Versions!).Value;
        Assert.Equal("1.0.0", version.GetProperty("version").GetString());
        Assert.Equal("abc123", version.GetProperty("dist").GetProperty("shasum").GetString());

        Assert.NotNull(package.DistTags);
        Assert.Equal("1.0.0", package.DistTags!["latest"]);
    }

    [Fact]
    public async Task GetPackageForProjectAsync_EncodesANamespacedProjectPath()
    {
        (HttpClient httpClient, StubHttpMessageHandler handler) = CreateClient(_ => new HttpResponseMessage(
            HttpStatusCode.OK) { Content = new StringContent(PackageJson, Encoding.UTF8, "application/json") });

        using HttpClient client = httpClient;
        GitLabApiConnection connection = new(client);
        PackagesNpmClient repository = new(connection);

        await repository.GetPackageForProjectAsync(ProjectId.FromPath("group/subgroup/project"), "my-package",
            TestContext.Current.CancellationToken);

        Assert.Equal(
            "https://gitlab.example/api/v4/projects/group%2Fsubgroup%2Fproject/packages/npm/my-package",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task GetDistTagsForProjectAsync_BuildsTheDashPackageDistTagsRoute_AndDeserializes()
    {
        (HttpClient httpClient, StubHttpMessageHandler handler) = CreateClient(_ => new HttpResponseMessage(
            HttpStatusCode.OK) { Content = new StringContent(DistTagsJson, Encoding.UTF8, "application/json") });

        using HttpClient client = httpClient;
        GitLabApiConnection connection = new(client);
        PackagesNpmClient repository = new(connection);

        GitLabNpmDistTags tags = await repository.GetDistTagsForProjectAsync(7, "my-package",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/projects/7/packages/npm/-/package/my-package/dist-tags",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        Assert.NotNull(tags.DistTags);
        Assert.Equal("1.0.1", tags.DistTags!["latest"]);
        Assert.Equal("1.1.0-beta.1", tags.DistTags["beta"]);
    }

    [Fact]
    public async Task SetDistTagForProjectAsync_PutsToTheTagRoute_WithNoBody()
    {
        (HttpClient httpClient, StubHttpMessageHandler handler) = CreateClient(_ =>
            new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient client = httpClient;
        GitLabApiConnection connection = new(client);
        PackagesNpmClient repository = new(connection);

        await repository.SetDistTagForProjectAsync(7, "my-package", "beta", TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/projects/7/packages/npm/-/package/my-package/dist-tags/beta",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task DeleteDistTagForProjectAsync_DeletesTheTagRoute()
    {
        (HttpClient httpClient, StubHttpMessageHandler handler) = CreateClient(_ =>
            new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient client = httpClient;
        GitLabApiConnection connection = new(client);
        PackagesNpmClient repository = new(connection);

        await repository.DeleteDistTagForProjectAsync(7, "my-package", "beta",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/projects/7/packages/npm/-/package/my-package/dist-tags/beta",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task PublishForProjectAsync_PutsMultipartFormData_AndReturnsTheRawResponse()
    {
        string? sentBody = null;
        MediaTypeHeaderValue? sentContentType = null;

        (HttpClient httpClient, StubHttpMessageHandler handler) = CreateClient(request =>
        {
            sentContentType = request.Content?.Headers.ContentType;
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"success":true}""", Encoding.UTF8, "application/json")
            };
        });

        using HttpClient client = httpClient;
        GitLabApiConnection connection = new(client);
        PackagesNpmClient repository = new(connection);

        using MemoryStream content = new([1, 2, 3, 4]);
        GitLabFileUpload upload = new()
        {
            Content = content, FileName = "my-package-1.0.0.tgz", ContentType = "application/octet-stream"
        };

        JsonElement result = await repository.PublishForProjectAsync(7, "@scope/my-package", upload,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/7/packages/npm/%40scope%2Fmy-package",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        Assert.Equal("multipart/form-data", sentContentType?.MediaType);
        Assert.NotNull(sentBody);
        Assert.Contains("name=file", sentBody, StringComparison.Ordinal);
        Assert.Contains("my-package-1.0.0.tgz", sentBody, StringComparison.Ordinal);

        Assert.True(result.GetProperty("success").GetBoolean());

        // The stream is borrowed, never owned: it must still be usable after the call returns.
        Assert.True(content.CanRead);
    }

    [Fact]
    public async Task DownloadTarballForProjectAsync_StreamsTheRawTarballBytes()
    {
        byte[] tarballBytes = [0x1F, 0x8B, 0x08, 0x00];

        (HttpClient httpClient, StubHttpMessageHandler handler) = CreateClient(_ =>
        {
            HttpResponseMessage response = new(HttpStatusCode.OK) { Content = new ByteArrayContent(tarballBytes) };
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            return response;
        });

        using HttpClient client = httpClient;
        GitLabApiConnection connection = new(client);
        PackagesNpmClient repository = new(connection);

        using GitLabFileResponse file = await repository.DownloadTarballForProjectAsync(7, "@scope/my-package",
            "my-package-1.0.0.tgz", TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/projects/7/packages/npm/%40scope%2Fmy-package/-/my-package-1.0.0.tgz",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        using MemoryStream copy = new();
        await file.Content.CopyToAsync(copy, TestContext.Current.CancellationToken);
        Assert.Equal(tarballBytes, copy.ToArray());
    }

    [Fact]
    public async Task BulkAdvisoriesForProjectAsync_PostsToTheAdvisoriesBulkRoute_WithNoBody()
    {
        (HttpClient httpClient, StubHttpMessageHandler handler) = CreateClient(_ =>
            new HttpResponseMessage(HttpStatusCode.OK));

        using HttpClient client = httpClient;
        GitLabApiConnection connection = new(client);
        PackagesNpmClient repository = new(connection);

        await repository.BulkAdvisoriesForProjectAsync(7, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/projects/7/packages/npm/-/npm/v1/security/advisories/bulk",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task QuickAuditForProjectAsync_PostsToTheAuditsQuickRoute_WithNoBody()
    {
        (HttpClient httpClient, StubHttpMessageHandler handler) = CreateClient(_ =>
            new HttpResponseMessage(HttpStatusCode.OK));

        using HttpClient client = httpClient;
        GitLabApiConnection connection = new(client);
        PackagesNpmClient repository = new(connection);

        await repository.QuickAuditForProjectAsync(7, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/projects/7/packages/npm/-/npm/v1/security/audits/quick",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task GetPackageForGroupAsync_BuildsTheDashPackagesRoute()
    {
        (HttpClient httpClient, StubHttpMessageHandler handler) = CreateClient(_ => new HttpResponseMessage(
            HttpStatusCode.OK) { Content = new StringContent(PackageJson, Encoding.UTF8, "application/json") });

        using HttpClient client = httpClient;
        GitLabApiConnection connection = new(client);
        PackagesNpmClient repository = new(connection);

        await repository.GetPackageForGroupAsync(42, "@scope/my-package", TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/groups/42/-/packages/npm/%40scope%2Fmy-package",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task GetDistTagsForGroupAsync_BuildsTheGroupDistTagsRoute_AndDeserializes()
    {
        (HttpClient httpClient, StubHttpMessageHandler handler) = CreateClient(_ => new HttpResponseMessage(
            HttpStatusCode.OK) { Content = new StringContent(DistTagsJson, Encoding.UTF8, "application/json") });

        using HttpClient client = httpClient;
        GitLabApiConnection connection = new(client);
        PackagesNpmClient repository = new(connection);

        GitLabNpmDistTags tags = await repository.GetDistTagsForGroupAsync(42, "my-package",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/groups/42/-/packages/npm/-/package/my-package/dist-tags",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        Assert.NotNull(tags.DistTags);
        Assert.Equal("1.0.1", tags.DistTags!["latest"]);
    }

    [Fact]
    public async Task SetDistTagForGroupAsync_PutsToTheGroupTagRoute_WithNoBody()
    {
        (HttpClient httpClient, StubHttpMessageHandler handler) = CreateClient(_ =>
            new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient client = httpClient;
        GitLabApiConnection connection = new(client);
        PackagesNpmClient repository = new(connection);

        await repository.SetDistTagForGroupAsync(42, "my-package", "beta", TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/groups/42/-/packages/npm/-/package/my-package/dist-tags/beta",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task DeleteDistTagForGroupAsync_BuildsTheGroupDistTagsRoute()
    {
        (HttpClient httpClient, StubHttpMessageHandler handler) = CreateClient(_ =>
            new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient client = httpClient;
        GitLabApiConnection connection = new(client);
        PackagesNpmClient repository = new(connection);

        await repository.DeleteDistTagForGroupAsync(42, "my-package", "beta",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/groups/42/-/packages/npm/-/package/my-package/dist-tags/beta",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task BulkAdvisoriesForGroupAsync_PostsToTheGroupSecurityRoute()
    {
        (HttpClient httpClient, StubHttpMessageHandler handler) = CreateClient(_ =>
            new HttpResponseMessage(HttpStatusCode.OK));

        using HttpClient client = httpClient;
        GitLabApiConnection connection = new(client);
        PackagesNpmClient repository = new(connection);

        await repository.BulkAdvisoriesForGroupAsync(42, TestContext.Current.CancellationToken);

        Assert.Equal(
            "https://gitlab.example/api/v4/groups/42/-/packages/npm/-/npm/v1/security/advisories/bulk",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task QuickAuditForGroupAsync_PostsToTheGroupSecurityRoute()
    {
        (HttpClient httpClient, StubHttpMessageHandler handler) = CreateClient(_ =>
            new HttpResponseMessage(HttpStatusCode.OK));

        using HttpClient client = httpClient;
        GitLabApiConnection connection = new(client);
        PackagesNpmClient repository = new(connection);

        await repository.QuickAuditForGroupAsync(42, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/groups/42/-/packages/npm/-/npm/v1/security/audits/quick",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task GetPackageAsync_BuildsTheInstanceScopedRoute_WithNoIdSegment()
    {
        (HttpClient httpClient, StubHttpMessageHandler handler) = CreateClient(_ => new HttpResponseMessage(
            HttpStatusCode.OK) { Content = new StringContent(PackageJson, Encoding.UTF8, "application/json") });

        using HttpClient client = httpClient;
        GitLabApiConnection connection = new(client);
        PackagesNpmClient repository = new(connection);

        await repository.GetPackageAsync("@scope/my-package", TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/packages/npm/%40scope%2Fmy-package",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task GetDistTagsAsync_BuildsTheInstanceScopedDistTagsRoute()
    {
        (HttpClient httpClient, StubHttpMessageHandler handler) = CreateClient(_ => new HttpResponseMessage(
            HttpStatusCode.OK) { Content = new StringContent(DistTagsJson, Encoding.UTF8, "application/json") });

        using HttpClient client = httpClient;
        GitLabApiConnection connection = new(client);
        PackagesNpmClient repository = new(connection);

        await repository.GetDistTagsAsync("my-package", TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/packages/npm/-/package/my-package/dist-tags",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task SetDistTagAsync_PutsToTheInstanceScopedTagRoute()
    {
        (HttpClient httpClient, StubHttpMessageHandler handler) = CreateClient(_ =>
            new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient client = httpClient;
        GitLabApiConnection connection = new(client);
        PackagesNpmClient repository = new(connection);

        await repository.SetDistTagAsync("my-package", "latest", TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/packages/npm/-/package/my-package/dist-tags/latest",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task DeleteDistTagAsync_DeletesTheInstanceScopedTagRoute()
    {
        (HttpClient httpClient, StubHttpMessageHandler handler) = CreateClient(_ =>
            new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient client = httpClient;
        GitLabApiConnection connection = new(client);
        PackagesNpmClient repository = new(connection);

        await repository.DeleteDistTagAsync("my-package", "beta", TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/packages/npm/-/package/my-package/dist-tags/beta",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task BulkAdvisoriesAsync_PostsToTheInstanceScopedSecurityRoute_WithNoBody()
    {
        (HttpClient httpClient, StubHttpMessageHandler handler) = CreateClient(_ =>
            new HttpResponseMessage(HttpStatusCode.OK));

        using HttpClient client = httpClient;
        GitLabApiConnection connection = new(client);
        PackagesNpmClient repository = new(connection);

        await repository.BulkAdvisoriesAsync(TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/packages/npm/-/npm/v1/security/advisories/bulk",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task QuickAuditAsync_PostsToTheInstanceScopedSecurityRoute()
    {
        (HttpClient httpClient, StubHttpMessageHandler handler) = CreateClient(_ =>
            new HttpResponseMessage(HttpStatusCode.OK));

        using HttpClient client = httpClient;
        GitLabApiConnection connection = new(client);
        PackagesNpmClient repository = new(connection);

        await repository.QuickAuditAsync(TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/packages/npm/-/npm/v1/security/audits/quick",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task GetPackageForProjectAsync_MapsA404ToTheTypedNotFoundException()
    {
        (HttpClient httpClient, _) = CreateClient(_ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent("""{"message":"404 Package Not Found"}""", Encoding.UTF8,
                "application/json")
        });

        using HttpClient client = httpClient;
        GitLabApiConnection connection = new(client);
        PackagesNpmClient repository = new(connection);

        GitLabNotFoundException exception = await Assert.ThrowsAsync<GitLabNotFoundException>(() =>
            repository.GetPackageForProjectAsync(7, "missing-package",
                TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
    }
}