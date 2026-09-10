using System.Net;
using System.Text;

using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Endpoints;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Endpoints;

public sealed class GpgKeysEndpointTests
{
    /// <summary>
    ///     A realistic payload: the key is ASCII-armored, so the JSON string carries embedded newlines and
    ///     must survive round-tripping verbatim.
    /// </summary>
    private const string KeyJson = """
                                   {
                                     "id": 1,
                                     "key": "-----BEGIN PGP PUBLIC KEY BLOCK-----\r\n\r\nxsBNBFVjnlIBCACibzXOLCiZiL2oyzYUaTOCkYnSUhymg3pdbjKR2ns6Rc5\r\n-----END PGP PUBLIC KEY BLOCK-----",
                                     "created_at": "2017-09-05T09:17:46.264Z"
                                   }
                                   """;

    private const string ArmoredKey =
        "-----BEGIN PGP PUBLIC KEY BLOCK-----\r\n\r\nxsBNBFVjnlIBCACibzXOLCiZiL2oyzYUaTOCkYnSUhymg3pdbjKR2ns6Rc5\r\n-----END PGP PUBLIC KEY BLOCK-----";

    [Fact]
    public async Task ListForCurrentUserAsync_BuildsTheSingularUserRoute_AndDeserializesKeys()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent($"[{KeyJson}]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        GpgKeysClient repository = new(connection);

        List<GitLabGpgKey> keys = new();
        await foreach (GitLabGpgKey item in
                       repository.ListForCurrentUserAsync(TestContext.Current.CancellationToken))
        {
            keys.Add(item);
        }

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);

        // The singular "user" root - the authenticated account - not "users".
        Assert.Equal("https://gitlab.example/api/v4/user/gpg_keys", handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabGpgKey key = Assert.Single(keys);
        Assert.Equal(1, key.Id);
        Assert.Equal(ArmoredKey, key.Key);
        Assert.Equal(new DateTimeOffset(2017, 9, 5, 9, 17, 46, 264, TimeSpan.Zero), key.CreatedAt);
    }

    [Fact]
    public async Task GetForCurrentUserAsync_BuildsTheSingularUserKeyRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(KeyJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        GpgKeysClient repository = new(connection);

        GitLabGpgKey key = await repository.GetForCurrentUserAsync(1, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/user/gpg_keys/1",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(1, key.Id);
        Assert.StartsWith("-----BEGIN PGP PUBLIC KEY BLOCK-----", key.Key, StringComparison.Ordinal);
    }

    [Fact]
    public async Task CreateForCurrentUserAsync_PutsTheArmoredKeyInTheBody_NotTheUrl()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(KeyJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        GpgKeysClient repository = new(connection);

        CreateGpgKeyRequest request = new() { Key = ArmoredKey };

        GitLabGpgKey key = await repository.CreateForCurrentUserAsync(request,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/user/gpg_keys",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("application/json", handler.LastRequest?.Content?.Headers.ContentType?.MediaType);

        // The armored block is the only member, and its newlines are JSON-escaped rather than dropped.
        Assert.NotNull(sentBody);
        Assert.StartsWith("{\"key\":\"-----BEGIN PGP PUBLIC KEY BLOCK-----", sentBody, StringComparison.Ordinal);
        Assert.Contains("\\r\\n", sentBody, StringComparison.Ordinal);

        Assert.Equal(1, key.Id);
    }

    [Fact]
    public async Task DeleteForCurrentUserAsync_SendsDeleteToTheSingularUserKeyRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        GpgKeysClient repository = new(connection);

        await repository.DeleteForCurrentUserAsync(1, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/user/gpg_keys/1",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task RevokeForCurrentUserAsync_PostsToTheRevokeRoute_WithNoBody()
    {
        // GitLab answers 202 Accepted with an empty body, which is why this uses the body-less POST.
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Accepted));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        GpgKeysClient repository = new(connection);

        await repository.RevokeForCurrentUserAsync(1, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/user/gpg_keys/1/revoke",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task ListForUserAsync_BuildsThePluralUsersRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent($"[{KeyJson}]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        GpgKeysClient repository = new(connection);

        List<GitLabGpgKey> keys = new();
        await foreach (GitLabGpgKey item in repository.ListForUserAsync(77, TestContext.Current.CancellationToken))
        {
            keys.Add(item);
        }

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);

        // The plural "users" root, addressing a named account - not the authenticated one.
        Assert.Equal("https://gitlab.example/api/v4/users/77/gpg_keys",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(1, Assert.Single(keys).Id);
    }

    [Fact]
    public async Task GetForUserAsync_BuildsThePluralUsersKeyRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(KeyJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        GpgKeysClient repository = new(connection);

        GitLabGpgKey key = await repository.GetForUserAsync(77, 1, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/users/77/gpg_keys/1",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(1, key.Id);
    }

    [Fact]
    public async Task CreateForUserAsync_PostsRequestBody_ToThePluralUsersRoute()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(KeyJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        GpgKeysClient repository = new(connection);

        CreateGpgKeyRequest request = new() { Key = ArmoredKey };

        await repository.CreateForUserAsync(77, request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/users/77/gpg_keys",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Contains("BEGIN PGP PUBLIC KEY BLOCK", sentBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task DeleteForUserAsync_SendsDeleteToThePluralUsersKeyRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        GpgKeysClient repository = new(connection);

        await repository.DeleteForUserAsync(77, 1, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/users/77/gpg_keys/1",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task RevokeForUserAsync_PostsToThePluralUsersRevokeRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Accepted));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        GpgKeysClient repository = new(connection);

        await repository.RevokeForUserAsync(77, 1, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/users/77/gpg_keys/1/revoke",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task GetForCurrentUserAsync_OnNotFound_ThrowsGitLabNotFoundException()
    {
        const string Json = """{ "message": "404 GPG Key Not Found" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        GpgKeysClient repository = new(connection);

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabNotFoundException>(() =>
            repository.GetForCurrentUserAsync(9999, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        Assert.Equal("404 GPG Key Not Found", exception.Message);
    }

    [Fact]
    public async Task CreateForCurrentUserAsync_OnValidationFailure_ThrowsGitLabValidationException()
    {
        const string Json = """{ "message": { "key": ["is invalid"] } }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        GpgKeysClient repository = new(connection);

        CreateGpgKeyRequest request = new() { Key = "not a key" };

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabValidationException>(() =>
            repository.CreateForCurrentUserAsync(request, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);
    }
}