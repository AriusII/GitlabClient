using System.Globalization;

using GitLab.Client.Composition.WorkItems;
using GitLab.Client.GraphQL.WorkItems.Connections;
using GitLab.Client.GraphQL.WorkItems.Identifiers;
using GitLab.Client.GraphQL.WorkItems.Models;
using GitLab.Client.GraphQL.WorkItems.Widgets;
using GitLab.Client.Models;

namespace GitLab.Client.Tests.Contracts.Composition.WorkItems;

public sealed class GitLabWorkItemCompositionMapperTests
{
    [Fact]
    public void Map_CompletePlan_PreservesRouteInstancesOpaqueIdsAndNormalizedWidgets()
    {
        GitLabWorkItemLocator locator = new("group/platform", 7);
        GitLabProject project = new() { Id = 42, PathWithNamespace = locator.NamespacePath };
        GitLabIssue issue = CreateIssue(locator.Iid, project.Id);
        GitLabEpic epic = new() { Id = 700, WorkItemId = 701, Iid = locator.Iid };
        IReadOnlyList<GitLabWorkItemUser> assignees = [CreateUser("gid://gitlab/User/47")];
        GitLabGraphQLGlobalId graphQLId = new("gid://gitlab/WorkItem/opaque-7");
        GitLabWorkItem graphQLWorkItem = CreateWorkItem(
            graphQLId,
            locator.Iid.ToString(CultureInfo.InvariantCulture),
            [
                new GitLabWorkItemDescriptionWidget
                {
                    Description = "GraphQL description", DescriptionHtml = "<p>GraphQL description</p>"
                },
                new GitLabWorkItemAssigneesWidget
                {
                    Assignees = new GitLabWorkItemUserConnection { Nodes = assignees }
                },
                new GitLabWorkItemHealthStatusWidget { HealthStatus = GitLabWorkItemHealthStatus.NeedsAttention },
                new GitLabWorkItemStartAndDueDateWidget
                {
                    StartDate = new DateOnly(2026, 1, 2), DueDate = new DateOnly(2026, 1, 3), IsFixed = true
                },
                new GitLabWorkItemColorWidget { Color = "#123456", TextColor = "#ffffff" }
            ]);

        GitLabWorkItemComposition composition = GitLabWorkItemCompositionMapper.Map(
            new GitLabWorkItemCompositionInput
            {
                Locator = locator,
                RestProject = project,
                RestIssue = issue,
                LegacyRestEpic = epic,
                GraphQLWorkItem = graphQLWorkItem
            },
            GitLabWorkItemCompositionLoadPlan.Complete);

        Assert.Same(locator, composition.Locator);
        Assert.Same(project, composition.RestProject);
        Assert.Same(issue, composition.RestIssue);
        Assert.Same(epic, composition.LegacyRestEpic);
        Assert.Same(graphQLWorkItem, composition.GraphQLWorkItem);
        GitLabWorkItem composedWorkItem = Assert.IsType<GitLabWorkItem>(composition.GraphQLWorkItem);
        GitLabGraphQLGlobalId composedWorkItemId = Assert.IsType<GitLabGraphQLGlobalId>(composedWorkItem.Id);
        Assert.Same(graphQLId, composedWorkItemId);
        Assert.Equal("gid://gitlab/WorkItem/opaque-7", composedWorkItemId.Value);
        Assert.NotEqual(epic.Id!.Value.ToString(CultureInfo.InvariantCulture),
            composedWorkItemId.Value);

        Assert.True(composition.GraphQLWidgets.HasDescriptionWidget);
        Assert.Equal("GraphQL description", composition.GraphQLWidgets.Description);
        Assert.Equal("<p>GraphQL description</p>", composition.GraphQLWidgets.DescriptionHtml);
        Assert.True(composition.GraphQLWidgets.HasAssigneesWidget);
        Assert.Same(assignees, composition.GraphQLWidgets.Assignees);
        Assert.True(composition.GraphQLWidgets.HasHealthStatusWidget);
        Assert.Equal(GitLabWorkItemHealthStatus.NeedsAttention, composition.GraphQLWidgets.HealthStatus);
        Assert.True(composition.GraphQLWidgets.HasStartAndDueDateWidget);
        Assert.Equal(new DateOnly(2026, 1, 2), composition.GraphQLWidgets.StartDate);
        Assert.Equal(new DateOnly(2026, 1, 3), composition.GraphQLWidgets.DueDate);
        Assert.True(composition.GraphQLWidgets.DatesAreFixed);
        Assert.True(composition.GraphQLWidgets.HasColorWidget);
        Assert.Equal("#123456", composition.GraphQLWidgets.Color);
        Assert.Equal("#ffffff", composition.GraphQLWidgets.TextColor);

        Assert.True(composition.Sources.RestProject.IsLoaded);
        Assert.True(composition.Sources.RestIssue.IsLoaded);
        Assert.True(composition.Sources.LegacyRestEpic.IsLoaded);
        Assert.True(composition.Sources.GraphQLCore.IsLoaded);
        Assert.True(composition.Sources.GraphQLDescriptionWidget.IsLoaded);
        Assert.True(composition.Sources.GraphQLAssigneesWidget.IsLoaded);
        Assert.True(composition.Sources.GraphQLHealthStatusWidget.IsLoaded);
        Assert.True(composition.Sources.GraphQLStartAndDueDateWidget.IsLoaded);
        Assert.True(composition.Sources.GraphQLColorWidget.IsLoaded);

        Assert.Equal(GitLabWorkItemCompositionCorrelationStatus.Matches,
            composition.Correlation.LocatorNamespaceToRestProjectPath);
        Assert.Equal(GitLabWorkItemCompositionCorrelationStatus.Matches,
            composition.Correlation.LocatorIidToRestIssueIid);
        Assert.Equal(GitLabWorkItemCompositionCorrelationStatus.Matches,
            composition.Correlation.LocatorIidToLegacyRestEpicIid);
        Assert.Equal(GitLabWorkItemCompositionCorrelationStatus.Matches,
            composition.Correlation.LocatorIidToGraphQLWorkItemIid);
        Assert.Equal(GitLabWorkItemCompositionCorrelationStatus.Matches,
            composition.Correlation.RestIssueProjectIdToRestProjectId);
        Assert.Equal(GitLabWorkItemCompositionCorrelationStatus.Matches,
            composition.Correlation.RestIssueIidToGraphQLWorkItemIid);
        Assert.Equal(GitLabWorkItemCompositionCorrelationStatus.Matches,
            composition.Correlation.LegacyRestEpicIidToGraphQLWorkItemIid);
    }

