using System.Net;
using System.Text;

using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Repositories;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Repositories;

public sealed class SshKeysRepositoryTests
{
    private const string KeyJson = """
                                   {
                                     "id": 1,
                                     "title": "Sample key 25",
                                     "key": "ssh-ed25519 AAAAC3NzaC1lZDI1NTE5AAAAIExampleKeyValue user@example",
                                     "created_at": "2015-09-03T07:24:44.627Z",
                                     "expires_at": "2020-09-03T07:24:44.627Z",
                                     "last_used_at": "2020-08-03T07:24:44.627Z",
                                     "usage_type": "auth_and_signing"
                                   }
                                   """;

    [Fact]
    public async Task ListForCurrentUserAsync_BuildsTheSingularUserRoute_AndDeserializesKeys()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent($"[{KeyJson}]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        SshKeysRepository repository = new(connection);

        List<GitLabSshKey> keys = new();
        await foreach (GitLabSshKey item in
                       repository.ListForCurrentUserAsync(TestContext.Current.CancellationToken))
        {
            keys.Add(item);
        }

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);

        // The singular "user" root - the authenticated account - not "users".
        Assert.Equal("https://gitlab.example/api/v4/user/keys", handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabSshKey key = Assert.Single(keys);
        Assert.Equal(1, key.Id);
        Assert.Equal("Sample key 25", key.Title);
        Assert.Equal("ssh-ed25519 AAAAC3NzaC1lZDI1NTE5AAAAIExampleKeyValue user@example", key.Key);
        Assert.Equal(new DateTimeOffset(2015, 9, 3, 7, 24, 44, 627, TimeSpan.Zero), key.CreatedAt);
        Assert.Equal(new DateTimeOffset(2020, 9, 3, 7, 24, 44, 627, TimeSpan.Zero), key.ExpiresAt);
        Assert.Equal(new DateTimeOffset(2020, 8, 3, 7, 24, 44, 627, TimeSpan.Zero), key.LastUsedAt);
        Assert.Equal("auth_and_signing", key.UsageType);
    }

