using System.Net;
using System.Net.Http.Headers;
using System.Text;

using GitLab.Client.Abstractions;
using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Endpoints;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Endpoints;

public sealed class AvatarsEndpointTests
{
    [Fact]
    public async Task GetForEmailAsync_EncodesTheEmail_AndSendsTheRequestedSize()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                """{ "avatar_url": "https://gitlab.example/uploads/-/system/user/avatar/1/git.png" }""",
                Encoding.UTF8,
                "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        AvatarsClient repository = new(new GitLabApiConnection(httpClient));

        GitLabAvatar avatar = await repository.GetForEmailAsync("first.last+gitlab@example.com", 64,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal(
            "/api/v4/avatar?email=first.last%2Bgitlab%40example.com&size=64",
            handler.LastRequest?.RequestUri?.PathAndQuery);
        Assert.Equal(new Uri("https://gitlab.example/uploads/-/system/user/avatar/1/git.png"), avatar.AvatarUrl);
    }

    [Fact]
    public async Task GetForEmailAsync_OmitsTheSizeParameterWhenItIsNotSet()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("""{ "avatar_url": null }""", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        AvatarsClient repository = new(new GitLabApiConnection(httpClient));

        GitLabAvatar avatar = await repository.GetForEmailAsync("root@example.com",
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("/api/v4/avatar?email=root%40example.com", handler.LastRequest?.RequestUri?.PathAndQuery);
        Assert.Null(avatar.AvatarUrl);
    }

    [Fact]
    public async Task DownloadForProjectAsync_EncodesTheNamespacedPath_AndStreamsTheBody()
    {
        using StubHttpMessageHandler handler = new(_ =>
        {
            HttpResponseMessage response = new(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(Encoding.UTF8.GetBytes("PNG-BYTES"))
            };

            response.Content.Headers.ContentType = new MediaTypeHeaderValue("image/png");
            response.Content.Headers.ContentDisposition =
                new ContentDispositionHeaderValue("attachment") { FileName = "avatar.png" };

            return response;
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        AvatarsClient repository = new(new GitLabApiConnection(httpClient));

        GitLabFileResponse file =
            await repository.DownloadForProjectAsync("gitlab-org/gitlab", TestContext.Current.CancellationToken);

        await using (file.ConfigureAwait(true))
        {
            Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
            Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/avatar",
                handler.LastRequest?.RequestUri?.AbsoluteUri);

            Assert.Equal("image/png", file.ContentType);
            Assert.Equal("avatar.png", file.FileName);
            Assert.Equal(HttpStatusCode.OK, file.StatusCode);

            using StreamReader reader = new(file.Content, Encoding.UTF8);
            Assert.Equal("PNG-BYTES",
                await reader.ReadToEndAsync(TestContext.Current.CancellationToken));
        }
    }

    [Fact]
    public async Task DownloadForGroupAsync_BuildsTheGroupRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(Encoding.UTF8.GetBytes("PNG"))
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        AvatarsClient repository = new(new GitLabApiConnection(httpClient));

        GitLabFileResponse file =
            await repository.DownloadForGroupAsync(9970, TestContext.Current.CancellationToken);

        await using (file.ConfigureAwait(true))
        {
            Assert.Equal("https://gitlab.example/api/v4/groups/9970/avatar",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
            Assert.NotNull(file.Content);
        }
    }

    [Fact]
    public async Task DownloadForProjectAsync_MapsAProjectWithoutAnAvatarToTheTypedNotFoundException()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent("""{"message":"404 Not found"}""", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        AvatarsClient repository = new(new GitLabApiConnection(httpClient));

        GitLabNotFoundException exception = await Assert.ThrowsAsync<GitLabNotFoundException>(() =>
            repository.DownloadForProjectAsync(42, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
    }
}