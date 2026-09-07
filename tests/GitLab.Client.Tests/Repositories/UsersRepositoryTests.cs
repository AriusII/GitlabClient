using System.Net;
using System.Text;

using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Repositories;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Repositories;

public sealed class UsersRepositoryTests
{
    /// <summary>
    ///     The administrator's view of an account - GitLab's <c>UserWithAdmin</c> entity - which is the
    ///     widest shape <see cref="GitLabUser" /> has to survive. Every member beyond the five the basic
    ///     embedding carries is nullable precisely so the same type deserializes both.
    /// </summary>
    private const string AdminUserJson = """
                                         {
                                           "id": 7,
                                           "username": "octocat",
                                           "name": "Octo Cat",
                                           "state": "active",
                                           "locked": false,
                                           "public_email": "octo@example.com",
                                           "avatar_url": "https://gitlab.example/avatar.png",
                                           "avatar_path": "/uploads/-/system/user/avatar/7/avatar.png",
                                           "web_url": "https://gitlab.example/octocat",
                                           "created_at": "2012-05-23T08:00:58Z",
                                           "bio": "Ships things.",
                                           "location": "Lyon",
                                           "linkedin": "",
                                           "twitter": "",
                                           "discord": "",
                                           "website_url": "",
                                           "github": "",
                                           "job_title": "Engineer",
                                           "pronouns": null,
                                           "organization": "Acme",
                                           "bot": false,
                                           "work_information": "Engineer at Acme",
                                           "followers": 3,
                                           "following": 4,
                                           "is_followed": true,
                                           "local_time": "3:38 PM",
                                           "last_sign_in_at": "2026-01-02T03:04:05Z",
                                           "confirmed_at": "2012-05-23T09:05:22Z",
                                           "last_activity_on": "2026-02-14",
                                           "email": "octo@internal.example.com",
                                           "theme_id": 1,
                                           "color_scheme_id": 2,
                                           "projects_limit": 100,
                                           "current_sign_in_at": "2026-02-14T07:00:00Z",
                                           "identities": [
                                             { "provider": "github", "extern_uid": "2435223452345" },
                                             { "provider": "group_saml", "extern_uid": "1", "saml_provider_id": 42 }
                                           ],
                                           "can_create_group": true,
                                           "can_create_project": true,
                                           "two_factor_enabled": true,
                                           "external": false,
                                           "private_profile": false,
                                           "commit_email": "_private",
                                           "preferred_language": "en",
                                           "shared_runners_minutes_limit": 133,
                                           "extra_shared_runners_minutes_limit": null,
                                           "is_admin": false,
                                           "note": "Contractor until June.",
                                           "namespace_id": 11,
                                           "provisioned_by_project_id": null,
                                           "created_by": {
                                             "id": 1,
                                             "username": "root",
                                             "name": "Administrator",
                                             "web_url": "https://gitlab.example/root"
                                           },
                                           "using_license_seat": true,
                                           "is_auditor": false,
                                           "provisioned_by_group_id": null,
                                           "enterprise_group_id": 90,
                                           "enterprise_group_associated_at": "2025-11-01T00:00:00Z"
                                         }
                                         """;

    private const string BasicUserJson =
        """{ "id": 3, "username": "alice", "name": "Alice", "web_url": "https://gitlab.example/alice" }""";