    [Fact]
    public void Map_GraphQLOnlyPlan_PreservesRequestedUnavailableResultWithoutSynthesizingRestDtos()
    {
        GitLabWorkItemLocator locator = new("group/platform", 8);
        GitLabProject project = new() { Id = 42, PathWithNamespace = locator.NamespacePath };
        GitLabIssue issue = CreateIssue(locator.Iid, project.Id);

        GitLabWorkItemComposition composition = GitLabWorkItemCompositionMapper.Map(
            new GitLabWorkItemCompositionInput { Locator = locator, RestProject = project, RestIssue = issue },
            GitLabWorkItemCompositionLoadPlan.GraphQLCore);

        Assert.Null(composition.RestProject);
        Assert.Null(composition.RestIssue);
        Assert.Null(composition.LegacyRestEpic);
        Assert.Null(composition.GraphQLWorkItem);
        Assert.True(composition.Sources.GraphQLCore.WasRequested);
        Assert.False(composition.Sources.GraphQLCore.IsAvailable);
        Assert.False(composition.Sources.GraphQLCore.IsLoaded);
        Assert.False(composition.Sources.RestProject.WasRequested);
        Assert.True(composition.Sources.RestProject.IsAvailable);
        Assert.False(composition.Sources.RestProject.IsLoaded);
        Assert.False(composition.Sources.RestIssue.WasRequested);
        Assert.True(composition.Sources.RestIssue.IsAvailable);
        Assert.False(composition.Sources.RestIssue.IsLoaded);
        Assert.Equal(GitLabWorkItemCompositionCorrelationStatus.Unavailable,
            composition.Correlation.LocatorIidToGraphQLWorkItemIid);
    }

    [Fact]
    public void Constructor_WidgetSelection_NormalizesGraphQLCoreDependency()
    {
        GitLabWorkItemCompositionLoadPlan plan = new(GitLabWorkItemCompositionSection.GraphQLDescriptionWidget);

        Assert.Equal(
            GitLabWorkItemCompositionSection.GraphQLCore |
            GitLabWorkItemCompositionSection.GraphQLDescriptionWidget,
            plan.Sections);
        Assert.True(plan.Includes(GitLabWorkItemCompositionSection.GraphQLCore));
        Assert.True(plan.Includes(GitLabWorkItemCompositionSection.GraphQLDescriptionWidget));
        Assert.False(plan.Includes(GitLabWorkItemCompositionSection.GraphQLAssigneesWidget));
    }

