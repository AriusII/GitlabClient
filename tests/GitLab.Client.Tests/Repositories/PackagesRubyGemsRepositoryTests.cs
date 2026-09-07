using System.Net;
using System.Net.Http.Headers;

using GitLab.Client.Abstractions;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Repositories;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Repositories;

public sealed class PackagesRubyGemsRepositoryTests
{
    private static readonly Uri BaseAddress = new("https://gitlab.example/api/v4/");

    private static readonly byte[] GemBytes = [0x1F, 0x8B, 0x08, 0x00];

    [Theory]
    [InlineData(GitLabRubyGemsSpecIndexFile.Specs, "specs.4.8.gz")]
    [InlineData(GitLabRubyGemsSpecIndexFile.LatestSpecs, "latest_specs.4.8.gz")]
    [InlineData(GitLabRubyGemsSpecIndexFile.PrereleaseSpecs, "prerelease_specs.4.8.gz")]
    public async Task GetSpecIndexAsync_ProjectsEachEnumMember_OntoItsWireFileName(
        GitLabRubyGemsSpecIndexFile file, string expectedFileName)
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(GemBytes)
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        PackagesRubyGemsRepository repository = new(connection);

        using GitLabFileResponse response =
            await repository.GetSpecIndexAsync(7, file, TestContext.Current.CancellationToken);

        Assert.Equal($"https://gitlab.example/api/v4/projects/7/packages/rubygems/{expectedFileName}",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetGemspecAsync_BuildsTheMarshalQuickRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(GemBytes)
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        PackagesRubyGemsRepository repository = new(connection);

        using GitLabFileResponse response = await repository.GetGemspecAsync(7, "my-gem-1.0.0.gemspec.rz",
            TestContext.Current.CancellationToken);

        Assert.Equal(
            "https://gitlab.example/api/v4/projects/7/packages/rubygems/quick/Marshal.4.8/my-gem-1.0.0.gemspec.rz",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task DownloadGemAsync_EscapesTheFileName()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(GemBytes)
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        PackagesRubyGemsRepository repository = new(connection);

        using GitLabFileResponse response = await repository.DownloadGemAsync(7, "my gem-1.0.0.gem",
            TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/7/packages/rubygems/gems/my%20gem-1.0.0.gem",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task AuthorizeGemUploadAsync_PostsToTheAuthorizeRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK));

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        PackagesRubyGemsRepository repository = new(connection);

        await repository.AuthorizeGemUploadAsync(7, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/7/packages/rubygems/api/v1/gems/authorize",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task UploadGemAsync_PostsMultipartFormData_UnderTheDefaultFileFieldName()
    {
        string? sentBody = null;
        MediaTypeHeaderValue? sentContentType = null;

        using StubHttpMessageHandler handler = new(request =>
        {
            sentContentType = request.Content?.Headers.ContentType;
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created);
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        PackagesRubyGemsRepository repository = new(connection);

        using MemoryStream content = new(GemBytes);
        GitLabFileUpload file = new() { Content = content, FileName = "my-gem-1.0.0.gem" };

        await repository.UploadGemAsync(7, file, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/7/packages/rubygems/api/v1/gems",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("multipart/form-data", sentContentType?.MediaType);
        Assert.NotNull(sentBody);
        Assert.Contains("name=file", sentBody, StringComparison.Ordinal);

        // The stream is borrowed, never owned: it must still be usable after the call returns.
        Assert.True(content.CanRead);
    }

    [Fact]
    public async Task GetDependenciesAsync_WithNoOptions_OmitsTheGemsQueryParameter()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(GemBytes)
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        PackagesRubyGemsRepository repository = new(connection);

        using GitLabFileResponse response =
            await repository.GetDependenciesAsync(7, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/7/packages/rubygems/api/v1/dependencies",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetDependenciesAsync_JoinsGemNames_AsACommaSeparatedQueryParameter()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(GemBytes)
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        PackagesRubyGemsRepository repository = new(connection);

        using GitLabFileResponse response = await repository.GetDependenciesAsync(
            7,
            new RubyGemsDependencyListOptions { Gems = ["rails", "rspec"] },
            TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/7/packages/rubygems/api/v1/dependencies?gems=rails,rspec",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}