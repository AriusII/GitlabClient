using System.Net;
using System.Net.Http.Headers;
using System.Text;

using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Repositories;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Repositories;

public sealed class PackagesTerraformModulesRepositoryTests
{
    private const string ModuleJson = """
                                      {
                                        "name": "hello-world/local",
                                        "provider": "local",
                                        "providers": ["local"],
                                        "root": {
                                          "dependencies": []
                                        },
                                        "source": "https://gitlab.example/group/hello-world",
                                        "submodules": [],
                                        "version": "1.0.0",
                                        "versions": ["1.0.0"]
                                      }
                                      """;

    private const string VersionListJson = """
                                           {
                                             "modules": [
                                               {
                                                 "versions": [
                                                   {
                                                     "version": "1.0.0",
                                                     "submodules": [],
                                                     "root": {
                                                       "dependencies": [],
                                                       "providers": [{"name": "local", "version": ""}]
                                                     }
                                                   }
                                                 ],
                                                 "source": "https://gitlab.example/group/hello-world"
                                               }
                                             ]
                                           }
                                           """;

    private static readonly Uri BaseAddress = new("https://gitlab.example/api/v4/");

    [Fact]
    public async Task GetModuleAsync_BuildsTheGroupScopedModuleRegistryRoute_AndDeserializesTheModule()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(ModuleJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        PackagesTerraformModulesRepository repository = new(connection);

        GitLabTerraformModule module = await repository.GetModuleAsync(
            42, "hello-world", "local", TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/packages/terraform/modules/v1/42/hello-world/local",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        Assert.Equal("hello-world/local", module.Name);
        Assert.Equal("local", module.Provider);
        Assert.Equal("1.0.0", module.Version);
        Assert.Equal(["1.0.0"], module.Versions);
        Assert.Empty(module.Root?.Dependencies ?? []);
    }

    /// <summary>
    ///     The module name and system are caller-supplied free text, so both are <c>Escaped</c> rather than
    ///     <c>Literal</c> - an unescaped slash would otherwise invent an extra path segment.
    /// </summary>
    [Fact]
    public async Task GetModuleVersionAsync_EscapesTheModuleNameAndSystem_AndUsesTheGroupPath()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(ModuleJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        PackagesTerraformModulesRepository repository = new(connection);

        await repository.GetModuleVersionAsync(
            GroupId.FromPath("group/subgroup"),
            "hello/world",
            "my system",
            "1.0.0",
            TestContext.Current.CancellationToken);

        Assert.Equal(
            "https://gitlab.example/api/v4/packages/terraform/modules/v1/group%2Fsubgroup/hello%2Fworld/my%20system/1.0.0",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task ListModuleVersionsAsync_BuildsTheVersionsRoute_AndDeserializesTheNestedShape()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(VersionListJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        PackagesTerraformModulesRepository repository = new(connection);

        GitLabTerraformModuleVersionList list = await repository.ListModuleVersionsAsync(
            42, "hello-world", "local", TestContext.Current.CancellationToken);

        Assert.Equal(
            "https://gitlab.example/api/v4/packages/terraform/modules/v1/42/hello-world/local/versions",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabTerraformModuleVersionsEntry entry = Assert.Single(list.Modules ?? []);
        GitLabTerraformModuleVersionInfo version = Assert.Single(entry.Versions ?? []);
        Assert.Equal("1.0.0", version.Version);
        Assert.Equal("local", version.Root?.Providers?[0].Name);
        Assert.Equal(new Uri("https://gitlab.example/group/hello-world"), entry.Source);
    }

    [Fact]
    public async Task DownloadModuleVersionFileAsync_StreamsTheArchiveThroughTheGroupScopedRoute()
    {
        byte[] archive = [0x1f, 0x8b, 0x01, 0x02];

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(archive)
            {
                Headers = { ContentType = new MediaTypeHeaderValue("application/gzip") }
            }
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        PackagesTerraformModulesRepository repository = new(connection);

        using GitLabFileResponse file = await repository.DownloadModuleVersionFileAsync(
            42, "hello-world", "local", "1.0.0", TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/packages/terraform/modules/v1/42/hello-world/local/1.0.0/file",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("application/gzip", file.ContentType);
    }

    [Fact]
    public async Task DownloadModuleAsync_BuildsTheGroupScopedDownloadRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        PackagesTerraformModulesRepository repository = new(connection);

        using GitLabFileResponse file = await repository.DownloadModuleAsync(
            42, "hello-world", "local", TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/packages/terraform/modules/v1/42/hello-world/local/download",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(HttpStatusCode.NoContent, file.StatusCode);
    }

    [Fact]
    public async Task DownloadModuleVersionAsync_ForAGroup_AddressesOneVersionThroughTheDownloadRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        PackagesTerraformModulesRepository repository = new(connection);

        using GitLabFileResponse file = await repository.DownloadModuleVersionAsync(
            GroupId.FromPath("group/subgroup"), "hello/world", "my system", "1.0.0",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/packages/terraform/modules/v1/group%2Fsubgroup/hello%2Fworld/"
            + "my%20system/1.0.0/download",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(HttpStatusCode.NoContent, file.StatusCode);
    }

    [Fact]
    public async Task DownloadLatestModuleAsync_BuildsTheProjectScopedRoute_AndOmitsTheFlagByDefault()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent([0x1f, 0x8b])
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        PackagesTerraformModulesRepository repository = new(connection);

        using GitLabFileResponse _ = await repository.DownloadLatestModuleAsync(
            7, "hello-world", "local", cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(
            "https://gitlab.example/api/v4/projects/7/packages/terraform/modules/hello-world/local",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task DownloadLatestModuleAsync_SendsTheTerraformGetFlag_WhenExplicitlySet()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        PackagesTerraformModulesRepository repository = new(connection);

        using GitLabFileResponse _ = await repository.DownloadLatestModuleAsync(
            7, "hello-world", "local", true, TestContext.Current.CancellationToken);

        Assert.Equal(
            "https://gitlab.example/api/v4/projects/7/packages/terraform/modules/hello-world/local?terraform-get=true",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task DownloadModuleVersionAsync_AddressesOneVersion_ForAProject()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent([0x1f, 0x8b])
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        PackagesTerraformModulesRepository repository = new(connection);

        using GitLabFileResponse _ = await repository.DownloadModuleVersionAsync(
            ProjectId.FromPath("group/infra"), "hello-world", "local", "1.0.0",
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(
            "https://gitlab.example/api/v4/projects/group%2Finfra/packages/terraform/modules/hello-world/local/1.0.0",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task UploadModuleFileAsync_PutsTheArchiveAsMultipart_AndDeserializesTheConfirmation()
    {
        string? sentBody = null;
        MediaTypeHeaderValue? sentContentType = null;

        using StubHttpMessageHandler handler = new(request =>
        {
            sentContentType = request.Content?.Headers.ContentType;
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();

            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent("""{"message":"201 Created"}""", Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        PackagesTerraformModulesRepository repository = new(connection);

        using MemoryStream content = new([0x1f, 0x8b, 0x08, 0x00]);
        GitLabFileUpload upload = new() { Content = content, FileName = "hello-world-local-1.0.0.tgz" };

        GitLabTerraformModuleUploadResult result = await repository.UploadModuleFileAsync(
            7, "hello-world", "local", "1.0.0", upload, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/projects/7/packages/terraform/modules/hello-world/local/1.0.0/file",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        Assert.Equal("multipart/form-data", sentContentType?.MediaType);
        Assert.NotNull(sentBody);
        Assert.Contains("name=file", sentBody, StringComparison.Ordinal);
        Assert.Equal("201 Created", result.Message);

        // The stream is borrowed, never owned: it must still be usable after the call returns.
        Assert.True(content.CanRead);
    }

    [Fact]
    public async Task AuthorizeModuleFileUploadAsync_PutsToTheAuthorizeRoute_WithNoBody()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("""{"TempPath":"/var/opt/gitlab/tmp"}""", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        PackagesTerraformModulesRepository repository = new(connection);

        await repository.AuthorizeModuleFileUploadAsync(
            7, "hello-world", "local", "1.0.0", TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/projects/7/packages/terraform/modules/hello-world/local/1.0.0/file/authorize",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Null(handler.LastRequest?.Content);
    }
}