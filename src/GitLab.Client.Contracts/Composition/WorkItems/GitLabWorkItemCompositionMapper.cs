using System.Globalization;

using GitLab.Client.GraphQL.WorkItems.Identifiers;
using GitLab.Client.GraphQL.WorkItems.Models;
using GitLab.Client.GraphQL.WorkItems.Widgets;
using GitLab.Client.Models;

namespace GitLab.Client.Composition.WorkItems;

/// <summary>
///     Pure mapper that joins existing REST and GraphQL DTOs into a <see cref="GitLabWorkItemComposition" />.
/// </summary>
/// <remarks>
///     The mapper performs no I/O, route construction, serialization, caching, DI lookup, reflection, or lazy
///     loading. It never parses or compares GraphQL global IDs. GraphQL widgets are normalized only when both their
///     containing GraphQL work item and their corresponding load-plan slice were selected.
/// </remarks>
public static class GitLabWorkItemCompositionMapper
{
    /// <summary>Assembles the slices selected by <paramref name="loadPlan" /> from supplied route DTOs.</summary>
    /// <exception cref="ArgumentNullException"><paramref name="input" /> or <paramref name="loadPlan" /> is null.</exception>
    public static GitLabWorkItemComposition Map(
        GitLabWorkItemCompositionInput input,
        GitLabWorkItemCompositionLoadPlan loadPlan)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(loadPlan);

        GitLabProject? restProject = Select(
            input.RestProject,
            loadPlan,
            GitLabWorkItemCompositionSection.RestProject);
        GitLabIssue? restIssue = Select(
            input.RestIssue,
            loadPlan,
            GitLabWorkItemCompositionSection.RestIssue);
        GitLabEpic? legacyRestEpic = Select(
            input.LegacyRestEpic,
            loadPlan,
            GitLabWorkItemCompositionSection.LegacyRestEpic);
        GitLabWorkItem? graphQLWorkItem = Select(
            input.GraphQLWorkItem,
            loadPlan,
            GitLabWorkItemCompositionSection.GraphQLCore);

