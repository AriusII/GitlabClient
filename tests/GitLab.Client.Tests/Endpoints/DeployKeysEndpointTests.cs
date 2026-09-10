using System.Net;
using System.Text;

using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Domain;
using GitLab.Client.Endpoints;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;
using GitLab.Client.Query;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Endpoints;

public sealed class DeployKeysEndpointTests
{
    private const string BaseAddress = "https://gitlab.example/api/v4/";

    private const string PublicKey =
        "ssh-ed25519 AAAAC3NzaC1lZDI1NTE5AAAAIJ0Yv2mYQ7pQ0V4wF1kJc5s3n8Bq2sZ2dQ1mZ1yPjQpG deploy@example.com";

    [Fact]
    public async Task ListAsync_BuildsProjectDeployKeysRoute_AndDeserializesEachKey()
    {
        string json = $$"""
                        [
                          {
                            "id": 12,
                            "title": "CI runner",
                            "key": "{{PublicKey}}",
                            "created_at": "2025-10-29T11:00:00Z",
                            "expires_at": "2027-01-01T00:00:00Z",
                            "last_used_at": "2026-08-30T08:15:00Z",
                            "usage_type": "auth_and_signing",
                            "fingerprint": "9a:71:1a:c1:52:0b:76:0f:29:31:9b:6d:6c:9b:2d:1f",
                            "fingerprint_sha256": "SHA256:nUYAF1c3vGVI6z1kJc5s3n8Bq2sZ2dQ1mZ1yPjQpGxY",
                            "can_push": true
                          },
                          {
                            "id": 13,
                            "title": "Read-only mirror",
                            "key": "{{PublicKey}}",
                            "can_push": false
                          }
                        ]
                        """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri(BaseAddress) };
        GitLabApiConnection connection = new(httpClient);
        DeployKeysClient repository = new(connection);

        List<GitLabDeployKey> keys = new();
        await foreach (GitLabDeployKey key in repository.ListAsync(ProjectId.FromId(5),
                           TestContext.Current.CancellationToken))
        {
            keys.Add(key);
        }

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/5/deploy_keys",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        Assert.Equal(2, keys.Count);

        GitLabDeployKey first = keys[0];
        Assert.Equal(12, first.Id);
        Assert.Equal("CI runner", first.Title);
        Assert.Equal(PublicKey, first.Key);
        Assert.Equal(new DateTimeOffset(2025, 10, 29, 11, 0, 0, TimeSpan.Zero), first.CreatedAt);

        // expires_at is a full timestamp here, not the date-only shape a member's expiry uses.
        Assert.Equal(new DateTimeOffset(2027, 1, 1, 0, 0, 0, TimeSpan.Zero), first.ExpiresAt);
        Assert.Equal(new DateTimeOffset(2026, 8, 30, 8, 15, 0, TimeSpan.Zero), first.LastUsedAt);
        Assert.Equal("auth_and_signing", first.UsageType);
        Assert.Equal("9a:71:1a:c1:52:0b:76:0f:29:31:9b:6d:6c:9b:2d:1f", first.Fingerprint);

        // Proves the default snake_case policy maps FingerprintSha256 onto "fingerprint_sha256".
        Assert.Equal("SHA256:nUYAF1c3vGVI6z1kJc5s3n8Bq2sZ2dQ1mZ1yPjQpGxY", first.FingerprintSha256);
        Assert.True(first.CanPush);

