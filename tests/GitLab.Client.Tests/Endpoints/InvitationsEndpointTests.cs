using System.Net;
using System.Text;

using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Endpoints;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;
using GitLab.Client.Query;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Endpoints;

public sealed class InvitationsEndpointTests
{
    private const string InvitationJson = """
                                          {
                                            "id": 1,
                                            "invite_email": "member@example.org",
                                            "created_at": "2020-10-22T14:13:35Z",
                                            "access_level": 30,
                                            "expires_at": "2020-11-22T14:13:35Z",
                                            "user_name": "Raymond Smith",
                                            "created_by_name": "Administrator",
                                            "invite_token": "5d3ae1b1e2e7f8f5b6c8"
                                          }
                                          """;

    [Fact]
    public async Task ListForProjectAsync_BuildsInvitationsRoute_WithQueryOptions_AndDeserializesInvitations()
    {
        string json = $"[{InvitationJson}]";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        InvitationsClient repository = new(connection);

        InvitationListOptions options = new() { Query = "member@example.org", Page = 2, PerPage = 20 };

        List<GitLabInvitation> invitations = new();
        await foreach (GitLabInvitation item in
                       repository.ListForProjectAsync(1, options, TestContext.Current.CancellationToken))
        {
            invitations.Add(item);
        }

        string? requestUri = handler.LastRequest?.RequestUri?.AbsoluteUri;
        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Contains("/projects/1/invitations", requestUri);
        Assert.Contains("query=member%40example.org", requestUri);
        Assert.Contains("page=2", requestUri);
        Assert.Contains("per_page=20", requestUri);

        GitLabInvitation invitation = Assert.Single(invitations);
        Assert.Equal(1, invitation.Id);
        Assert.Equal("member@example.org", invitation.InviteEmail);
        Assert.Equal(30, invitation.AccessLevel);
        Assert.Equal(new DateTimeOffset(2020, 10, 22, 14, 13, 35, TimeSpan.Zero), invitation.CreatedAt);
        Assert.Equal(new DateTimeOffset(2020, 11, 22, 14, 13, 35, TimeSpan.Zero), invitation.ExpiresAt);
        Assert.Equal("Raymond Smith", invitation.UserName);
        Assert.Equal("Administrator", invitation.CreatedByName);
        Assert.Equal("5d3ae1b1e2e7f8f5b6c8", invitation.InviteToken);
    }

    [Fact]
    public async Task ListForProjectAsync_EncodesNamespacedProjectPath_AndOmitsAnAbsentOptions()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        InvitationsClient repository = new(connection);

        await foreach (GitLabInvitation _ in repository.ListForProjectAsync("gitlab-org/gitlab",
                           cancellationToken: TestContext.Current.CancellationToken))
        {
            Assert.Fail("The stubbed response is an empty page.");
        }

        Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/invitations",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task ListForGroupAsync_BuildsGroupRoute_AndEncodesNamespacedGroupPath()
    {
        string json = $"[{InvitationJson}]";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        InvitationsClient repository = new(connection);

        InvitationListOptions options = new() { Query = "raymond" };

        List<GitLabInvitation> invitations = new();
        await foreach (GitLabInvitation item in repository.ListForGroupAsync("parent-group/subgroup", options,
                           TestContext.Current.CancellationToken))
        {
            invitations.Add(item);
        }

        string? requestUri = handler.LastRequest?.RequestUri?.AbsoluteUri;
        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Contains("/groups/parent-group%2Fsubgroup/invitations", requestUri);
        Assert.Contains("query=raymond", requestUri);
        Assert.Equal("member@example.org", Assert.Single(invitations).InviteEmail);
    }