        return new GitLabWorkItemComposition
        {
            LoadPlan = loadPlan,
            Locator = input.Locator,
            RestProject = restProject,
            RestIssue = restIssue,
            LegacyRestEpic = legacyRestEpic,
            GraphQLWorkItem = graphQLWorkItem,
            GraphQLWidgets = NormalizeWidgets(graphQLWorkItem, loadPlan),
            Sources = CreateSources(input, loadPlan),
            Correlation = CreateCorrelation(input.Locator, restProject, restIssue, legacyRestEpic, graphQLWorkItem)
        };
    }

    private static T? Select<T>(
        T? source,
        GitLabWorkItemCompositionLoadPlan loadPlan,
        GitLabWorkItemCompositionSection section)
        where T : class
    {
        return loadPlan.Includes(section) ? source : null;
    }

    private static GitLabWorkItemCompositionSources CreateSources(
        GitLabWorkItemCompositionInput input,
        GitLabWorkItemCompositionLoadPlan loadPlan)
    {
        IReadOnlyList<GitLabWorkItemWidget?>? widgets = input.GraphQLWorkItem?.Widgets;

        return new GitLabWorkItemCompositionSources
        {
            RestProject = Status(loadPlan, GitLabWorkItemCompositionSection.RestProject, input.RestProject is not null),
            RestIssue = Status(loadPlan, GitLabWorkItemCompositionSection.RestIssue, input.RestIssue is not null),
            LegacyRestEpic = Status(
                loadPlan,
                GitLabWorkItemCompositionSection.LegacyRestEpic,
                input.LegacyRestEpic is not null),
            GraphQLCore = Status(
                loadPlan,
                GitLabWorkItemCompositionSection.GraphQLCore,
                input.GraphQLWorkItem is not null),
            GraphQLDescriptionWidget = Status(
                loadPlan,
                GitLabWorkItemCompositionSection.GraphQLDescriptionWidget,
                FindWidget<GitLabWorkItemDescriptionWidget>(widgets) is not null),
            GraphQLAssigneesWidget = Status(
                loadPlan,
                GitLabWorkItemCompositionSection.GraphQLAssigneesWidget,
                FindWidget<GitLabWorkItemAssigneesWidget>(widgets) is not null),
            GraphQLHealthStatusWidget = Status(
                loadPlan,
                GitLabWorkItemCompositionSection.GraphQLHealthStatusWidget,
                FindWidget<GitLabWorkItemHealthStatusWidget>(widgets) is not null),
            GraphQLStartAndDueDateWidget = Status(
                loadPlan,
                GitLabWorkItemCompositionSection.GraphQLStartAndDueDateWidget,
                FindWidget<GitLabWorkItemStartAndDueDateWidget>(widgets) is not null),
            GraphQLColorWidget = Status(
                loadPlan,
                GitLabWorkItemCompositionSection.GraphQLColorWidget,
                FindWidget<GitLabWorkItemColorWidget>(widgets) is not null),
            GraphQLLabelsWidget = Status(
                loadPlan,
                GitLabWorkItemCompositionSection.GraphQLLabelsWidget,
                FindWidget<GitLabWorkItemLabelsWidget>(widgets) is not null),
            GraphQLMilestoneWidget = Status(
                loadPlan,
                GitLabWorkItemCompositionSection.GraphQLMilestoneWidget,
                FindWidget<GitLabWorkItemMilestoneWidget>(widgets) is not null),
            GraphQLIterationWidget = Status(
                loadPlan,
                GitLabWorkItemCompositionSection.GraphQLIterationWidget,
                FindWidget<GitLabWorkItemIterationWidget>(widgets) is not null),
            GraphQLHierarchyWidget = Status(
                loadPlan,
                GitLabWorkItemCompositionSection.GraphQLHierarchyWidget,
                FindWidget<GitLabWorkItemHierarchyWidget>(widgets) is not null)
        };
    }

    private static GitLabWorkItemCompositionSourceStatus Status(
        GitLabWorkItemCompositionLoadPlan loadPlan,
        GitLabWorkItemCompositionSection section,
        bool isAvailable)
    {
        return new GitLabWorkItemCompositionSourceStatus
        {
            WasRequested = loadPlan.Includes(section), IsAvailable = isAvailable
        };
    }

    private static GitLabWorkItemWidgetComposition NormalizeWidgets(
        GitLabWorkItem? workItem,
        GitLabWorkItemCompositionLoadPlan loadPlan)
    {
        IReadOnlyList<GitLabWorkItemWidget?>? widgets = workItem?.Widgets;
        GitLabWorkItemDescriptionWidget? description = SelectWidget<GitLabWorkItemDescriptionWidget>(
            widgets,
            loadPlan,
            GitLabWorkItemCompositionSection.GraphQLDescriptionWidget);
        GitLabWorkItemAssigneesWidget? assignees = SelectWidget<GitLabWorkItemAssigneesWidget>(
            widgets,
            loadPlan,
            GitLabWorkItemCompositionSection.GraphQLAssigneesWidget);
        GitLabWorkItemHealthStatusWidget? healthStatus = SelectWidget<GitLabWorkItemHealthStatusWidget>(
            widgets,
            loadPlan,
            GitLabWorkItemCompositionSection.GraphQLHealthStatusWidget);
        GitLabWorkItemStartAndDueDateWidget? dates = SelectWidget<GitLabWorkItemStartAndDueDateWidget>(
            widgets,
            loadPlan,
            GitLabWorkItemCompositionSection.GraphQLStartAndDueDateWidget);
        GitLabWorkItemColorWidget? color = SelectWidget<GitLabWorkItemColorWidget>(
            widgets,
            loadPlan,
            GitLabWorkItemCompositionSection.GraphQLColorWidget);
        GitLabWorkItemLabelsWidget? labels = SelectWidget<GitLabWorkItemLabelsWidget>(
            widgets,
            loadPlan,
            GitLabWorkItemCompositionSection.GraphQLLabelsWidget);
        GitLabWorkItemMilestoneWidget? milestone = SelectWidget<GitLabWorkItemMilestoneWidget>(
            widgets,
            loadPlan,
            GitLabWorkItemCompositionSection.GraphQLMilestoneWidget);
        GitLabWorkItemIterationWidget? iteration = SelectWidget<GitLabWorkItemIterationWidget>(
            widgets,
            loadPlan,
            GitLabWorkItemCompositionSection.GraphQLIterationWidget);
        GitLabWorkItemHierarchyWidget? hierarchy = SelectWidget<GitLabWorkItemHierarchyWidget>(
            widgets,
            loadPlan,
            GitLabWorkItemCompositionSection.GraphQLHierarchyWidget);

        return new GitLabWorkItemWidgetComposition
        {
            HasDescriptionWidget = description is not null,
            Description = description?.Description,
            DescriptionHtml = description?.DescriptionHtml,
            HasAssigneesWidget = assignees is not null,
            Assignees = assignees?.Assignees?.Nodes,
            HasHealthStatusWidget = healthStatus is not null,
            HealthStatus = healthStatus?.HealthStatus,
            HasStartAndDueDateWidget = dates is not null,
            StartDate = dates?.StartDate,
            DueDate = dates?.DueDate,
            DatesAreFixed = dates?.IsFixed,
            HasColorWidget = color is not null,
            Color = color?.Color,
            TextColor = color?.TextColor,
            HasLabelsWidget = labels is not null,
            Labels = labels?.Labels,
            HasMilestoneWidget = milestone is not null,
            Milestone = milestone?.Milestone,
            HasIterationWidget = iteration is not null,
            Iteration = iteration?.Iteration,
            HasHierarchyWidget = hierarchy is not null,
            Parent = hierarchy?.Parent,
            Children = hierarchy?.Children,
            Ancestors = hierarchy?.Ancestors,
            HasChildren = hierarchy?.HasChildren,
            HasParent = hierarchy?.HasParent
        };
    }

    private static TWidget? SelectWidget<TWidget>(
        IReadOnlyList<GitLabWorkItemWidget?>? widgets,
        GitLabWorkItemCompositionLoadPlan loadPlan,
        GitLabWorkItemCompositionSection section)
        where TWidget : GitLabWorkItemWidget
    {
        return loadPlan.Includes(section) ? FindWidget<TWidget>(widgets) : null;
    }

    private static TWidget? FindWidget<TWidget>(IReadOnlyList<GitLabWorkItemWidget?>? widgets)
        where TWidget : GitLabWorkItemWidget
    {
        if (widgets is null)
        {
            return null;
        }

        for (int index = 0; index < widgets.Count; index++)
        {
            if (widgets[index] is TWidget widget)
            {
                return widget;
            }
        }

        return null;
    }

    private static GitLabWorkItemCompositionCorrelation CreateCorrelation(
        GitLabWorkItemLocator? locator,
        GitLabProject? project,
        GitLabIssue? issue,
        GitLabEpic? legacyEpic,
        GitLabWorkItem? graphQLWorkItem)
    {
        return new GitLabWorkItemCompositionCorrelation
        {
            LocatorNamespaceToRestProjectPath = CompareStrings(locator?.NamespacePath, project?.PathWithNamespace),
            LocatorIidToRestIssueIid = ComparePositiveIntegers(locator?.Iid, issue?.Iid),
            LocatorIidToLegacyRestEpicIid = ComparePositiveIntegers(locator?.Iid, legacyEpic?.Iid),
            LocatorIidToGraphQLWorkItemIid = ComparePositiveIntegerToGraphQLIid(locator?.Iid, graphQLWorkItem?.Iid),
            RestIssueProjectIdToRestProjectId = ComparePositiveIntegers(issue?.ProjectId, project?.Id),
            RestIssueIidToGraphQLWorkItemIid = ComparePositiveIntegerToGraphQLIid(issue?.Iid, graphQLWorkItem?.Iid),
            LegacyRestEpicIidToGraphQLWorkItemIid =
                ComparePositiveIntegerToGraphQLIid(legacyEpic?.Iid, graphQLWorkItem?.Iid)
        };
    }

    private static GitLabWorkItemCompositionCorrelationStatus ComparePositiveIntegerToGraphQLIid(
        long? numericIid,
        string? graphQLIid)
    {
        if (!numericIid.HasValue || string.IsNullOrWhiteSpace(graphQLIid))
        {
            return GitLabWorkItemCompositionCorrelationStatus.Unavailable;
        }

        if (numericIid <= 0 ||
            !long.TryParse(graphQLIid, NumberStyles.None, CultureInfo.InvariantCulture, out long parsedIid) ||
            parsedIid <= 0)
        {
            return GitLabWorkItemCompositionCorrelationStatus.Invalid;
        }

        return numericIid.Value == parsedIid
            ? GitLabWorkItemCompositionCorrelationStatus.Matches
            : GitLabWorkItemCompositionCorrelationStatus.Mismatch;
    }

    private static GitLabWorkItemCompositionCorrelationStatus ComparePositiveIntegers(long? left, long? right)
    {
        if (!left.HasValue || !right.HasValue)
        {
            return GitLabWorkItemCompositionCorrelationStatus.Unavailable;
        }

        if (left <= 0 || right <= 0)
        {
            return GitLabWorkItemCompositionCorrelationStatus.Invalid;
        }

        return left.Value == right.Value
            ? GitLabWorkItemCompositionCorrelationStatus.Matches
            : GitLabWorkItemCompositionCorrelationStatus.Mismatch;
    }

    private static GitLabWorkItemCompositionCorrelationStatus CompareStrings(string? left, string? right)
    {
        if (string.IsNullOrWhiteSpace(left) || string.IsNullOrWhiteSpace(right))
        {
            return GitLabWorkItemCompositionCorrelationStatus.Unavailable;
        }

        return string.Equals(left, right, StringComparison.Ordinal)
            ? GitLabWorkItemCompositionCorrelationStatus.Matches
            : GitLabWorkItemCompositionCorrelationStatus.Mismatch;
    }
}