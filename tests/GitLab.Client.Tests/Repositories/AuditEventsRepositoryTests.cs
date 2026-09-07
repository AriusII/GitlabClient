using System.Net;
using System.Text;

using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Repositories;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Repositories;

public sealed class AuditEventsRepositoryTests
{
    private const string AuditEventJson = """
                                          {
                                            "id": 4,
                                            "author_id": 3,
                                            "entity_id": 7,
                                            "entity_type": "Project",
                                            "event_name": "project_archived",
                                            "details": {
                                              "custom_message": "Project archived",
                                              "author_name": "Sidney Jones",
                                              "target_id": 7,
                                              "target_type": "Project",
                                              "target_details": "myproject",
                                              "ip_address": "127.0.0.1",
                                              "entity_path": "group/myproject"
                                            },
                                            "created_at": "2023-08-31T15:53:00.073Z"
                                          }
                                          """;

    [Fact]
    public async Task ListAsync_BuildsAuditEventsRoute_WithQueryOptions_AndDeserializesTheDetailsPayload()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent($"[{AuditEventJson}]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        AuditEventsRepository repository = new(connection);

        AuditEventListOptions options = new()
        {
            EntityType = "Project",
            EntityId = 7,
            CreatedAfter = new DateTimeOffset(2023, 1, 1, 0, 0, 0, TimeSpan.Zero),
            PerPage = 50
        };

        List<GitLabAuditEvent> events = new();
        await foreach (GitLabAuditEvent item in repository.ListAsync(options, TestContext.Current.CancellationToken))
        {
            events.Add(item);
        }

        string? requestUri = handler.LastRequest?.RequestUri?.AbsoluteUri;
        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Contains("/audit_events?", requestUri, StringComparison.Ordinal);
        Assert.Contains("entity_type=Project", requestUri, StringComparison.Ordinal);
        Assert.Contains("entity_id=7", requestUri, StringComparison.Ordinal);
        Assert.Contains("per_page=50", requestUri, StringComparison.Ordinal);

        GitLabAuditEvent auditEvent = Assert.Single(events);
        Assert.Equal(4, auditEvent.Id);
        Assert.Equal(3, auditEvent.AuthorId);
        Assert.Equal(7, auditEvent.EntityId);
        Assert.Equal("Project", auditEvent.EntityType);
        Assert.Equal("project_archived", auditEvent.EventName);
        Assert.Equal(new DateTimeOffset(2023, 8, 31, 15, 53, 0, 73, TimeSpan.Zero), auditEvent.CreatedAt);
        Assert.True(auditEvent.Details.HasValue);
        Assert.Equal("group/myproject", auditEvent.Details!.Value.GetProperty("entity_path").GetString());
    }

    [Fact]
    public async Task ListAsync_WithoutOptions_SendsNoQueryString()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        AuditEventsRepository repository = new(connection);

        await foreach (GitLabAuditEvent _ in
                       repository.ListAsync(cancellationToken: TestContext.Current.CancellationToken))
        {
            Assert.Fail("The stub returns an empty page.");
        }

        Assert.Equal("https://gitlab.example/api/v4/audit_events", handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task GetAsync_BuildsAuditEventByIdRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(AuditEventJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        AuditEventsRepository repository = new(connection);

        GitLabAuditEvent auditEvent = await repository.GetAsync(4, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/audit_events/4", handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(4, auditEvent.Id);
    }

    [Fact]
    public async Task ListForGroupAsync_BuildsGroupAuditEventsRoute_AndEncodesNamespacedPath()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent($"[{AuditEventJson}]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        AuditEventsRepository repository = new(connection);

        GroupAuditEventListOptions options = new() { PerPage = 20 };

        List<GitLabAuditEvent> events = new();
        await foreach (GitLabAuditEvent item in
                       repository.ListForGroupAsync("parent-group/subgroup", options,
                           TestContext.Current.CancellationToken))
        {
            events.Add(item);
        }

        string? requestUri = handler.LastRequest?.RequestUri?.AbsoluteUri;
        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.StartsWith("https://gitlab.example/api/v4/groups/parent-group%2Fsubgroup/audit_events",
            requestUri, StringComparison.Ordinal);
        Assert.Contains("per_page=20", requestUri, StringComparison.Ordinal);
        Assert.Single(events);
    }
}