    [Fact]
    public void Map_RequestedWidgetAbsentFromGraphQLProjection_TracksPartialAvailabilityWithoutInventingValues()
    {
        GitLabWorkItem graphQLWorkItem = CreateWorkItem(
            new GitLabGraphQLGlobalId("gid://gitlab/WorkItem/no-description"),
            "9",
            []);
        GitLabWorkItemCompositionLoadPlan plan = new(GitLabWorkItemCompositionSection.GraphQLDescriptionWidget);

        GitLabWorkItemComposition composition = GitLabWorkItemCompositionMapper.Map(
            new GitLabWorkItemCompositionInput { GraphQLWorkItem = graphQLWorkItem },
            plan);

        Assert.True(composition.Sources.GraphQLCore.IsLoaded);
        Assert.True(composition.Sources.GraphQLDescriptionWidget.WasRequested);
        Assert.False(composition.Sources.GraphQLDescriptionWidget.IsAvailable);
        Assert.False(composition.Sources.GraphQLDescriptionWidget.IsLoaded);
        Assert.False(composition.GraphQLWidgets.HasDescriptionWidget);
        Assert.Null(composition.GraphQLWidgets.Description);
        Assert.Null(composition.GraphQLWidgets.DescriptionHtml);
    }

    [Fact]
    public void Map_UnrequestedAvailableWidget_DoesNotLeakTheProjectionIntoNormalizedOutput()
    {
        GitLabWorkItem graphQLWorkItem = CreateWorkItem(
            new GitLabGraphQLGlobalId("gid://gitlab/WorkItem/description-not-requested"),
            "10",
            [new GitLabWorkItemDescriptionWidget { Description = "must remain unselected" }]);

        GitLabWorkItemComposition composition = GitLabWorkItemCompositionMapper.Map(
            new GitLabWorkItemCompositionInput { GraphQLWorkItem = graphQLWorkItem },
            GitLabWorkItemCompositionLoadPlan.GraphQLCore);

        Assert.Same(graphQLWorkItem, composition.GraphQLWorkItem);
        Assert.False(composition.Sources.GraphQLDescriptionWidget.WasRequested);
        Assert.True(composition.Sources.GraphQLDescriptionWidget.IsAvailable);
        Assert.False(composition.Sources.GraphQLDescriptionWidget.IsLoaded);
        Assert.False(composition.GraphQLWidgets.HasDescriptionWidget);
        Assert.Null(composition.GraphQLWidgets.Description);
    }

    [Fact]
    public void Map_EmptyAssigneeNodes_RemainsDistinctFromAnUnavailableWidget()
    {
        IReadOnlyList<GitLabWorkItemUser> assignees = [];
        GitLabWorkItem graphQLWorkItem = CreateWorkItem(
            new GitLabGraphQLGlobalId("gid://gitlab/WorkItem/no-assignees"),
            "11",
            [
                new GitLabWorkItemAssigneesWidget { Assignees = new GitLabWorkItemUserConnection { Nodes = assignees } }
            ]);
        GitLabWorkItemCompositionLoadPlan plan = new(GitLabWorkItemCompositionSection.GraphQLAssigneesWidget);

        GitLabWorkItemComposition composition = GitLabWorkItemCompositionMapper.Map(
            new GitLabWorkItemCompositionInput { GraphQLWorkItem = graphQLWorkItem },
            plan);

        Assert.True(composition.Sources.GraphQLAssigneesWidget.IsLoaded);
        Assert.True(composition.GraphQLWidgets.HasAssigneesWidget);
        Assert.NotNull(composition.GraphQLWidgets.Assignees);
        Assert.Same(assignees, composition.GraphQLWidgets.Assignees);
        Assert.Empty(composition.GraphQLWidgets.Assignees);
    }

