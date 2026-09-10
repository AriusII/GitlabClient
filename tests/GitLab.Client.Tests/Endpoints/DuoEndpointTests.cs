using System.Net;
using System.Text;
using System.Text.Json;

using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Endpoints;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Endpoints;

public sealed class DuoEndpointTests
{
    private const string CompletionJson = """
                                          {
                                            "id": "id",
                                            "model": { "engine": "vertex-ai", "name": "code-gecko", "lang": "ruby" },
                                            "object": "text_completion",
                                            "created": 1682031100,
                                            "choices": [
                                              { "text": "  puts 'hello'", "index": 0, "finish_reason": "length" }
                                            ]
                                          }
                                          """;

    private const string GlqlResultJson = """
                                          {
                                            "success": true,
                                            "data": {
                                              "count": 2,
                                              "nodes": [
                                                { "title": "First", "iid": 1 },
                                                { "title": "Second", "iid": 2 }
                                              ],
                                              "pageInfo": {
                                                "endCursor": "eyJpZCI6IjE3In0",
                                                "hasNextPage": true,
                                                "hasPreviousPage": false,
                                                "startCursor": "eyJpZCI6IjEifQ"
                                              }
                                            },
                                            "fields": [
                                              { "key": "title", "label": "Title", "name": "title", "field": "title" },
                                              {
                                                "key": "p50",
                                                "label": "P50",
                                                "name": "durationQuantile",
                                                "field": "durationQuantile",
                                                "type": "metric",
                                                "parameters": { "granularity": "weekly" }
                                              }
                                            ]
                                          }
                                          """;

