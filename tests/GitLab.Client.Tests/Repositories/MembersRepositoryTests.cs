using System.Net;
using System.Text;

using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Repositories;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Repositories;

public sealed class MembersRepositoryTests
{
    /// <summary>
    ///     A group member as GitLab actually sends it, including the members this library does not model
    ///     (<c>group_saml_identity</c>, <c>group_scim_identity</c>, <c>member_role</c>) - the context's
    ///     UnmappedMemberHandling.Skip is what keeps those from failing the deserialization.
    /// </summary>
    private const string FullMemberJson = """
                                          {
                                            "id": 29,
                                            "username": "john_smith",
                                            "name": "John Smith",
                                            "state": "active",
                                            "locked": false,
                                            "public_email": "john@example.com",
                                            "email": "john.smith@example.com",
                                            "custom_attributes": [
                                              { "key": "employee_id", "value": "4471" }
                                            ],
                                            "avatar_url": "https://gitlab.example/uploads/-/system/user/avatar/29/avatar.png",
                                            "avatar_path": "/uploads/-/system/user/avatar/29/avatar.png",
                                            "web_url": "https://gitlab.example/john_smith",
                                            "access_level": 30,
                                            "created_at": "2025-09-22T14:13:35Z",
                                            "created_by": {
                                              "id": 1,
                                              "username": "root",
                                              "name": "Administrator",
                                              "state": "active",
                                              "web_url": "https://gitlab.example/root"
                                            },
                                            "expires_at": "2026-12-31",
                                            "two_factor_enabled": true,
                                            "is_using_seat": true,
                                            "override": false,
                                            "membership_state": "active",
                                            "group_saml_identity": {
                                              "provider": "group_saml",
                                              "extern_uid": "4085",
                                              "saml_provider_id": 52
                                            },
                                            "group_scim_identity": {
                                              "extern_uid": "4085",
                                              "group_id": 9,
                                              "active": true
                                            },
                                            "member_role": {
                                              "id": 2,
                                              "name": "Custom role",
                                              "base_access_level": 30
                                            }
                                          }
                                          """;