        Assert.Equal(13, keys[1].Id);
        Assert.False(keys[1].CanPush);
        Assert.Null(keys[1].ExpiresAt);
        Assert.Null(keys[1].UsageType);
    }

    [Fact]
    public async Task ListAsync_EncodesNamespacedProjectPath()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri(BaseAddress) };
        GitLabApiConnection connection = new(httpClient);
        DeployKeysClient repository = new(connection);

        await foreach (GitLabDeployKey _ in repository.ListAsync(ProjectId.FromPath("gitlab-org/gitlab"),
                           TestContext.Current.CancellationToken))
        {
            Assert.Fail("The stub returns an empty page.");
        }

        Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/deploy_keys",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task GetAsync_BuildsSingleKeyRoute_AndDeserializesTheKey()
    {
        string json = $$"""
                        {
                          "id": 12,
                          "title": "CI runner",
                          "key": "{{PublicKey}}",
                          "created_at": "2025-10-29T11:00:00Z",
                          "usage_type": "auth",
                          "can_push": false
                        }
                        """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri(BaseAddress) };
        GitLabApiConnection connection = new(httpClient);
        DeployKeysClient repository = new(connection);

        GitLabDeployKey key = await repository.GetAsync("gitlab-org/gitlab", 12,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/deploy_keys/12",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(12, key.Id);
        Assert.Equal("CI runner", key.Title);
        Assert.Equal("auth", key.UsageType);
        Assert.False(key.CanPush);
    }

    [Fact]
    public async Task AddAsync_PostsToProjectDeployKeysRoute_WithSerializedBody_AndDeserializesTheCreatedKey()
    {
        string json = $$"""
                        {
                          "id": 14,
                          "title": "Staging deploy",
                          "key": "{{PublicKey}}",
                          "created_at": "2026-03-01T12:00:00Z",
                          "expires_at": "2026-12-31T00:00:00Z",
                          "can_push": true
                        }
                        """;

        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri(BaseAddress) };
        GitLabApiConnection connection = new(httpClient);
        DeployKeysClient repository = new(connection);

        CreateDeployKeyRequest request = new()
        {
            Key = PublicKey,
            Title = "Staging deploy",
            CanPush = true,
            ExpiresAt = new DateTimeOffset(2026, 12, 31, 0, 0, 0, TimeSpan.Zero)
        };

        GitLabDeployKey key = await repository.AddAsync(5, request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/5/deploy_keys",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("application/json", handler.LastRequest?.Content?.Headers.ContentType?.MediaType);
        Assert.Contains("\"title\":\"Staging deploy\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"can_push\":true", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"expires_at\":\"2026-12-31T00:00:00+00:00\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"key\":\"ssh-ed25519 ", sentBody, StringComparison.Ordinal);

        Assert.Equal(14, key.Id);
        Assert.Equal("Staging deploy", key.Title);
        Assert.True(key.CanPush);
    }

    [Fact]
    public async Task AddAsync_OmitsUnsetOptionalFields()
    {
        string json = $$"""{ "id": 15, "title": "Minimal", "key": "{{PublicKey}}" }""";

        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri(BaseAddress) };
        GitLabApiConnection connection = new(httpClient);
        DeployKeysClient repository = new(connection);

        GitLabDeployKey key = await repository.AddAsync(5,
            new CreateDeployKeyRequest { Key = PublicKey, Title = "Minimal" },
            TestContext.Current.CancellationToken);

        Assert.DoesNotContain("can_push", sentBody, StringComparison.Ordinal);
        Assert.DoesNotContain("expires_at", sentBody, StringComparison.Ordinal);
        Assert.Equal(15, key.Id);
        Assert.Null(key.CanPush);
    }

    [Fact]
    public async Task UpdateAsync_PutsToSingleKeyRoute_WithSerializedBody()
    {
        // PUT answers with APIEntitiesDeployKey, which carries no can_push at all.
        string json = $$"""
                        {
                          "id": 12,
                          "title": "Renamed key",
                          "key": "{{PublicKey}}",
                          "created_at": "2025-10-29T11:00:00Z"
                        }
                        """;

        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri(BaseAddress) };
        GitLabApiConnection connection = new(httpClient);
        DeployKeysClient repository = new(connection);

        GitLabDeployKey key = await repository.UpdateAsync(5, 12,
            new UpdateDeployKeyRequest { Title = "Renamed key", CanPush = false },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/5/deploy_keys/12",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("{\"title\":\"Renamed key\",\"can_push\":false}", sentBody);
        Assert.Equal("Renamed key", key.Title);
        Assert.Null(key.CanPush);
    }

    [Fact]
    public async Task DeleteAsync_SendsDeleteToSingleKeyRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri(BaseAddress) };
        GitLabApiConnection connection = new(httpClient);
        DeployKeysClient repository = new(connection);

        await repository.DeleteAsync("gitlab-org/gitlab", 12, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/deploy_keys/12",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task EnableAsync_PostsToEnableRoute_WithNoBody_AndDeserializesTheKey()
    {
        string json = $$"""
                        {
                          "id": 12,
                          "title": "CI runner",
                          "key": "{{PublicKey}}",
                          "created_at": "2025-10-29T11:00:00Z"
                        }
                        """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri(BaseAddress) };
        GitLabApiConnection connection = new(httpClient);
        DeployKeysClient repository = new(connection);

        GitLabDeployKey key = await repository.EnableAsync(5, 12, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/5/deploy_keys/12/enable",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Null(handler.LastRequest?.Content);
        Assert.Equal(12, key.Id);
        Assert.Equal("CI runner", key.Title);
    }

    [Fact]
    public async Task ListAllAsync_BuildsInstanceWideRoute_AndSendsThePublicFilter()
    {
        string json = $$"""
                        [
                          {
                            "id": 1,
                            "title": "Instance key",
                            "key": "{{PublicKey}}",
                            "usage_type": "auth",
                            "projects_with_write_access": [
                              {
                                "id": 7,
                                "name": "shop",
                                "path": "shop",
                                "path_with_namespace": "acme/shop"
                              },
                              {
                                "id": 9,
                                "name": "api",
                                "path": "api",
                                "path_with_namespace": "acme/api"
                              }
                            ],
                            "projects_with_readonly_access": [
                              {
                                "id": 8,
                                "name": "docs",
                                "path": "docs",
                                "path_with_namespace": "acme/docs"
                              }
                            ]
                          }
                        ]
                        """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri(BaseAddress) };
        GitLabApiConnection connection = new(httpClient);
        DeployKeysClient repository = new(connection);

        List<GitLabDeployKey> keys = new();
        await foreach (GitLabDeployKey key in repository.ListAllAsync(true, TestContext.Current.CancellationToken))
        {
            keys.Add(key);
        }

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/deploy_keys?public=true",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabDeployKey only = Assert.Single(keys);
        Assert.Equal(1, only.Id);
        Assert.Equal("Instance key", only.Title);

        // The instance-wide shape has no can_push at all.
        Assert.Null(only.CanPush);

        // The plain APIEntitiesDeployKey shape carries project access - not just on the create response.
        Assert.Collection(only.ProjectsWithWriteAccess ?? [],
            project =>
            {
                Assert.Equal(7, project.Id);
                Assert.Equal("acme/shop", project.PathWithNamespace);
            },
            project =>
            {
                Assert.Equal(9, project.Id);
                Assert.Equal("acme/api", project.PathWithNamespace);
            });
        GitLabProjectIdentity readonlyProject = Assert.Single(only.ProjectsWithReadonlyAccess ?? []);
        Assert.Equal(8, readonlyProject.Id);
        Assert.Equal("acme/docs", readonlyProject.PathWithNamespace);
    }

    [Fact]
    public async Task ListAllAsync_OmitsThePublicFilterWhenItIsNotSet()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri(BaseAddress) };
        GitLabApiConnection connection = new(httpClient);
        DeployKeysClient repository = new(connection);

        await foreach (GitLabDeployKey _ in repository.ListAllAsync(
                           cancellationToken: TestContext.Current.CancellationToken))
        {
            Assert.Fail("The stub returns an empty page.");
        }

        Assert.Equal("https://gitlab.example/api/v4/deploy_keys", handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task CreateAsync_LegacyProjectRequestProjectsToTheInstanceDeployKeySchema()
    {
        string json = $$"""
                        {
                          "id": 21,
                          "title": "Instance-wide key",
                          "key": "{{PublicKey}}",
                          "created_at": "2026-02-01T09:00:00Z",
                          "expires_at": "2027-02-01T00:00:00Z",
                          "usage_type": "auth",
                          "fingerprint": "aa:bb:cc:dd:ee:ff:00:11:22:33:44:55:66:77:88:99",
                          "fingerprint_sha256": "SHA256:abcd1234",
                          "projects_with_write_access": [
                            {
                              "id": 7,
                              "name": "shop",
                              "path": "shop",
                              "path_with_namespace": "acme/shop"
                            }
                          ],
                          "projects_with_readonly_access": [
                            {
                              "id": 8,
                              "name": "docs",
                              "path": "docs",
                              "path_with_namespace": "acme/docs"
                            }
                          ]
                        }
                        """;

        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri(BaseAddress) };
        GitLabApiConnection connection = new(httpClient);
        DeployKeysClient repository = new(connection);

        CreateDeployKeyRequest request = new()
        {
            Key = PublicKey,
            Title = "Instance-wide key",
            CanPush = true,
            ExpiresAt = new DateTimeOffset(2027, 2, 1, 0, 0, 0, TimeSpan.Zero)
        };

        GitLabDeployKey key = await repository.CreateAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/deploy_keys", handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Contains("\"title\":\"Instance-wide key\"", sentBody, StringComparison.Ordinal);

        // The instance-wide request has no can_push - it is never sent, even though the legacy DTO carries it.
        Assert.DoesNotContain("can_push", sentBody, StringComparison.Ordinal);

        Assert.Equal(21, key.Id);
        Assert.Null(key.CanPush);
        GitLabProjectIdentity writeProject = Assert.Single(key.ProjectsWithWriteAccess ?? []);
        Assert.Equal(7, writeProject.Id);
        Assert.Equal("acme/shop", writeProject.PathWithNamespace);
        GitLabProjectIdentity readonlyProject = Assert.Single(key.ProjectsWithReadonlyAccess ?? []);
        Assert.Equal(8, readonlyProject.Id);
        Assert.Equal("acme/docs", readonlyProject.PathWithNamespace);
    }

    [Fact]
    public async Task CreateForInstanceAsync_PostsOnlyTheInstanceDeployKeySchema()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent($$"""{ "id": 21, "title": "Instance-wide key", "key": "{{PublicKey}}" }""",
                    Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri(BaseAddress) };
        DeployKeysClient repository = new(new GitLabApiConnection(httpClient));

        GitLabDeployKey key = await repository.CreateForInstanceAsync(
            new CreateInstanceDeployKeyRequest
            {
                Key = PublicKey,
                Title = "Instance-wide key",
                ExpiresAt = new DateTimeOffset(2027, 2, 1, 0, 0, 0, TimeSpan.Zero)
            }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/deploy_keys", handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Contains("\"key\":\"ssh-ed25519 ", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"title\":\"Instance-wide key\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"expires_at\":\"2027-02-01T00:00:00+00:00\"", sentBody, StringComparison.Ordinal);
        Assert.DoesNotContain("can_push", sentBody, StringComparison.Ordinal);
        Assert.Equal(21, key.Id);
    }

    [Fact]
    public async Task GetAsync_OnNotFoundResponse_ThrowsGitLabNotFoundException()
    {
        const string Json = """{ "message": "404 Deploy Key Not Found" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri(BaseAddress) };
        GitLabApiConnection connection = new(httpClient);
        DeployKeysClient repository = new(connection);

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabNotFoundException>(() =>
            repository.GetAsync(5, 999, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        Assert.Equal("404 Deploy Key Not Found", exception.Message);
    }

    [Fact]
    public async Task AddAsync_OnValidationError_ThrowsGitLabValidationException()
    {
        const string Json = """{ "message": { "key": ["has already been taken"] } }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri(BaseAddress) };
        GitLabApiConnection connection = new(httpClient);
        DeployKeysClient repository = new(connection);

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabValidationException>(() =>
            repository.AddAsync(5, new CreateDeployKeyRequest { Key = PublicKey, Title = "Duplicate" },
                TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);
    }

    [Fact]
    public async Task ListForUserAsync_BuildsUserProjectDeployKeysRoute_WithPaging_AndDeserializesEachKey()
    {
        string json = $$"""
                        [
                          {
                            "id": 30,
                            "title": "Personal laptop",
                            "key": "{{PublicKey}}",
                            "can_push": true
                          }
                        ]
                        """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri(BaseAddress) };
        GitLabApiConnection connection = new(httpClient);
        DeployKeysClient repository = new(connection);

        List<GitLabDeployKey> keys = new();
        await foreach (GitLabDeployKey key in repository.ListForUserAsync(7,
                           new UserProjectDeployKeyListOptions { Page = 2, PerPage = 50 },
                           TestContext.Current.CancellationToken))
        {
            keys.Add(key);
        }

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/users/7/project_deploy_keys?page=2&per_page=50",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabDeployKey only = Assert.Single(keys);
        Assert.Equal(30, only.Id);
        Assert.Equal("Personal laptop", only.Title);
        Assert.True(only.CanPush);
    }

    [Fact]
    public async Task ListForUserAsync_OmitsQueryString_WhenOptionsAreNotSet()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri(BaseAddress) };
        GitLabApiConnection connection = new(httpClient);
        DeployKeysClient repository = new(connection);

        await foreach (GitLabDeployKey _ in repository.ListForUserAsync(7,
                           cancellationToken: TestContext.Current.CancellationToken))
        {
            Assert.Fail("The stub returns an empty page.");
        }

        Assert.Equal("https://gitlab.example/api/v4/users/7/project_deploy_keys",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task ListForUserAsync_WithUsername_EscapesTheUserRouteSegment()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri(BaseAddress) };
        DeployKeysClient repository = new(new GitLabApiConnection(httpClient));

        await foreach (GitLabDeployKey _ in repository.ListForUserAsync("release/bot",
                           cancellationToken: TestContext.Current.CancellationToken))
        {
            Assert.Fail("The stub returns an empty page.");
        }

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/users/release%2Fbot/project_deploy_keys",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }
}