    [Fact]
    public void Map_MismatchedComparableSources_ReportsEveryMismatchWithoutDiscardingRouteDtos()
    {
        GitLabWorkItemLocator locator = new("group/platform", 12);
        GitLabProject project = new() { Id = 42, PathWithNamespace = "other/namespace" };
        GitLabIssue issue = CreateIssue(13, 43);
        GitLabEpic epic = new() { Iid = 14 };
        GitLabWorkItem graphQLWorkItem = CreateWorkItem(
            new GitLabGraphQLGlobalId("gid://gitlab/WorkItem/different"),
            "15",
            []);

        GitLabWorkItemComposition composition = GitLabWorkItemCompositionMapper.Map(
            new GitLabWorkItemCompositionInput
            {
                Locator = locator,
                RestProject = project,
                RestIssue = issue,
                LegacyRestEpic = epic,
                GraphQLWorkItem = graphQLWorkItem
            },
            GitLabWorkItemCompositionLoadPlan.Complete);

        Assert.Same(project, composition.RestProject);
        Assert.Same(issue, composition.RestIssue);
        Assert.Same(epic, composition.LegacyRestEpic);
        Assert.Same(graphQLWorkItem, composition.GraphQLWorkItem);
        Assert.Equal(GitLabWorkItemCompositionCorrelationStatus.Mismatch,
            composition.Correlation.LocatorNamespaceToRestProjectPath);
        Assert.Equal(GitLabWorkItemCompositionCorrelationStatus.Mismatch,
            composition.Correlation.LocatorIidToRestIssueIid);
        Assert.Equal(GitLabWorkItemCompositionCorrelationStatus.Mismatch,
            composition.Correlation.LocatorIidToLegacyRestEpicIid);
        Assert.Equal(GitLabWorkItemCompositionCorrelationStatus.Mismatch,
            composition.Correlation.LocatorIidToGraphQLWorkItemIid);
        Assert.Equal(GitLabWorkItemCompositionCorrelationStatus.Mismatch,
            composition.Correlation.RestIssueProjectIdToRestProjectId);
        Assert.Equal(GitLabWorkItemCompositionCorrelationStatus.Mismatch,
            composition.Correlation.RestIssueIidToGraphQLWorkItemIid);
        Assert.Equal(GitLabWorkItemCompositionCorrelationStatus.Mismatch,
            composition.Correlation.LegacyRestEpicIidToGraphQLWorkItemIid);
    }

    [Fact]
    public void Map_InvalidGraphQLIid_ReportsInvalidWithoutParsingOrReplacingItsOpaqueGlobalId()
    {
        GitLabGraphQLGlobalId opaqueId = new("gid://gitlab/Issue/legacy-compatible-value");
        GitLabWorkItem graphQLWorkItem = CreateWorkItem(opaqueId, "not-a-positive-integer", []);

        GitLabWorkItemComposition composition = GitLabWorkItemCompositionMapper.Map(
            new GitLabWorkItemCompositionInput
            {
                Locator = new GitLabWorkItemLocator("group/platform", 16), GraphQLWorkItem = graphQLWorkItem
            },
            GitLabWorkItemCompositionLoadPlan.GraphQLCore);

        GitLabWorkItem composedWorkItem = Assert.IsType<GitLabWorkItem>(composition.GraphQLWorkItem);
        GitLabGraphQLGlobalId composedWorkItemId = Assert.IsType<GitLabGraphQLGlobalId>(composedWorkItem.Id);
        Assert.Same(opaqueId, composedWorkItemId);
        Assert.Equal("gid://gitlab/Issue/legacy-compatible-value", composedWorkItemId.Value);
        Assert.Equal(GitLabWorkItemCompositionCorrelationStatus.Invalid,
            composition.Correlation.LocatorIidToGraphQLWorkItemIid);
    }

    [Fact]
    public void Map_LegacyEpicNumericIds_AreNotInferredFromGraphQLGlobalId()
    {
        GitLabEpic epic = new() { Id = 900, WorkItemId = 901, Iid = 17 };
        GitLabGraphQLGlobalId opaqueId = new("gid://gitlab/WorkItem/902");
        GitLabWorkItem graphQLWorkItem = CreateWorkItem(opaqueId, "17", []);
        GitLabWorkItemCompositionLoadPlan plan = new(
            GitLabWorkItemCompositionSection.LegacyRestEpic |
            GitLabWorkItemCompositionSection.GraphQLCore);

        GitLabWorkItemComposition composition = GitLabWorkItemCompositionMapper.Map(
            new GitLabWorkItemCompositionInput { LegacyRestEpic = epic, GraphQLWorkItem = graphQLWorkItem },
            plan);

        Assert.Same(epic, composition.LegacyRestEpic);
        Assert.Same(graphQLWorkItem, composition.GraphQLWorkItem);
        GitLabWorkItem composedWorkItem = Assert.IsType<GitLabWorkItem>(composition.GraphQLWorkItem);
        Assert.Equal("gid://gitlab/WorkItem/902", Assert.IsType<GitLabGraphQLGlobalId>(composedWorkItem.Id).Value);
        Assert.Equal(GitLabWorkItemCompositionCorrelationStatus.Matches,
            composition.Correlation.LegacyRestEpicIidToGraphQLWorkItemIid);
        Assert.Equal(GitLabWorkItemCompositionCorrelationStatus.Unavailable,
            composition.Correlation.LocatorIidToLegacyRestEpicIid);
    }