    [Fact]
    public async Task ListAsync_BuildsMembersRoute_AndDeserializesEachMember()
    {
        const string Json = """
                            [
                              {
                                "id": 1,
                                "username": "raymond_smith",
                                "name": "Raymond Smith",
                                "state": "active",
                                "avatar_url": "https://gitlab.example/uploads/-/system/user/avatar/1/avatar.png",
                                "web_url": "https://gitlab.example/raymond_smith",
                                "access_level": 30
                              },
                              {
                                "id": 2,
                                "username": "john_doe",
                                "name": "John Doe",
                                "state": "active",
                                "web_url": "https://gitlab.example/john_doe",
                                "access_level": 40,
                                "expires_at": "2026-12-31"
                              }
                            ]
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        MembersRepository repository = new(connection);

        List<GitLabMember> members = new();
        await foreach (GitLabMember member in repository.ListAsync(ProjectId.FromId(5),
                           TestContext.Current.CancellationToken))
        {
            members.Add(member);
        }

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/5/members", handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(2, members.Count);
        Assert.Equal("raymond_smith", members[0].Username);
        Assert.Equal(30, members[0].AccessLevel);
        Assert.Null(members[0].ExpiresAt);
        Assert.Equal("john_doe", members[1].Username);
        Assert.Equal(40, members[1].AccessLevel);
        Assert.Equal(new DateOnly(2026, 12, 31), members[1].ExpiresAt);
    }

    [Fact]
    public async Task ListAsync_EncodesNamespacedProjectPath()
    {
        const string Json = "[]";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        MembersRepository repository = new(connection);

        await foreach (GitLabMember _ in repository.ListAsync(ProjectId.FromPath("gitlab-org/gitlab"),
                           TestContext.Current.CancellationToken))
        {
            // Draining the list is enough to trigger the request; the route is asserted below.
        }

        Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/members",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task GetAsync_BuildsMemberRoute_AndDeserializesMember()
    {
        const string Json = """
                            {
                              "id": 1,
                              "username": "raymond_smith",
                              "name": "Raymond Smith",
                              "state": "active",
                              "avatar_url": "https://gitlab.example/uploads/-/system/user/avatar/1/avatar.png",
                              "web_url": "https://gitlab.example/raymond_smith",
                              "access_level": 30
                            }
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        MembersRepository repository = new(connection);

        GitLabMember member = await repository.GetAsync(ProjectId.FromId(5), 1, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/5/members/1",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(1, member.Id);
        Assert.Equal("raymond_smith", member.Username);
        Assert.Equal("Raymond Smith", member.Name);
        Assert.Equal("active", member.State);
        Assert.Equal(new Uri("https://gitlab.example/uploads/-/system/user/avatar/1/avatar.png"), member.AvatarUrl);
        Assert.Equal(new Uri("https://gitlab.example/raymond_smith"), member.WebUrl);
        Assert.Equal(30, member.AccessLevel);
    }

    [Fact]
    public async Task GetAsync_OnErrorResponse_ThrowsGitLabApiExceptionWithMessage()
    {
        const string Json = """{ "message": "404 Member Not Found" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        MembersRepository repository = new(connection);

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabNotFoundException>(() =>
            repository.GetAsync(ProjectId.FromId(5), 999, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        Assert.Equal("404 Member Not Found", exception.Message);
    }

    [Fact]
    public async Task AddAsync_PostsToMembersRoute_WithSerializedBody_AndDeserializesAddedMember()
    {
        const string Json = """
                            {
                              "id": 3,
                              "username": "new_member",
                              "name": "New Member",
                              "state": "active",
                              "web_url": "https://gitlab.example/new_member",
                              "access_level": 30,
                              "expires_at": "2026-12-31"
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
        MembersRepository repository = new(connection);

        AddMemberRequest request = new() { UserId = 3, AccessLevel = 30, ExpiresAt = new DateOnly(2026, 12, 31) };

        GitLabMember member =
            await repository.AddAsync(ProjectId.FromId(5), request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/5/members", handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("application/json", handler.LastRequest?.Content?.Headers.ContentType?.MediaType);
        Assert.Contains("\"user_id\":3", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"access_level\":30", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"expires_at\":\"2026-12-31\"", sentBody, StringComparison.Ordinal);
        Assert.Equal(3, member.Id);
        Assert.Equal("new_member", member.Username);
        Assert.Equal(30, member.AccessLevel);
        Assert.Equal(new DateOnly(2026, 12, 31), member.ExpiresAt);
    }

    [Fact]
    public async Task AddAsync_WithUsernameInsteadOfUserId_SendsUsernameAndInviteSource()
    {
        const string Json = """
                            {
                              "id": 3,
                              "username": "new_member",
                              "name": "New Member",
                              "state": "active",
                              "web_url": "https://gitlab.example/new_member",
                              "access_level": 30
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
        MembersRepository repository = new(connection);

        AddMemberRequest request = new() { AccessLevel = 30, Username = "new_member", InviteSource = "members-page" };

        await repository.AddAsync(ProjectId.FromId(5), request, TestContext.Current.CancellationToken);

        Assert.Contains("\"username\":\"new_member\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"invite_source\":\"members-page\"", sentBody, StringComparison.Ordinal);

        // user_id and username are mutually exclusive; leaving user_id unset must omit it, not send null.
        Assert.DoesNotContain("user_id", sentBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task UpdateAsync_PutsToMemberRoute_WithSerializedBody_AndDeserializesUpdatedMember()
    {
        const string Json = """
                            {
                              "id": 1,
                              "username": "raymond_smith",
                              "name": "Raymond Smith",
                              "state": "active",
                              "web_url": "https://gitlab.example/raymond_smith",
                              "access_level": 40
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
        MembersRepository repository = new(connection);

        UpdateMemberRequest request = new() { AccessLevel = 40 };

        GitLabMember member =
            await repository.UpdateAsync(ProjectId.FromId(5), 1, request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/5/members/1",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("application/json", handler.LastRequest?.Content?.Headers.ContentType?.MediaType);
        Assert.Contains("\"access_level\":40", sentBody, StringComparison.Ordinal);
        Assert.DoesNotContain("expires_at", sentBody, StringComparison.Ordinal);
        Assert.Equal(40, member.AccessLevel);
    }

    [Fact]
    public async Task UpdateAsync_WithMemberRoleId_SendsMemberRoleId()
    {
        const string Json = """
                            {
                              "id": 1,
                              "username": "raymond_smith",
                              "name": "Raymond Smith",
                              "state": "active",
                              "web_url": "https://gitlab.example/raymond_smith",
                              "access_level": 40
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
        MembersRepository repository = new(connection);

        UpdateMemberRequest request = new() { AccessLevel = 40, MemberRoleId = 7 };

        await repository.UpdateAsync(ProjectId.FromId(5), 1, request, TestContext.Current.CancellationToken);

        Assert.Contains("\"access_level\":40", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"member_role_id\":7", sentBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task UpdateAsync_OnErrorResponse_ThrowsGitLabApiExceptionWithMessage()
    {
        const string Json = """{ "message": "403 Forbidden" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Forbidden)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        MembersRepository repository = new(connection);

        UpdateMemberRequest request = new() { AccessLevel = 50 };

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabForbiddenException>(() =>
            repository.UpdateAsync(ProjectId.FromId(5), 1, request, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.Forbidden, exception.StatusCode);
        Assert.Equal("403 Forbidden", exception.Message);
    }

    [Fact]
    public async Task RemoveAsync_DeletesMemberRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        MembersRepository repository = new(connection);

        await repository.RemoveAsync(ProjectId.FromId(5), 1, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/5/members/1",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task RemoveAsync_OnErrorResponse_ThrowsGitLabApiExceptionWithMessage()
    {
        const string Json = """{ "message": "404 Not Found" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        MembersRepository repository = new(connection);

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabNotFoundException>(() =>
            repository.RemoveAsync(ProjectId.FromId(5), 999, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        Assert.Equal("404 Not Found", exception.Message);
    }

    [Fact]
    public async Task ListForGroupAsync_BuildsGroupMembersRoute_WithEveryFilter()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        MembersRepository repository = new(connection);

        GroupMemberListOptions options = new()
        {
            Query = "smith",
            UserIds = [1, 2],
            SkipUsers = [3],
            ShowSeatInfo = true,
            WithSamlIdentity = false,
            PerPage = 50,
            Page = 2
        };

        await foreach (GitLabMember _ in repository.ListForGroupAsync(GroupId.FromId(9), options,
                           TestContext.Current.CancellationToken))
        {
            // Draining the sequence is what issues the request; the route is asserted below.
        }

        string? requestUri = handler.LastRequest?.RequestUri?.AbsoluteUri;
        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.StartsWith("https://gitlab.example/api/v4/groups/9/members?", requestUri, StringComparison.Ordinal);
        Assert.Contains("query=smith", requestUri, StringComparison.Ordinal);
        Assert.Contains("user_ids[]=1", requestUri, StringComparison.Ordinal);
        Assert.Contains("user_ids[]=2", requestUri, StringComparison.Ordinal);
        Assert.Contains("skip_users[]=3", requestUri, StringComparison.Ordinal);
        Assert.Contains("show_seat_info=true", requestUri, StringComparison.Ordinal);
        Assert.Contains("with_saml_identity=false", requestUri, StringComparison.Ordinal);
        Assert.Contains("per_page=50", requestUri, StringComparison.Ordinal);
        Assert.Contains("page=2", requestUri, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ListForGroupAsync_EncodesNamespacedGroupPath()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        MembersRepository repository = new(connection);

        await foreach (GitLabMember _ in repository.ListForGroupAsync(GroupId.FromPath("group/subgroup"),
                           cancellationToken: TestContext.Current.CancellationToken))
        {
            // Draining the sequence is what issues the request; the route is asserted below.
        }

        Assert.Equal("https://gitlab.example/api/v4/groups/group%2Fsubgroup/members",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task ListIncludingInheritedForProjectAsync_BuildsMembersAllRoute_AndEncodesNamespacedProjectPath()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        MembersRepository repository = new(connection);

        AllMemberListOptions options = new() { State = GitLabMembershipState.Awaiting, ShowSeatInfo = true, Page = 2 };

        await foreach (GitLabMember _ in repository.ListIncludingInheritedForProjectAsync(
                           ProjectId.FromPath("group/subgroup/project"), options,
                           TestContext.Current.CancellationToken))
        {
            // Draining the sequence is what issues the request; the route is asserted below.
        }

        string? requestUri = handler.LastRequest?.RequestUri?.AbsoluteUri;
        Assert.StartsWith("https://gitlab.example/api/v4/projects/group%2Fsubgroup%2Fproject/members/all?",
            requestUri, StringComparison.Ordinal);
        Assert.Contains("show_seat_info=true", requestUri, StringComparison.Ordinal);
        Assert.Contains("page=2", requestUri, StringComparison.Ordinal);

        // The filter value comes from the enum's [JsonStringEnumMemberName], not from a hand-written literal.
        Assert.Contains("state=awaiting", requestUri, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ListIncludingInheritedForGroupAsync_BuildsGroupMembersAllRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        MembersRepository repository = new(connection);

        await foreach (GitLabMember _ in repository.ListIncludingInheritedForGroupAsync(GroupId.FromId(9),
                           cancellationToken: TestContext.Current.CancellationToken))
        {
            // Draining the sequence is what issues the request; the route is asserted below.
        }

        Assert.Equal("https://gitlab.example/api/v4/groups/9/members/all",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task GetIncludingInheritedForGroupAsync_BuildsRoute_AndDeserializesTheFullMemberShape()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(FullMemberJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        MembersRepository repository = new(connection);

        GitLabMember member =
            await repository.GetIncludingInheritedForGroupAsync(GroupId.FromId(9), 29,
                TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/9/members/all/29",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(29, member.Id);
        Assert.Equal("john_smith", member.Username);
        Assert.Equal(30, member.AccessLevel);
        Assert.False(member.Locked);
        Assert.Equal("john@example.com", member.PublicEmail);
        Assert.Equal("john.smith@example.com", member.Email);
        GitLabCustomAttribute attribute = Assert.Single(member.CustomAttributes ?? []);
        Assert.Equal("employee_id", attribute.Key);
        Assert.Equal("4471", attribute.Value);
        Assert.Equal("/uploads/-/system/user/avatar/29/avatar.png", member.AvatarPath);
        Assert.Equal(new DateTimeOffset(2025, 9, 22, 14, 13, 35, TimeSpan.Zero), member.CreatedAt);
        Assert.Equal("root", member.CreatedBy?.Username);
        Assert.Equal(new DateOnly(2026, 12, 31), member.ExpiresAt);
        Assert.True(member.TwoFactorEnabled);
        Assert.True(member.IsUsingSeat);
        Assert.False(member.Override);
        Assert.Equal(GitLabMembershipState.Active, member.MembershipState);
    }

    [Fact]
    public async Task GetForGroupAsync_BuildsDirectGroupMemberRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(FullMemberJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        MembersRepository repository = new(connection);

        GitLabMember member =
            await repository.GetForGroupAsync(GroupId.FromId(9), 29, TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/groups/9/members/29",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(29, member.Id);
    }

    [Fact]
    public async Task AddForGroupAsync_PostsToGroupMembersRoute_WithSerializedBody()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(FullMemberJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        MembersRepository repository = new(connection);

        AddGroupMemberRequest request = new()
        {
            AccessLevel = 30,
            Username = "john_smith",
            ExpiresAt = new DateOnly(2026, 12, 31),
            InviteSource = "members-page"
        };

        GitLabMember member =
            await repository.AddForGroupAsync(GroupId.FromId(9), request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/9/members", handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Contains("\"access_level\":30", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"username\":\"john_smith\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"expires_at\":\"2026-12-31\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"invite_source\":\"members-page\"", sentBody, StringComparison.Ordinal);

        // Unset members must be omitted rather than sent as null - user_id and username are mutually exclusive.
        Assert.DoesNotContain("user_id", sentBody, StringComparison.Ordinal);
        Assert.Equal(29, member.Id);
    }

    [Fact]
    public async Task UpdateForGroupAsync_PutsToGroupMemberRoute_WithSerializedBody()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(FullMemberJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        MembersRepository repository = new(connection);

        UpdateGroupMemberRequest request = new() { AccessLevel = 40, MemberRoleId = 7 };

        await repository.UpdateForGroupAsync(GroupId.FromId(9), 29, request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/9/members/29",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Contains("\"access_level\":40", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"member_role_id\":7", sentBody, StringComparison.Ordinal);
        Assert.DoesNotContain("expires_at", sentBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task RemoveForGroupAsync_DeletesGroupMemberRoute_WithRemovalSwitches()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        MembersRepository repository = new(connection);

        RemoveGroupMemberOptions options = new() { SkipSubresources = true, UnassignIssuables = true };

        await repository.RemoveForGroupAsync(GroupId.FromId(9), 29, options, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/9/members/29?skip_subresources=true&unassign_issuables=true",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task RemoveForGroupAsync_WithoutOptions_SendsNoQueryString()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        MembersRepository repository = new(connection);

        await repository.RemoveForGroupAsync(GroupId.FromId(9), 29,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/groups/9/members/29",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task ApproveForGroupAsync_PutsToApproveRoute_WithoutABody()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        MembersRepository repository = new(connection);

        await repository.ApproveForGroupAsync(GroupId.FromId(9), 168, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/9/members/168/approve",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Null(handler.LastRequest?.Content);
    }

    [Fact]
    public async Task ApproveAllForGroupAsync_PostsToApproveAllRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Created));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        MembersRepository repository = new(connection);

        await repository.ApproveAllForGroupAsync(GroupId.FromId(9), TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/9/members/approve_all",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task UpdateStateForGroupAsync_PutsToStateRoute_WithTheStateParameter()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        MembersRepository repository = new(connection);

        await repository.UpdateStateForGroupAsync(GroupId.FromId(9), 29, GitLabMembershipState.Active,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/9/members/29/state?state=active",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task SetOverrideForGroupAsync_PostsToOverrideRoute_AndDeserializesTheMember()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = new StringContent(FullMemberJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        MembersRepository repository = new(connection);

        GitLabMember member =
            await repository.SetOverrideForGroupAsync(GroupId.FromId(9), 29, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/9/members/29/override",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Null(handler.LastRequest?.Content);
        Assert.Equal(29, member.Id);
    }

    [Fact]
    public async Task RemoveOverrideForGroupAsync_DeletesOverrideRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(FullMemberJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        MembersRepository repository = new(connection);

        await repository.RemoveOverrideForGroupAsync(GroupId.FromId(9), 29, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/9/members/29/override",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task ListPendingForGroupAsync_BuildsPendingMembersRoute_AndDeserializesUnredeemedInvitations()
    {
        const string Json = """
                            [
                              {
                                "id": 168,
                                "name": "Alex Garcia",
                                "username": "alex",
                                "email": "alex@example.com",
                                "avatar_url": "https://gitlab.example/uploads/-/system/user/avatar/168/avatar.png",
                                "web_url": "https://gitlab.example/alex",
                                "approved": false,
                                "invited": false
                              },
                              {
                                "email": "invitee@example.com",
                                "avatar_url": null,
                                "approved": false,
                                "invited": true
                              }
                            ]
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        MembersRepository repository = new(connection);

        List<GitLabPendingMember> pending = new();
        await foreach (GitLabPendingMember member in repository.ListPendingForGroupAsync(GroupId.FromId(9),
                           TestContext.Current.CancellationToken))
        {
            pending.Add(member);
        }

        Assert.Equal("https://gitlab.example/api/v4/groups/9/pending_members",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(2, pending.Count);
        Assert.Equal(168, pending[0].Id);
        Assert.Equal("alex", pending[0].Username);
        Assert.False(pending[0].Invited);

        // An invitation nobody has redeemed yet carries no user at all - only the email address.
        Assert.Null(pending[1].Id);
        Assert.Null(pending[1].Username);
        Assert.Null(pending[1].AvatarUrl);
        Assert.Equal("invitee@example.com", pending[1].Email);
        Assert.True(pending[1].Invited);
    }

    [Fact]
    public async Task ListBillableMembershipsForGroupAsync_BuildsMembershipsRoute_AndDeserializesTheAccessLevelObject()
    {
        const string Json = """
                            [
                              {
                                "id": 168,
                                "source_id": 131,
                                "source_full_name": "Root group / Sub group",
                                "source_members_url": "https://gitlab.example/groups/root/sub/-/group_members",
                                "created_at": "2025-03-31T17:28:44.812Z",
                                "expires_at": "2026-03-21",
                                "access_level": {
                                  "string_value": "Developer",
                                  "integer_value": 30
                                }
                              }
                            ]
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        MembersRepository repository = new(connection);

        List<GitLabBillableMembership> memberships = new();
        await foreach (GitLabBillableMembership membership in repository.ListBillableMembershipsForGroupAsync(
                           GroupId.FromPath("root"), 29, TestContext.Current.CancellationToken))
        {
            memberships.Add(membership);
        }

        Assert.Equal("https://gitlab.example/api/v4/groups/root/billable_members/29/memberships",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabBillableMembership only = Assert.Single(memberships);
        Assert.Equal(168, only.Id);
        Assert.Equal(131, only.SourceId);
        Assert.Equal("Root group / Sub group", only.SourceFullName);
        Assert.Equal(new Uri("https://gitlab.example/groups/root/sub/-/group_members"), only.SourceMembersUrl);
        Assert.Equal(new DateOnly(2026, 3, 21), only.ExpiresAt);
        Assert.Equal(30, only.AccessLevel?.IntegerValue);
        Assert.Equal("Developer", only.AccessLevel?.StringValue);
    }

    [Fact]
    public async Task ListIndirectBillableMembershipsForGroupAsync_BuildsIndirectRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        MembersRepository repository = new(connection);

        await foreach (GitLabBillableMembership _ in repository.ListIndirectBillableMembershipsForGroupAsync(
                           GroupId.FromId(9), 29, TestContext.Current.CancellationToken))
        {
            // Draining the sequence is what issues the request; the route is asserted below.
        }

        Assert.Equal("https://gitlab.example/api/v4/groups/9/billable_members/29/indirect",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task RemoveBillableMemberForGroupAsync_DeletesBillableMemberRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        MembersRepository repository = new(connection);

        await repository.RemoveBillableMemberForGroupAsync(GroupId.FromId(9), 29,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/9/billable_members/29",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task GetForGroupAsync_OnForbiddenResponse_ThrowsGitLabForbiddenException()
    {
        const string Json = """{ "message": "403 Forbidden" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Forbidden)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        MembersRepository repository = new(connection);

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabForbiddenException>(() =>
            repository.GetForGroupAsync(GroupId.FromId(9), 29, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.Forbidden, exception.StatusCode);
        Assert.Equal("403 Forbidden", exception.Message);
    }
}