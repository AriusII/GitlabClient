using System.Net;
using System.Net.Http.Headers;
using System.Text;

using GitLab.Client.Abstractions;
using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Endpoints;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;
using GitLab.Client.Query;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Endpoints;

public sealed class PackagesGenericEndpointTests
{
    private const string PackageFileJson = """
                                           {
                                             "id": 25,
                                             "package_id": 4,
                                             "created_at": "2018-11-07T15:25:52.199Z",
                                             "file_name": "my-app-1.5-20181107.152550-1.jar",
                                             "size": 2421,
                                             "file_md5": "58e6a45a629910c6ff99145a688971ac",
                                             "file_sha1": "ebd193463d3915d7e22219f52740056dfd26cbfe",
                                             "file_sha256": "a903393463d3915d7e22219f52740056dfd26cbfeff321b",
                                             "pipelines": {
                                               "id": 1,
                                               "iid": 2,
                                               "project_id": 3,
                                               "sha": "0ec9e58fdfca6cdd6652c083c9edb53abc0bad52",
                                               "ref": "feature-branch",
                                               "status": "success",
                                               "source": "push",
                                               "created_at": "2022-10-21T16:49:48.000+02:00",
                                               "updated_at": "2022-10-21T16:49:48.000+02:00",
                                               "web_url": "https://gitlab.example.com/gitlab-org/gitlab-foss/-/pipelines/61",
                                               "user": {
                                                 "id": 1,
                                                 "username": "admin",
                                                 "name": "Administrator",
                                                 "web_url": "https://gitlab.example.com/root"
                                               }
                                             }
                                           }
                                           """;

    private const string PackageJson = """
                                       {
                                         "id": 1,
                                         "name": "com/mycompany/my-app",
                                         "version": "1.0-SNAPSHOT",
                                         "package_type": "maven",
                                         "status": "default",
                                         "_links": {
                                           "web_path": "/root/my-company/app-project/-/packages/1",
                                           "delete_api_path": "/api/v4/projects/1/packages/1"
                                         },
                                         "created_at": "2019-11-27T03:37:38.711Z",
                                         "project_id": 1,
                                         "project_path": "my-company/app-project",
                                         "tags": ""
                                       }
                                       """;

    private static readonly Uri BaseAddress = new("https://gitlab.example/api/v4/");

    private static readonly byte[] FileBytes = [0x50, 0x4B, 0x03, 0x04];

    private static (HttpClient httpClient, StubHttpMessageHandler handler) CreateClient(
        Func<HttpRequestMessage, HttpResponseMessage> respond)
    {
        StubHttpMessageHandler handler = new(respond);
        HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        return (httpClient, handler);
    }

    [Fact]
    public async Task DownloadMavenPackageFileForGroupAsync_EscapesTheSlashBearingPath()
    {
        (HttpClient httpClient, StubHttpMessageHandler handler) = CreateClient(_ =>
            new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(FileBytes) });

        GitLabApiConnection connection = new(httpClient);
        PackagesGenericClient repository = new(connection);

        using GitLabFileResponse file = await repository.DownloadMavenPackageFileForGroupAsync(
            "gitlab-org/gitlab", "foo/bar/mypkg/1.0-SNAPSHOT", "mypkg-1.0-SNAPSHOT.jar",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/groups/gitlab-org%2Fgitlab/-/packages/maven/"
            + "foo%2Fbar%2Fmypkg%2F1.0-SNAPSHOT/mypkg-1.0-SNAPSHOT.jar",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(HttpStatusCode.OK, file.StatusCode);
    }

    [Fact]
    public async Task DownloadMavenPackageFileAsync_BuildsTheInstanceScopedRoute_WithNoId()
    {
        (HttpClient httpClient, StubHttpMessageHandler handler) = CreateClient(_ =>
            new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(FileBytes) });

        GitLabApiConnection connection = new(httpClient);
        PackagesGenericClient repository = new(connection);

        using GitLabFileResponse file = await repository.DownloadMavenPackageFileAsync(
            "foo/bar/mypkg/1.0-SNAPSHOT", "mypkg-1.0-SNAPSHOT.jar", TestContext.Current.CancellationToken);

