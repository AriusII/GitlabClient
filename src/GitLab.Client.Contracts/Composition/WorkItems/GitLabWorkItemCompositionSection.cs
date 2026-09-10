namespace GitLab.Client.Composition.WorkItems;

/// <summary>
///     The independently loadable source slices of a <see cref="GitLabWorkItemComposition" />.
/// </summary>
/// <remarks>
///     The REST and GraphQL entries are intentionally separate. GitLab Work Item global IDs are opaque and must not
///     be treated as numeric REST identifiers. Widget flags select only the stable GraphQL widget fragments
///     represented by this SDK.
/// </remarks>
[Flags]
public enum GitLabWorkItemCompositionSection
{
    /// <summary>No optional source slice.</summary>
    None = 0,

    /// <summary>The REST project route DTO.</summary>
    RestProject = 1 << 0,

    /// <summary>The REST issue route DTO.</summary>
    RestIssue = 1 << 1,

    /// <summary>The deprecated-but-readable REST epic route DTO.</summary>
    LegacyRestEpic = 1 << 2,

    /// <summary>The stable GraphQL work-item projection.</summary>
    GraphQLCore = 1 << 3,

    /// <summary>The GraphQL description widget fragment.</summary>
    GraphQLDescriptionWidget = 1 << 4,

    /// <summary>The GraphQL assignees widget fragment.</summary>
    GraphQLAssigneesWidget = 1 << 5,

    /// <summary>The GraphQL health-status widget fragment.</summary>
    GraphQLHealthStatusWidget = 1 << 6,

    /// <summary>The GraphQL start/due-date widget fragment.</summary>
    GraphQLStartAndDueDateWidget = 1 << 7,

    /// <summary>The GraphQL color widget fragment.</summary>
    GraphQLColorWidget = 1 << 8,

    /// <summary>The GraphQL labels widget fragment.</summary>
    GraphQLLabelsWidget = 1 << 9,

    /// <summary>The GraphQL milestone widget fragment.</summary>
    GraphQLMilestoneWidget = 1 << 10,

    /// <summary>The GraphQL iteration widget fragment.</summary>
    GraphQLIterationWidget = 1 << 11,

    /// <summary>The GraphQL shallow hierarchy widget fragment.</summary>
    GraphQLHierarchyWidget = 1 << 12,

    /// <summary>All REST source slices.</summary>
    Rest = RestProject | RestIssue | LegacyRestEpic,

    /// <summary>All currently curated GraphQL widget fragments.</summary>
    GraphQLWidgets = GraphQLDescriptionWidget |
                     GraphQLAssigneesWidget |
                     GraphQLHealthStatusWidget |
                     GraphQLStartAndDueDateWidget |
                     GraphQLColorWidget |
                     GraphQLLabelsWidget |
                     GraphQLMilestoneWidget |
                     GraphQLIterationWidget |
                     GraphQLHierarchyWidget,

    /// <summary>Every source slice currently represented by this composition.</summary>
    All = Rest | GraphQLCore | GraphQLWidgets
}