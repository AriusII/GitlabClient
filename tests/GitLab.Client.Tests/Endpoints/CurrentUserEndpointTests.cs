using System.Net;
using System.Text;

using GitLab.Client.Abstractions;
using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Endpoints;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;
using GitLab.Client.Query;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Endpoints;

public sealed class CurrentUserEndpointTests
{
    private const string EmailJson = """
                                     {
                                       "id": 17,
                                       "email": "release-bot@example.com",
                                       "confirmed_at": "2024-03-04T05:06:07.000Z"
                                     }
                                     """;

    private const string StatusJson = """
                                      {
                                        "emoji": "coffee",
                                        "message": "I crave coffee",
                                        "message_html": "<gl-emoji title=\"hot beverage\">I crave coffee</gl-emoji>",
                                        "availability": "busy",
                                        "clear_status_at": "2024-05-06T07:08:00.000Z"
                                      }
                                      """;

    private const string PreferencesJson = """
                                           {
                                             "id": 3,
                                             "user_id": 42,
                                             "view_diffs_file_by_file": true,
                                             "show_whitespace_in_diffs": false,
                                             "pass_user_identities_to_ci_jwt": false,
                                             "policy_advanced_editor": true
                                           }
                                           """;

    private static StubHttpMessageHandler Answering(HttpStatusCode status, string? json)
    {
        return new StubHttpMessageHandler(_ =>
        {
            HttpResponseMessage response = new(status);

            if (json is not null)
            {
                response.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            return response;
        });
    }

    [Fact]
    public async Task ListActivitiesAsync_BuildsTheActivitiesRoute_WithTheFromFilterAsADate()
    {
        const string json = """
                            [
                              { "username": "root", "last_activity_on": "2024-01-31", "last_activity_at": "2024-01-31" }
                            ]
                            """;

        using StubHttpMessageHandler handler = Answering(HttpStatusCode.OK, json);
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        CurrentUserClient repository = new(new GitLabApiConnection(httpClient));

        List<GitLabUserActivity> activities = [];
        await foreach (GitLabUserActivity activity in repository.ListActivitiesAsync(
                           new UserActivityListOptions { From = new DateOnly(2024, 1, 1), PerPage = 50 },
                           TestContext.Current.CancellationToken))
        {
            activities.Add(activity);
        }

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/user/activities?from=2024-01-01&per_page=50",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabUserActivity single = Assert.Single(activities);
        Assert.Equal("root", single.Username);
        Assert.Equal(new DateOnly(2024, 1, 31), single.LastActivityOn);
        Assert.Equal(new DateOnly(2024, 1, 31), single.LastActivityAt);
    }

    [Fact]
    public async Task ListActivitiesAsync_WithoutOptions_SendsNoQueryString()
    {
        using StubHttpMessageHandler handler = Answering(HttpStatusCode.OK, "[]");
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        CurrentUserClient repository = new(new GitLabApiConnection(httpClient));

        await foreach (GitLabUserActivity _ in repository.ListActivitiesAsync(
                           cancellationToken: TestContext.Current.CancellationToken))
        {
            Assert.Fail("The stubbed response is an empty page.");
        }

        Assert.Equal("https://gitlab.example/api/v4/user/activities",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task ListEmailsAsync_BuildsTheEmailsRoute_AndDeserializesTheConfirmationTimestamp()
    {
        using StubHttpMessageHandler handler = Answering(HttpStatusCode.OK, $"[{EmailJson}]");
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        CurrentUserClient repository = new(new GitLabApiConnection(httpClient));

        List<GitLabEmail> emails = [];
        await foreach (GitLabEmail email in repository.ListEmailsAsync(TestContext.Current.CancellationToken))
        {
            emails.Add(email);
        }

        Assert.Equal("https://gitlab.example/api/v4/user/emails", handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabEmail single = Assert.Single(emails);
        Assert.Equal(17, single.Id);
        Assert.Equal("release-bot@example.com", single.Email);
        Assert.Equal(new DateTimeOffset(2024, 3, 4, 5, 6, 7, TimeSpan.Zero), single.ConfirmedAt);
    }

    [Fact]
    public async Task ListEmailsAsync_LeavesAnUnconfirmedAddressNull()
    {
        const string json = """[{ "id": 18, "email": "new@example.com", "confirmed_at": null }]""";

        using StubHttpMessageHandler handler = Answering(HttpStatusCode.OK, json);
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        CurrentUserClient repository = new(new GitLabApiConnection(httpClient));

        List<GitLabEmail> emails = [];
        await foreach (GitLabEmail email in repository.ListEmailsAsync(TestContext.Current.CancellationToken))
        {
            emails.Add(email);
        }

        Assert.Null(Assert.Single(emails).ConfirmedAt);
    }

    [Fact]
    public async Task GetEmailAsync_AddressesTheEmailByItsOwnId()
    {
        using StubHttpMessageHandler handler = Answering(HttpStatusCode.OK, EmailJson);
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        CurrentUserClient repository = new(new GitLabApiConnection(httpClient));

        GitLabEmail email = await repository.GetEmailAsync(17, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/user/emails/17", handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("release-bot@example.com", email.Email);
    }

    [Fact]
    public async Task AddEmailAsync_PostsTheAddress()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(EmailJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        CurrentUserClient repository = new(new GitLabApiConnection(httpClient));

        GitLabEmail email = await repository.AddEmailAsync(
            new AddCurrentUserEmailRequest { Email = "release-bot@example.com" },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/user/emails", handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"email":"release-bot@example.com"}""", sentBody);
        Assert.Equal(17, email.Id);
    }

    [Fact]
    public async Task DeleteEmailAsync_IssuesADeleteAndAcceptsAnEmptyBody()
    {
        using StubHttpMessageHandler handler = Answering(HttpStatusCode.NoContent, null);
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        CurrentUserClient repository = new(new GitLabApiConnection(httpClient));

        await repository.DeleteEmailAsync(17, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/user/emails/17", handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task DeleteEmailAsync_MapsAMissingAddressToTheTypedNotFoundException()
    {
        using StubHttpMessageHandler handler = Answering(HttpStatusCode.NotFound,
            """{"message":"404 Email Not Found"}""");
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        CurrentUserClient repository = new(new GitLabApiConnection(httpClient));

        GitLabNotFoundException exception = await Assert.ThrowsAsync<GitLabNotFoundException>(() =>
            repository.DeleteEmailAsync(404, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
    }

    [Fact]
    public async Task GetPreferencesAsync_DeserializesTheFlagsAsBooleans()
    {
        using StubHttpMessageHandler handler = Answering(HttpStatusCode.OK, PreferencesJson);
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        CurrentUserClient repository = new(new GitLabApiConnection(httpClient));

        GitLabUserPreferences preferences =
            await repository.GetPreferencesAsync(TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/user/preferences",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(3, preferences.Id);
        Assert.Equal(42, preferences.UserId);
        Assert.True(preferences.ViewDiffsFileByFile);
        Assert.False(preferences.ShowWhitespaceInDiffs);
        Assert.False(preferences.PassUserIdentitiesToCiJwt);
        Assert.True(preferences.PolicyAdvancedEditor);
    }

    [Fact]
    public async Task UpdatePreferencesAsync_PutsOnlyTheFlagsThatWereSet()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(PreferencesJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        CurrentUserClient repository = new(new GitLabApiConnection(httpClient));

        await repository.UpdatePreferencesAsync(
            new UpdateUserPreferencesRequest { ViewDiffsFileByFile = true },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/user/preferences",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"view_diffs_file_by_file":true}""", sentBody);
    }

    [Fact]
    public async Task GetStatusAsync_DeserializesTheStatus()
    {
        using StubHttpMessageHandler handler = Answering(HttpStatusCode.OK, StatusJson);
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        CurrentUserClient repository = new(new GitLabApiConnection(httpClient));

        GitLabUserStatus status = await repository.GetStatusAsync(TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/user/status", handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("coffee", status.Emoji);
        Assert.Equal("I crave coffee", status.Message);
        Assert.Equal("busy", status.Availability);
        Assert.Contains("gl-emoji", status.MessageHtml, StringComparison.Ordinal);
        Assert.Equal(new DateTimeOffset(2024, 5, 6, 7, 8, 0, TimeSpan.Zero), status.ClearStatusAt);
    }

    [Fact]
    public async Task GetStatusAsync_ToleratesAnAccountThatNeverSetOne()
    {
        const string json = """
                            {
                              "emoji": null, "message": null, "message_html": null,
                              "availability": "not_set", "clear_status_at": null
                            }
                            """;

        using StubHttpMessageHandler handler = Answering(HttpStatusCode.OK, json);
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        CurrentUserClient repository = new(new GitLabApiConnection(httpClient));

        GitLabUserStatus status = await repository.GetStatusAsync(TestContext.Current.CancellationToken);

        Assert.Null(status.Emoji);
        Assert.Null(status.ClearStatusAt);
        Assert.Equal("not_set", status.Availability);
    }

    [Fact]
    public async Task SetStatusAsync_UsesPut_AndWritesTheClearAfterEnumAsGitLabsWireValue()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(StatusJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        CurrentUserClient repository = new(new GitLabApiConnection(httpClient));

        await repository.SetStatusAsync(
            new SetUserStatusRequest
            {
                Emoji = "coffee",
                Message = "I crave coffee",
                Availability = "busy",
                ClearStatusAfter = GitLabUserStatusClearAfter.ThirtyMinutes
            },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/user/status", handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(
            """{"emoji":"coffee","message":"I crave coffee","availability":"busy","clear_status_after":"30_minutes"}""",
            sentBody);
    }

    [Fact]
    public async Task UpdateStatusAsync_UsesPatch_SoOmittedFieldsAreLeftAlone()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(StatusJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        CurrentUserClient repository = new(new GitLabApiConnection(httpClient));

        await repository.UpdateStatusAsync(
            new SetUserStatusRequest { Message = "back tomorrow" },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Patch, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/user/status", handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"message":"back tomorrow"}""", sentBody);
    }

    [Fact]
    public async Task GetSupportPinAsync_DeserializesThePinAndItsExpiry()
    {
        const string json = """{ "pin": "123456", "expires_at": "2024-07-08T09:10:11.000Z" }""";

        using StubHttpMessageHandler handler = Answering(HttpStatusCode.OK, json);
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        CurrentUserClient repository = new(new GitLabApiConnection(httpClient));

        GitLabSupportPin pin = await repository.GetSupportPinAsync(TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/user/support_pin",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("123456", pin.Pin);
        Assert.Equal(new DateTimeOffset(2024, 7, 8, 9, 10, 11, TimeSpan.Zero), pin.ExpiresAt);
    }

    [Fact]
    public async Task CreateSupportPinAsync_PostsWithNoRequestBody()
    {
        const string json = """{ "pin": "654321", "expires_at": "2024-07-15T09:10:11.000Z" }""";

        long? sentLength = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentLength = request.Content?.Headers.ContentLength;
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        CurrentUserClient repository = new(new GitLabApiConnection(httpClient));

        GitLabSupportPin pin = await repository.CreateSupportPinAsync(TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/user/support_pin",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.True(sentLength is null or 0);
        Assert.Equal("654321", pin.Pin);
    }

    [Fact]
    public async Task CreateRunnerAsync_PostsTheRunnerScope_AndReturnsTheOneTimeToken()
    {
        const string json = """
                            {
                              "id": 12345,
                              "token": "glrt-secret",
                              "token_expires_at": "2024-09-01T00:00:00.000Z"
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

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        CurrentUserClient repository = new(new GitLabApiConnection(httpClient));

        GitLabRunnerRegistration registration = await repository.CreateRunnerAsync(
            new CreateUserRunnerRequest
            {
                RunnerType = "project_type",
                ProjectId = 7,
                Description = "release runner",
                TagList = ["linux", "docker"],
                RunUntagged = false
            },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/user/runners", handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(
            """
            {"runner_type":"project_type","project_id":7,"description":"release runner","run_untagged":false,"tag_list":["linux","docker"]}
            """,
            sentBody);

        Assert.Equal(12345, registration.Id);
        Assert.Equal("glrt-secret", registration.Token);
        Assert.Equal(new DateTimeOffset(2024, 9, 1, 0, 0, 0, TimeSpan.Zero), registration.TokenExpiresAt);
    }

    [Fact]
    public async Task SetAvatarAsync_PutsMultipartFormData_UnderTheAvatarFieldNameGitLabExpects()
    {
        string? sentBody = null;
        string? sentMediaType = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            sentMediaType = request.Content?.Headers.ContentType?.MediaType;
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    """{ "avatar_url": "https://gitlab.example/uploads/-/system/user/avatar/42/me.png" }""",
                    Encoding.UTF8,
                    "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        CurrentUserClient repository = new(new GitLabApiConnection(httpClient));

        using MemoryStream content = new(Encoding.UTF8.GetBytes("PNG-BYTES"));
        GitLabAvatar avatar = await repository.SetAvatarAsync(
            new GitLabFileUpload
            {
                Content = content,
                FileName = "me.png",
                ContentType = "image/png",

                // Deliberately wrong: the repository must override it, because this is the one GitLab
                // upload endpoint that does not call its part "file".
                FieldName = "file"
            },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/user/avatar", handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("multipart/form-data", sentMediaType);

        // .NET quotes multipart parameters only when it has to, so compare against an unquoted form.
        string unquoted = (sentBody ?? string.Empty).Replace("\"", string.Empty, StringComparison.Ordinal);
        Assert.Contains("name=avatar", unquoted, StringComparison.Ordinal);
        Assert.DoesNotContain("name=file", unquoted, StringComparison.Ordinal);
        Assert.Contains("filename=me.png", unquoted, StringComparison.Ordinal);
        Assert.Contains("Content-Type: image/png", sentBody, StringComparison.Ordinal);
        Assert.Contains("PNG-BYTES", sentBody, StringComparison.Ordinal);

        // The upload stream is borrowed, never owned: disposing it is the caller's business.
        Assert.True(content.CanRead);

        Assert.Equal(new Uri("https://gitlab.example/uploads/-/system/user/avatar/42/me.png"), avatar.AvatarUrl);
    }

    [Fact]
    public async Task SetAvatarAsync_MapsAnOversizedImageToTheTypedValidationException()
    {
        using StubHttpMessageHandler handler = Answering(HttpStatusCode.BadRequest,
            """{"message":{"avatar":["is too big (should be at most 200 KiB)"]}}""");
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        CurrentUserClient repository = new(new GitLabApiConnection(httpClient));

        using MemoryStream content = new(Encoding.UTF8.GetBytes("PNG-BYTES"));

        GitLabValidationException exception = await Assert.ThrowsAsync<GitLabValidationException>(() =>
            repository.SetAvatarAsync(
                new GitLabFileUpload { Content = content, FileName = "me.png" },
                TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);
    }
}