        Assert.Equal(
            "https://gitlab.example/api/v4/packages/maven/foo%2Fbar%2Fmypkg%2F1.0-SNAPSHOT/"
            + "mypkg-1.0-SNAPSHOT.jar",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(HttpStatusCode.OK, file.StatusCode);
    }

    [Fact]
    public async Task DownloadMavenPackageFileForProjectAsync_BuildsTheProjectRoute()
    {
        (HttpClient httpClient, StubHttpMessageHandler handler) = CreateClient(_ =>
            new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(FileBytes) });

        GitLabApiConnection connection = new(httpClient);
        PackagesGenericClient repository = new(connection);

        using GitLabFileResponse file = await repository.DownloadMavenPackageFileForProjectAsync(
            42, "foo/bar/mypkg/1.0-SNAPSHOT", "mypkg-1.0-SNAPSHOT.pom", TestContext.Current.CancellationToken);

        Assert.Equal(
            "https://gitlab.example/api/v4/projects/42/packages/maven/foo%2Fbar%2Fmypkg%2F1.0-SNAPSHOT/"
            + "mypkg-1.0-SNAPSHOT.pom",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(HttpStatusCode.OK, file.StatusCode);
    }

    [Fact]
    public async Task AuthorizeMavenPackageFileUploadAsync_PutsToTheAuthorizeRoute_WithNoBody()
    {
        (HttpClient httpClient, StubHttpMessageHandler handler) =
            CreateClient(_ => new HttpResponseMessage(HttpStatusCode.OK));

        GitLabApiConnection connection = new(httpClient);
        PackagesGenericClient repository = new(connection);

        await repository.AuthorizeMavenPackageFileUploadAsync(42, "foo/bar/mypkg/1.0-SNAPSHOT",
            "mypkg-1.0-SNAPSHOT.pom", TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/projects/42/packages/maven/foo%2Fbar%2Fmypkg%2F1.0-SNAPSHOT/"
            + "mypkg-1.0-SNAPSHOT.pom/authorize",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Null(handler.LastRequest?.Content);
    }

    [Fact]
    public async Task UploadMavenPackageFileAsync_PutsTheFileToTheMavenRoute_WithNoResponseBody()
    {
        MediaTypeHeaderValue? sentContentType = null;

        (HttpClient httpClient, StubHttpMessageHandler handler) = CreateClient(request =>
        {
            sentContentType = request.Content?.Headers.ContentType;
            return new HttpResponseMessage(HttpStatusCode.OK);
        });

        GitLabApiConnection connection = new(httpClient);
        PackagesGenericClient repository = new(connection);

        using MemoryStream content = new(FileBytes);
        GitLabFileUpload file = new() { Content = content, FileName = "mypkg-1.0-SNAPSHOT.pom" };

        await repository.UploadMavenPackageFileAsync(42, "foo/bar/mypkg/1.0-SNAPSHOT", "mypkg-1.0-SNAPSHOT.pom",
            file, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/projects/42/packages/maven/foo%2Fbar%2Fmypkg%2F1.0-SNAPSHOT/"
            + "mypkg-1.0-SNAPSHOT.pom",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("multipart/form-data", sentContentType?.MediaType);
    }

    [Fact]
    public async Task UploadMavenPackageFileAsync_ThrowsOnNullFile()
    {
        (HttpClient httpClient, _) = CreateClient(_ => new HttpResponseMessage(HttpStatusCode.OK));

        GitLabApiConnection connection = new(httpClient);
        PackagesGenericClient repository = new(connection);

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            repository.UploadMavenPackageFileAsync(42, "foo/bar/mypkg/1.0-SNAPSHOT", "mypkg-1.0-SNAPSHOT.pom",
                null!, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task DownloadGenericPackageFileAsync_BuildsTheRoute()
    {
        (HttpClient httpClient, StubHttpMessageHandler handler) = CreateClient(_ =>
            new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(FileBytes) });

        GitLabApiConnection connection = new(httpClient);
        PackagesGenericClient repository = new(connection);

        using GitLabFileResponse file = await repository.DownloadGenericPackageFileAsync(1, "my-package", "1.0.0",
            "my-file.tar.gz", TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/projects/1/packages/generic/my-package/1.0.0/my-file.tar.gz",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(HttpStatusCode.OK, file.StatusCode);
    }

    [Fact]
    public async Task UploadGenericPackageFileAsync_SendsMultipartWithSelectAndStatus_AndDeserializesTheCreatedFile()
    {
        string? sentBody = null;
        MediaTypeHeaderValue? sentContentType = null;

        (HttpClient httpClient, StubHttpMessageHandler handler) = CreateClient(request =>
        {
            sentContentType = request.Content?.Headers.ContentType;
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(PackageFileJson, Encoding.UTF8, "application/json")
            };
        });

        GitLabApiConnection connection = new(httpClient);
        PackagesGenericClient repository = new(connection);

        using MemoryStream content = new(FileBytes);
        GitLabFileUpload file = new() { Content = content, FileName = "my-file.tar.gz" };

        GitLabPackageFile created = await repository.UploadGenericPackageFileAsync(1, "my-package", "1.0.0",
            "my-file.tar.gz", file, GitLabPackageFileStatus.Hidden, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/projects/1/packages/generic/my-package/1.0.0/my-file.tar.gz",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("multipart/form-data", sentContentType?.MediaType);
        Assert.NotNull(sentBody);

        // .NET quotes multipart parameters only when it has to, so compare against an unquoted form.
        string unquoted = sentBody.Replace("\"", string.Empty, StringComparison.Ordinal);
        Assert.Contains("name=select", unquoted, StringComparison.Ordinal);
        Assert.Contains("package_file", unquoted, StringComparison.Ordinal);
        Assert.Contains("name=status", unquoted, StringComparison.Ordinal);
        Assert.Contains("hidden", unquoted, StringComparison.Ordinal);

        Assert.Equal(25, created.Id);
        Assert.Equal(4, created.PackageId);
        Assert.Equal("my-app-1.5-20181107.152550-1.jar", created.FileName);
        Assert.Equal(2421, created.Size);
        Assert.NotNull(created.Pipelines);
        Assert.Equal(1, created.Pipelines!.Id);
        Assert.Equal("success", created.Pipelines.Status);
        Assert.Equal("admin", created.Pipelines.User?.Username);
    }

    [Fact]
    public async Task UploadGenericPackageFileAsync_OmitsStatusFormField_WhenNotProvided()
    {
        string? sentBody = null;

        (HttpClient httpClient, StubHttpMessageHandler handler) = CreateClient(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(PackageFileJson, Encoding.UTF8, "application/json")
            };
        });

        GitLabApiConnection connection = new(httpClient);
        PackagesGenericClient repository = new(connection);

        using MemoryStream content = new(FileBytes);
        GitLabFileUpload file = new() { Content = content, FileName = "my-file.tar.gz" };

        await repository.UploadGenericPackageFileAsync(1, "my-package", "1.0.0", "my-file.tar.gz", file,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(sentBody);
        string unquoted = sentBody.Replace("\"", string.Empty, StringComparison.Ordinal);
        Assert.Contains("name=select", unquoted, StringComparison.Ordinal);
        Assert.DoesNotContain("name=status", unquoted, StringComparison.Ordinal);
        Assert.NotNull(handler.LastRequest);
    }

    [Fact]
    public async Task UploadGenericPackageFileAsync_ThrowsOnNullFile_BeforeSendingARequest()
    {
        (HttpClient httpClient, StubHttpMessageHandler handler) = CreateClient(_ =>
            new HttpResponseMessage(HttpStatusCode.OK));

        GitLabApiConnection connection = new(httpClient);
        PackagesGenericClient repository = new(connection);

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            repository.UploadGenericPackageFileAsync(1, "my-package", "1.0.0", "my-file.tar.gz", null!,
                cancellationToken: TestContext.Current.CancellationToken));

        Assert.Null(handler.LastRequest);
    }

    [Fact]
    public async Task AuthorizeGenericPackageFileUploadAsync_PutsTheStatusBody_ToTheAuthorizeRoute()
    {
        string? sentBody = null;

        (HttpClient httpClient, StubHttpMessageHandler handler) = CreateClient(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK);
        });

        GitLabApiConnection connection = new(httpClient);
        PackagesGenericClient repository = new(connection);

        await repository.AuthorizeGenericPackageFileUploadAsync(1, "my-package", "1.0.0", "my-file.tar.gz",
            new AuthorizeGenericPackageFileUploadRequest { Status = GitLabPackageFileStatus.Hidden },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/projects/1/packages/generic/my-package/1.0.0/my-file.tar.gz/authorize",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"status":"hidden"}""", sentBody);
    }

