using System.Net;
using System.Text;

using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Repositories;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Repositories;

public sealed class PackagesCargoRepositoryTests
{
    private static readonly Uri BaseAddress = new("https://gitlab.example/api/v4/");

    private static readonly byte[] CrateBytes = [0x1F, 0x8B, 0x08, 0x00];

    [Fact]
    public async Task GetSparseIndexForOneCharacterNameAsync_BuildsTheOneCharacterRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("""{"name":"a","vers":"1.0.0"}""", Encoding.UTF8, "text/plain")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        PackagesCargoRepository repository = new(connection);

        using GitLabFileResponse response =
            await repository.GetSparseIndexForOneCharacterNameAsync(7, "a", TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/7/packages/cargo/1/a",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetSparseIndexForTwoCharacterNameAsync_BuildsTheTwoCharacterRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("""{"name":"ab","vers":"1.0.0"}""", Encoding.UTF8, "text/plain")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        PackagesCargoRepository repository = new(connection);

        using GitLabFileResponse response =
            await repository.GetSparseIndexForTwoCharacterNameAsync(7, "ab", TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/7/packages/cargo/2/ab",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetSparseIndexForThreeCharacterNameAsync_BuildsTheThreeCharacterRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("""{"name":"abc","vers":"1.0.0"}""", Encoding.UTF8, "text/plain")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        PackagesCargoRepository repository = new(connection);

        using GitLabFileResponse response = await repository.GetSparseIndexForThreeCharacterNameAsync(7, "a", "abc",
            TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/7/packages/cargo/3/a/abc",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetSparseIndexAsync_BuildsTheFourPlusCharacterRoute_AndEscapesEachPrefix()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("""{"name":"my crate","vers":"1.0.0"}""", Encoding.UTF8, "text/plain")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        PackagesCargoRepository repository = new(connection);

        using GitLabFileResponse response = await repository.GetSparseIndexAsync(7, "my", "-c", "my-crate",
            TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/7/packages/cargo/my/-c/my-crate",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetConfigAsync_BuildsTheConfigJsonRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("""{"dl":"https://gitlab.example/x","api":"https://gitlab.example/x"}""",
                Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        PackagesCargoRepository repository = new(connection);

        using GitLabFileResponse response =
            await repository.GetConfigAsync(ProjectId.FromPath("group/sub/crates"),
                TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/group%2Fsub%2Fcrates/packages/cargo/config.json",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task DownloadCrateAsync_BuildsTheDownloadRoute_AndStreamsTheCrateBytes()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(CrateBytes)
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        PackagesCargoRepository repository = new(connection);

        using GitLabFileResponse response = await repository.DownloadCrateAsync(7, "my-crate", "1.2.3",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/7/packages/cargo/my-crate/1.2.3/download",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        using MemoryStream copy = new();
        await response.Content.CopyToAsync(copy, TestContext.Current.CancellationToken);
        Assert.Equal(CrateBytes, copy.ToArray());
    }

    [Fact]
    public async Task DownloadCrateAsync_EscapesAPlusBearingSemverBuildMetadataVersion()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(CrateBytes)
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        PackagesCargoRepository repository = new(connection);

        // Cargo/semver versions legally carry build metadata after a '+' (e.g. "1.2.3+build.4"), which is
        // not in Uri.EscapeDataString's unreserved set - proof that the version segment went through
        // .Escaped(...), not .Literal(...).
        using GitLabFileResponse response = await repository.DownloadCrateAsync(7, "my-crate", "1.2.3+build.4",
            TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/7/packages/cargo/my-crate/1.2.3%2Bbuild.4/download",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}