using System.Net;
using System.Text;

using GitLab.Client.Endpoints;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Endpoints;

public sealed class VsCodeEndpointTests
{
    private const string SettingsContextHash = "d41d8cd98f00b204e9800998ecf8427e";

    [Fact]
    public async Task GetManifestAsync_BuildsTheUserGlobalManifestRoute_AndDeserializes()
    {
        const string json = """{ "latest": "settings", "session": "1" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        VsCodeClient repository = new(connection);

        GitLabVsCodeSettingsManifest manifest =
            await repository.GetManifestAsync(cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/vscode/settings_sync/v1/manifest",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("settings", manifest.Latest);
        Assert.Equal("1", manifest.Session);
    }

    /// <summary>
    ///     The settings-context form of every route puts the hash BETWEEN <c>settings_sync</c> and
    ///     <c>v1</c>, not after <c>v1</c>. Getting that wrong yields a 404 on a path GitLab does not serve.
    /// </summary>
    [Fact]
    public async Task GetManifestAsync_InsertsTheSettingsContextHashBeforeTheVersionSegment()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("""{ "latest": null, "session": null }""", Encoding.UTF8,
                "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        VsCodeClient repository = new(connection);

        GitLabVsCodeSettingsManifest manifest =
            await repository.GetManifestAsync(SettingsContextHash, TestContext.Current.CancellationToken);

        Assert.Equal(
            $"https://gitlab.example/api/v4/vscode/settings_sync/{SettingsContextHash}/v1/manifest",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Null(manifest.Latest);
        Assert.Null(manifest.Session);
    }

    [Fact]
    public async Task ListReferencesAsync_BuildsTheResourceRoute_AndDeserializesReferences()
    {
        const string json = """
                            [
                              {
                                "url": "https://gitlab.example/api/v4/vscode/settings_sync/v1/resource/settings/12",
                                "created": "2024-05-01T09:12:33.000Z"
                              }
                            ]
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        VsCodeClient repository = new(connection);

        List<GitLabVsCodeSettingReference> references = [];
        await foreach (GitLabVsCodeSettingReference reference in repository.ListReferencesAsync(
                           GitLabVsCodeSettingResource.Settings,
                           cancellationToken: TestContext.Current.CancellationToken))
        {
            references.Add(reference);
        }

        Assert.Equal("https://gitlab.example/api/v4/vscode/settings_sync/v1/resource/settings",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabVsCodeSettingReference only = Assert.Single(references);
        Assert.Equal(
            new Uri("https://gitlab.example/api/v4/vscode/settings_sync/v1/resource/settings/12"),
            only.Url);
        Assert.Equal("2024-05-01T09:12:33.000Z", only.Created);
    }

    /// <summary>
    ///     Every resource name in the closed vocabulary must reach the route as GitLab spells it -
    ///     including <c>globalState</c>, the one camelCase word, which no PascalCase-to-snake_case
    ///     convention would produce.
    /// </summary>
    [Theory]
    [InlineData(GitLabVsCodeSettingResource.Settings, "settings")]
    [InlineData(GitLabVsCodeSettingResource.Extensions, "extensions")]
    [InlineData(GitLabVsCodeSettingResource.GlobalState, "globalState")]
    [InlineData(GitLabVsCodeSettingResource.Machines, "machines")]
    [InlineData(GitLabVsCodeSettingResource.Keybindings, "keybindings")]
    [InlineData(GitLabVsCodeSettingResource.Snippets, "snippets")]
    [InlineData(GitLabVsCodeSettingResource.Tasks, "tasks")]
    [InlineData(GitLabVsCodeSettingResource.Profiles, "profiles")]
    public async Task ListReferencesAsync_ProjectsEveryResourceNameOntoItsGitLabWireWord(
        GitLabVsCodeSettingResource resourceName,
        string expectedPathWord)
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        VsCodeClient repository = new(connection);

        await foreach (GitLabVsCodeSettingReference _ in repository.ListReferencesAsync(
                           resourceName, cancellationToken: TestContext.Current.CancellationToken))
        {
            // Draining the (empty) sequence is what issues the request.
        }

        Assert.Equal($"https://gitlab.example/api/v4/vscode/settings_sync/v1/resource/{expectedPathWord}",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task GetSettingAsync_BuildsTheRevisionRoute_AndReadsTheCamelCaseMachineId()
    {
        const string json = """
                            {
                              "content": "{\"editor.fontSize\":13}",
                              "machines": "[]",
                              "version": "3",
                              "machineId": "5f1c9a2e-1c2b-4f0f-9d4a-0f1b2c3d4e5f"
                            }
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        VsCodeClient repository = new(connection);

        GitLabVsCodeSetting setting = await repository.GetSettingAsync(
            GitLabVsCodeSettingResource.GlobalState,
            "latest",
            SettingsContextHash,
            TestContext.Current.CancellationToken);

        Assert.Equal(
            $"https://gitlab.example/api/v4/vscode/settings_sync/{SettingsContextHash}"
            + "/v1/resource/globalState/latest",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        Assert.Equal("""{"editor.fontSize":13}""", setting.Content);
        Assert.Equal("[]", setting.Machines);
        Assert.Equal("3", setting.Version);

        // machineId is camelCase on the wire, against the snake_case naming policy the context applies to
        // everything else - so this only passes because of the explicit [JsonPropertyName].
        Assert.Equal("5f1c9a2e-1c2b-4f0f-9d4a-0f1b2c3d4e5f", setting.MachineId);
    }

    [Fact]
    public async Task GetSettingAsync_PercentEncodesARevisionIdContainingASlash()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("""{ "content": null }""", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        VsCodeClient repository = new(connection);

        await repository.GetSettingAsync(GitLabVsCodeSettingResource.Snippets, "rev/7",
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/vscode/settings_sync/v1/resource/snippets/rev%2F7",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    /// <summary>
    ///     The spec declares no request body for this operation, so none is sent. If GitLab later
    ///     documents one, this assertion is what will fail first.
    /// </summary>
    [Fact]
    public async Task CreateOrUpdateAsync_PostsToTheResourceRoute_WithNoBody()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        VsCodeClient repository = new(connection);

        await repository.CreateOrUpdateAsync(GitLabVsCodeSettingResource.Keybindings,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/vscode/settings_sync/v1/resource/keybindings",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Null(handler.LastRequest?.Content);
    }

    [Fact]
    public async Task DeleteCollectionAsync_BuildsTheCollectionRoute_ForBothRouteFamilies()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        VsCodeClient repository = new(connection);

        await repository.DeleteCollectionAsync(cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/vscode/settings_sync/v1/collection",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        await repository.DeleteCollectionAsync(SettingsContextHash, TestContext.Current.CancellationToken);

        Assert.Equal($"https://gitlab.example/api/v4/vscode/settings_sync/{SettingsContextHash}/v1/collection",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }
}