    [Fact]
    public void Constructor_AdditionalWidgetSelection_NormalizesGraphQLCoreDependency()
    {
        GitLabWorkItemCompositionLoadPlan plan = new(
            GitLabWorkItemCompositionSection.GraphQLLabelsWidget |
            GitLabWorkItemCompositionSection.GraphQLMilestoneWidget |
            GitLabWorkItemCompositionSection.GraphQLIterationWidget |
            GitLabWorkItemCompositionSection.GraphQLHierarchyWidget);

        Assert.True(plan.Includes(GitLabWorkItemCompositionSection.GraphQLCore));
        Assert.True(plan.Includes(GitLabWorkItemCompositionSection.GraphQLLabelsWidget));
        Assert.True(plan.Includes(GitLabWorkItemCompositionSection.GraphQLMilestoneWidget));
        Assert.True(plan.Includes(GitLabWorkItemCompositionSection.GraphQLIterationWidget));
        Assert.True(plan.Includes(GitLabWorkItemCompositionSection.GraphQLHierarchyWidget));
        Assert.False(plan.Includes(GitLabWorkItemCompositionSection.GraphQLDescriptionWidget));
        Assert.True(GitLabWorkItemCompositionLoadPlan.CuratedGraphQL.Includes(
            GitLabWorkItemCompositionSection.GraphQLLabelsWidget |
            GitLabWorkItemCompositionSection.GraphQLMilestoneWidget |
            GitLabWorkItemCompositionSection.GraphQLIterationWidget |
            GitLabWorkItemCompositionSection.GraphQLHierarchyWidget));
        Assert.True(GitLabWorkItemCompositionLoadPlan.Complete.Includes(
            GitLabWorkItemCompositionSection.GraphQLLabelsWidget |
            GitLabWorkItemCompositionSection.GraphQLMilestoneWidget |
            GitLabWorkItemCompositionSection.GraphQLIterationWidget |
            GitLabWorkItemCompositionSection.GraphQLHierarchyWidget));
    }

