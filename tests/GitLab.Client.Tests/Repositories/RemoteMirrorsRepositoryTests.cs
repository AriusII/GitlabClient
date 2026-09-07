using System.Globalization;
using System.Net;
using System.Text;
using System.Text.Json;

using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Repositories;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Repositories;

public sealed class RemoteMirrorsRepositoryTests
{
    [Fact]
    public async Task ListAsync_BuildsRemoteMirrorsRoute_AndDeserializesEachMirror()
    {
        const string Json = """
                            [
                              {
                                "id": 101486,
                                "enabled": true,
                                "url": "https://*****:*****@example.com/gitlab/example.git",
                                "update_status": "finished",
                                "last_update_at": "2020-01-06T17:32:02.823Z",
                                "last_update_started_at": "2020-01-06T17:31:55.864Z",
                                "last_successful_update_at": "2020-01-06T17:32:02.823Z",
                                "last_error": null,
                                "only_protected_branches": true,
                                "keep_divergent_refs": false,
                                "auth_method": "ssh_public_key",
                                "host_keys": [
                                  { "fingerprint_sha256": "SHA256:abcd1234" }
                                ],
                                "mirror_branch_regex": null
                              },
                              {
                                "id": 101487,
                                "enabled": false,
                                "url": "https://example.com/other/example.git",
                                "update_status": "failed",
                                "last_error": "13:fetch remote: signal: killed",
                                "auth_method": "password",
                                "mirror_branch_regex": "^release/"
                              }
                            ]
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        RemoteMirrorsRepository repository = new(connection);

        List<GitLabRemoteMirror> mirrors = new();
        await foreach (GitLabRemoteMirror mirror in repository.ListAsync(42, TestContext.Current.CancellationToken))
        {
            mirrors.Add(mirror);
        }

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/42/remote_mirrors",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        Assert.Equal(2, mirrors.Count);

        GitLabRemoteMirror first = mirrors[0];
        Assert.Equal(101486, first.Id);
        Assert.True(first.Enabled);

        // The URL keeps its scrubbed "*****:*****" userinfo verbatim - it is a string, never parsed as a Uri.
        Assert.Equal("https://*****:*****@example.com/gitlab/example.git", first.Url);
        Assert.Equal("finished", first.UpdateStatus);
        Assert.Equal(DateTimeOffset.Parse("2020-01-06T17:32:02.823Z", CultureInfo.InvariantCulture),
            first.LastUpdateAt);
        Assert.Equal(DateTimeOffset.Parse("2020-01-06T17:31:55.864Z", CultureInfo.InvariantCulture),
            first.LastUpdateStartedAt);
        Assert.Equal(DateTimeOffset.Parse("2020-01-06T17:32:02.823Z", CultureInfo.InvariantCulture),
            first.LastSuccessfulUpdateAt);
        Assert.Null(first.LastError);
        Assert.True(first.OnlyProtectedBranches);
        Assert.False(first.KeepDivergentRefs);
        Assert.Equal("ssh_public_key", first.AuthMethod);
        Assert.Null(first.MirrorBranchRegex);

        // FingerprintSha256 must land on "fingerprint_sha256" under SnakeCaseLower - no separator between
        // the 'a' and the '256'.
        Assert.Equal("SHA256:abcd1234", Assert.Single(first.HostKeys!).FingerprintSha256);

        GitLabRemoteMirror second = mirrors[1];
        Assert.Equal(101487, second.Id);
        Assert.False(second.Enabled);
        Assert.Equal("13:fetch remote: signal: killed", second.LastError);
        Assert.Equal("^release/", second.MirrorBranchRegex);
        Assert.Null(second.HostKeys);
        Assert.Null(second.LastUpdateAt);
    }

    [Fact]
    public async Task GetAsync_EncodesNamespacedProjectPath_AndDeserializesMirror()
    {
        const string Json = """
                            {
                              "id": 101486,
                              "enabled": true,
                              "url": "https://*****:*****@example.com/gitlab/example.git",
                              "update_status": "scheduled",
                              "keep_divergent_refs": true,
                              "auth_method": "password"
                            }
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        RemoteMirrorsRepository repository = new(connection);

        GitLabRemoteMirror mirror =
            await repository.GetAsync("gitlab-org/gitlab", 101486, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/remote_mirrors/101486",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(101486, mirror.Id);
        Assert.Equal("scheduled", mirror.UpdateStatus);
        Assert.True(mirror.KeepDivergentRefs);
        Assert.Equal("password", mirror.AuthMethod);
    }

    [Fact]
    public async Task GetAsync_OnErrorResponse_ThrowsGitLabNotFoundException()
    {
        const string Json = """{ "message": "404 Remote Mirror Not Found" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        RemoteMirrorsRepository repository = new(connection);

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabNotFoundException>(() =>
            repository.GetAsync(42, 999, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        Assert.Equal("404 Remote Mirror Not Found", exception.Message);
    }

    [Fact]
    public async Task CreateAsync_PostsToRemoteMirrorsRoute_WithSerializedBody_AndDeserializesCreatedMirror()
    {
        const string Json = """
                            {
                              "id": 101486,
                              "enabled": false,
                              "url": "https://*****:*****@example.com/gitlab/example.git",
                              "update_status": "none",
                              "only_protected_branches": true,
                              "keep_divergent_refs": true,
                              "auth_method": "ssh_public_key",
                              "host_keys": [
                                { "fingerprint_sha256": "SHA256:abcd1234" }
                              ]
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
        RemoteMirrorsRepository repository = new(connection);

        CreateRemoteMirrorRequest request = new()
        {
            Url = "https://user:token@example.com/gitlab/example.git",
            Enabled = true,
            AuthMethod = "ssh_public_key",
            KeepDivergentRefs = true,
            OnlyProtectedBranches = true,
            HostKeys = ["ssh-ed25519 AAAAC3Nza"]
        };

        GitLabRemoteMirror mirror = await repository.CreateAsync(42, request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/42/remote_mirrors",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("application/json", handler.LastRequest?.Content?.Headers.ContentType?.MediaType);

        Assert.Contains("\"url\":\"https://user:token@example.com/gitlab/example.git\"", sentBody,
            StringComparison.Ordinal);
        Assert.Contains("\"enabled\":true", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"auth_method\":\"ssh_public_key\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"keep_divergent_refs\":true", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"only_protected_branches\":true", sentBody, StringComparison.Ordinal);

        // host_keys goes out as raw strings and comes back as fingerprint objects.
        Assert.Contains("\"host_keys\":[\"ssh-ed25519 AAAAC3Nza\"]", sentBody, StringComparison.Ordinal);
        Assert.DoesNotContain("mirror_branch_regex", sentBody, StringComparison.Ordinal);

        Assert.Equal(101486, mirror.Id);
        Assert.Equal("SHA256:abcd1234", Assert.Single(mirror.HostKeys!).FingerprintSha256);
    }

    [Fact]
    public async Task UpdateAsync_PutsToMirrorRoute_WithOnlyTheSpecifiedFields()
    {
        const string Json = """
                            {
                              "id": 101486,
                              "enabled": false,
                              "url": "https://*****:*****@example.com/gitlab/example.git",
                              "update_status": "none",
                              "mirror_branch_regex": "^release/"
                            }
                            """;

        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(Json, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        RemoteMirrorsRepository repository = new(connection);

        UpdateRemoteMirrorRequest request = new() { Enabled = false, MirrorBranchRegex = "^release/" };

        GitLabRemoteMirror mirror =
            await repository.UpdateAsync(42, 101486, request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/42/remote_mirrors/101486",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        // Unset members are omitted rather than sent as null, and there is no `url` to send at all.
        Assert.Equal("""{"enabled":false,"mirror_branch_regex":"^release/"}""", sentBody);

        Assert.False(mirror.Enabled);
        Assert.Equal("^release/", mirror.MirrorBranchRegex);
    }

    [Fact]
    public async Task DeleteAsync_SendsDeleteToMirrorRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        RemoteMirrorsRepository repository = new(connection);

        await repository.DeleteAsync("gitlab-org/gitlab", 101486, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/remote_mirrors/101486",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task SyncAsync_PostsEmptyBodyToSyncRoute_AndAcceptsNoContent()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        RemoteMirrorsRepository repository = new(connection);

        await repository.SyncAsync(42, 101486, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/42/remote_mirrors/101486/sync",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        // 204 with no body: nothing is deserialized, so the request carries no content either.
        Assert.Null(handler.LastRequest?.Content);
    }

    [Fact]
    public async Task GetPublicKeyAsync_HitsThePublicKeyRoute_AndReturnsTheRawJson()
    {
        const string Json = """{ "public_key": "ssh-rsa AAAAB3NzaC1yc2EAAAADAQAB" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        RemoteMirrorsRepository repository = new(connection);

        JsonElement publicKey =
            await repository.GetPublicKeyAsync(42, 101486, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/42/remote_mirrors/101486/public_key",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("ssh-rsa AAAAB3NzaC1yc2EAAAADAQAB", publicKey.GetProperty("public_key").GetString());
    }

    [Fact]
    public async Task SyncAsync_OnErrorResponse_ThrowsGitLabValidationException()
    {
        const string Json = """{ "message": "Cannot proceed with the push mirroring" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        RemoteMirrorsRepository repository = new(connection);

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabValidationException>(() =>
            repository.SyncAsync(42, 101486, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);
        Assert.Equal("Cannot proceed with the push mirroring", exception.Message);
    }
}