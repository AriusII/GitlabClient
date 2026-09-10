using System.Text.Json;

using GitLab.Client.GraphQL.Protocol;
using GitLab.Client.GraphQL.WorkItems.Connections;
using GitLab.Client.GraphQL.WorkItems.Models;
using GitLab.Client.GraphQL.WorkItems.Queries;
using GitLab.Client.GraphQL.WorkItems.Widgets;

using GitLabGraphQLJsonContext = GitLab.Client.GraphQL.Serialization.GitLabGraphQLJsonContext;

namespace GitLab.Client.Tests.GraphQL.WorkItems;

/// <summary>Protects the closed, source-generated Work Item widget union against schema-selection regressions.</summary>
public sealed class GitLabWorkItemWidgetPolymorphismTests
{
    [Fact]
    public void WorkItemResponse_DeserializesEveryCuratedWidgetDiscriminator()
    {
        const string json = """
                            {
                              "data": {
                                "workItem": {
                                  "id": "gid://gitlab/WorkItem/42",
                                  "iid": "42",
                                  "title": "GraphQL coverage",
                                  "state": "OPEN",
                                  "widgets": [
                                    { "__typename": "WorkItemWidgetDescription", "description": "plain", "descriptionHtml": "<p>plain</p>" },
                                    {
                                      "__typename": "WorkItemWidgetAssignees",
                                      "assignees": {
                                        "nodes": [{ "id": "gid://gitlab/User/7", "name": "Ada", "username": "ada", "webUrl": "https://gitlab.example/ada" }],
                                        "pageInfo": { "hasNextPage": false, "hasPreviousPage": false, "startCursor": "first", "endCursor": "last" }
                                      }
                                    },
                                    { "__typename": "WorkItemWidgetColor", "color": "#1068bf", "textColor": "#ffffff" },
                                    { "__typename": "WorkItemWidgetHealthStatus", "healthStatus": "onTrack" },
                                    { "__typename": "WorkItemWidgetStartAndDueDate", "startDate": "2026-09-01", "dueDate": "2026-09-30", "isFixed": true }
                                  ]
                                }
                              }
                            }
                            """;

        GitLabGraphQLResponse<GitLabWorkItemByIdQueryData>? response = JsonSerializer.Deserialize(
            json,
            GitLabGraphQLJsonContext.Default.GitLabGraphQLResponseGitLabWorkItemByIdQueryData);

        GitLabWorkItem workItem =
            Assert.IsType<GitLabWorkItem>(Assert.IsType<GitLabWorkItemByIdQueryData>(response?.Data).WorkItem);
        IReadOnlyList<GitLabWorkItemWidget?> widgets = Assert.IsAssignableFrom<IReadOnlyList<GitLabWorkItemWidget?>>(
            workItem.Widgets);

        Assert.Collection(
            widgets,
            widget => Assert.IsType<GitLabWorkItemDescriptionWidget>(widget),
            widget => Assert.IsType<GitLabWorkItemAssigneesWidget>(widget),
            widget => Assert.IsType<GitLabWorkItemColorWidget>(widget),
            widget => Assert.IsType<GitLabWorkItemHealthStatusWidget>(widget),
            widget => Assert.IsType<GitLabWorkItemStartAndDueDateWidget>(widget));

        GitLabWorkItemAssigneesWidget assignees = Assert.IsType<GitLabWorkItemAssigneesWidget>(widgets[1]);
        IReadOnlyList<GitLabWorkItemUser?> assigneeNodes = Assert.IsAssignableFrom<IReadOnlyList<GitLabWorkItemUser?>>(
            Assert.IsType<GitLabWorkItemUserConnection>(assignees.Assignees).Nodes);
        GitLabWorkItemUser assignee = Assert.IsType<GitLabWorkItemUser>(
            Assert.Single(assigneeNodes));
        Assert.Equal("ada", assignee.Username);
        Assert.Equal(GitLabWorkItemHealthStatus.OnTrack,
            Assert.IsType<GitLabWorkItemHealthStatusWidget>(widgets[3]).HealthStatus);
        Assert.Equal(new DateOnly(2026, 9, 30),
            Assert.IsType<GitLabWorkItemStartAndDueDateWidget>(widgets[4]).DueDate);
    }
}