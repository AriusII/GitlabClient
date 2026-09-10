namespace GitLab.Client.Composition.WorkItems;

/// <summary>
///     An immutable declaration of the REST and GraphQL slices a caller intends to compose for one work item.
/// </summary>
/// <remarks>
///     This contract intentionally owns no scheduling, pagination, cache, retry, transport, or authorization
///     policy. A future executor can load its selected slices using either API and pass the resulting DTOs to
///     <see cref="GitLabWorkItemCompositionMapper" />. Selecting a GraphQL widget automatically selects the GraphQL
///     core because a widget cannot be identified without its containing work item.
/// </remarks>
public sealed record GitLabWorkItemCompositionLoadPlan
{
    /// <summary>Initializes and normalizes a work-item composition plan.</summary>
    /// <exception cref="ArgumentOutOfRangeException">An unsupported section flag was supplied.</exception>
    public GitLabWorkItemCompositionLoadPlan(GitLabWorkItemCompositionSection sections)
    {
        if ((sections & ~GitLabWorkItemCompositionSection.All) != GitLabWorkItemCompositionSection.None)
        {
            throw new ArgumentOutOfRangeException(nameof(sections), sections,
                "The plan contains an unsupported work-item-composition section.");
        }

        Sections = Normalize(sections);
    }

    /// <summary>A plan with no source request, useful when a caller only wants diagnostics for supplied data.</summary>
    public static GitLabWorkItemCompositionLoadPlan Empty { get; } = new(GitLabWorkItemCompositionSection.None);

    /// <summary>A plan containing the stable GraphQL work-item projection without optional widgets.</summary>
    public static GitLabWorkItemCompositionLoadPlan GraphQLCore { get; } =
        new(GitLabWorkItemCompositionSection.GraphQLCore);

    /// <summary>A plan containing the GraphQL core and every curated widget fragment.</summary>
    public static GitLabWorkItemCompositionLoadPlan CuratedGraphQL { get; } =
        new(GitLabWorkItemCompositionSection.GraphQLCore | GitLabWorkItemCompositionSection.GraphQLWidgets);

    /// <summary>A plan containing every supported REST and GraphQL source slice.</summary>
    public static GitLabWorkItemCompositionLoadPlan Complete { get; } =
        new(GitLabWorkItemCompositionSection.All);

    /// <summary>The normalized source slices selected by this plan.</summary>
    public GitLabWorkItemCompositionSection Sections { get; }

    /// <summary>Returns whether every flag in <paramref name="section" /> was selected.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="section" /> is empty or unsupported.</exception>
    public bool Includes(GitLabWorkItemCompositionSection section)
    {
        if (section == GitLabWorkItemCompositionSection.None ||
            (section & ~GitLabWorkItemCompositionSection.All) != GitLabWorkItemCompositionSection.None)
        {
            throw new ArgumentOutOfRangeException(nameof(section), section,
                "Specify one or more supported work-item-composition sections.");
        }

        return (Sections & section) == section;
    }

    private static GitLabWorkItemCompositionSection Normalize(GitLabWorkItemCompositionSection sections)
    {
        return (sections & GitLabWorkItemCompositionSection.GraphQLWidgets) !=
               GitLabWorkItemCompositionSection.None
            ? sections | GitLabWorkItemCompositionSection.GraphQLCore
            : sections;
    }
}