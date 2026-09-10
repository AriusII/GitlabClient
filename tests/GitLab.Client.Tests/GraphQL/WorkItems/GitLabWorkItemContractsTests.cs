using System.Text.Json;

using GitLab.Client.GraphQL.Protocol;
using GitLab.Client.GraphQL.WorkItems.Connections;
using GitLab.Client.GraphQL.WorkItems.Identifiers;
using GitLab.Client.GraphQL.WorkItems.Models;
using GitLab.Client.GraphQL.WorkItems.Mutations;
using GitLab.Client.GraphQL.WorkItems.Queries;
using GitLab.Client.GraphQL.WorkItems.Widgets;
using GitLab.Client.GraphQL.WorkItems.Widgets.Inputs;

using GitLabGraphQLJsonContext = GitLab.Client.GraphQL.Serialization.GitLabGraphQLJsonContext;

namespace GitLab.Client.Tests.GraphQL.WorkItems;

public sealed class GitLabWorkItemContractsTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" \t")]
    public void GlobalId_BlankValue_ThrowsArgumentException(string? value)
    {
        ArgumentException exception = Assert.ThrowsAny<ArgumentException>(() => new GitLabGraphQLGlobalId(value!));

        Assert.Equal("value", exception.ParamName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void Locator_BlankNamespacePath_ThrowsArgumentException(string? namespacePath)
    {
        ArgumentException exception = Assert.ThrowsAny<ArgumentException>(() =>
            new GitLabWorkItemLocator(namespacePath!, 1));

        Assert.Equal("namespacePath", exception.ParamName);
    }

    [Theory]
    [InlineData(0L)]
    [InlineData(-1L)]
    public void Locator_NonPositiveIid_ThrowsArgumentOutOfRangeException(long iid)
    {
        ArgumentOutOfRangeException exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new GitLabWorkItemLocator("group/project", iid));

        Assert.Equal("iid", exception.ParamName);
    }

    [Fact]
    public void CreateInput_MissingRequiredIdentity_ThrowsAtTheBoundary()
    {
        GitLabGraphQLGlobalId typeId = new("gid://gitlab/WorkItemType/4");

        ArgumentException blankTitle = Assert.Throws<ArgumentException>(() =>
            new GitLabWorkItemCreateInput(" ", "group/project", typeId));
        ArgumentException blankNamespace = Assert.Throws<ArgumentException>(() =>
            new GitLabWorkItemCreateInput("Title", " ", typeId));
        ArgumentNullException nullType = Assert.Throws<ArgumentNullException>(() =>
            new GitLabWorkItemCreateInput("Title", "group/project", null!));

        Assert.Equal("title", blankTitle.ParamName);
        Assert.Equal("namespacePath", blankNamespace.ParamName);
        Assert.Equal("workItemTypeId", nullType.ParamName);
    }

    [Fact]
    public void QueryVariables_InvalidPaginationBoundaries_ThrowInsteadOfSendingInvalidGraphQL()
    {
        ArgumentOutOfRangeException zeroPage = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new GitLabWorkItemsQueryVariables("group/project", 0));
        ArgumentOutOfRangeException negativePage = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new GitLabWorkItemTypesQueryVariables("group/project", -1));
        ArgumentException blankCursor = Assert.Throws<ArgumentException>(() =>
            new GitLabWorkItemsQueryVariables("group/project", after: " "));

        Assert.Equal("first", zeroPage.ParamName);
        Assert.Equal("first", negativePage.ParamName);
        Assert.Equal("after", blankCursor.ParamName);
    }

    [Fact]
    public void CreateVariables_SourceGeneratedJson_UsesCamelCaseAndWritesOpaqueIdsAsScalars()
    {
        GitLabWorkItemCreateInput input = new(
            "Ship the GraphQL SDK",
            "gitlab-org/gitlab",
            new GitLabGraphQLGlobalId("gid://gitlab/WorkItemType/18"))
        {
            Confidential = true,
            DescriptionWidget = new GitLabWorkItemDescriptionWidgetInput { Description = "AOT-safe." },
            AssigneesWidget = new GitLabWorkItemAssigneesWidgetInput
            {
                AssigneeIds = [new GitLabGraphQLGlobalId("gid://gitlab/User/9")]
            }
        };

        string json = JsonSerializer.Serialize(
            new GitLabWorkItemCreateMutationVariables(input),
            GitLabGraphQLJsonContext.Default.GitLabWorkItemCreateMutationVariables);

        using JsonDocument document = JsonDocument.Parse(json);
        JsonElement root = document.RootElement;
        JsonElement serializedInput = root.GetProperty("input");

        Assert.Equal("Ship the GraphQL SDK", serializedInput.GetProperty("title").GetString());
        Assert.Equal("gitlab-org/gitlab", serializedInput.GetProperty("namespacePath").GetString());
        Assert.Equal("gid://gitlab/WorkItemType/18", serializedInput.GetProperty("workItemTypeId").GetString());
        Assert.True(serializedInput.GetProperty("confidential").GetBoolean());
        Assert.Equal("AOT-safe.",
            serializedInput.GetProperty("descriptionWidget").GetProperty("description").GetString());
        Assert.Equal("gid://gitlab/User/9", serializedInput.GetProperty("assigneesWidget")
            .GetProperty("assigneeIds")[0].GetString());
        Assert.False(serializedInput.TryGetProperty("colorWidget", out _));
        Assert.False(serializedInput.TryGetProperty("startAndDueDateWidget", out _));
        Assert.False(json.Contains("namespace_path", StringComparison.Ordinal));
        Assert.False(json.Contains("work_item_type_id", StringComparison.Ordinal));
    }

    [Fact]
    public void LocatorVariables_SourceGeneratedJson_UsesInvariantStringIid()
    {
        GitLabWorkItemByLocatorQueryVariables variables = new(new GitLabWorkItemLocator("group/project", 42));
        string json = JsonSerializer.Serialize(
            variables,
            GitLabGraphQLJsonContext.Default.GitLabWorkItemByLocatorQueryVariables);

        using JsonDocument document = JsonDocument.Parse(json);
        Assert.Equal("group/project", document.RootElement.GetProperty("fullPath").GetString());
        Assert.Equal("42", document.RootElement.GetProperty("iid").GetString());
    }

    [Fact]
    public void ResponseDeserialization_SourceGeneratedContextMaterializesEveryCuratedWidgetDiscriminator()
    {
        const string responseJson = """
                                    {
                                      "data": {
                                        "workItem": {
                                          "id": "gid://gitlab/WorkItem/71",
                                          "iid": "17",
                                          "title": "Polymorphic widgets",
                                          "state": "OPEN",
                                          "widgets": [
                                            {
                                              "__typename": "WorkItemWidgetDescription",
                                              "description": "A description",
                                              "descriptionHtml": "<p>A description</p>"
                                            },
                                            {
                                              "__typename": "WorkItemWidgetAssignees",
                                              "assignees": {
                                                "nodes": [
                                                  {
                                                    "id": "gid://gitlab/User/9",
                                                    "name": "Root",
                                                    "username": "root",
                                                    "webUrl": "https://gitlab.example/root"
                                                  },
                                                  null
                                                ],
                                                "pageInfo": {
                                                  "hasNextPage": false,
                                                  "endCursor": "assignee-end"
                                                }
                                              }
                                            },
                                            {
                                              "__typename": "WorkItemWidgetColor",
                                              "color": "#1068bf",
                                              "textColor": "#ffffff"
                                            },
                                            {
                                              "__typename": "WorkItemWidgetHealthStatus",
                                              "healthStatus": "onTrack"
                                            },
                                            {
                                              "__typename": "WorkItemWidgetStartAndDueDate",
                                              "startDate": "2026-01-02",
                                              "dueDate": "2026-02-03",
                                              "isFixed": true
                                            },
                                            null
                                          ]
                                        }
                                      },
                                      "errors": [
                                        {
                                          "message": "One selected widget was unavailable.",
                                          "path": ["workItem", "widgets", 5]
                                        }
                                      ]
                                    }
                                    """;

        GitLabGraphQLResponse<GitLabWorkItemByIdQueryData>? response = JsonSerializer.Deserialize(
            responseJson,
            GitLabGraphQLJsonContext.Default.GitLabGraphQLResponseGitLabWorkItemByIdQueryData);

        GitLabGraphQLResponse<GitLabWorkItemByIdQueryData> nonNullResponse = Assert.IsType<
            GitLabGraphQLResponse<GitLabWorkItemByIdQueryData>>(response);
        GitLabWorkItem workItem = Assert.IsType<GitLabWorkItem>(nonNullResponse.Data?.WorkItem);
        IReadOnlyList<GitLabWorkItemWidget?> widgets = Assert.IsAssignableFrom<IReadOnlyList<GitLabWorkItemWidget?>>(
            workItem.Widgets);

        GitLabWorkItemDescriptionWidget description = Assert.IsType<GitLabWorkItemDescriptionWidget>(widgets[0]);
        GitLabWorkItemAssigneesWidget assignees = Assert.IsType<GitLabWorkItemAssigneesWidget>(widgets[1]);
        GitLabWorkItemColorWidget color = Assert.IsType<GitLabWorkItemColorWidget>(widgets[2]);
        GitLabWorkItemHealthStatusWidget health = Assert.IsType<GitLabWorkItemHealthStatusWidget>(widgets[3]);
        GitLabWorkItemStartAndDueDateWidget dates = Assert.IsType<GitLabWorkItemStartAndDueDateWidget>(widgets[4]);

        Assert.Equal("A description", description.Description);
        Assert.Equal("Root", assignees.Assignees?.Nodes?[0]?.Name);
        Assert.Null(assignees.Assignees?.Nodes?[1]);
        Assert.Equal("assignee-end", assignees.Assignees?.PageInfo?.EndCursor);
        Assert.Equal("#1068bf", color.Color);
        Assert.Equal("#ffffff", color.TextColor);
        Assert.Equal(GitLabWorkItemHealthStatus.OnTrack, health.HealthStatus);
        Assert.Equal(new DateOnly(2026, 1, 2), dates.StartDate);
        Assert.Equal(new DateOnly(2026, 2, 3), dates.DueDate);
        Assert.True(dates.IsFixed);
        Assert.Null(widgets[5]);

        Assert.True(nonNullResponse.HasErrors);
        Assert.Equal("One selected widget was unavailable.", Assert.Single(nonNullResponse.Errors!).Message);
    }

    [Fact]
    public void ResponseDeserialization_MissingWidgetsRemainDistinctFromAnEmptyWidgetSelection()
    {
        const string responseJson = """
                                    {
                                      "data": {
                                        "workItem": {
                                          "id": "gid://gitlab/Issue/81",
                                          "iid": "20",
                                          "title": "Core-only partial data",
                                          "state": "OPEN"
                                        }
                                      },
                                      "errors": [
                                        {
                                          "message": "Widget access is not available.",
                                          "path": ["workItem", "widgets"]
                                        }
                                      ]
                                    }
                                    """;

        GitLabGraphQLResponse<GitLabWorkItemByIdQueryData>? response = JsonSerializer.Deserialize(
            responseJson,
            GitLabGraphQLJsonContext.Default.GitLabGraphQLResponseGitLabWorkItemByIdQueryData);

        GitLabGraphQLResponse<GitLabWorkItemByIdQueryData> nonNullResponse = Assert.IsType<
            GitLabGraphQLResponse<GitLabWorkItemByIdQueryData>>(response);
        GitLabWorkItem workItem = Assert.IsType<GitLabWorkItem>(nonNullResponse.Data?.WorkItem);

        // Global IDs remain opaque strings: even the temporary Issue-form WorkItemID must never be coerced to long.
        Assert.Equal("gid://gitlab/Issue/81", workItem.Id?.Value);
        Assert.Null(workItem.Widgets);
        Assert.True(nonNullResponse.HasErrors);
        Assert.Equal("Widget access is not available.", Assert.Single(nonNullResponse.Errors!).Message);
    }

    [Fact]
    public void ResponseDeserialization_SourceGeneratedContextMaterializesTheExtendedProfileWidgets()
    {
        const string responseJson = """
                                    {
                                      "data": {
                                        "workItem": {
                                          "id": "gid://gitlab/WorkItem/91",
                                          "iid": "31",
                                          "title": "Profiled planning work item",
                                          "state": "OPEN",
                                          "widgets": [
                                            {
                                              "__typename": "WorkItemWidgetLabels",
                                              "labels": {
                                                "nodes": [
                                                  {
                                                    "id": "gid://gitlab/Label/3",
                                                    "title": "platform",
                                                    "description": "Platform-owned",
                                                    "color": "#1068bf",
                                                    "textColor": "#ffffff"
                                                  }
                                                ],
                                                "pageInfo": { "hasNextPage": false, "endCursor": "label-end" }
                                              }
                                            },
                                            {
                                              "__typename": "WorkItemWidgetMilestone",
                                              "milestone": {
                                                "id": "gid://gitlab/Milestone/4",
                                                "title": "19.4",
                                                "description": "Compatibility release",
                                                "startDate": "2026-09-01",
                                                "dueDate": "2026-09-30",
                                                "webUrl": "https://gitlab.example/groups/platform/-/milestones/4"
                                              }
                                            },
                                            {
                                              "__typename": "WorkItemWidgetIteration",
                                              "iteration": {
                                                "id": "gid://gitlab/Iteration/5",
                                                "title": "Sprint 19",
                                                "startDate": "2026-09-08",
                                                "dueDate": "2026-09-21",
                                                "webUrl": "https://gitlab.example/groups/platform/-/iterations/5"
                                              }
                                            },
                                            {
                                              "__typename": "WorkItemWidgetHierarchy",
                                              "parent": {
                                                "id": "gid://gitlab/WorkItem/90",
                                                "iid": "30",
                                                "title": "Parent",
                                                "state": "OPEN"
                                              },
                                              "children": {
                                                "nodes": [
                                                  {
                                                    "id": "gid://gitlab/WorkItem/92",
                                                    "iid": "32",
                                                    "title": "Child",
                                                    "state": "CLOSED"
                                                  },
                                                  null
                                                ],
                                                "pageInfo": { "hasNextPage": true, "endCursor": "children-end" }
                                              },
                                              "ancestors": {
                                                "nodes": [],
                                                "pageInfo": { "hasPreviousPage": false }
                                              },
                                              "hasChildren": true,
                                              "hasParent": true
                                            }
                                          ]
                                        }
                                      }
                                    }
                                    """;

        GitLabGraphQLResponse<GitLabWorkItemByIdQueryData>? response = JsonSerializer.Deserialize(
            responseJson,
            GitLabGraphQLJsonContext.Default.GitLabGraphQLResponseGitLabWorkItemByIdQueryData);

        GitLabWorkItem workItem = Assert.IsType<GitLabWorkItem>(response?.Data?.WorkItem);
        IReadOnlyList<GitLabWorkItemWidget?> widgets = Assert.IsAssignableFrom<IReadOnlyList<GitLabWorkItemWidget?>>(
            workItem.Widgets);

        GitLabWorkItemLabelsWidget labels = Assert.IsType<GitLabWorkItemLabelsWidget>(widgets[0]);
        GitLabWorkItemMilestoneWidget milestone = Assert.IsType<GitLabWorkItemMilestoneWidget>(widgets[1]);
        GitLabWorkItemIterationWidget iteration = Assert.IsType<GitLabWorkItemIterationWidget>(widgets[2]);
        GitLabWorkItemHierarchyWidget hierarchy = Assert.IsType<GitLabWorkItemHierarchyWidget>(widgets[3]);

        GitLabWorkItemLabel label = Assert.IsType<GitLabWorkItemLabel>(labels.Labels?.Nodes?[0]);
        Assert.Equal("gid://gitlab/Label/3", label.Id?.Value);
        Assert.Equal("platform", label.Title);
        Assert.Equal("label-end", labels.Labels?.PageInfo?.EndCursor);

        Assert.Equal("gid://gitlab/Milestone/4", milestone.Milestone?.Id?.Value);
        Assert.Equal("19.4", milestone.Milestone?.Title);
        Assert.Equal(new DateOnly(2026, 9, 30), milestone.Milestone?.DueDate);
        Assert.Equal("gid://gitlab/Iteration/5", iteration.Iteration?.Id?.Value);
        Assert.Equal("Sprint 19", iteration.Iteration?.Title);
        Assert.Equal(new DateOnly(2026, 9, 8), iteration.Iteration?.StartDate);

        Assert.Equal("gid://gitlab/WorkItem/90", hierarchy.Parent?.Id?.Value);
        Assert.True(hierarchy.HasParent);
        Assert.True(hierarchy.HasChildren);
        Assert.Equal("Child", hierarchy.Children?.Nodes?[0]?.Title);
        Assert.Null(hierarchy.Children?.Nodes?[1]);
        Assert.True(hierarchy.Children?.PageInfo?.HasNextPage);
        GitLabWorkItemReferenceConnection ancestors = Assert.IsType<GitLabWorkItemReferenceConnection>(
            hierarchy.Ancestors);
        IReadOnlyList<GitLabWorkItemReference?> ancestorNodes = Assert.IsAssignableFrom<
            IReadOnlyList<GitLabWorkItemReference?>>(ancestors.Nodes);
        Assert.Empty(ancestorNodes);
        Assert.False(ancestors.PageInfo?.HasPreviousPage);
    }

    [Fact]
    public void MutationVariables_NullInputs_RejectTheInvalidRequestBeforeTransport()
    {
        ArgumentNullException create = Assert.Throws<ArgumentNullException>(() =>
            new GitLabWorkItemCreateMutationVariables(null!));
        ArgumentNullException update = Assert.Throws<ArgumentNullException>(() =>
            new GitLabWorkItemUpdateMutationVariables(null!));
        ArgumentNullException delete = Assert.Throws<ArgumentNullException>(() =>
            new GitLabWorkItemDeleteMutationVariables(null!));

        Assert.Equal("input", create.ParamName);
        Assert.Equal("input", update.ParamName);
        Assert.Equal("input", delete.ParamName);
    }
}