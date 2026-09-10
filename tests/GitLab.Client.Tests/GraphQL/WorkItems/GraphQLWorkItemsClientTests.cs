using System.Net;
using System.Text;
using System.Text.Json;

using GitLab.Client.Abstractions;
using GitLab.Client.Configuration;
using GitLab.Client.Endpoints;
using GitLab.Client.GraphQL.Protocol;
using GitLab.Client.GraphQL.WorkItems;
using GitLab.Client.GraphQL.WorkItems.Connections;
using GitLab.Client.GraphQL.WorkItems.Identifiers;
using GitLab.Client.GraphQL.WorkItems.Models;
using GitLab.Client.GraphQL.WorkItems.Mutations;
using GitLab.Client.GraphQL.WorkItems.Queries;
using GitLab.Client.GraphQL.WorkItems.Widgets.Inputs;
using GitLab.Client.Infrastructure.GraphQL;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Tests.TestSupport;

using Microsoft.Extensions.Options;

namespace GitLab.Client.Tests.GraphQL.WorkItems;

public sealed class GraphQLWorkItemsClientTests
{
    private static readonly Uri RestBaseAddress = new("https://gitlab.example/api/v4/");

    [Fact]
    public async Task GetAsync_ByLocator_PostsAnOperationWithTypedVariablesToTheVersionlessEndpoint()
    {
        string? requestBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            requestBody = request.Content?.ReadAsStringAsync(TestContext.Current.CancellationToken)
                .GetAwaiter().GetResult();
            return JsonResponse("""
                                {
                                  "data": {
                                    "namespace": {
                                      "fullPath": "gitlab-org/gitlab",
                                      "workItem": {
                                        "id": "gid://gitlab/WorkItem/42",
                                        "iid": "7",
                                        "title": "Ship GraphQL",
                                        "state": "OPEN"
                                      }
                                    }
                                  }
                                }
                                """);
        });
        using HttpClient httpClient = new(handler) { BaseAddress = RestBaseAddress };
        GraphQLClient client = CreateClient(httpClient);

        GitLabGraphQLResponse<GitLabWorkItemByLocatorQueryData> response = await client.WorkItems.GetAsync(
            new GitLabWorkItemLocator("gitlab-org/gitlab", 7),
            TestContext.Current.CancellationToken);

        AssertVersionlessGraphQLPost(handler);
        using JsonDocument body = ParseRequestBody(requestBody);
        AssertCoreDocumentDoesNotSelectWidgets(body.RootElement);
        AssertTypedQuery(body.RootElement, "fullPath", "gitlab-org/gitlab");
        Assert.Equal("7", body.RootElement.GetProperty("variables").GetProperty("iid").GetString());

        GitLabWorkItem workItem = Assert.IsType<GitLabWorkItem>(response.Data?.Namespace?.WorkItem);
        GitLabGraphQLGlobalId workItemId = Assert.IsType<GitLabGraphQLGlobalId>(workItem.Id);
        Assert.Equal("gid://gitlab/WorkItem/42", workItemId.Value);
        Assert.Equal("7", workItem.Iid);
        Assert.Equal("Ship GraphQL", workItem.Title);
        Assert.Equal(GitLabWorkItemState.Open, workItem.State);
    }

    [Fact]
    public async Task GetAsync_ByGlobalId_UsesAnOpaqueScalarVariableWithoutInterpolatingItIntoTheDocument()
    {
        string? requestBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            requestBody = request.Content?.ReadAsStringAsync(TestContext.Current.CancellationToken)
                .GetAwaiter().GetResult();
            return JsonResponse("""
                                {
                                  "data": {
                                    "workItem": {
                                      "id": "gid://gitlab/WorkItem/44",
                                      "iid": "8",
                                      "title": "Typed ID",
                                      "state": "CLOSED"
                                    }
                                  }
                                }
                                """);
        });
        using HttpClient httpClient = new(handler) { BaseAddress = RestBaseAddress };
        GraphQLClient client = CreateClient(httpClient);
        GitLabGraphQLGlobalId id = new("gid://gitlab/WorkItem/44");

        GitLabGraphQLResponse<GitLabWorkItemByIdQueryData> response = await client.WorkItems.GetAsync(
            id,
            TestContext.Current.CancellationToken);

        AssertVersionlessGraphQLPost(handler);
        using JsonDocument body = ParseRequestBody(requestBody);
        AssertCoreDocumentDoesNotSelectWidgets(body.RootElement);
        AssertTypedQuery(body.RootElement, "id", id.Value);
        GitLabWorkItem workItem = Assert.IsType<GitLabWorkItem>(response.Data?.WorkItem);
        Assert.Equal("gid://gitlab/WorkItem/44", Assert.IsType<GitLabGraphQLGlobalId>(workItem.Id).Value);
        Assert.Equal(GitLabWorkItemState.Closed, workItem.State);
    }

    [Fact]
    public async Task GetTypesAsync_MapsCursorDataAndKeepsItsCursorInTheVariablesObject()
    {
        string? requestBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            requestBody = request.Content?.ReadAsStringAsync(TestContext.Current.CancellationToken)
                .GetAwaiter().GetResult();
            return JsonResponse("""
                                {
                                  "data": {
                                    "namespace": {
                                      "workItemTypes": {
                                        "nodes": [
                                          {
                                            "id": "gid://gitlab/WorkItemType/3",
                                            "name": "Issue",
                                            "baseType": "ISSUE"
                                          }
                                        ],
                                        "pageInfo": { "hasNextPage": true, "endCursor": "next-type" }
                                      }
                                    }
                                  }
                                }
                                """);
        });
        using HttpClient httpClient = new(handler) { BaseAddress = RestBaseAddress };
        GraphQLClient client = CreateClient(httpClient);

        GitLabGraphQLResponse<GitLabWorkItemTypesQueryData> response = await client.WorkItems.GetTypesAsync(
            "gitlab-org/gitlab",
            25,
            "previous-type",
            TestContext.Current.CancellationToken);

        using JsonDocument body = ParseRequestBody(requestBody);
        AssertCoreDocumentDoesNotSelectWidgets(body.RootElement);
        JsonElement variables = body.RootElement.GetProperty("variables");
        Assert.Equal("gitlab-org/gitlab", variables.GetProperty("fullPath").GetString());
        Assert.Equal(25, variables.GetProperty("first").GetInt32());
        Assert.Equal("previous-type", variables.GetProperty("after").GetString());

        GitLabWorkItemTypeConnection connection = Assert.IsType<GitLabWorkItemTypeConnection>(
            response.Data?.Namespace?.WorkItemTypes);
        GitLabWorkItemType type = Assert.IsType<GitLabWorkItemType>(Assert.Single(connection.Nodes!));
        Assert.Equal("gid://gitlab/WorkItemType/3", Assert.IsType<GitLabGraphQLGlobalId>(type.Id).Value);
        Assert.Equal("Issue", type.Name);
        Assert.True(connection.PageInfo?.HasNextPage);
        Assert.Equal("next-type", connection.PageInfo?.EndCursor);
    }

    [Fact]
    public async Task ListAsync_UsesGraphQLCursorPagingAndPreservesPageInfoWithoutRestLinkHeaders()
    {
        string? requestBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            requestBody = request.Content?.ReadAsStringAsync(TestContext.Current.CancellationToken)
                .GetAwaiter().GetResult();
            return JsonResponse("""
                                {
                                  "data": {
                                    "namespace": {
                                      "workItems": {
                                        "nodes": [
                                          {
                                            "id": "gid://gitlab/WorkItem/51",
                                            "iid": "15",
                                            "title": "Cursor work item",
                                            "state": "OPEN"
                                          }
                                        ],
                                        "count": 1,
                                        "pageInfo": {
                                          "hasNextPage": false,
                                          "hasPreviousPage": true,
                                          "startCursor": "start",
                                          "endCursor": "end"
                                        }
                                      }
                                    }
                                  }
                                }
                                """);
        });
        using HttpClient httpClient = new(handler) { BaseAddress = RestBaseAddress };
        GraphQLClient client = CreateClient(httpClient);

        GitLabGraphQLResponse<GitLabWorkItemsQueryData> response = await client.WorkItems.ListAsync(
            "gitlab-org/gitlab",
            10,
            "start",
            TestContext.Current.CancellationToken);

        AssertVersionlessGraphQLPost(handler);
        using JsonDocument body = ParseRequestBody(requestBody);
        AssertCoreDocumentDoesNotSelectWidgets(body.RootElement);
        JsonElement variables = body.RootElement.GetProperty("variables");
        Assert.Equal("gitlab-org/gitlab", variables.GetProperty("fullPath").GetString());
        Assert.Equal(10, variables.GetProperty("first").GetInt32());
        Assert.Equal("start", variables.GetProperty("after").GetString());

        GitLabWorkItemConnection connection =
            Assert.IsType<GitLabWorkItemConnection>(response.Data?.Namespace?.WorkItems);
        Assert.Equal(1, connection.Count);
        GitLabWorkItem workItem = Assert.IsType<GitLabWorkItem>(Assert.Single(connection.Nodes!));
        Assert.Equal("Cursor work item", workItem.Title);
        Assert.False(connection.PageInfo?.HasNextPage);
        Assert.True(connection.PageInfo?.HasPreviousPage);
        Assert.Equal("end", connection.PageInfo?.EndCursor);
    }

    [Fact]
    public async Task CreateAsync_SendsTheTypedInputAndReturnsTheMutationPayload()
    {
        string? requestBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            requestBody = request.Content?.ReadAsStringAsync(TestContext.Current.CancellationToken)
                .GetAwaiter().GetResult();
            return JsonResponse("""
                                {
                                  "data": {
                                    "workItemCreate": {
                                      "workItem": {
                                        "id": "gid://gitlab/WorkItem/61",
                                        "iid": "16",
                                        "title": "Create through GraphQL",
                                        "state": "OPEN"
                                      },
                                      "errors": [],
                                      "clientMutationId": "create-61"
                                    }
                                  }
                                }
                                """);
        });
        using HttpClient httpClient = new(handler) { BaseAddress = RestBaseAddress };
        GraphQLClient client = CreateClient(httpClient);
        GitLabWorkItemCreateInput input = new(
            "Create through GraphQL",
            "gitlab-org/gitlab",
            new GitLabGraphQLGlobalId("gid://gitlab/WorkItemType/3"))
        {
            DescriptionWidget = new GitLabWorkItemDescriptionWidgetInput { Description = "Typed input" }
        };

        GitLabGraphQLResponse<GitLabWorkItemCreateMutationData> response = await client.WorkItems.CreateAsync(
            input,
            TestContext.Current.CancellationToken);

        AssertVersionlessGraphQLPost(handler);
        using JsonDocument body = ParseRequestBody(requestBody);
        AssertCoreDocumentDoesNotSelectWidgets(body.RootElement);
        Assert.Contains("mutation", body.RootElement.GetProperty("query").GetString(), StringComparison.Ordinal);
        JsonElement serializedInput = body.RootElement.GetProperty("variables").GetProperty("input");
        Assert.Equal("Create through GraphQL", serializedInput.GetProperty("title").GetString());
        Assert.Equal("gitlab-org/gitlab", serializedInput.GetProperty("namespacePath").GetString());
        Assert.Equal("gid://gitlab/WorkItemType/3", serializedInput.GetProperty("workItemTypeId").GetString());
        Assert.Equal("Typed input",
            serializedInput.GetProperty("descriptionWidget").GetProperty("description").GetString());

        GitLabWorkItemCreatePayload payload = Assert.IsType<GitLabWorkItemCreatePayload>(response.Data?.WorkItemCreate);
        Assert.Empty(payload.Errors!);
        Assert.Equal("create-61", payload.ClientMutationId);
        Assert.Equal("Create through GraphQL", payload.WorkItem?.Title);
    }

    [Fact]
    public async Task UpdateAsync_PreservesMutationPayloadAndTopLevelGraphQLErrorsTogether()
    {
        string? requestBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            requestBody = request.Content?.ReadAsStringAsync(TestContext.Current.CancellationToken)
                .GetAwaiter().GetResult();
            return JsonResponse("""
                                {
                                  "data": {
                                    "workItemUpdate": {
                                      "workItem": null,
                                      "errors": ["The selected widget is unavailable."],
                                      "clientMutationId": "update-62"
                                    }
                                  },
                                  "errors": [
                                    { "message": "The color field was not selected.", "path": ["workItemUpdate", "workItem"] }
                                  ]
                                }
                                """);
        });
        using HttpClient httpClient = new(handler) { BaseAddress = RestBaseAddress };
        GraphQLClient client = CreateClient(httpClient);
        GitLabWorkItemUpdateInput input = new(new GitLabGraphQLGlobalId("gid://gitlab/WorkItem/62"))
        {
            Title = "Updated title"
        };

        GitLabGraphQLResponse<GitLabWorkItemUpdateMutationData> response = await client.WorkItems.UpdateAsync(
            input,
            TestContext.Current.CancellationToken);

        using JsonDocument body = ParseRequestBody(requestBody);
        AssertCoreDocumentDoesNotSelectWidgets(body.RootElement);
        Assert.Contains("mutation", body.RootElement.GetProperty("query").GetString(), StringComparison.Ordinal);
        JsonElement serializedInput = body.RootElement.GetProperty("variables").GetProperty("input");
        Assert.Equal("gid://gitlab/WorkItem/62", serializedInput.GetProperty("id").GetString());
        Assert.Equal("Updated title", serializedInput.GetProperty("title").GetString());

        Assert.True(response.HasErrors);
        Assert.Equal("The color field was not selected.", Assert.Single(response.Errors!).Message);
        GitLabWorkItemUpdatePayload payload = Assert.IsType<GitLabWorkItemUpdatePayload>(response.Data?.WorkItemUpdate);
        Assert.Null(payload.WorkItem);
        Assert.Equal("The selected widget is unavailable.", Assert.Single(payload.Errors!));
        Assert.Equal("update-62", payload.ClientMutationId);
    }

    [Fact]
    public async Task DeleteAsync_UsesTheOpaqueIdInputAndReturnsPayloadErrorsWithoutThrowingOnHttp200()
    {
        string? requestBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            requestBody = request.Content?.ReadAsStringAsync(TestContext.Current.CancellationToken)
                .GetAwaiter().GetResult();
            return JsonResponse("""
                                {
                                  "data": {
                                    "workItemDelete": {
                                      "namespace": null,
                                      "errors": ["You are not allowed to delete this work item."],
                                      "clientMutationId": "delete-63"
                                    }
                                  }
                                }
                                """);
        });
        using HttpClient httpClient = new(handler) { BaseAddress = RestBaseAddress };
        GraphQLClient client = CreateClient(httpClient);
        GitLabWorkItemDeleteInput input = new(new GitLabGraphQLGlobalId("gid://gitlab/WorkItem/63"));

        GitLabGraphQLResponse<GitLabWorkItemDeleteMutationData> response = await client.WorkItems.DeleteAsync(
            input,
            TestContext.Current.CancellationToken);

        AssertVersionlessGraphQLPost(handler);
        using JsonDocument body = ParseRequestBody(requestBody);
        AssertCoreDocumentDoesNotSelectWidgets(body.RootElement);
        Assert.Contains("mutation", body.RootElement.GetProperty("query").GetString(), StringComparison.Ordinal);
        Assert.Equal("gid://gitlab/WorkItem/63", body.RootElement.GetProperty("variables")
            .GetProperty("input").GetProperty("id").GetString());

        GitLabWorkItemDeletePayload payload = Assert.IsType<GitLabWorkItemDeletePayload>(response.Data?.WorkItemDelete);
        Assert.Null(payload.Namespace);
        Assert.Equal("You are not allowed to delete this work item.", Assert.Single(payload.Errors!));
        Assert.Equal("delete-63", payload.ClientMutationId);
    }

    [Fact]
    public async Task GetAsync_WithStandardProfile_SelectsOnlyStandardWidgetsAndKeepsTheOpaqueIdInVariables()
    {
        string? requestBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            requestBody = request.Content?.ReadAsStringAsync(TestContext.Current.CancellationToken)
                .GetAwaiter().GetResult();
            return JsonResponse("""{ "data": { "workItem": { "id": "gid://gitlab/WorkItem/101" } } }""");
        });
        using HttpClient httpClient = new(handler) { BaseAddress = RestBaseAddress };
        GraphQLClient client = CreateClient(httpClient);
        GitLabGraphQLGlobalId id = new("gid://gitlab/WorkItem/101");

        await client.WorkItems.GetAsync(id, GitLabWorkItemWidgetProfile.Standard,
            TestContext.Current.CancellationToken);

        AssertVersionlessGraphQLPost(handler);
        using JsonDocument body = ParseRequestBody(requestBody);
        AssertWidgetProfileDocument(
            body.RootElement,
            ["DESCRIPTION", "ASSIGNEES", "LABELS", "MILESTONE"],
            ["COLOR", "START_AND_DUE_DATE", "ITERATION", "HIERARCHY", "HEALTH_STATUS"]);
        AssertTypedQuery(body.RootElement, "id", id.Value);
    }

    [Fact]
    public async Task GetAsync_WithPlanningProfile_SelectsIterationAndHierarchyWithoutUnrelatedStandardWidgets()
    {
        string? requestBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            requestBody = request.Content?.ReadAsStringAsync(TestContext.Current.CancellationToken)
                .GetAwaiter().GetResult();
            return JsonResponse("""
                                { "data": { "namespace": { "workItem": { "id": "gid://gitlab/WorkItem/102" } } } }
                                """);
        });
        using HttpClient httpClient = new(handler) { BaseAddress = RestBaseAddress };
        GraphQLClient client = CreateClient(httpClient);
        GitLabWorkItemLocator locator = new("group/planning", 102);

        await client.WorkItems.GetAsync(locator, GitLabWorkItemWidgetProfile.Planning,
            TestContext.Current.CancellationToken);

        using JsonDocument body = ParseRequestBody(requestBody);
        AssertWidgetProfileDocument(
            body.RootElement,
            ["COLOR", "START_AND_DUE_DATE", "ITERATION", "HIERARCHY"],
            ["DESCRIPTION", "ASSIGNEES", "LABELS", "MILESTONE", "HEALTH_STATUS"]);
        AssertTypedQuery(body.RootElement, "fullPath", "group/planning");
        Assert.Equal("102", body.RootElement.GetProperty("variables").GetProperty("iid").GetString());
    }

    [Fact]
    public async Task GetAsync_WithUltimateProfile_SelectsOnlyTheHealthStatusWidget()
    {
        string? requestBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            requestBody = request.Content?.ReadAsStringAsync(TestContext.Current.CancellationToken)
                .GetAwaiter().GetResult();
            return JsonResponse("""{ "data": { "workItem": { "id": "gid://gitlab/WorkItem/103" } } }""");
        });
        using HttpClient httpClient = new(handler) { BaseAddress = RestBaseAddress };
        GraphQLClient client = CreateClient(httpClient);
        GitLabGraphQLGlobalId id = new("gid://gitlab/WorkItem/103");

        await client.WorkItems.GetAsync(id, GitLabWorkItemWidgetProfile.Ultimate,
            TestContext.Current.CancellationToken);

        using JsonDocument body = ParseRequestBody(requestBody);
        AssertWidgetProfileDocument(
            body.RootElement,
            ["HEALTH_STATUS"],
            [
                "DESCRIPTION", "ASSIGNEES", "LABELS", "MILESTONE", "COLOR", "START_AND_DUE_DATE", "ITERATION",
                "HIERARCHY"
            ]);
        AssertTypedQuery(body.RootElement, "id", id.Value);
    }

    [Fact]
    public async Task ListAsync_WithComprehensiveProfile_SelectsTheClosedUnionAndKeepsCursorValuesOutsideTheDocument()
    {
        string? requestBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            requestBody = request.Content?.ReadAsStringAsync(TestContext.Current.CancellationToken)
                .GetAwaiter().GetResult();
            return JsonResponse("""{ "data": { "namespace": { "workItems": { "nodes": [] } } } }""");
        });
        using HttpClient httpClient = new(handler) { BaseAddress = RestBaseAddress };
        GraphQLClient client = CreateClient(httpClient);

        await client.WorkItems.ListAsync(
            "group/comprehensive",
            25,
            "cursor-104",
            GitLabWorkItemWidgetProfile.Comprehensive,
            TestContext.Current.CancellationToken);

        using JsonDocument body = ParseRequestBody(requestBody);
        AssertWidgetProfileDocument(
            body.RootElement,
            [
                "DESCRIPTION", "ASSIGNEES", "LABELS", "MILESTONE", "COLOR", "START_AND_DUE_DATE", "ITERATION",
                "HIERARCHY", "HEALTH_STATUS"
            ],
            []);
        JsonElement variables = body.RootElement.GetProperty("variables");
        Assert.Equal("group/comprehensive", variables.GetProperty("fullPath").GetString());
        Assert.Equal(25, variables.GetProperty("first").GetInt32());
        Assert.Equal("cursor-104", variables.GetProperty("after").GetString());
        Assert.DoesNotContain("group/comprehensive", body.RootElement.GetProperty("query").GetString(),
            StringComparison.Ordinal);
        Assert.DoesNotContain("cursor-104", body.RootElement.GetProperty("query").GetString(),
            StringComparison.Ordinal);
    }

    private static GraphQLClient CreateClient(HttpClient httpClient)
    {
        IGitLabApiConnection restConnection = new GitLabApiConnection(httpClient);
        IOptionsMonitor<GitLabClientOptions> options = new FixedOptionsMonitor<GitLabClientOptions>(
            new GitLabClientOptions { AccessToken = "glpat-test-token", BaseAddress = RestBaseAddress });
        IGitLabGraphQLConnection graphQLConnection = new GitLabGraphQLConnection(restConnection, options);

        return new GraphQLClient(graphQLConnection);
    }

    private static void AssertVersionlessGraphQLPost(StubHttpMessageHandler handler)
    {
        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/graphql", handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    private static void AssertTypedQuery(JsonElement request, string variableName, string expectedValue)
    {
        string query = Assert.IsType<string>(request.GetProperty("query").GetString());

        Assert.Contains("$", query, StringComparison.Ordinal);
        Assert.DoesNotContain(expectedValue, query, StringComparison.Ordinal);
        Assert.Equal(expectedValue, request.GetProperty("variables").GetProperty(variableName).GetString());
    }

    private static void AssertCoreDocumentDoesNotSelectWidgets(JsonElement request)
    {
        string query = Assert.IsType<string>(request.GetProperty("query").GetString());

        Assert.DoesNotContain("widgets", query, StringComparison.Ordinal);
        Assert.DoesNotContain("__typename", query, StringComparison.Ordinal);
    }

    private static void AssertWidgetProfileDocument(
        JsonElement request,
        IReadOnlyList<string> expectedWidgetTypes,
        IReadOnlyList<string> excludedWidgetTypes)
    {
        string query = Assert.IsType<string>(request.GetProperty("query").GetString());

        Assert.Contains("widgets(onlyTypes:", query, StringComparison.Ordinal);
        Assert.Contains("__typename", query, StringComparison.Ordinal);

        foreach (string expected in expectedWidgetTypes)
        {
            Assert.Contains(expected, query, StringComparison.Ordinal);
            Assert.Contains("... on " + GetWidgetFragmentType(expected), query, StringComparison.Ordinal);
        }

        foreach (string excluded in excludedWidgetTypes)
        {
            Assert.DoesNotContain(excluded, query, StringComparison.Ordinal);
            Assert.DoesNotContain("... on " + GetWidgetFragmentType(excluded), query, StringComparison.Ordinal);
        }
    }

    private static string GetWidgetFragmentType(string widgetType)
    {
        return widgetType switch
        {
            "DESCRIPTION" => "WorkItemWidgetDescription",
            "ASSIGNEES" => "WorkItemWidgetAssignees",
            "LABELS" => "WorkItemWidgetLabels",
            "MILESTONE" => "WorkItemWidgetMilestone",
            "COLOR" => "WorkItemWidgetColor",
            "START_AND_DUE_DATE" => "WorkItemWidgetStartAndDueDate",
            "ITERATION" => "WorkItemWidgetIteration",
            "HIERARCHY" => "WorkItemWidgetHierarchy",
            "HEALTH_STATUS" => "WorkItemWidgetHealthStatus",
            _ => throw new ArgumentOutOfRangeException(nameof(widgetType), widgetType, "Unknown curated widget type.")
        };
    }

    private static JsonDocument ParseRequestBody(string? requestBody)
    {
        return JsonDocument.Parse(Assert.IsType<string>(requestBody));
    }

    private static HttpResponseMessage JsonResponse(string content)
    {
        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(content, Encoding.UTF8, "application/json")
        };
    }

    private sealed class FixedOptionsMonitor<TOptions>(TOptions currentValue) : IOptionsMonitor<TOptions>
        where TOptions : class
    {
        public TOptions CurrentValue => currentValue;

        public TOptions Get(string? name)
        {
            return currentValue;
        }

        public IDisposable OnChange(Action<TOptions, string?> listener)
        {
            return NoOpDisposable.Instance;
        }

        private sealed class NoOpDisposable : IDisposable
        {
            public static NoOpDisposable Instance { get; } = new();

            public void Dispose()
            {
            }
        }
    }
}