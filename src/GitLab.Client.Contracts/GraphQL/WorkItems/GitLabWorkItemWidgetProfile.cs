namespace GitLab.Client.GraphQL.WorkItems;

/// <summary>Closed client-side widget-selection profiles for curated GitLab Work Items GraphQL operations.</summary>
/// <remarks>
///     Profiles are selection budgets, not GitLab entitlement assertions. A client must still request only widgets
///     exposed by the target work-item type and instance. <see cref="Core" /> intentionally requests no widgets.
/// </remarks>
public enum GitLabWorkItemWidgetProfile
{
    /// <summary>Requests the core work-item projection and no widgets.</summary>
    Core = 0,

    /// <summary>Requests description, assignees, labels, and milestone widgets.</summary>
    Standard,

    /// <summary>Requests start/due-date, iteration, hierarchy, and color widgets.</summary>
    Planning,

    /// <summary>Requests the health-status widget.</summary>
    Ultimate,

    /// <summary>Requests every curated widget from the Core, Standard, Planning, and Ultimate profiles.</summary>
    Comprehensive
}