    [Fact]
    public void Map_AdditionalWidgets_PreservesDirectGraphQLReferencesWithoutRestNumericIdJoins()
    {
        GitLabGraphQLGlobalId labelId = new("gid://gitlab/Label/opaque-300");
        GitLabGraphQLGlobalId milestoneId = new("gid://gitlab/Milestone/opaque-400");
        GitLabGraphQLGlobalId iterationId = new("gid://gitlab/Iteration/opaque-500");
        GitLabGraphQLGlobalId parentId = new("gid://gitlab/WorkItem/opaque-parent");
        GitLabGraphQLGlobalId childId = new("gid://gitlab/WorkItem/opaque-child");
        GitLabGraphQLGlobalId ancestorId = new("gid://gitlab/WorkItem/opaque-ancestor");
        GitLabWorkItemLabel label = new() { Id = labelId, Title = "GraphQL-only label" };
        GitLabWorkItemLabelConnection labels = new()
        {
            Nodes = [label],
            PageInfo = new GitLabWorkItemPageInfo { HasNextPage = true, EndCursor = "labels-cursor" }
        };
        GitLabWorkItemMilestone milestone = new() { Id = milestoneId, Title = "GraphQL milestone" };
        GitLabWorkItemIteration iteration = new() { Id = iterationId, Title = "GraphQL iteration" };
        GitLabWorkItemReference parent = new() { Id = parentId, Iid = "21", Title = "Parent" };
        GitLabWorkItemReference child = new() { Id = childId, Iid = "22", Title = "Child" };
        GitLabWorkItemReference ancestor = new() { Id = ancestorId, Iid = "20", Title = "Ancestor" };
        GitLabWorkItemReferenceConnection children = new()
        {
            Nodes = [child],
            PageInfo = new GitLabWorkItemPageInfo { HasNextPage = true, EndCursor = "children-cursor" }
        };
        GitLabWorkItemReferenceConnection ancestors = new()
        {
            Nodes = [ancestor],
            PageInfo = new GitLabWorkItemPageInfo { HasPreviousPage = true, StartCursor = "ancestors-cursor" }
        };
        GitLabWorkItem graphQLWorkItem = CreateWorkItem(
            new GitLabGraphQLGlobalId("gid://gitlab/WorkItem/opaque-root"),
            "23",
            [
                new GitLabWorkItemLabelsWidget { Labels = labels },
                new GitLabWorkItemMilestoneWidget { Milestone = milestone },
                new GitLabWorkItemIterationWidget { Iteration = iteration },
                new GitLabWorkItemHierarchyWidget
                {
                    Parent = parent,
                    Children = children,
                    Ancestors = ancestors,
                    HasChildren = true,
                    HasParent = true
                }
            ]);
        GitLabEpic legacyEpic = new() { Id = 300, WorkItemId = 400, Iid = 23 };
        GitLabLabel restLabel = new() { Id = 300, Name = "REST label" };
        GitLabMilestone restMilestone = new()
        {
            Id = 400,
            Iid = 4,
            Title = "REST milestone",
            State = "active",
            WebUrl = new Uri("https://gitlab.example/groups/group/-/milestones/4")
        };
        GitLabIteration restIteration = new() { Id = 500 };
        GitLabWorkItemCompositionLoadPlan plan = new(
            GitLabWorkItemCompositionSection.LegacyRestEpic |
            GitLabWorkItemCompositionSection.GraphQLLabelsWidget |
            GitLabWorkItemCompositionSection.GraphQLMilestoneWidget |
            GitLabWorkItemCompositionSection.GraphQLIterationWidget |
            GitLabWorkItemCompositionSection.GraphQLHierarchyWidget);

        GitLabWorkItemComposition composition = GitLabWorkItemCompositionMapper.Map(
            new GitLabWorkItemCompositionInput { LegacyRestEpic = legacyEpic, GraphQLWorkItem = graphQLWorkItem },
            plan);

        Assert.True(composition.Sources.GraphQLLabelsWidget.IsLoaded);
        Assert.True(composition.Sources.GraphQLMilestoneWidget.IsLoaded);
        Assert.True(composition.Sources.GraphQLIterationWidget.IsLoaded);
        Assert.True(composition.Sources.GraphQLHierarchyWidget.IsLoaded);
        Assert.True(composition.GraphQLWidgets.HasLabelsWidget);
        Assert.Same(labels, composition.GraphQLWidgets.Labels);
        GitLabWorkItemLabel composedLabel =
            Assert.IsType<GitLabWorkItemLabel>(Assert.Single(composition.GraphQLWidgets.Labels!.Nodes!));
        GitLabGraphQLGlobalId composedLabelId = Assert.IsType<GitLabGraphQLGlobalId>(composedLabel.Id);
        Assert.Same(labelId, composedLabelId);
        Assert.True(composition.GraphQLWidgets.HasMilestoneWidget);
        Assert.Same(milestone, composition.GraphQLWidgets.Milestone);
        Assert.Same(milestoneId, composition.GraphQLWidgets.Milestone!.Id);
        Assert.True(composition.GraphQLWidgets.HasIterationWidget);
        Assert.Same(iteration, composition.GraphQLWidgets.Iteration);
        Assert.Same(iterationId, composition.GraphQLWidgets.Iteration!.Id);
        Assert.True(composition.GraphQLWidgets.HasHierarchyWidget);
        Assert.Same(parent, composition.GraphQLWidgets.Parent);
        Assert.Same(children, composition.GraphQLWidgets.Children);
        Assert.Same(ancestors, composition.GraphQLWidgets.Ancestors);
        Assert.True(composition.GraphQLWidgets.HasChildren);
        Assert.True(composition.GraphQLWidgets.HasParent);
        Assert.IsType<GitLabWorkItemReference>(composition.GraphQLWidgets.Parent);
        Assert.IsType<GitLabWorkItemReference>(Assert.Single(composition.GraphQLWidgets.Children!.Nodes!));
        Assert.NotEqual(restLabel.Id.ToString(CultureInfo.InvariantCulture), labelId.Value);
        Assert.NotEqual(restMilestone.Id.ToString(CultureInfo.InvariantCulture), milestoneId.Value);
        Assert.NotEqual(restIteration.Id.ToString(CultureInfo.InvariantCulture), iterationId.Value);
        Assert.NotEqual(legacyEpic.Id!.Value.ToString(CultureInfo.InvariantCulture), parentId.Value);
    }