    private static StubHttpMessageHandler RespondWith(string json, HttpStatusCode status = HttpStatusCode.OK)
    {
        return new StubHttpMessageHandler(_ => new HttpResponseMessage(status)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });
    }

    [Fact]
    public async Task GenerateCodeCompletionAsync_PostsToTheCompletionsRoute_AndReturnsTheRawEnvelope()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(CompletionJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        DuoClient repository = new(connection);

        JsonElement completion = await repository.GenerateCodeCompletionAsync(
            new GenerateCodeCompletionRequest
            {
                CurrentFile =
                    new CodeSuggestionCurrentFile
                    {
                        FileName = "app/models/user.rb",
                        ContentAboveCursor = "def hello\n",
                        ContentBelowCursor = "end\n"
                    },
                Intent = CodeSuggestionIntent.Generation,
                GenerationType = CodeSuggestionGenerationType.EmptyFunction,
                ProjectPath = "gitlab-org/gitlab",
                Context =
                [
                    new CodeSuggestionContextPart
                    {
                        Type = CodeSuggestionContextType.Snippet, Name = "helper.rb", Content = "def helper; end"
                    }
                ]
            },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/code_suggestions/completions",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        Assert.NotNull(sentBody);
        Assert.Contains("\"current_file\":{\"file_name\":\"app/models/user.rb\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"content_above_cursor\":\"def hello\\n\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"intent\":\"generation\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"generation_type\":\"empty_function\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"project_path\":\"gitlab-org/gitlab\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"context\":[{\"type\":\"snippet\"", sentBody, StringComparison.Ordinal);

        // The spec declares no response schema, so the answer stays a raw element the caller reads itself.
        Assert.Equal(JsonValueKind.Object, completion.ValueKind);
        Assert.Equal("  puts 'hello'", completion.GetProperty("choices")[0].GetProperty("text").GetString());
    }

    [Fact]
    public async Task GetCodeSuggestionsConnectionDetailsAsync_PostsWithNoBody()
    {
        bool hadContent = true;
        using StubHttpMessageHandler handler = new(request =>
        {
            hadContent = request.Content is not null;
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent("""{"application_id":"abc"}""", Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        DuoClient repository = new(connection);

        JsonElement details =
            await repository.GetCodeSuggestionsConnectionDetailsAsync(TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/code_suggestions/connection_details",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.False(hadContent);
        Assert.Equal("abc", details.GetProperty("application_id").GetString());
    }

    [Fact]
    public async Task GetCodeSuggestionsDirectAccessAsync_WithoutARequest_SendsNoBodyAtAll()
    {
        bool hadContent = true;
        using StubHttpMessageHandler handler = new(request =>
        {
            hadContent = request.Content is not null;
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent("""{"base_url":"https://cloud.gitlab.com/ai"}""", Encoding.UTF8,
                    "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        DuoClient repository = new(connection);

        JsonElement access =
            await repository.GetCodeSuggestionsDirectAccessAsync(cancellationToken: TestContext.Current
                .CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/code_suggestions/direct_access",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.False(hadContent);
        Assert.Equal("https://cloud.gitlab.com/ai", access.GetProperty("base_url").GetString());
    }

    [Fact]
    public async Task GetCodeSuggestionsDirectAccessAsync_WithARequest_SendsTheProjectPath()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent("{}", Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        DuoClient repository = new(connection);

        await repository.GetCodeSuggestionsDirectAccessAsync(
            new CodeSuggestionsDirectAccessRequest { ProjectPath = "gitlab-org/gitlab" },
            TestContext.Current.CancellationToken);

        Assert.Equal("""{"project_path":"gitlab-org/gitlab"}""", sentBody);
    }

    [Fact]
    public async Task ValidateCodeSuggestionsEnabledAsync_PostsTheProjectPath_AndIgnoresTheEmptyBody()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK);
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        DuoClient repository = new(connection);

        await repository.ValidateCodeSuggestionsEnabledAsync(
            new CodeSuggestionsEnabledRequest { ProjectPath = "gitlab-org/gitlab" },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/code_suggestions/enabled",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"project_path":"gitlab-org/gitlab"}""", sentBody);
    }

    [Fact]
    public async Task ValidateCodeSuggestionsEnabledAsync_SurfacesTheDisabledAnswerAsForbidden()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Forbidden)
        {
            Content = new StringContent("""{"message":"403 Code Suggestions Disabled"}""", Encoding.UTF8,
                "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        DuoClient repository = new(connection);

        GitLabForbiddenException exception = await Assert.ThrowsAsync<GitLabForbiddenException>(() =>
            repository.ValidateCodeSuggestionsEnabledAsync(
                new CodeSuggestionsEnabledRequest { ProjectPath = "gitlab-org/gitlab" },
                TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.Forbidden, exception.StatusCode);
    }

    [Fact]
    public async Task GenerateGitCommandAsync_PostsToTheLlmGitCommandRoute()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent("""{"predictions":[{"candidate":"git reset --soft HEAD~2"}]}""",
                    Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        DuoClient repository = new(connection);

        JsonElement result = await repository.GenerateGitCommandAsync(
            new GenerateGitCommandRequest { Prompt = "undo my last two commits but keep the changes" },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/ai/llm/git_command",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"prompt":"undo my last two commits but keep the changes"}""", sentBody);
        Assert.Equal("git reset --soft HEAD~2",
            result.GetProperty("predictions")[0].GetProperty("candidate").GetString());
    }

    [Fact]
    public async Task GetThirdPartyAgentsDirectAccessAsync_PostsToTheThirdPartyAgentsRoute_WithNoBody()
    {
        bool hadContent = true;
        using StubHttpMessageHandler handler = new(request =>
        {
            hadContent = request.Content is not null;
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent("""{"token":"secret","expires_at":1750000000}""", Encoding.UTF8,
                    "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        DuoClient repository = new(connection);

        JsonElement access =
            await repository.GetThirdPartyAgentsDirectAccessAsync(TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/ai/third_party_agents/direct_access",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.False(hadContent);
        Assert.Equal("secret", access.GetProperty("token").GetString());
    }

    [Fact]
    public async Task ChatAsync_PostsToTheChatCompletionsRoute_WithGitLabsOwnVocabulary()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent("""{"request_id":"c1"}""", Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        DuoClient repository = new(connection);

        using JsonDocument resourceId = JsonDocument.Parse("42");

        JsonElement answer = await repository.ChatAsync(
            new DuoChatRequest
            {
                Content = "What changed here?",
                ResourceType = DuoChatResourceType.MergeRequest,
                ResourceId = resourceId.RootElement.Clone(),
                RefererUrl = new Uri("https://gitlab.example/gitlab-org/gitlab/-/merge_requests/7"),
                WithCleanHistory = true,
                AdditionalContext =
                [
                    new DuoChatAdditionalContext
                    {
                        Category = DuoChatContextCategory.LocalGit, Id = "status", Content = "On branch main"
                    }
                ]
            },
            TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/chat/completions", handler.LastRequest?.RequestUri?.AbsoluteUri);

        Assert.NotNull(sentBody);
        Assert.Contains("\"resource_type\":\"merge_request\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"resource_id\":42", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"with_clean_history\":true", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"category\":\"local_git\"", sentBody, StringComparison.Ordinal);
        Assert.Contains(
            "\"referer_url\":\"https://gitlab.example/gitlab-org/gitlab/-/merge_requests/7\"",
            sentBody,
            StringComparison.Ordinal);

        Assert.Equal("c1", answer.GetProperty("request_id").GetString());
    }

    [Fact]
    public async Task EvaluateCodeReviewAsync_PostsToTheEvaluationsRoute_WithTheFileMap()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent("""{"review":"Looks good."}""", Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        DuoClient repository = new(connection);

        await repository.EvaluateCodeReviewAsync(
            new EvaluateCodeReviewRequest
            {
                Diffs = "@@ -1 +1 @@",
                MrTitle = "Fix the thing",
                MrDescription = "It was broken.",
                FilesContent = new Dictionary<string, string> { ["app/main.rb"] = "puts 1" }
            },
            TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/duo_code_review/evaluations",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        Assert.NotNull(sentBody);
        Assert.Contains("\"mr_title\":\"Fix the thing\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"mr_description\":\"It was broken.\"", sentBody, StringComparison.Ordinal);

        // The dictionary keys are file paths and must survive verbatim - the snake_case naming policy
        // applies to member names, never to dictionary keys.
        Assert.Contains("\"files_content\":{\"app/main.rb\":\"puts 1\"}", sentBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ExecuteGlqlQueryAsync_PostsToTheGlqlRoute_AndDeserializesTheCamelCasePageInfo()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(GlqlResultJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        DuoClient repository = new(connection);

        GitLabGlqlResult result = await repository.ExecuteGlqlQueryAsync(
            new ExecuteGlqlQueryRequest { GlqlYaml = "query: assignee = currentUser()", After = "eyJpZCI6IjEifQ" },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/glql", handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.NotNull(sentBody);
        Assert.Contains("\"glql_yaml\":\"query: assignee = currentUser()\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"after\":\"eyJpZCI6IjEifQ\"", sentBody, StringComparison.Ordinal);

        Assert.True(result.Success);
        Assert.Null(result.Error);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data!.Count);

        // GLQL is GraphQL-derived, so these four keys are camelCase while the rest of the API is snake_case.
        Assert.NotNull(result.Data.PageInfo);
        Assert.Equal("eyJpZCI6IjE3In0", result.Data.PageInfo!.EndCursor);
        Assert.True(result.Data.PageInfo.HasNextPage);
        Assert.False(result.Data.PageInfo.HasPreviousPage);
        Assert.Equal("eyJpZCI6IjEifQ", result.Data.PageInfo.StartCursor);

        // The rows themselves are shaped by the query, so they stay a raw element.
        Assert.NotNull(result.Data.Nodes);
        Assert.Equal(JsonValueKind.Array, result.Data.Nodes!.Value.ValueKind);
        Assert.Equal("Second", result.Data.Nodes.Value[1].GetProperty("title").GetString());

        Assert.NotNull(result.Fields);
        Assert.Equal(2, result.Fields!.Count);
        Assert.Equal("title", result.Fields[0].Key);
        Assert.Null(result.Fields[0].Type);
        Assert.Equal("p50", result.Fields[1].Key);
        Assert.Equal("durationQuantile", result.Fields[1].Name);
        Assert.Equal("metric", result.Fields[1].Type);
        Assert.Equal("weekly", result.Fields[1].Parameters?.GetProperty("granularity").GetString());
    }

    [Fact]
    public async Task ExecuteGlqlQueryAsync_ReportsARejectedQueryInsideASuccessfulResponse()
    {
        using StubHttpMessageHandler handler = RespondWith(
            """{"success":false,"error":"Unknown field: assigne"}""");

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        DuoClient repository = new(connection);

        GitLabGlqlResult result = await repository.ExecuteGlqlQueryAsync(
            new ExecuteGlqlQueryRequest { GlqlYaml = "query: assigne = currentUser()" },
            TestContext.Current.CancellationToken);

        Assert.False(result.Success);
        Assert.Equal("Unknown field: assigne", result.Error);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task GetGlqlSchemaAsync_GetsTheSchemaRoute()
    {
        using StubHttpMessageHandler handler = RespondWith("""{"sources":[{"name":"issues"}]}""");

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        DuoClient repository = new(connection);

        JsonElement schema = await repository.GetGlqlSchemaAsync(TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/glql/schema", handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("issues", schema.GetProperty("sources")[0].GetProperty("name").GetString());
    }

    [Fact]
    public async Task SendMcpRequestAsync_PostsAJsonRpcEnvelope_WithTheFixedProtocolVersion()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent("""{"jsonrpc":"2.0","id":1,"result":{"tools":[]}}""", Encoding.UTF8,
                    "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        DuoClient repository = new(connection);

        using JsonDocument id = JsonDocument.Parse("1");
        using JsonDocument parameters = JsonDocument.Parse("""{"cursor":"abc"}""");

        JsonElement response = await repository.SendMcpRequestAsync(
            new McpJsonRpcRequest
            {
                Method = "tools/list", Id = id.RootElement.Clone(), Parameters = parameters.RootElement.Clone()
            },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/mcp", handler.LastRequest?.RequestUri?.AbsoluteUri);

        Assert.NotNull(sentBody);
        Assert.Contains("\"jsonrpc\":\"2.0\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"method\":\"tools/list\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"id\":1", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"params\":{\"cursor\":\"abc\"}", sentBody, StringComparison.Ordinal);

        Assert.Equal(JsonValueKind.Array, response.GetProperty("result").GetProperty("tools").ValueKind);
    }

    [Fact]
    public async Task ListenMcpAsync_GetsTheMcpRoute()
    {
        using StubHttpMessageHandler handler = RespondWith("""{"jsonrpc":"2.0","id":1,"result":{}}""");

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        DuoClient repository = new(connection);

        JsonElement response = await repository.ListenMcpAsync(TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/mcp", handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("2.0", response.GetProperty("jsonrpc").GetString());
    }
}