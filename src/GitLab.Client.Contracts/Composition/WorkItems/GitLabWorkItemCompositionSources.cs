namespace GitLab.Client.Composition.WorkItems;

/// <summary>Availability metadata for every source slice supported by a work-item composition.</summary>
public sealed record GitLabWorkItemCompositionSources
{
    /// <summary>Status of the REST project DTO.</summary>
    public required GitLabWorkItemCompositionSourceStatus RestProject { get; init; }

    /// <summary>Status of the REST issue DTO.</summary>
    public required GitLabWorkItemCompositionSourceStatus RestIssue { get; init; }

    /// <summary>Status of the legacy REST epic DTO.</summary>
    public required GitLabWorkItemCompositionSourceStatus LegacyRestEpic { get; init; }

    /// <summary>Status of the stable GraphQL work-item DTO.</summary>
    public required GitLabWorkItemCompositionSourceStatus GraphQLCore { get; init; }

    /// <summary>Status of the GraphQL description widget fragment.</summary>
    public required GitLabWorkItemCompositionSourceStatus GraphQLDescriptionWidget { get; init; }

    /// <summary>Status of the GraphQL assignees widget fragment.</summary>
    public required GitLabWorkItemCompositionSourceStatus GraphQLAssigneesWidget { get; init; }

    /// <summary>Status of the GraphQL health-status widget fragment.</summary>
    public required GitLabWorkItemCompositionSourceStatus GraphQLHealthStatusWidget { get; init; }

    /// <summary>Status of the GraphQL start/due-date widget fragment.</summary>
    public required GitLabWorkItemCompositionSourceStatus GraphQLStartAndDueDateWidget { get; init; }

    /// <summary>Status of the GraphQL color widget fragment.</summary>
    public required GitLabWorkItemCompositionSourceStatus GraphQLColorWidget { get; init; }

    /// <summary>Status of the GraphQL labels widget fragment.</summary>
    public required GitLabWorkItemCompositionSourceStatus GraphQLLabelsWidget { get; init; }

    /// <summary>Status of the GraphQL milestone widget fragment.</summary>
    public required GitLabWorkItemCompositionSourceStatus GraphQLMilestoneWidget { get; init; }

    /// <summary>Status of the GraphQL iteration widget fragment.</summary>
    public required GitLabWorkItemCompositionSourceStatus GraphQLIterationWidget { get; init; }

    /// <summary>Status of the GraphQL shallow hierarchy widget fragment.</summary>
    public required GitLabWorkItemCompositionSourceStatus GraphQLHierarchyWidget { get; init; }
}