    [Fact]
    public async Task GetForCurrentUserAsync_BuildsTheSingularUserKeyRoute_AndDeserializesTheKey()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(KeyJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        SshKeysRepository repository = new(connection);

        GitLabSshKey key = await repository.GetForCurrentUserAsync(1, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/user/keys/1", handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("Sample key 25", key.Title);
        Assert.Equal("auth_and_signing", key.UsageType);
    }

    [Fact]
    public async Task CreateForCurrentUserAsync_PostsRequestBody_ToTheSingularUserRoute()
    {
        const string Json = """
                            {
                              "id": 12,
                              "title": "laptop",
                              "key": "ssh-ed25519 AAAAC3NzaC1lZDI1NTE5AAAAINewKey user@example",
                              "created_at": "2026-01-05T12:00:00.000Z",
                              "expires_at": "2027-01-05T12:00:00.000Z",
                              "usage_type": "auth"
                            }
                            """;

        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(Json, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        SshKeysRepository repository = new(connection);

        CreateSshKeyRequest request = new()
        {
            Key = "ssh-ed25519 AAAAC3NzaC1lZDI1NTE5AAAAINewKey user@example",
            Title = "laptop",
            ExpiresAt = new DateTimeOffset(2027, 1, 5, 12, 0, 0, TimeSpan.Zero),
            UsageType = "auth"
        };

        GitLabSshKey key = await repository.CreateForCurrentUserAsync(request,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/user/keys", handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("application/json", handler.LastRequest?.Content?.Headers.ContentType?.MediaType);
        Assert.Contains("\"title\":\"laptop\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"key\":\"ssh-ed25519 AAAAC3NzaC1lZDI1NTE5AAAAINewKey user@example\"", sentBody,
            StringComparison.Ordinal);
        Assert.Contains("\"usage_type\":\"auth\"", sentBody, StringComparison.Ordinal);

        // expires_at is a full date-time on this body, unlike the access-token request bodies.
        Assert.Contains("\"expires_at\":\"2027-01-05T12:00:00+00:00\"", sentBody, StringComparison.Ordinal);

        Assert.Equal(12, key.Id);
        Assert.Equal("laptop", key.Title);
        Assert.Equal("auth", key.UsageType);
    }

    [Fact]
    public async Task CreateForCurrentUserAsync_OmitsUnsetOptionalMembers()
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
        SshKeysRepository repository = new(connection);

        CreateSshKeyRequest request = new() { Key = "ssh-ed25519 AAAA minimal", Title = "minimal" };

        await repository.CreateForCurrentUserAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal("{\"key\":\"ssh-ed25519 AAAA minimal\",\"title\":\"minimal\"}", sentBody);
    }

    [Fact]
    public async Task DeleteForCurrentUserAsync_SendsDeleteToTheSingularUserKeyRoute()
    {
        // GitLab answers this one with 200 and the deleted key, not 204; the body is discarded.
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(KeyJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        SshKeysRepository repository = new(connection);

        await repository.DeleteForCurrentUserAsync(1, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/user/keys/1", handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task ListForUserAsync_BuildsThePluralUsersRoute_AndDeserializesKeys()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent($"[{KeyJson}]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        SshKeysRepository repository = new(connection);

        List<GitLabSshKey> keys = new();
        await foreach (GitLabSshKey item in repository.ListForUserAsync(77, TestContext.Current.CancellationToken))
        {
            keys.Add(item);
        }

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);

        // The plural "users" root, addressing a named account - not the authenticated one.
        Assert.Equal("https://gitlab.example/api/v4/users/77/keys", handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("Sample key 25", Assert.Single(keys).Title);
    }

    [Fact]
    public async Task GetForUserAsync_BuildsThePluralUsersKeyRoute_AndDeserializesTheKey()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(KeyJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        SshKeysRepository repository = new(connection);

        GitLabSshKey key = await repository.GetForUserAsync(77, 1, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/users/77/keys/1",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(1, key.Id);
        Assert.Equal("ssh-ed25519 AAAAC3NzaC1lZDI1NTE5AAAAIExampleKeyValue user@example", key.Key);
    }

    [Fact]
    public async Task CreateForUserAsync_PostsRequestBody_ToThePluralUsersRoute()
    {
        const string Json = """
                            {
                              "id": 31,
                              "title": "bot key",
                              "key": "ssh-rsa AAAAB3NzaC1yc2EAAAA bot@example",
                              "usage_type": "signing"
                            }
                            """;

        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(Json, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        SshKeysRepository repository = new(connection);

        CreateSshKeyRequest request = new()
        {
            Key = "ssh-rsa AAAAB3NzaC1yc2EAAAA bot@example", Title = "bot key", UsageType = "signing"
        };

        GitLabSshKey key = await repository.CreateForUserAsync(77, request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/users/77/keys",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Contains("\"title\":\"bot key\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"usage_type\":\"signing\"", sentBody, StringComparison.Ordinal);
        Assert.Equal(31, key.Id);
        Assert.Equal("signing", key.UsageType);
    }

    [Fact]
    public async Task DeleteForUserAsync_SendsDeleteToThePluralUsersKeyRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(KeyJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        SshKeysRepository repository = new(connection);

        await repository.DeleteForUserAsync(77, 1, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/users/77/keys/1",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task GetForUserAsync_OnNotFound_ThrowsGitLabNotFoundException()
    {
        const string Json = """{ "message": "404 Key Not Found" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        SshKeysRepository repository = new(connection);

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabNotFoundException>(() =>
            repository.GetForUserAsync(77, 9999, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        Assert.Equal("404 Key Not Found", exception.Message);
    }

    [Fact]
    public async Task CreateForCurrentUserAsync_OnValidationFailure_ThrowsGitLabValidationException()
    {
        const string Json = """{ "message": { "key": ["has already been taken"] } }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        SshKeysRepository repository = new(connection);

        CreateSshKeyRequest request = new() { Key = "ssh-ed25519 AAAA duplicate", Title = "duplicate" };

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabValidationException>(() =>
            repository.CreateForCurrentUserAsync(request, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);
    }

    [Fact]
    public async Task GetUserByFingerprintAsync_PercentEncodesTheFingerprintIntoTheQueryString()
    {
        const string Json = """
                            {
                              "id": 25,
                              "username": "john_smith",
                              "name": "John Smith",
                              "state": "active",
                              "locked": false,
                              "public_email": "john@example.com",
                              "avatar_url": "https://gitlab.example/uploads/user/avatar/25/index.jpg",
                              "web_url": "https://gitlab.example/john_smith"
                            }
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        SshKeysRepository repository = new(connection);

        // A SHA-256 fingerprint is base64 and carries ":", "+", "/" and "=" - every one of which would
        // change the meaning of the URL if it were pasted in raw.
        GitLabUser user = await repository.GetUserByFingerprintAsync("SHA256:nUhz+g/hK2b8T4/9wI+xQ0=",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/keys?fingerprint=SHA256%3AnUhz%2Bg%2FhK2b8T4%2F9wI%2BxQ0%3D",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        Assert.Equal(25, user.Id);
        Assert.Equal("john_smith", user.Username);
        Assert.Equal(new Uri("https://gitlab.example/john_smith"), user.WebUrl);
    }

    [Fact]
    public async Task GetByIdAsync_BuildsTheInstanceWideKeysRoute_AndDeserializesTheEmbeddedOwner()
    {
        const string Json = """
                            {
                              "id": 1,
                              "title": "Sample key 1",
                              "key": "ssh-ed25519 AAAAC3NzaC1lZDI1NTE5AAAAIExampleKeyValue user@example",
                              "created_at": "2015-09-03T07:24:44.627Z",
                              "expires_at": null,
                              "usage_type": "auth_and_signing",
                              "user": {
                                "id": 25,
                                "username": "john_smith",
                                "name": "John Smith",
                                "state": "active",
                                "web_url": "https://gitlab.example/john_smith"
                              }
                            }
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        SshKeysRepository repository = new(connection);

        GitLabSshKey key = await repository.GetByIdAsync(1, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);

        // The instance-wide root - no "user"/"users" prefix at all.
        Assert.Equal("https://gitlab.example/api/v4/keys/1", handler.LastRequest?.RequestUri?.AbsoluteUri);

        Assert.Equal("Sample key 1", key.Title);
        Assert.Null(key.ExpiresAt);
        Assert.Equal("john_smith", key.User?.Username);
        Assert.Equal(25, key.User?.Id);
    }

    [Fact]
    public async Task ListGroupCertificatesAsync_EncodesANamespacedGroupPath()
    {
        const string Json = """
                            [
                              {
                                "id": 12,
                                "title": "corp CA",
                                "key": "ssh-ed25519 AAAAC3NzaC1lZDI1NTE5AAAAICertAuthority ca@example",
                                "created_at": "2026-02-01T09:30:00.000Z"
                              }
                            ]
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        SshKeysRepository repository = new(connection);

        List<GitLabSshCertificate> certificates = new();
        await foreach (GitLabSshCertificate item in repository.ListGroupCertificatesAsync(
                           GroupId.FromPath("acme/platform"), TestContext.Current.CancellationToken))
        {
            certificates.Add(item);
        }

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/acme%2Fplatform/ssh_certificates",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabSshCertificate certificate = Assert.Single(certificates);
        Assert.Equal(12, certificate.Id);
        Assert.Equal("corp CA", certificate.Title);
        Assert.Equal("ssh-ed25519 AAAAC3NzaC1lZDI1NTE5AAAAICertAuthority ca@example", certificate.Key);
        Assert.Equal(new DateTimeOffset(2026, 2, 1, 9, 30, 0, TimeSpan.Zero), certificate.CreatedAt);
    }

    [Fact]
    public async Task AddGroupCertificateAsync_PostsTitleAndKey_ToTheGroupRoute()
    {
        const string Json = """
                            {
                              "id": 13,
                              "title": "corp CA",
                              "key": "ssh-ed25519 AAAAC3NzaC1lZDI1NTE5AAAAICertAuthority ca@example",
                              "created_at": "2026-02-01T09:30:00.000Z"
                            }
                            """;

        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(Json, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        SshKeysRepository repository = new(connection);

        CreateSshCertificateRequest request = new()
        {
            Title = "corp CA", Key = "ssh-ed25519 AAAAC3NzaC1lZDI1NTE5AAAAICertAuthority ca@example"
        };

        GitLabSshCertificate certificate =
            await repository.AddGroupCertificateAsync(9, request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/9/ssh_certificates",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        // The key is a long single-line string with spaces: it belongs in the body, never in the URL.
        Assert.Equal(
            "{\"title\":\"corp CA\",\"key\":\"ssh-ed25519 AAAAC3NzaC1lZDI1NTE5AAAAICertAuthority ca@example\"}",
            sentBody);

        Assert.Equal(13, certificate.Id);
    }

    [Fact]
    public async Task DeleteGroupCertificateAsync_SendsDeleteToTheCertificateRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        SshKeysRepository repository = new(connection);

        await repository.DeleteGroupCertificateAsync(9, 13, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/9/ssh_certificates/13",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task GetUserByFingerprintAsync_WhenNotAnAdministrator_ThrowsGitLabForbiddenException()
    {
        const string Json = """{ "message": "403 Forbidden" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Forbidden)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        SshKeysRepository repository = new(connection);

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabForbiddenException>(() =>
            repository.GetUserByFingerprintAsync("SHA256:unknown", TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.Forbidden, exception.StatusCode);
    }
}