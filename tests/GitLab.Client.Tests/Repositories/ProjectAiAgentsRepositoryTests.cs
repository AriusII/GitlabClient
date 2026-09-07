using System.Net;
using System.Text;
using System.Text.Json;

using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Repositories;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Repositories;

public sealed class ProjectAiAgentsRepositoryTests
{
    private const string IdentityJson = """
                                        {
                                          "id": 11,
                                          "agent_type": "claude-code",
                                          "revoked_at": null,
                                          "created_at": "2026-01-14T09:12:33.000Z"
                                        }
                                        """;

    private const string SessionJson = """
                                       {
                                         "id": 501,
                                         "agent_type": "claude-code",
                                         "agent_identity_id": 11,
                                         "user_id": 18,
                                         "sync_type": "hook",
                                         "status": "running",
                                         "goal": "Wrap the uploads API",
                                         "created_at": "2026-01-14T09:13:00.000Z",
                                         "updated_at": "2026-01-14T09:44:10.000Z"
                                       }
                                       """;

    private static readonly Uri BaseAddress = new("https://gitlab.example/api/v4/");

    [Fact]
    public async Task RegisterIdentityAsync_PostsToTheIdentitiesRoute_AndSendsTheHyphenatedAgentType()
    {
        string? sentBody = null;

        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();

            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(IdentityJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        ProjectAiAgentsRepository repository = new(connection);

        GitLabAgentIdentity identity = await repository.RegisterIdentityAsync(
            7,
            new RegisterAgentIdentityRequest
            {
                AgentType = GitLabAgentType.ClaudeCode,
                MachineFingerprint = "5e884898da28047151d0e56f8dc6292773603d0d"
            },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/7/ai_agent/identities",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        // "claude-code" is hyphenated on the wire, which only [JsonStringEnumMemberName] gets right.
        Assert.Equal(
            """
            {"agent_type":"claude-code","machine_fingerprint":"5e884898da28047151d0e56f8dc6292773603d0d"}
            """,
            sentBody);

        Assert.Equal(11, identity.Id);
        Assert.Equal("claude-code", identity.AgentType);
        Assert.Null(identity.RevokedAt);
        Assert.Equal(new DateTimeOffset(2026, 1, 14, 9, 12, 33, TimeSpan.Zero), identity.CreatedAt);
    }

    [Fact]
    public async Task RegisterIdentityAsync_MapsA403ToTheTypedForbiddenException()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Forbidden)
        {
            Content = new StringContent("""{"message":"403 Forbidden - identity revoked"}""", Encoding.UTF8,
                "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        ProjectAiAgentsRepository repository = new(connection);

        GitLabForbiddenException exception = await Assert.ThrowsAsync<GitLabForbiddenException>(() =>
            repository.RegisterIdentityAsync(
                7,
                new RegisterAgentIdentityRequest
                {
                    AgentType = GitLabAgentType.OpenCode, MachineFingerprint = "deadbeef"
                },
                TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.Forbidden, exception.StatusCode);
    }

    [Fact]
    public async Task ListSessionsAsync_ProjectsEveryFilterOntoTheQueryString()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent($"[{SessionJson}]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        ProjectAiAgentsRepository repository = new(connection);

        AgentSessionListOptions options = new()
        {
            AgentType = GitLabAgentType.ClaudeCode,
            Status = GitLabAgentSessionStatus.Running,
            CreatedAfter = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
            PerPage = 50
        };

        List<GitLabAgentSession> sessions = [];
        await foreach (GitLabAgentSession session in
                       repository.ListSessionsAsync(7, options, TestContext.Current.CancellationToken))
        {
            sessions.Add(session);
        }

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/projects/7/ai_agent/sessions"
            + "?agent_type=claude-code&status=running&created_after=2026-01-01T00:00:00Z&per_page=50",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabAgentSession only = Assert.Single(sessions);
        Assert.Equal(501, only.Id);
        Assert.Equal(11, only.AgentIdentityId);
        Assert.Equal(18, only.UserId);
        Assert.Equal("hook", only.SyncType);
        Assert.Equal("running", only.Status);
        Assert.Equal("Wrap the uploads API", only.Goal);
        Assert.Equal(new DateTimeOffset(2026, 1, 14, 9, 44, 10, TimeSpan.Zero), only.UpdatedAt);
    }

    [Fact]
    public async Task ListSessionsAsync_WithoutOptions_EncodesANamespacedProjectPath()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        ProjectAiAgentsRepository repository = new(connection);

        await foreach (GitLabAgentSession _ in repository.ListSessionsAsync(
                           ProjectId.FromPath("group/subgroup/project"),
                           cancellationToken: TestContext.Current.CancellationToken))
        {
            // Draining the sequence is what issues the request.
        }

        Assert.Equal("https://gitlab.example/api/v4/projects/group%2Fsubgroup%2Fproject/ai_agent/sessions",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task CreateSessionAsync_SendsTheSyncTypeAndTheIdempotencyKey()
    {
        string? sentBody = null;

        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();

            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(SessionJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        ProjectAiAgentsRepository repository = new(connection);

        GitLabAgentSession session = await repository.CreateSessionAsync(
            7,
            new CreateAgentSessionRequest
            {
                AgentType = GitLabAgentType.ClaudeCode,
                AgentIdentityId = 11,
                SyncType = GitLabAgentSyncType.Hook,
                Goal = "Wrap the uploads API",
                IdempotencyKey = "0f6d1c1e-3a4b-4c5d-8e9f-0a1b2c3d4e5f"
            },
            TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/7/ai_agent/sessions",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        // started_at is unset, so it must not appear at all - GitLab would otherwise read an explicit null.
        Assert.Equal(
            """
            {"agent_type":"claude-code","agent_identity_id":11,"sync_type":"hook","goal":"Wrap the uploads API","idempotency_key":"0f6d1c1e-3a4b-4c5d-8e9f-0a1b2c3d4e5f"}
            """,
            sentBody);

        Assert.Equal(501, session.Id);
    }

    [Fact]
    public async Task CompleteSessionAsync_PatchesOneSession_AndSpellsTheTranscriptDigest()
    {
        string? sentBody = null;

        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(SessionJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        ProjectAiAgentsRepository repository = new(connection);

        await repository.CompleteSessionAsync(
            7,
            501,
            new CompleteAgentSessionRequest
            {
                Status = GitLabAgentSessionOutcome.Completed,
                JsonlSha256 = "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855"
            },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Patch, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/7/ai_agent/sessions/501",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        Assert.Equal(
            """
            {"status":"completed","jsonl_sha256":"e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855"}
            """,
            sentBody);
    }

    [Fact]
    public async Task IngestAuditEventsAsync_PostsTheBatchVerbatim_AndExpectsNoBody()
    {
        string? sentBody = null;

        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();

            // 202 Accepted with an empty body: the deserializing overload would fail here, which is why
            // the repository uses the no-content POST.
            return new HttpResponseMessage(HttpStatusCode.Accepted);
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        ProjectAiAgentsRepository repository = new(connection);

        using JsonDocument first = JsonDocument.Parse("""{"cloud_event_id":"a1","type":"tool_call"}""");
        using JsonDocument second = JsonDocument.Parse("""{"cloud_event_id":"a2","type":"file_write"}""");

        await repository.IngestAuditEventsAsync(
            7,
            new IngestAgentAuditEventsRequest { SessionId = 501, Events = [first.RootElement, second.RootElement] },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/7/ai_agent/audit_events",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        // The events are opaque to this library: whatever the caller handed over goes out untouched.
        Assert.Equal(
            """
            {"session_id":501,"events":[{"cloud_event_id":"a1","type":"tool_call"},{"cloud_event_id":"a2","type":"file_write"}]}
            """,
            sentBody);
    }
}