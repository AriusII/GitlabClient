using System.Net;
using System.Text;

using GitLab.Client.Abstractions;
using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Repositories;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Repositories;

public sealed class DependencyProxyRepositoryTests
{
    private static readonly Uri BaseAddress = new("https://gitlab.example/api/v4/");

    [Fact]
    public async Task PurgeCacheAsync_BuildsTheCacheRoute_AndAcceptsTheAsynchronousAnswer()
    {
        // GitLab queues the purge and answers 202 Accepted with no body.
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Accepted));

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        DependencyProxyRepository repository = new(connection);

        await repository.PurgeCacheAsync(5, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/5/dependency_proxy/cache",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task PurgeCacheAsync_EncodesTheNamespacedGroupPath()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Accepted));

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        DependencyProxyRepository repository = new(connection);

        await repository.PurgeCacheAsync("parent-group/subgroup", TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/groups/parent-group%2Fsubgroup/dependency_proxy/cache",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task PurgeCacheAsync_WithoutTheOwnerRole_ThrowsGitLabForbiddenException()
    {
        const string Json = """{ "message": "403 Forbidden" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Forbidden)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        DependencyProxyRepository repository = new(connection);

        GitLabForbiddenException exception = await Assert.ThrowsAsync<GitLabForbiddenException>(() =>
            repository.PurgeCacheAsync(5, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.Forbidden, exception.StatusCode);
    }

    [Fact]
    public async Task DownloadMavenPackageFileAsync_EscapesTheSlashBearingPath_AsOneSegment()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(Encoding.UTF8.GetBytes("POM-BYTES"))
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        DependencyProxyRepository repository = new(connection);

        using GitLabFileResponse file = await repository.DownloadMavenPackageFileAsync(42,
            "com/example/mylib/1.0", "mylib-1.0.pom", TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/projects/42/dependency_proxy/packages/maven/"
            + "com%2Fexample%2Fmylib%2F1.0/mylib-1.0.pom",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(HttpStatusCode.OK, file.StatusCode);
    }

    [Fact]
    public async Task DownloadMavenPackageFileAsync_EncodesTheNamespacedProjectPath()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(Encoding.UTF8.GetBytes("POM-BYTES"))
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        DependencyProxyRepository repository = new(connection);

        using GitLabFileResponse file = await repository.DownloadMavenPackageFileAsync("gitlab-org/gitlab",
            "com/example", "mylib-1.0.jar", TestContext.Current.CancellationToken);

        Assert.Equal(
            "https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/dependency_proxy/packages/maven/"
            + "com%2Fexample/mylib-1.0.jar",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.NotNull(file.Content);
    }

    [Fact]
    public async Task DownloadNpmPackageTarballAsync_EscapesAScopedPackageName_AsOneSegment()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(Encoding.UTF8.GetBytes("TARBALL-BYTES"))
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        DependencyProxyRepository repository = new(connection);

        using GitLabFileResponse file = await repository.DownloadNpmPackageTarballAsync(42, "@my-scope/my-package",
            "my-package-1.0.0.tgz", TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/projects/42/dependency_proxy/packages/npm/"
            + "%40my-scope%2Fmy-package/-/my-package-1.0.0.tgz",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(HttpStatusCode.OK, file.StatusCode);
    }

    [Fact]
    public async Task DownloadNpmPackageTarballAsync_OnCacheMiss_ThrowsGitLabNotFoundException()
    {
        const string Json = """{ "message": "404 Not found" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        DependencyProxyRepository repository = new(connection);

        GitLabNotFoundException exception = await Assert.ThrowsAsync<GitLabNotFoundException>(() =>
            repository.DownloadNpmPackageTarballAsync(42, "my-package", "my-package-1.0.0.tgz",
                TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
    }
}