    [Fact]
    public async Task AuthorizeGenericPackageFileUploadAsync_SendsAnEmptyObject_WhenNoRequestGiven()
    {
        string? sentBody = null;

        (HttpClient httpClient, StubHttpMessageHandler handler) = CreateClient(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK);
        });

        GitLabApiConnection connection = new(httpClient);
        PackagesGenericClient repository = new(connection);

        await repository.AuthorizeGenericPackageFileUploadAsync(1, "my-package", "1.0.0", "my-file.tar.gz",
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("{}", sentBody);
        Assert.NotNull(handler.LastRequest);
    }

    [Fact]
    public async Task DownloadGenericPackageFileByPathAsync_BuildsTheNestedPathRoute()
    {
        (HttpClient httpClient, StubHttpMessageHandler handler) = CreateClient(_ =>
            new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(FileBytes) });

        GitLabApiConnection connection = new(httpClient);
        PackagesGenericClient repository = new(connection);

        using GitLabFileResponse file = await repository.DownloadGenericPackageFileByPathAsync(1, "my-package",
            "1.0.0", "nested/dir", "my-file.tar.gz", TestContext.Current.CancellationToken);

        Assert.Equal(
            "https://gitlab.example/api/v4/projects/1/packages/generic/my-package/1.0.0/nested%2Fdir/"
            + "my-file.tar.gz",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(HttpStatusCode.OK, file.StatusCode);
    }

    [Fact]
    public async Task UploadGenericPackageFileByPathAsync_SendsSelectFormField_ToTheNestedPathRoute()
    {
        string? sentBody = null;

        (HttpClient httpClient, StubHttpMessageHandler handler) = CreateClient(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();

            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(PackageFileJson, Encoding.UTF8, "application/json")
            };
        });

        GitLabApiConnection connection = new(httpClient);
        PackagesGenericClient repository = new(connection);

        using MemoryStream content = new(FileBytes);
        GitLabFileUpload file = new() { Content = content, FileName = "my-file.tar.gz" };

        GitLabPackageFile created = await repository.UploadGenericPackageFileByPathAsync(1, "my-package", "1.0.0",
            "nested/dir", "my-file.tar.gz", file, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/projects/1/packages/generic/my-package/1.0.0/nested%2Fdir/"
            + "my-file.tar.gz",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.NotNull(sentBody);
        Assert.Contains("package_file", sentBody, StringComparison.Ordinal);
        Assert.Equal(25, created.Id);
    }

    [Fact]
    public async Task AuthorizeGenericPackageFileUploadByPathAsync_PutsToTheNestedPathAuthorizeRoute()
    {
        (HttpClient httpClient, StubHttpMessageHandler handler) =
            CreateClient(_ => new HttpResponseMessage(HttpStatusCode.OK));

        GitLabApiConnection connection = new(httpClient);
        PackagesGenericClient repository = new(connection);

        await repository.AuthorizeGenericPackageFileUploadByPathAsync(1, "my-package", "1.0.0", "nested/dir",
            "my-file.tar.gz", cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/projects/1/packages/generic/my-package/1.0.0/nested%2Fdir/"
            + "my-file.tar.gz/authorize",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task ListGoModuleVersionsAsync_BuildsTheAtVListRoute()
    {
        const string ResponseBody = "v1.0.0\nv1.1.0\n";

        (HttpClient httpClient, StubHttpMessageHandler handler) = CreateClient(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(ResponseBody, Encoding.UTF8, "text/plain")
            });

        GitLabApiConnection connection = new(httpClient);
        PackagesGenericClient repository = new(connection);

        using GitLabFileResponse file = await repository.ListGoModuleVersionsAsync(1, "gitlab.example.com/my/module",
            TestContext.Current.CancellationToken);

        Assert.Equal(
            "https://gitlab.example/api/v4/projects/1/packages/go/gitlab.example.com%2Fmy%2Fmodule/@v/list",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        using StreamReader reader = new(file.Content);
        Assert.Equal(ResponseBody, await reader.ReadToEndAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task GetGoModuleVersionInfoAsync_BuildsTheDotInfoRoute_AndDeserializesThePascalCaseBody()
    {
        const string InfoJson = """{"Version":"v1.0.0","Time":"2019-06-28T10:22:30Z"}""";

        (HttpClient httpClient, StubHttpMessageHandler handler) = CreateClient(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(InfoJson, Encoding.UTF8, "application/json")
            });

        GitLabApiConnection connection = new(httpClient);
        PackagesGenericClient repository = new(connection);

        GitLabGoModuleVersionInfo info = await repository.GetGoModuleVersionInfoAsync(1,
            "gitlab.example.com/my/module", "v1.0.0", TestContext.Current.CancellationToken);

        Assert.Equal(
            "https://gitlab.example/api/v4/projects/1/packages/go/gitlab.example.com%2Fmy%2Fmodule/"
            + "@v/v1.0.0.info",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("v1.0.0", info.Version);
        Assert.Equal("2019-06-28T10:22:30Z", info.Time);
    }

    [Fact]
    public async Task DownloadGoModuleFileAsync_BuildsTheDotModRoute()
    {
        (HttpClient httpClient, StubHttpMessageHandler handler) = CreateClient(_ =>
            new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(FileBytes) });

        GitLabApiConnection connection = new(httpClient);
        PackagesGenericClient repository = new(connection);

        using GitLabFileResponse file = await repository.DownloadGoModuleFileAsync(1,
            "gitlab.example.com/my/module", "v1.0.0", TestContext.Current.CancellationToken);

        Assert.Equal(
            "https://gitlab.example/api/v4/projects/1/packages/go/gitlab.example.com%2Fmy%2Fmodule/"
            + "@v/v1.0.0.mod",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(HttpStatusCode.OK, file.StatusCode);
    }

    [Fact]
    public async Task DownloadGoModuleSourceAsync_BuildsTheDotZipRoute()
    {
        (HttpClient httpClient, StubHttpMessageHandler handler) = CreateClient(_ =>
            new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(FileBytes) });

        GitLabApiConnection connection = new(httpClient);
        PackagesGenericClient repository = new(connection);

        using GitLabFileResponse file = await repository.DownloadGoModuleSourceAsync(1,
            "gitlab.example.com/my/module", "v1.0.0", TestContext.Current.CancellationToken);

        Assert.Equal(
            "https://gitlab.example/api/v4/projects/1/packages/go/gitlab.example.com%2Fmy%2Fmodule/"
            + "@v/v1.0.0.zip",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(HttpStatusCode.OK, file.StatusCode);
    }

    [Fact]
    public async Task ListPackagesAsync_BuildsTheQueryString_AndDeserializesThePackageSummary()
    {
        string json = $"[{PackageJson}]";

        (HttpClient httpClient, StubHttpMessageHandler handler) = CreateClient(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });

        GitLabApiConnection connection = new(httpClient);
        PackagesGenericClient repository = new(connection);

        PackageListOptions options = new()
        {
            OrderBy = GitLabPackageOrderBy.Version,
            Sort = GitLabPackageSort.Desc,
            PackageType = GitLabPackageType.Maven
        };

        List<GitLabPackage> packages = [];
        await foreach (GitLabPackage package in
                       repository.ListPackagesAsync(1, options, TestContext.Current.CancellationToken))
        {
            packages.Add(package);
        }

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/projects/1/packages?order_by=version&sort=desc&package_type=maven",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabPackage only = Assert.Single(packages);
        Assert.Equal(1, only.Id);
        Assert.Equal("com/mycompany/my-app", only.Name);
        Assert.Equal("1.0-SNAPSHOT", only.Version);
        Assert.Equal(GitLabPackageType.Maven, only.PackageType);
        Assert.Equal(GitLabPackageStatus.Default, only.Status);
        Assert.Equal("/root/my-company/app-project/-/packages/1", only.Links?.WebPath);
        Assert.Equal("my-company/app-project", only.ProjectPath);
    }

    [Theory]
    [InlineData(GitLabPackageType.Rpm, "rpm")]
    [InlineData(GitLabPackageType.MlModel, "ml_model")]
    [InlineData(GitLabPackageType.Cargo, "cargo")]
    public async Task ListPackagesAsync_SupportsEveryGitLab19PackageTypeAddedAfterTheOriginalRegistryFormats(
        GitLabPackageType packageType, string wireValue)
    {
        string json = $"[{PackageJson.Replace("\"maven\"", $"\"{wireValue}\"", StringComparison.Ordinal)}]";

        (HttpClient httpClient, StubHttpMessageHandler handler) = CreateClient(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });

        GitLabApiConnection connection = new(httpClient);
        PackagesGenericClient repository = new(connection);

        List<GitLabPackage> packages = [];
        await foreach (GitLabPackage package in repository.ListPackagesAsync(1,
                           new PackageListOptions { PackageType = packageType },
                           TestContext.Current.CancellationToken))
        {
            packages.Add(package);
        }

        Assert.Equal($"https://gitlab.example/api/v4/projects/1/packages?package_type={wireValue}",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(packageType, Assert.Single(packages).PackageType);
    }

    [Fact]
    public async Task ListPackagesForGroupAsync_BuildsTheGroupRoute_WithExcludeSubgroupsAndOrderBy()
    {
        string json = $"[{PackageJson}]";

        (HttpClient httpClient, StubHttpMessageHandler handler) = CreateClient(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });

        GitLabApiConnection connection = new(httpClient);
        PackagesGenericClient repository = new(connection);

        GroupPackageListOptions options = new()
        {
            ExcludeSubgroups = true, OrderBy = GitLabGroupPackageOrderBy.ProjectPath
        };

        List<GitLabPackage> packages = [];
        await foreach (GitLabPackage package in
                       repository.ListPackagesForGroupAsync(9, options, TestContext.Current.CancellationToken))
        {
            packages.Add(package);
        }

        Assert.Equal(
            "https://gitlab.example/api/v4/groups/9/packages?exclude_subgroups=true&order_by=project_path",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        Assert.Single(packages);
    }

    [Fact]
    public async Task GetPackageAsync_BuildsThePackageRoute_AndDeserializesTheLinksObject()
    {
        (HttpClient httpClient, StubHttpMessageHandler handler) = CreateClient(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(PackageJson, Encoding.UTF8, "application/json")
            });

        GitLabApiConnection connection = new(httpClient);
        PackagesGenericClient repository = new(connection);

        GitLabPackage package = await repository.GetPackageAsync(1, 1, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/packages/1",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("1.0-SNAPSHOT", package.Version);
        Assert.Equal("/api/v4/projects/1/packages/1", package.Links?.DeleteApiPath);
    }

    [Fact]
    public async Task DeletePackageAsync_SendsDeleteToThePackageRoute()
    {
        (HttpClient httpClient, StubHttpMessageHandler handler) =
            CreateClient(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        GitLabApiConnection connection = new(httpClient);
        PackagesGenericClient repository = new(connection);

        await repository.DeletePackageAsync(1, 4, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/packages/4",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task ListPackageFilesAsync_BuildsTheQueryString_AndDeserializesTheNestedPipeline()
    {
        string json = $"[{PackageFileJson}]";

        (HttpClient httpClient, StubHttpMessageHandler handler) = CreateClient(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });

        GitLabApiConnection connection = new(httpClient);
        PackagesGenericClient repository = new(connection);

        PackageFileListOptions options = new()
        {
            Page = 2, PerPage = 10, OrderBy = GitLabPackageFileOrderBy.FileName, Sort = GitLabPackageFileSort.Desc
        };

        List<GitLabPackageFile> files = [];
        await foreach (GitLabPackageFile file in
                       repository.ListPackageFilesAsync(1, 4, options, TestContext.Current.CancellationToken))
        {
            files.Add(file);
        }

        Assert.Equal(
            "https://gitlab.example/api/v4/projects/1/packages/4/package_files?"
            + "page=2&per_page=10&order_by=file_name&sort=desc",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabPackageFile only = Assert.Single(files);
        Assert.Equal("my-app-1.5-20181107.152550-1.jar", only.FileName);
        Assert.Equal("58e6a45a629910c6ff99145a688971ac", only.FileMd5);
        Assert.NotNull(only.Pipelines);
        Assert.Equal("feature-branch", only.Pipelines!.Ref);
    }

    [Fact]
    public async Task DeletePackageFileAsync_SendsDeleteToThePackageFileRoute()
    {
        (HttpClient httpClient, StubHttpMessageHandler handler) =
            CreateClient(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        GitLabApiConnection connection = new(httpClient);
        PackagesGenericClient repository = new(connection);

        await repository.DeletePackageFileAsync(1, 4, 225, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/packages/4/package_files/225",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task DownloadPackageFileAsync_BuildsTheDownloadRoute()
    {
        (HttpClient httpClient, StubHttpMessageHandler handler) = CreateClient(_ =>
            new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(FileBytes) });

        GitLabApiConnection connection = new(httpClient);
        PackagesGenericClient repository = new(connection);

        using GitLabFileResponse file =
            await repository.DownloadPackageFileAsync(1, 4, 225, TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/1/packages/4/package_files/225/download",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(HttpStatusCode.OK, file.StatusCode);
    }

    [Fact]
    public async Task ListPackagePipelinesAsync_BuildsTheCursorQueryString_AndReusesGitLabPipeline()
    {
        const string PipelineJson = """
                                    {
                                      "id": 1,
                                      "iid": 2,
                                      "project_id": 3,
                                      "sha": "0ec9e58fdfca6cdd6652c083c9edb53abc0bad52",
                                      "ref": "feature-branch",
                                      "status": "success",
                                      "source": "push",
                                      "web_url": "https://gitlab.example.com/gitlab-org/gitlab-foss/-/pipelines/61"
                                    }
                                    """;

        string json = $"[{PipelineJson}]";

        (HttpClient httpClient, StubHttpMessageHandler handler) = CreateClient(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });

        GitLabApiConnection connection = new(httpClient);
        PackagesGenericClient repository = new(connection);

        PackagePipelineListOptions options = new() { PerPage = 20, Cursor = "eyJpZCI6MX0" };

        List<GitLabPipeline> pipelines = [];
        await foreach (GitLabPipeline pipeline in
                       repository.ListPackagePipelinesAsync(1, 4, options, TestContext.Current.CancellationToken))
        {
            pipelines.Add(pipeline);
        }

        Assert.Equal(
            "https://gitlab.example/api/v4/projects/1/packages/4/pipelines?per_page=20&cursor=eyJpZCI6MX0",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabPipeline only = Assert.Single(pipelines);
        Assert.Equal(1, only.Id);
        Assert.Equal("success", only.Status);
    }

    [Fact]
    public async Task DownloadGenericPackageFileAsync_MapsA404ToTheTypedNotFoundException()
    {
        (HttpClient httpClient, StubHttpMessageHandler _) = CreateClient(_ =>
            new HttpResponseMessage(HttpStatusCode.NotFound)
            {
                Content = new StringContent("""{"message":"404 Not Found"}""", Encoding.UTF8, "application/json")
            });

        GitLabApiConnection connection = new(httpClient);
        PackagesGenericClient repository = new(connection);

        GitLabNotFoundException exception = await Assert.ThrowsAsync<GitLabNotFoundException>(() =>
            repository.DownloadGenericPackageFileAsync(1, "missing", "1.0.0", "file.bin",
                TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
    }
}