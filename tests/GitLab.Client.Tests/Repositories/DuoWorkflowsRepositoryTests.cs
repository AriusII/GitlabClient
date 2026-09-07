using System.Net;
using System.Text;
using System.Text.Json;

using GitLab.Client.Abstractions;
using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Repositories;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Repositories;

public sealed class DuoWorkflowsRepositoryTests
{
    private const string FlowCallbackJson = """
                                            {
                                              "id": 42,
                                              "url": "https://autoflow.example.com/duo/callbacks",
                                              "name": "AutoFlow",
                                              "signing_token_set": true,
                                              "token_set": false,
                                              "created_at": "2026-07-22T11:37:00Z"
                                            }
                                            """;

    private static readonly Uri BaseAddress = new("https://gitlab.example/api/v4/");

    private static readonly int[] ExpectedAgentPrivileges = [1, 2];

    [Fact]
    public async Task ListFlowCallbacksAsync_BuildsTheFlowCallbacksRoute_AndDeserializesThePayload()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent($"[{FlowCallbackJson}]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        DuoWorkflowsRepository repository = new(new GitLabApiConnection(httpClient));

        List<GitLabDuoWorkflowFlowCallbackHook> hooks = [];
        await foreach (GitLabDuoWorkflowFlowCallbackHook hook in
                       repository.ListFlowCallbacksAsync(cancellationToken: TestContext.Current.CancellationToken))
        {
            hooks.Add(hook);
        }

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/ai/duo_workflows/flow_callbacks",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabDuoWorkflowFlowCallbackHook single = Assert.Single(hooks);
        Assert.Equal(42, single.Id);
        Assert.Equal(new Uri("https://autoflow.example.com/duo/callbacks"), single.Url);
        Assert.Equal("AutoFlow", single.Name);
        Assert.True(single.SigningTokenSet);
        Assert.False(single.TokenSet);
        Assert.Equal(new DateTimeOffset(2026, 7, 22, 11, 37, 0, TimeSpan.Zero), single.CreatedAt);
    }

    [Fact]
    public async Task ListFlowCallbacksAsync_ProjectsThePagingOptionsOntoTheQueryString()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        DuoWorkflowsRepository repository = new(new GitLabApiConnection(httpClient));

        List<GitLabDuoWorkflowFlowCallbackHook> hooks = [];
        await foreach (GitLabDuoWorkflowFlowCallbackHook hook in repository.ListFlowCallbacksAsync(
                           new DuoWorkflowFlowCallbackListOptions { PerPage = 75 },
                           TestContext.Current.CancellationToken))
        {
            hooks.Add(hook);
        }

        Assert.Empty(hooks);