    [Fact]
    public async Task GetAsync_RequestsTheNumericUserRoute_AndDeserializesTheResponse()
    {
        const string Json = """
                            {
                              "id": 7,
                              "username": "octocat",
                              "name": "Octo Cat",
                              "state": "active",
                              "locked": false,
                              "public_email": "octo@example.com",
                              "avatar_url": "https://gitlab.example/avatar.png",
                              "web_url": "https://gitlab.example/octocat"
                            }
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        UsersRepository repository = new(connection);

        GitLabUser user = await repository.GetAsync(7, TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/users/7", handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(7, user.Id);
        Assert.Equal("octocat", user.Username);
        Assert.Equal("Octo Cat", user.Name);
        Assert.Equal("active", user.State);
        Assert.False(user.Locked);
        Assert.Equal("octo@example.com", user.PublicEmail);
        Assert.Equal(new Uri("https://gitlab.example/avatar.png"), user.AvatarUrl);
        Assert.Equal(new Uri("https://gitlab.example/octocat"), user.WebUrl);
    }

    [Fact]
    public async Task GetAsync_DeserializesTheAdministratorWidthOfTheUserEntity()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(AdminUserJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        UsersRepository repository = new(connection);

        GitLabUser user = await repository.GetAsync(7, TestContext.Current.CancellationToken);

        Assert.Equal("/uploads/-/system/user/avatar/7/avatar.png", user.AvatarPath);
        Assert.Equal(new DateTimeOffset(2012, 5, 23, 8, 0, 58, TimeSpan.Zero), user.CreatedAt);
        Assert.Equal("Ships things.", user.Bio);
        Assert.Equal("Acme", user.Organization);
        Assert.Equal("Engineer at Acme", user.WorkInformation);
        Assert.Equal(3, user.Followers);
        Assert.Equal(4, user.Following);
        Assert.True(user.IsFollowed);
        Assert.Equal("3:38 PM", user.LocalTime);

        // A plain date, not a timestamp - typing it as DateTimeOffset would fail to parse "2026-02-14".
        Assert.Equal(new DateOnly(2026, 2, 14), user.LastActivityOn);

        // GitLab sends "" rather than null for an unset website, which is why it is not a Uri.
        Assert.Equal(string.Empty, user.WebsiteUrl);

        Assert.Equal("octo@internal.example.com", user.Email);
        Assert.Equal(100, user.ProjectsLimit);
        Assert.Equal(133, user.SharedRunnersMinutesLimit);
        Assert.Null(user.ExtraSharedRunnersMinutesLimit);
        Assert.False(user.IsAdmin);
        Assert.Equal("Contractor until June.", user.Note);
        Assert.Equal(11, user.NamespaceId);
        Assert.True(user.UsingLicenseSeat);
        Assert.Equal(90, user.EnterpriseGroupId);

        Assert.Equal("root", user.CreatedBy?.Username);

        Assert.NotNull(user.Identities);
        Assert.Equal(2, user.Identities.Count);
        Assert.Equal("github", user.Identities[0].Provider);
        Assert.Null(user.Identities[0].SamlProviderId);
        Assert.Equal(42, user.Identities[1].SamlProviderId);
    }

    [Fact]
    public async Task GetCurrentAsync_RequestsTheSingularUserRoute_WithoutATrailingIdSegment()
    {
        const string Json =
            """{ "id": 1, "username": "self", "name": "Current User", "web_url": "https://gitlab.example/self" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        UsersRepository repository = new(connection);

        GitLabUser user = await repository.GetCurrentAsync(TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/user", handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(1, user.Id);
        Assert.Equal("self", user.Username);
    }

    [Fact]
    public async Task ListAsync_BuildsTheQueryString_FromTheProvidedOptions()
    {
        const string Json =
            """[{ "id": 3, "username": "alice", "name": "Alice", "web_url": "https://gitlab.example/alice" }]""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        UsersRepository repository = new(connection);

        List<GitLabUser> users = new();
        await foreach (GitLabUser user in repository.ListAsync(new UserListOptions { Username = "alice", PerPage = 20 },
                           TestContext.Current.CancellationToken))
        {
            users.Add(user);
        }

        Assert.Equal("https://gitlab.example/api/v4/users?username=alice&per_page=20",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Single(users);
        Assert.Equal("alice", users[0].Username);
    }

    [Fact]
    public async Task ListAsync_ProjectsEveryAdministratorFilter_InDeclarationOrder()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent($"[{BasicUserJson}]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        UsersRepository repository = new(connection);

        UserListOptions options = new()
        {
            Username = "alice",
            ExternUid = "uid-1",
            Provider = "ldapmain",
            Search = "ali",
            Blocked = false,
            Admins = true,
            TwoFactor = "enabled",
            CreatedAfter = new DateTimeOffset(2024, 1, 2, 3, 4, 5, TimeSpan.Zero),
            WithoutProjectBots = true,
            ExcludeInternal = true,
            SkipLdap = false,
            WithCustomAttributes = true,
            OrderBy = GitLabUserOrderBy.CreatedAt,
            Sort = GitLabUserSort.Asc,
            PerPage = 50
        };

        await foreach (GitLabUser _ in repository.ListAsync(options, TestContext.Current.CancellationToken))
        {
            // Draining the sequence is what issues the request.
        }

        Assert.Equal(
            "https://gitlab.example/api/v4/users?username=alice&extern_uid=uid-1&provider=ldapmain&search=ali"
            + "&blocked=false&admins=true&two_factor=enabled&created_after=2024-01-02T03:04:05Z"
            + "&without_project_bots=true&exclude_internal=true&skip_ldap=false&with_custom_attributes=true"
            + "&order_by=created_at&sort=asc&per_page=50",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task GetAsync_OnErrorResponse_ThrowsGitLabApiExceptionWithMessage()
    {
        const string Json = """{ "message": "404 User Not Found" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        UsersRepository repository = new(connection);

        GitLabApiException exception =
            await Assert.ThrowsAsync<GitLabNotFoundException>(() =>
                repository.GetAsync(999, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        Assert.Equal("404 User Not Found", exception.Message);
    }

    [Fact]
    public async Task GetCountsAsync_RequestsTheInstanceLevelCountsRoute()
    {
        const string Json = """
                            {
                              "merge_requests": 4,
                              "assigned_issues": 15,
                              "assigned_merge_requests": 4,
                              "review_requested_merge_requests": 2,
                              "todos": 9
                            }
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        UsersRepository repository = new(connection);

        GitLabUserCounts counts = await repository.GetCountsAsync(TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/user_counts", handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(15, counts.AssignedIssues);
        Assert.Equal(4, counts.AssignedMergeRequests);
        Assert.Equal(2, counts.ReviewRequestedMergeRequests);
        Assert.Equal(9, counts.Todos);
    }

    [Fact]
    public async Task CreateAsync_PostsTheSerializedBody_AndOmitsUnsetMembers()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(AdminUserJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        UsersRepository repository = new(connection);

        GitLabUser created = await repository.CreateAsync(
            new CreateUserRequest
            {
                Email = "octo@example.com",
                Name = "Octo Cat",
                Username = "octocat",
                ResetPassword = true,
                SkipConfirmation = false,
                ProjectsLimit = 10,
                GroupIdForSaml = 90
            },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/users", handler.LastRequest?.RequestUri?.AbsoluteUri);

        Assert.NotNull(sentBody);
        Assert.Contains("\"email\":\"octo@example.com\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"username\":\"octocat\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"reset_password\":true", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"skip_confirmation\":false", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"group_id_for_saml\":90", sentBody, StringComparison.Ordinal);

        // Unset members are omitted rather than sent as null - and no credential appears in the payload
        // when the caller chose the reset-password strategy.
        Assert.DoesNotContain("\"password\":", sentBody, StringComparison.Ordinal);
        Assert.DoesNotContain("\"bio\"", sentBody, StringComparison.Ordinal);

        Assert.Equal(7, created.Id);
    }

    [Fact]
    public async Task UpdateAsync_PutsOnlySuppliedMembers_ToTheNumericUserRoute()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(AdminUserJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        UsersRepository repository = new(connection);

        GitLabUser updated = await repository.UpdateAsync(
            7,
            new UpdateUserRequest { Note = "Contractor until June.", Admin = false, Organization = string.Empty },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/users/7", handler.LastRequest?.RequestUri?.AbsoluteUri);

        Assert.NotNull(sentBody);
        Assert.Contains("\"organization\":\"\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"admin\":false", sentBody, StringComparison.Ordinal);
        Assert.DoesNotContain("\"username\"", sentBody, StringComparison.Ordinal);

        Assert.Equal("Contractor until June.", updated.Note);
    }

    [Fact]
    public async Task DeleteAsync_WithoutHardDelete_SendsNoQueryString()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        UsersRepository repository = new(connection);

        await repository.DeleteAsync(7, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/users/7", handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task DeleteAsync_WithHardDelete_SendsTheFlagAsAQueryParameter()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        UsersRepository repository = new(connection);

        await repository.DeleteAsync(7, true, TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/users/7?hard_delete=true",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Theory]
    [InlineData("activate")]
    [InlineData("deactivate")]
    [InlineData("block")]
    [InlineData("unblock")]
    [InlineData("ban")]
    [InlineData("unban")]
    [InlineData("approve")]
    [InlineData("reject")]
    public async Task StateActions_PostToTheirOwnRoute_WithNoBody(string action)
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Created));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        UsersRepository repository = new(connection);
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;

        switch (action)
        {
            case "activate":
                await repository.ActivateAsync(7, cancellationToken);
                break;
            case "deactivate":
                await repository.DeactivateAsync(7, cancellationToken);
                break;
            case "block":
                await repository.BlockAsync(7, cancellationToken);
                break;
            case "unblock":
                await repository.UnblockAsync(7, cancellationToken);
                break;
            case "ban":
                await repository.BanAsync(7, cancellationToken);
                break;
            case "unban":
                await repository.UnbanAsync(7, cancellationToken);
                break;
            case "approve":
                await repository.ApproveAsync(7, cancellationToken);
                break;
            case "reject":
                await repository.RejectAsync(7, cancellationToken);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(action), action, "Unhandled action.");
        }

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal($"https://gitlab.example/api/v4/users/7/{action}",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Null(handler.LastRequest?.Content);
    }

    [Fact]
    public async Task DisableTwoFactorAsync_UsesPatch_NotPostOrPut()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        UsersRepository repository = new(connection);

        await repository.DisableTwoFactorAsync(7, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Patch, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/users/7/disable_two_factor",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task GetAssociationsCountAsync_DeserializesTheCountsGitLabDeclaresAsStrings()
    {
        const string Json = """
                            {
                              "groups_count": 2,
                              "projects_count": 3,
                              "issues_count": 8,
                              "merge_requests_count": "5"
                            }
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        UsersRepository repository = new(connection);

        GitLabUserAssociationsCount counts =
            await repository.GetAssociationsCountAsync(7, TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/users/7/associations_count",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(2, counts.GroupsCount);
        Assert.Equal(3, counts.ProjectsCount);
        Assert.Equal(8, counts.IssuesCount);

        // NumberHandling.AllowReadingFromString is what makes the spec's "string" declaration harmless.
        Assert.Equal(5, counts.MergeRequestsCount);
    }

    [Fact]
    public async Task ListEmailsAsync_StreamsEveryPage_FollowingTheLinkHeader()
    {
        int call = 0;
        using StubHttpMessageHandler handler = new(_ =>
        {
            call++;
            HttpResponseMessage response = new(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    call == 1
                        ? """[{ "id": 1, "email": "octo@example.com", "confirmed_at": "2026-01-02T03:04:05Z" }]"""
                        : """[{ "id": 2, "email": "second@example.com", "confirmed_at": null }]""",
                    Encoding.UTF8,
                    "application/json")
            };

            if (call == 1)
            {
                response.Headers.TryAddWithoutValidation(
                    "Link",
                    "<https://gitlab.example/api/v4/users/7/emails?page=2>; rel=\"next\"");
            }

            return response;
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        UsersRepository repository = new(connection);

        List<GitLabEmail> emails = new();
        await foreach (GitLabEmail email in repository.ListEmailsAsync(7, TestContext.Current.CancellationToken))
        {
            emails.Add(email);
        }

        Assert.Equal(2, call);
        Assert.Equal("https://gitlab.example/api/v4/users/7/emails?page=2",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(2, emails.Count);
        Assert.Equal("octo@example.com", emails[0].Email);
        Assert.Equal(new DateTimeOffset(2026, 1, 2, 3, 4, 5, TimeSpan.Zero), emails[0].ConfirmedAt);
        Assert.Null(emails[1].ConfirmedAt);
    }

    [Fact]
    public async Task AddEmailAsync_PostsToTheUserEmailsRoute()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(
                    """{ "id": 9, "email": "extra@example.com", "confirmed_at": null }""",
                    Encoding.UTF8,
                    "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        UsersRepository repository = new(connection);

        GitLabEmail email = await repository.AddEmailAsync(
            7,
            new AddUserEmailRequest { Email = "extra@example.com", SkipConfirmation = true },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/users/7/emails",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.NotNull(sentBody);
        Assert.Contains("\"email\":\"extra@example.com\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"skip_confirmation\":true", sentBody, StringComparison.Ordinal);
        Assert.Equal(9, email.Id);
    }

    [Fact]
    public async Task DeleteEmailAsync_TargetsTheNumericEmailIdRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        UsersRepository repository = new(connection);

        await repository.DeleteEmailAsync(7, 9, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/users/7/emails/9",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task DeleteIdentityAsync_PercentEncodesTheProviderName()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        UsersRepository repository = new(connection);

        await repository.DeleteIdentityAsync(7, "saml/main", TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/users/7/identities/saml%2Fmain",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task ListMembershipsAsync_ProjectsTheCapitalisedTypeFilter_AndDeserializesTheFlatShape()
    {
        const string Json = """
                            [
                              {
                                "source_id": 3,
                                "source_name": "Backend",
                                "source_type": "Namespace",
                                "access_level": 30
                              }
                            ]
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        UsersRepository repository = new(connection);

        List<GitLabUserMembership> memberships = new();
        await foreach (GitLabUserMembership membership in repository.ListMembershipsAsync(
                           7,
                           new UserMembershipListOptions { Type = GitLabUserMembershipType.Namespace, PerPage = 50 },
                           TestContext.Current.CancellationToken))
        {
            memberships.Add(membership);
        }

        Assert.Equal("https://gitlab.example/api/v4/users/7/memberships?type=Namespace&per_page=50",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabUserMembership only = Assert.Single(memberships);
        Assert.Equal(3, only.SourceId);
        Assert.Equal("Backend", only.SourceName);
        Assert.Equal("Namespace", only.SourceType);
        Assert.Equal(30, only.AccessLevel);
    }

    [Fact]
    public async Task FollowAsync_PostsAndReturnsTheFollowedUser()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = new StringContent(BasicUserJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        UsersRepository repository = new(connection);

        GitLabUser followed = await repository.FollowAsync(3, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/users/3/follow",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("alice", followed.Username);
    }

    [Fact]
    public async Task UnfollowAsync_PostsAndReturnsTheUnfollowedUser()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = new StringContent(BasicUserJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        UsersRepository repository = new(connection);

        GitLabUser unfollowed = await repository.UnfollowAsync(3, TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/users/3/unfollow",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(3, unfollowed.Id);
    }

    [Fact]
    public async Task ListFollowersAsync_AndListFollowingAsync_HitTheirOwnRoutes()
    {
        using StubHttpMessageHandler followers = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent($"[{BasicUserJson}]", Encoding.UTF8, "application/json")
        });

        using HttpClient followersClient = new(followers) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection followersConnection = new(followersClient);
        UsersRepository followersRepository = new(followersConnection);

        await foreach (GitLabUser _ in followersRepository.ListFollowersAsync(3,
                           TestContext.Current.CancellationToken))
        {
            // Draining the sequence is what issues the request.
        }

        Assert.Equal("https://gitlab.example/api/v4/users/3/followers",
            followers.LastRequest?.RequestUri?.AbsoluteUri);

        using StubHttpMessageHandler following = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent($"[{BasicUserJson}]", Encoding.UTF8, "application/json")
        });

        using HttpClient followingClient = new(following) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection followingConnection = new(followingClient);
        UsersRepository followingRepository = new(followingConnection);

        await foreach (GitLabUser _ in followingRepository.ListFollowingAsync(3,
                           TestContext.Current.CancellationToken))
        {
            // Draining the sequence is what issues the request.
        }

        Assert.Equal("https://gitlab.example/api/v4/users/3/following",
            following.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task GetStatusAsync_AcceptsAUsername_AndPercentEncodesIt()
    {
        const string Json = """
                            {
                              "emoji": "coffee",
                              "message": "I crave coffee",
                              "message_html": "I crave coffee",
                              "availability": "busy",
                              "clear_status_at": "2026-03-01T12:00:00Z"
                            }
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        UsersRepository repository = new(connection);

        GitLabUserStatus status = await repository.GetStatusAsync("group/alice", TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/users/group%2Falice/status",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("coffee", status.Emoji);
        Assert.Equal("I crave coffee", status.Message);
        Assert.Equal("busy", status.Availability);
        Assert.Equal(new DateTimeOffset(2026, 3, 1, 12, 0, 0, TimeSpan.Zero), status.ClearStatusAt);
    }

    [Fact]
    public async Task GetSupportPinAsync_DeserializesThePinAndItsExpiry()
    {
        const string Json = """{ "pin": "123456", "expires_at": "2026-03-01T12:00:00Z" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        UsersRepository repository = new(connection);

        GitLabUserSupportPin pin = await repository.GetSupportPinAsync(7, TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/users/7/support_pin",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("123456", pin.Pin);
        Assert.Equal(new DateTimeOffset(2026, 3, 1, 12, 0, 0, TimeSpan.Zero), pin.ExpiresAt);
    }

    [Fact]
    public async Task RevokeSupportPinAsync_PostsToTheNestedRevokeRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        UsersRepository repository = new(connection);

        await repository.RevokeSupportPinAsync(7, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/users/7/support_pin/revoke",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task BlockAsync_OnForbiddenResponse_ThrowsTheTypedForbiddenException()
    {
        const string Json = """{ "message": "403 Forbidden" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Forbidden)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        UsersRepository repository = new(connection);

        GitLabForbiddenException exception =
            await Assert.ThrowsAsync<GitLabForbiddenException>(() =>
                repository.BlockAsync(7, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.Forbidden, exception.StatusCode);
        Assert.Equal(HttpMethod.Post, exception.RequestMethod);
    }

    [Fact]
    public async Task ListEnterpriseUsersAsync_BuildsTheGroupScopedRoute_WithTheProvidedOptions()
    {
        const string Json = """
                            [
                              {
                                "id": 7,
                                "username": "octocat",
                                "name": "Octo Cat",
                                "web_url": "https://gitlab.example/octocat",
                                "email": "octo@example.com",
                                "custom_attributes": [{ "key": "cost_center", "value": "42" }],
                                "scim_identities": [{ "active": true, "extern_uid": "abc" }]
                              }
                            ]
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        UsersRepository repository = new(connection);

        EnterpriseUserListOptions options = new() { Username = "octocat", Active = true, PerPage = 20 };

        List<GitLabUser> users = new();
        await foreach (GitLabUser user in
                       repository.ListEnterpriseUsersAsync(42, options, TestContext.Current.CancellationToken))
        {
            users.Add(user);
        }

        Assert.Equal(
            "https://gitlab.example/api/v4/groups/42/enterprise_users?username=octocat&active=true&per_page=20",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabUser user2 = Assert.Single(users);
        Assert.Equal("octocat", user2.Username);
        Assert.NotNull(user2.CustomAttributes);
        Assert.Equal("cost_center", user2.CustomAttributes![0].Key);
        Assert.NotNull(user2.ScimIdentities);
        Assert.Single(user2.ScimIdentities!);
    }

    [Fact]
    public async Task ListEnterpriseUsersAsync_EncodesANamespacedGroupPath()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        UsersRepository repository = new(connection);

        await foreach (GitLabUser _ in repository.ListEnterpriseUsersAsync("parent/subgroup",
                           cancellationToken: TestContext.Current.CancellationToken))
        {
            // Draining the sequence is what issues the request.
        }

        Assert.Equal("https://gitlab.example/api/v4/groups/parent%2Fsubgroup/enterprise_users",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task GetEnterpriseUserAsync_RequestsTheNestedGroupRoute()
    {
        const string Json =
            """{ "id": 7, "username": "octocat", "name": "Octo Cat", "web_url": "https://gitlab.example/octocat" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        UsersRepository repository = new(connection);

        GitLabUser user = await repository.GetEnterpriseUserAsync(42, 7, TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/groups/42/enterprise_users/7",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(7, user.Id);
    }

    [Fact]
    public async Task UpdateEnterpriseUserAsync_PatchesTheNestedRoute_WithTheRequestBody()
    {
        string? sentBody = null;

        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(BasicUserJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        UsersRepository repository = new(connection);

        UpdateEnterpriseUserRequest request = new() { Name = "Alice Updated", CanCreateGroup = false };

        GitLabUser user =
            await repository.UpdateEnterpriseUserAsync(42, 3, request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Patch, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/42/enterprise_users/3",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Contains("\"name\":\"Alice Updated\"", sentBody);
        Assert.Contains("\"can_create_group\":false", sentBody);
        Assert.DoesNotContain("email", sentBody, StringComparison.Ordinal);
        Assert.Equal("alice", user.Username);
    }

    [Fact]
    public async Task DeleteEnterpriseUserAsync_SendsTheHardDeleteQueryParameter()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        UsersRepository repository = new(connection);

        await repository.DeleteEnterpriseUserAsync(42, 3, true, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/42/enterprise_users/3?hard_delete=true",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task DisableEnterpriseUserTwoFactorAsync_PatchesTheNestedRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        UsersRepository repository = new(connection);

        await repository.DisableEnterpriseUserTwoFactorAsync(42, 3, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Patch, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/42/enterprise_users/3/disable_two_factor",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task GetEnterpriseUserAsync_OnNotFoundResponse_ThrowsGitLabNotFoundException()
    {
        const string Json = """{ "message": "404 User Not Found" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        UsersRepository repository = new(connection);

        await Assert.ThrowsAsync<GitLabNotFoundException>(() =>
            repository.GetEnterpriseUserAsync(42, 999, TestContext.Current.CancellationToken));
    }
}