    [Fact]
    public async Task CreateForProjectAsync_PostsEmailsAndUserIdsUnderTheirSingularWireNames()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(InvitationJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        InvitationsClient repository = new(connection);

        CreateInvitationRequest request = new()
        {
            AccessLevel = 30,
            Emails = ["member@example.org", "other@example.org"],
            UserIds = ["12", "34"],
            ExpiresAt = new DateTimeOffset(2020, 11, 22, 14, 13, 35, TimeSpan.Zero),
            InviteSource = "invitations-api",
            MemberRoleId = 7
        };

        GitLabInvitation invitation =
            await repository.CreateForProjectAsync(42, request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/42/invitations",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("application/json", handler.LastRequest?.Content?.Headers.ContentType?.MediaType);

        Assert.Contains("\"access_level\":30", sentBody, StringComparison.Ordinal);

        // Singular wire names carrying arrays: the snake_case default would send "emails"/"user_ids",
        // which GitLab answers 200 to and silently ignores.
        Assert.Contains("\"email\":[\"member@example.org\",\"other@example.org\"]", sentBody,
            StringComparison.Ordinal);
        Assert.Contains("\"user_id\":[\"12\",\"34\"]", sentBody, StringComparison.Ordinal);
        Assert.DoesNotContain("\"emails\"", sentBody, StringComparison.Ordinal);
        Assert.DoesNotContain("\"user_ids\"", sentBody, StringComparison.Ordinal);

        Assert.Contains("\"invite_source\":\"invitations-api\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"member_role_id\":7", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"expires_at\":\"2020-11-22T14:13:35+00:00\"", sentBody, StringComparison.Ordinal);

        Assert.Equal("member@example.org", invitation.InviteEmail);
        Assert.Equal(30, invitation.AccessLevel);
    }

    [Fact]
    public async Task CreateForProjectAsync_OnAStatusEnvelopeResponse_YieldsAnAllNullInvitation()
    {
        // GitLab answers a multi-invitee create with a status envelope instead of the invitation entity
        // the spec advertises. Every GitLabInvitation member is nullable precisely so this deserializes
        // rather than throwing.
        const string Json = """{ "status": "success" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        InvitationsClient repository = new(connection);

        CreateInvitationRequest request = new() { AccessLevel = 30, Emails = ["member@example.org"] };

        GitLabInvitation invitation =
            await repository.CreateForProjectAsync(1, request, TestContext.Current.CancellationToken);

        Assert.Null(invitation.InviteEmail);
        Assert.Null(invitation.AccessLevel);
        Assert.Null(invitation.CreatedAt);
        Assert.Null(invitation.ExpiresAt);
        Assert.Null(invitation.UserName);
        Assert.Null(invitation.CreatedByName);
        Assert.Null(invitation.InviteToken);
    }

    [Fact]
    public async Task CreateForGroupAsync_PostsToTheGroupInvitationsRoute()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(InvitationJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        InvitationsClient repository = new(connection);

        CreateInvitationRequest request = new() { AccessLevel = 10, Emails = ["member@example.org"] };

        GitLabInvitation invitation = await repository.CreateForGroupAsync("parent-group/subgroup", request,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/parent-group%2Fsubgroup/invitations",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Contains("\"access_level\":10", sentBody, StringComparison.Ordinal);
        Assert.Equal("Administrator", invitation.CreatedByName);
    }

    [Fact]
    public async Task UpdateForProjectAsync_EscapesTheEmailSegment_AndPutsTheChangedFields()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(InvitationJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        InvitationsClient repository = new(connection);

        UpdateInvitationRequest request = new()
        {
            AccessLevel = 40, ExpiresAt = new DateTimeOffset(2021, 1, 1, 0, 0, 0, TimeSpan.Zero), MemberRoleId = 9
        };

        GitLabInvitation invitation = await repository.UpdateForProjectAsync(1, "member+alias@example.org", request,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/invitations/member%2Balias%40example.org",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"access_level":40,"expires_at":"2021-01-01T00:00:00+00:00","member_role_id":9}""", sentBody);
        Assert.Equal(30, invitation.AccessLevel);
    }

    [Fact]
    public async Task UpdateForGroupAsync_EscapesTheEmailSegment_OnTheGroupRoute()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(InvitationJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        InvitationsClient repository = new(connection);

        UpdateInvitationRequest request = new() { AccessLevel = 20 };

        await repository.UpdateForGroupAsync(9970, "member@example.org", request,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/9970/invitations/member%40example.org",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        // Unset members are omitted, not sent as null - a null expires_at on a PUT would clear the value.
        Assert.Equal("""{"access_level":20}""", sentBody);
    }

    [Fact]
    public async Task DeleteForProjectAsync_EscapesTheEmailSegment_AndSendsDelete()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        InvitationsClient repository = new(connection);

        await repository.DeleteForProjectAsync("gitlab-org/gitlab", "member@example.org",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/invitations/member%40example.org",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task DeleteForGroupAsync_EscapesTheEmailSegment_OnTheGroupRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        InvitationsClient repository = new(connection);

        await repository.DeleteForGroupAsync("parent-group/subgroup", "member@example.org",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/parent-group%2Fsubgroup/invitations/member%40example.org",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task CreateForProjectAsync_OnValidationFailure_ThrowsGitLabValidationException()
    {
        const string Json = """{ "message": { "email": ["is invalid"] } }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        InvitationsClient repository = new(connection);

        CreateInvitationRequest request = new() { AccessLevel = 30, Emails = ["not-an-email"] };

        GitLabValidationException exception = await Assert.ThrowsAsync<GitLabValidationException>(() =>
            repository.CreateForProjectAsync(1, request, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);
        Assert.Contains("is invalid", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task DeleteForProjectAsync_OnMissingInvitation_ThrowsGitLabNotFoundException()
    {
        const string Json = """{ "message": "404 Not found" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        InvitationsClient repository = new(connection);

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabNotFoundException>(() =>
            repository.DeleteForProjectAsync(1, "nobody@example.org", TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        Assert.Equal("404 Not found", exception.Message);
    }
}