    [Fact]
    public void Map_AdditionalWidgetNullValuesAndEmptyLabels_RemainDistinctFromAbsentWidgets()
    {
        IReadOnlyList<GitLabWorkItemLabel?> emptyLabels = [];
        GitLabWorkItemLabelConnection labels = new() { Nodes = emptyLabels };
        GitLabWorkItem graphQLWorkItem = CreateWorkItem(
            new GitLabGraphQLGlobalId("gid://gitlab/WorkItem/partial-additional-widgets"),
            "24",
            [
                new GitLabWorkItemLabelsWidget { Labels = labels },
                new GitLabWorkItemMilestoneWidget(),
                new GitLabWorkItemIterationWidget(),
                new GitLabWorkItemHierarchyWidget()
            ]);
        GitLabWorkItemCompositionLoadPlan plan = new(
            GitLabWorkItemCompositionSection.GraphQLLabelsWidget |
            GitLabWorkItemCompositionSection.GraphQLMilestoneWidget |
            GitLabWorkItemCompositionSection.GraphQLIterationWidget |
            GitLabWorkItemCompositionSection.GraphQLHierarchyWidget);

        GitLabWorkItemComposition composition = GitLabWorkItemCompositionMapper.Map(
            new GitLabWorkItemCompositionInput { GraphQLWorkItem = graphQLWorkItem },
            plan);

        Assert.True(composition.Sources.GraphQLLabelsWidget.IsLoaded);
        Assert.True(composition.Sources.GraphQLMilestoneWidget.IsLoaded);
        Assert.True(composition.Sources.GraphQLIterationWidget.IsLoaded);
        Assert.True(composition.Sources.GraphQLHierarchyWidget.IsLoaded);
        Assert.True(composition.GraphQLWidgets.HasLabelsWidget);
        Assert.Same(labels, composition.GraphQLWidgets.Labels);
        Assert.Same(emptyLabels, composition.GraphQLWidgets.Labels!.Nodes);
        Assert.Empty(composition.GraphQLWidgets.Labels.Nodes!);
        Assert.True(composition.GraphQLWidgets.HasMilestoneWidget);
        Assert.Null(composition.GraphQLWidgets.Milestone);
        Assert.True(composition.GraphQLWidgets.HasIterationWidget);
        Assert.Null(composition.GraphQLWidgets.Iteration);
        Assert.True(composition.GraphQLWidgets.HasHierarchyWidget);
        Assert.Null(composition.GraphQLWidgets.Parent);
        Assert.Null(composition.GraphQLWidgets.Children);
        Assert.Null(composition.GraphQLWidgets.Ancestors);
        Assert.Null(composition.GraphQLWidgets.HasChildren);
        Assert.Null(composition.GraphQLWidgets.HasParent);
    }

    [Fact]
    public void Map_RequestedAdditionalWidgetsAbsent_TracksAvailabilityWithoutInventingValues()
    {
        GitLabWorkItem graphQLWorkItem = CreateWorkItem(
            new GitLabGraphQLGlobalId("gid://gitlab/WorkItem/absent-additional-widgets"),
            "25",
            []);
        GitLabWorkItemCompositionLoadPlan plan = new(
            GitLabWorkItemCompositionSection.GraphQLLabelsWidget |
            GitLabWorkItemCompositionSection.GraphQLMilestoneWidget |
            GitLabWorkItemCompositionSection.GraphQLIterationWidget |
            GitLabWorkItemCompositionSection.GraphQLHierarchyWidget);

        GitLabWorkItemComposition composition = GitLabWorkItemCompositionMapper.Map(
            new GitLabWorkItemCompositionInput { GraphQLWorkItem = graphQLWorkItem },
            plan);

        Assert.True(composition.Sources.GraphQLCore.IsLoaded);
        Assert.True(composition.Sources.GraphQLLabelsWidget.WasRequested);
        Assert.True(composition.Sources.GraphQLMilestoneWidget.WasRequested);
        Assert.True(composition.Sources.GraphQLIterationWidget.WasRequested);
        Assert.True(composition.Sources.GraphQLHierarchyWidget.WasRequested);
        Assert.False(composition.Sources.GraphQLLabelsWidget.IsAvailable);
        Assert.False(composition.Sources.GraphQLMilestoneWidget.IsAvailable);
        Assert.False(composition.Sources.GraphQLIterationWidget.IsAvailable);
        Assert.False(composition.Sources.GraphQLHierarchyWidget.IsAvailable);
        Assert.False(composition.GraphQLWidgets.HasLabelsWidget);
        Assert.False(composition.GraphQLWidgets.HasMilestoneWidget);
        Assert.False(composition.GraphQLWidgets.HasIterationWidget);
        Assert.False(composition.GraphQLWidgets.HasHierarchyWidget);
        Assert.Null(composition.GraphQLWidgets.Labels);
        Assert.Null(composition.GraphQLWidgets.Milestone);
        Assert.Null(composition.GraphQLWidgets.Iteration);
        Assert.Null(composition.GraphQLWidgets.Parent);
        Assert.Null(composition.GraphQLWidgets.Children);
        Assert.Null(composition.GraphQLWidgets.Ancestors);
    }