        Assert.Equal("https://gitlab.example/api/v4/ai/duo_workflows/flow_callbacks?per_page=75",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task RegisterFlowCallbackAsync_PostsTheEndpointAndItsSecrets()
    {
        string? sentBody = null;

        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();

            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(FlowCallbackJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        DuoWorkflowsRepository repository = new(new GitLabApiConnection(httpClient));

        GitLabDuoWorkflowFlowCallbackHook hook = await repository.RegisterFlowCallbackAsync(
            new RegisterDuoWorkflowFlowCallbackRequest
            {
                Url = new Uri("https://autoflow.example.com/duo/callbacks"),
                Name = "AutoFlow",
                SigningToken = "whsec_abc"
            },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/ai/duo_workflows/flow_callbacks",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        // "token" is unset and must be omitted rather than sent as null.
        Assert.Equal(
            """
            {"url":"https://autoflow.example.com/duo/callbacks","name":"AutoFlow","signing_token":"whsec_abc"}
            """,
            sentBody);

        Assert.Equal(42, hook.Id);
    }

    [Fact]
    public async Task GetFlowCallbackAsync_BuildsTheSingleFlowCallbackRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(FlowCallbackJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        DuoWorkflowsRepository repository = new(new GitLabApiConnection(httpClient));

        GitLabDuoWorkflowFlowCallbackHook hook =
            await repository.GetFlowCallbackAsync(42, TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/ai/duo_workflows/flow_callbacks/42",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(42, hook.Id);
        Assert.Equal(new Uri("https://autoflow.example.com/duo/callbacks"), hook.Url);
    }

    [Fact]
    public async Task DeleteFlowCallbackAsync_SendsDeleteAndExpectsNoBody()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        DuoWorkflowsRepository repository = new(new GitLabApiConnection(httpClient));

        await repository.DeleteFlowCallbackAsync(42, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/ai/duo_workflows/flow_callbacks/42",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task CreateAsync_SendsTheEnumWireValues_AndReturnsTheRawBody()
    {
        string? sentBody = null;

        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"id":7,"status":"created"}""", Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        DuoWorkflowsRepository repository = new(new GitLabApiConnection(httpClient));

        JsonElement response = await repository.CreateAsync(
            new CreateDuoWorkflowRequest
            {
                ProjectId = "gitlab-org/gitlab",
                Goal = "Fix the pipeline",
                WorkflowDefinition = "software_developer",
                Environment = GitLabDuoWorkflowEnvironment.ChatPartial,
                Source = GitLabDuoWorkflowSource.MergeRequestFixPipeline,
                AgentPrivileges = [1, 2],
                CallbackHookId = 42,
                AdditionalContext =
                [
                    new DuoWorkflowAdditionalContext { Category = "file", Content = "README.md" }
                ]
            },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/ai/duo_workflows/workflows",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        Assert.NotNull(sentBody);
        using JsonDocument sent = JsonDocument.Parse(sentBody!);
        JsonElement root = sent.RootElement;

        Assert.Equal("gitlab-org/gitlab", root.GetProperty("project_id").GetString());
        Assert.Equal("chat_partial", root.GetProperty("environment").GetString());
        Assert.Equal("merge_request_fix_pipeline", root.GetProperty("source").GetString());
        Assert.Equal(42, root.GetProperty("callback_hook_id").GetInt64());
        Assert.Equal(ExpectedAgentPrivileges,
            root.GetProperty("agent_privileges").EnumerateArray().Select(static e => e.GetInt32()).ToArray());

        // GitLab spells the two additional-context keys with a leading capital, unlike everything else
        // on the wire, so the snake_case policy must not be allowed to touch them.
        JsonElement context = Assert.Single(root.GetProperty("additional_context").EnumerateArray().ToList());
        Assert.Equal("file", context.GetProperty("Category").GetString());
        Assert.Equal("README.md", context.GetProperty("Content").GetString());

        // Unset members are omitted entirely rather than sent as null.
        Assert.False(root.TryGetProperty("namespace_id", out _));

        Assert.Equal(7, response.GetProperty("id").GetInt32());
    }

    [Fact]
    public async Task CreateAgentWorkflowAsync_UsesTheAgentWorkflowsRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = new StringContent("""{"id":8}""", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        DuoWorkflowsRepository repository = new(new GitLabApiConnection(httpClient));

        JsonElement response = await repository.CreateAgentWorkflowAsync(
            new CreateDuoAgentWorkflowRequest { Goal = "Update the changelog" },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/ai/duo_workflows/agent_workflows",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(8, response.GetProperty("id").GetInt32());
    }

    [Fact]
    public async Task GetAsync_BuildsTheWorkflowRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("""{"id":7,"status":"running"}""", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        DuoWorkflowsRepository repository = new(new GitLabApiConnection(httpClient));

        JsonElement workflow = await repository.GetAsync(7, TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/ai/duo_workflows/workflows/7",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("running", workflow.GetProperty("status").GetString());
    }

    [Fact]
    public async Task UpdateStatusAsync_SendsAPatchWithTheStatusEvent()
    {
        string? sentBody = null;

        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"status":"finished"}""", Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        DuoWorkflowsRepository repository = new(new GitLabApiConnection(httpClient));

        await repository.UpdateStatusAsync(7, new UpdateDuoWorkflowStatusRequest { StatusEvent = "finish" },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Patch, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/ai/duo_workflows/workflows/7",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"status_event":"finish"}""", sentBody);
    }

    [Fact]
    public async Task ListAgentPrivilegesAsync_BuildsTheAgentPrivilegesRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("""{"all_privileges":[{"id":1,"name":"read_write_files"}]}""",
                Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        DuoWorkflowsRepository repository = new(new GitLabApiConnection(httpClient));

        JsonElement privileges =
            await repository.ListAgentPrivilegesAsync(TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/ai/duo_workflows/workflows/agent_privileges",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(1, privileges.GetProperty("all_privileges")[0].GetProperty("id").GetInt32());
    }

    [Fact]
    public async Task ResumeAsync_PercentEncodesACallerSuppliedWorkflowId()
    {
        string? sentBody = null;

        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();

            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent("""{"status":"running"}""", Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        DuoWorkflowsRepository repository = new(new GitLabApiConnection(httpClient));

        // The spec types this route's workflow_id as free text, so a value containing '/' must stay one
        // path segment rather than silently becoming a route that does not exist.
        await repository.ResumeAsync(
            "session/7 beta",
            new ResumeDuoWorkflowRequest { HumanApproval = true, HumanMessage = "Looks good" },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/ai/duo_workflows/workflows/session%2F7%20beta/resume",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"human_approval":true,"human_message":"Looks good"}""", sentBody);
    }

    [Fact]
    public async Task GetTraceAsync_StreamsTheJsonLinesBody_AndProjectsItsFilters()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{\"a\":1}\n{\"a\":2}\n", Encoding.UTF8, "application/x-ndjson")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        DuoWorkflowsRepository repository = new(new GitLabApiConnection(httpClient));

        GitLabFileResponse trace = await repository.GetTraceAsync(
            7,
            new DuoWorkflowTraceOptions { Full = true, Thread = "latest" },
            TestContext.Current.CancellationToken);

        await using (trace.ConfigureAwait(true))
        {
            Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
            Assert.Equal(
                "https://gitlab.example/api/v4/ai/duo_workflows/workflows/7/trace.jsonl?full=true&thread=latest",
                handler.LastRequest?.RequestUri?.AbsoluteUri);

            using StreamReader reader = new(trace.Content, Encoding.UTF8);
            Assert.Equal("{\"a\":1}\n{\"a\":2}\n",
                await reader.ReadToEndAsync(TestContext.Current.CancellationToken));
        }
    }

    [Fact]
    public async Task GetTraceAsync_MapsAForbiddenFullTraceToTheTypedException()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Forbidden)
        {
            Content = new StringContent("""{"message":"403 Forbidden"}""", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        DuoWorkflowsRepository repository = new(new GitLabApiConnection(httpClient));

        await Assert.ThrowsAsync<GitLabForbiddenException>(() => repository.GetTraceAsync(
            7,
            new DuoWorkflowTraceOptions { Full = true },
            TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task ListCheckpointsAsync_ProjectsItsFiltersOntoTheQueryString()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        DuoWorkflowsRepository repository = new(new GitLabApiConnection(httpClient));

        await repository.ListCheckpointsAsync(
            7,
            new DuoWorkflowCheckpointListOptions { AcceptCompressed = true, CheckpointNs = "delegate:node/1" },
            TestContext.Current.CancellationToken);

        Assert.Equal(
            "https://gitlab.example/api/v4/ai/duo_workflows/workflows/7/checkpoints"
            + "?accept_compressed=true&checkpoint_ns=delegate%3Anode%2F1",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task CreateCheckpointAsync_SendsTheOpaqueCheckpointAndMetadataVerbatim()
    {
        string? sentBody = null;

        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();

            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent("""{"thread_ts":"1700000000.1"}""", Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        DuoWorkflowsRepository repository = new(new GitLabApiConnection(httpClient));

        using JsonDocument metadata = JsonDocument.Parse("""{"step":3}""");
        using JsonDocument checkpoint = JsonDocument.Parse("""{"channel_values":{"messages":[]}}""");

        JsonElement created = await repository.CreateCheckpointAsync(
            7,
            new CreateDuoWorkflowCheckpointRequest
            {
                ThreadTs = "1700000000.1",
                Metadata = metadata.RootElement,
                Checkpoint = checkpoint.RootElement,
                ChannelKeys = ["messages"],
                ChannelBlobs =
                [
                    new DuoWorkflowChannelBlob
                    {
                        Channel = "messages",
                        Version = "1",
                        WriteType = "json",
                        StepAction = GitLabDuoWorkflowChannelBlobStepAction.Compaction,
                        Data = "eJw="
                    }
                ]
            },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/ai/duo_workflows/workflows/7/checkpoints",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        Assert.NotNull(sentBody);
        using JsonDocument sent = JsonDocument.Parse(sentBody!);
        Assert.Equal(3, sent.RootElement.GetProperty("metadata").GetProperty("step").GetInt32());
        Assert.Equal("compaction",
            sent.RootElement.GetProperty("channel_blobs")[0].GetProperty("step_action").GetString());
        Assert.Equal("1700000000.1", created.GetProperty("thread_ts").GetString());
    }

    [Fact]
    public async Task GetCheckpointByThreadTimestampAsync_EscapesTheThreadTimestampIntoTheQueryString()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("""{"thread_ts":"1700000000.1"}""", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        DuoWorkflowsRepository repository = new(new GitLabApiConnection(httpClient));

        await repository.GetCheckpointByThreadTimestampAsync(7, "1700000000.1+00:00", true,
            TestContext.Current.CancellationToken);

        Assert.Equal(
            "https://gitlab.example/api/v4/ai/duo_workflows/workflows/7/checkpoints/by_thread_ts"
            + "?thread_ts=1700000000.1%2B00%3A00&accept_compressed=true",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task GetCheckpointAsync_BuildsTheCheckpointRoute_AndOmitsTheUnsetFilter()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("""{"id":11}""", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        DuoWorkflowsRepository repository = new(new GitLabApiConnection(httpClient));

        await repository.GetCheckpointAsync(7, 11, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/ai/duo_workflows/workflows/7/checkpoints/11",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task CreateCheckpointWritesAsync_PostsTheBatchAndExpectsNoBody()
    {
        string? sentBody = null;

        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();

            // 201 Created with an empty body: the deserializing overload would fail here, which is why
            // the repository uses the no-content POST for the batch endpoints.
            return new HttpResponseMessage(HttpStatusCode.Created);
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        DuoWorkflowsRepository repository = new(new GitLabApiConnection(httpClient));

        await repository.CreateCheckpointWritesAsync(
            7,
            new CreateDuoWorkflowCheckpointWritesRequest
            {
                ThreadTs = "1700000000.1",
                CheckpointWrites =
                [
                    new DuoWorkflowCheckpointWrite
                    {
                        TaskId = "task-1",
                        Index = 0,
                        Channel = "messages",
                        WriteType = "json",
                        Data = "eJw="
                    }
                ]
            },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/ai/duo_workflows/workflows/7/checkpoint_writes_batch",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        // "task" and "idx" are GitLab's own wire names; the C# members are spelled out for readability.
        Assert.Equal(
            """
            {"thread_ts":"1700000000.1","checkpoint_writes":[{"task":"task-1","idx":0,"channel":"messages","write_type":"json","data":"eJw="}]}
            """,
            sentBody);
    }

    [Fact]
    public async Task ListEventsAsync_BuildsTheEventsRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("""[{"id":3,"event_type":"pause"}]""", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        DuoWorkflowsRepository repository = new(new GitLabApiConnection(httpClient));

        JsonElement events = await repository.ListEventsAsync(7, TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/ai/duo_workflows/workflows/7/events",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(JsonValueKind.Array, events.ValueKind);
        Assert.Equal("pause", events[0].GetProperty("event_type").GetString());
    }

    [Fact]
    public async Task CreateEventAsync_SendsTheUnderscoredEventTypeWireValue()
    {
        string? sentBody = null;

        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();

            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent("""{"id":3}""", Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        DuoWorkflowsRepository repository = new(new GitLabApiConnection(httpClient));

        await repository.CreateEventAsync(
            7,
            new CreateDuoWorkflowEventRequest
            {
                EventType = GitLabDuoWorkflowEventType.RequireInput, Message = "Which branch?"
            },
            TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/ai/duo_workflows/workflows/7/events",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"event_type":"require_input","message":"Which branch?"}""", sentBody);
    }

    [Fact]
    public async Task UpdateEventAsync_PutsTheEventStatus()
    {
        string? sentBody = null;

        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"id":3,"event_status":"delivered"}""", Encoding.UTF8,
                    "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        DuoWorkflowsRepository repository = new(new GitLabApiConnection(httpClient));

        await repository.UpdateEventAsync(7, 3,
            new UpdateDuoWorkflowEventRequest { EventStatus = GitLabDuoWorkflowEventStatus.Delivered },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/ai/duo_workflows/workflows/7/events/3",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"event_status":"delivered"}""", sentBody);
    }

    [Fact]
    public async Task IngestAuditEventsAsync_PostsTheCloudEventsBatch()
    {
        string? sentBody = null;

        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created);
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        DuoWorkflowsRepository repository = new(new GitLabApiConnection(httpClient));

        using JsonDocument data = JsonDocument.Parse("""{"model":"claude"}""");

        await repository.IngestAuditEventsAsync(
            7,
            new IngestDuoWorkflowAuditEventsRequest
            {
                Events =
                [
                    new DuoWorkflowAuditEvent
                    {
                        Id = "6b0f5b1e-0000-4000-8000-000000000000",
                        Type = "ai_llm_input_sent",
                        Source = "duo-workflow-service",
                        Time = new DateTimeOffset(2026, 7, 22, 11, 37, 0, TimeSpan.Zero),
                        Data = data.RootElement
                    }
                ]
            },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/ai/duo_workflows/workflows/7/audit_events",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        Assert.NotNull(sentBody);
        using JsonDocument sent = JsonDocument.Parse(sentBody!);
        JsonElement envelope = sent.RootElement.GetProperty("events")[0];
        Assert.Equal("ai_llm_input_sent", envelope.GetProperty("type").GetString());
        Assert.Equal("claude", envelope.GetProperty("data").GetProperty("model").GetString());
    }

    [Fact]
    public async Task RequestDirectAccessAsync_PostsToTheDirectAccessRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = new StringContent("""{"gitlab_rails":{"base_url":"https://gitlab.example"}}""",
                Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        DuoWorkflowsRepository repository = new(new GitLabApiConnection(httpClient));

        JsonElement access = await repository.RequestDirectAccessAsync(
            new DuoWorkflowDirectAccessRequest { WorkflowDefinition = "software_developer" },
            TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/ai/duo_workflows/direct_access",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("https://gitlab.example",
            access.GetProperty("gitlab_rails").GetProperty("base_url").GetString());
    }

    [Fact]
    public async Task RequestDirectAccessAsync_MapsAnExhaustedQuotaToTheTypedRateLimitException()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.TooManyRequests)
        {
            Content = new StringContent("""{"message":"429 Too Many Requests"}""", Encoding.UTF8,
                "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        DuoWorkflowsRepository repository = new(new GitLabApiConnection(httpClient));

        await Assert.ThrowsAsync<GitLabRateLimitExceededException>(() => repository.RequestDirectAccessAsync(
            new DuoWorkflowDirectAccessRequest(),
            TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task ListToolsAsync_OmitsTheDefinitionFilterWhenUnset()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("""{"tools":[]}""", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        DuoWorkflowsRepository repository = new(new GitLabApiConnection(httpClient));

        await repository.ListToolsAsync(cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/ai/duo_workflows/list_tools",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task ListToolsAsync_AppendsTheDefinitionFilterWhenSet()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("""{"tools":[]}""", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        DuoWorkflowsRepository repository = new(new GitLabApiConnection(httpClient));

        await repository.ListToolsAsync("software_developer", TestContext.Current.CancellationToken);

        Assert.Equal(
            "https://gitlab.example/api/v4/ai/duo_workflows/list_tools?workflow_definition=software_developer",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task GetWebSocketConnectionAsync_PassesTheWorkflowIdAsAQueryParameter()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("""{"url":"wss://gitlab.example/-/cable"}""", Encoding.UTF8,
                "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        DuoWorkflowsRepository repository = new(new GitLabApiConnection(httpClient));

        await repository.GetWebSocketConnectionAsync("7", TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/ai/duo_workflows/ws?workflow_id=7",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task RevokeTokenAsync_PostsTheTokenAndExpectsNoBody()
    {
        string? sentBody = null;

        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK);
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        DuoWorkflowsRepository repository = new(new GitLabApiConnection(httpClient));

        await repository.RevokeTokenAsync(new RevokeDuoWorkflowTokenRequest { Token = "glpat-secret" },
            TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/ai/duo_workflows/revoke_token",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"token":"glpat-secret"}""", sentBody);
    }

    [Fact]
    public async Task AddCodeReviewCommentsAsync_PostsToTheCodeReviewRoute()
    {
        string? sentBody = null;

        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK);
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        DuoWorkflowsRepository repository = new(new GitLabApiConnection(httpClient));

        await repository.AddCodeReviewCommentsAsync(
            new AddDuoCodeReviewCommentsRequest
            {
                ProjectId = "gitlab-org/gitlab", MergeRequestIid = 12, ReviewOutput = "LGTM"
            },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/ai/duo_workflows/code_review/add_comments",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(
            """
            {"project_id":"gitlab-org/gitlab","merge_request_iid":12,"review_output":"LGTM"}
            """,
            sentBody);
    }

    [Fact]
    public async Task GetCodeReviewCustomInstructionsAsync_EscapesTheNamespacedProjectPathIntoTheQueryString()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("""{"custom_instructions":[]}""", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        DuoWorkflowsRepository repository = new(new GitLabApiConnection(httpClient));

        await repository.GetCodeReviewCustomInstructionsAsync("gitlab-org/sub/gitlab", 12,
            TestContext.Current.CancellationToken);

        Assert.Equal(
            "https://gitlab.example/api/v4/ai/duo_workflows/code_review/custom_instructions"
            + "?project_id=gitlab-org%2Fsub%2Fgitlab&merge_request_iid=12",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task SubmitRiskClassificationResultsAsync_PostsTheClaimsAndExpectsNoContent()
    {
        string? sentBody = null;

        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.NoContent);
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        DuoWorkflowsRepository repository = new(new GitLabApiConnection(httpClient));

        await repository.SubmitRiskClassificationResultsAsync(
            new SubmitDuoWorkflowRiskClassificationRequest
            {
                ProjectId = "7",
                MergeRequestIid = 12,
                DiffSha = "0123456789abcdef0123456789abcdef01234567",
                Claims =
                [
                    new DuoWorkflowRiskClassificationClaim
                    {
                        Name = "touches_auth", Value = "true", Evidence = "lib/auth.rb:42"
                    }
                ],
                Summary = "Adds a new session guard."
            },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/ai/duo_workflows/tools/risk_classification/results",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        Assert.NotNull(sentBody);
        using JsonDocument sent = JsonDocument.Parse(sentBody!);
        Assert.Equal("touches_auth", sent.RootElement.GetProperty("claims")[0].GetProperty("name").GetString());
    }

    [Fact]
    public async Task GetAsync_MapsAnUnavailableDuoNamespaceToTheTypedNotFoundException()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent("""{"message":"404 Workflow Not Found"}""", Encoding.UTF8,
                "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        DuoWorkflowsRepository repository = new(new GitLabApiConnection(httpClient));

        GitLabNotFoundException exception =
            await Assert.ThrowsAsync<GitLabNotFoundException>(() =>
                repository.GetAsync(7, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
    }
}