    [Fact]
    public void Map_UnrequestedAdditionalWidgets_DoNotLeakIntoNormalizedOutput()
    {
        GitLabWorkItemLabelConnection labels = new() { Nodes = [] };
        GitLabWorkItem graphQLWorkItem = CreateWorkItem(
            new GitLabGraphQLGlobalId("gid://gitlab/WorkItem/unrequested-additional-widgets"),
            "26",
            [
                new GitLabWorkItemLabelsWidget { Labels = labels },
                new GitLabWorkItemMilestoneWidget { Milestone = new GitLabWorkItemMilestone { Title = "hidden" } },
                new GitLabWorkItemIterationWidget { Iteration = new GitLabWorkItemIteration { Title = "hidden" } },
                new GitLabWorkItemHierarchyWidget { HasChildren = true, HasParent = true }
            ]);

        GitLabWorkItemComposition composition = GitLabWorkItemCompositionMapper.Map(
            new GitLabWorkItemCompositionInput { GraphQLWorkItem = graphQLWorkItem },
            GitLabWorkItemCompositionLoadPlan.GraphQLCore);

        Assert.False(composition.Sources.GraphQLLabelsWidget.WasRequested);
        Assert.True(composition.Sources.GraphQLLabelsWidget.IsAvailable);
        Assert.False(composition.Sources.GraphQLMilestoneWidget.WasRequested);
        Assert.True(composition.Sources.GraphQLMilestoneWidget.IsAvailable);
        Assert.False(composition.Sources.GraphQLIterationWidget.WasRequested);
        Assert.True(composition.Sources.GraphQLIterationWidget.IsAvailable);
        Assert.False(composition.Sources.GraphQLHierarchyWidget.WasRequested);
        Assert.True(composition.Sources.GraphQLHierarchyWidget.IsAvailable);
        Assert.False(composition.GraphQLWidgets.HasLabelsWidget);
        Assert.False(composition.GraphQLWidgets.HasMilestoneWidget);
        Assert.False(composition.GraphQLWidgets.HasIterationWidget);
        Assert.False(composition.GraphQLWidgets.HasHierarchyWidget);
        Assert.Null(composition.GraphQLWidgets.Labels);
        Assert.Null(composition.GraphQLWidgets.Milestone);
        Assert.Null(composition.GraphQLWidgets.Iteration);
        Assert.Null(composition.GraphQLWidgets.Parent);
        Assert.Null(composition.GraphQLWidgets.Children);
        Assert.Null(composition.GraphQLWidgets.Ancestors);
    }

    [Fact]
    public void Map_NullInput_ThrowsWithTheInputParameterName()
    {
        ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() =>
            GitLabWorkItemCompositionMapper.Map(null!, GitLabWorkItemCompositionLoadPlan.Empty));

        Assert.Equal("input", exception.ParamName);
    }

    [Fact]
    public void Constructor_UnsupportedSection_Throws()
    {
        ArgumentOutOfRangeException exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new GitLabWorkItemCompositionLoadPlan((GitLabWorkItemCompositionSection)(1 << 13)));

        Assert.Equal("sections", exception.ParamName);
    }

    private static GitLabIssue CreateIssue(long iid, long? projectId)
    {
        return new GitLabIssue
        {
            Id = 1000 + iid,
            Iid = iid,
            ProjectId = projectId,
            Title = "Work item route DTO",
            State = "opened",
            WebUrl = new Uri($"https://gitlab.example/group/platform/-/issues/{iid}")
        };
    }

    private static GitLabWorkItem CreateWorkItem(
        GitLabGraphQLGlobalId id,
        string iid,
        IReadOnlyList<GitLabWorkItemWidget> widgets)
    {
        return new GitLabWorkItem
        {
            Id = id,
            Iid = iid,
            Title = "Work item GraphQL DTO",
            State = GitLabWorkItemState.Open,
            Widgets = widgets
        };
    }

    private static GitLabWorkItemUser CreateUser(string id)
    {
        return new GitLabWorkItemUser { Id = new GitLabGraphQLGlobalId(id), Name = "Ada Lovelace", Username = "ada" };
    }
}