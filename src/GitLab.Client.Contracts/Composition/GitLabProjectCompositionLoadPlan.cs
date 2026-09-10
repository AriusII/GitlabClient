namespace GitLab.Client.Composition;

/// <summary>
///     An immutable declaration of the route results a caller intends to assemble into a
///     <see cref="GitLabProjectComposition" />.
/// </summary>
/// <remarks>
///     This type has no transport dependency. It is a contract for a future composition executor: the executor
///     owns scheduling, pagination, caching, retries and the choice of REST or GraphQL; the plan only makes the
///     requested shape observable. The project and latest-pipeline dependencies are normalized automatically.
/// </remarks>
public sealed record GitLabProjectCompositionLoadPlan
{
    /// <summary>Creates a plan and normalizes required parent route slices.</summary>
    /// <param name="sections">The route slices to load.</param>
    /// <exception cref="ArgumentOutOfRangeException">A value outside the supported section flags was supplied.</exception>
    public GitLabProjectCompositionLoadPlan(GitLabProjectCompositionSection sections)
    {
        if ((sections & ~GitLabProjectCompositionSection.All) != GitLabProjectCompositionSection.None)
        {
            throw new ArgumentOutOfRangeException(nameof(sections), sections,
                "The plan contains an unsupported project-composition section.");
        }

        Sections = Normalize(sections);
    }

    /// <summary>A plan containing only the root project response.</summary>
    public static GitLabProjectCompositionLoadPlan ProjectOnly { get; } = new(GitLabProjectCompositionSection.None);

    /// <summary>A plan suited to deployment and CI views.</summary>
    public static GitLabProjectCompositionLoadPlan Delivery { get; } = new(
        GitLabProjectCompositionSection.LatestPipeline |
        GitLabProjectCompositionSection.LatestPipelineJobs |
        GitLabProjectCompositionSection.Environments);

    /// <summary>A plan suited to issue and merge-request views.</summary>
    public static GitLabProjectCompositionLoadPlan Collaboration { get; } = new(
        GitLabProjectCompositionSection.MergeRequests |
        GitLabProjectCompositionSection.Issues);

    /// <summary>A plan containing every project-composition route slice currently modeled.</summary>
    public static GitLabProjectCompositionLoadPlan Complete { get; } = new(GitLabProjectCompositionSection.All);

    /// <summary>The normalized route slices selected by this plan.</summary>
    public GitLabProjectCompositionSection Sections { get; }

    /// <summary>Returns whether every flag in <paramref name="section" /> is selected.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="section" /> is empty or unsupported.</exception>
    public bool Includes(GitLabProjectCompositionSection section)
    {
        return section == GitLabProjectCompositionSection.None ||
               (section & ~GitLabProjectCompositionSection.All) != GitLabProjectCompositionSection.None
            ? throw new ArgumentOutOfRangeException(nameof(section), section,
                "Specify one or more supported project-composition sections.")
            : (Sections & section) == section;
    }

    private static GitLabProjectCompositionSection Normalize(GitLabProjectCompositionSection sections)
    {
        GitLabProjectCompositionSection normalized = sections | GitLabProjectCompositionSection.Project;

        if ((normalized & GitLabProjectCompositionSection.LatestPipelineJobs) !=
            GitLabProjectCompositionSection.None)
        {
            normalized |= GitLabProjectCompositionSection.LatestPipeline;
        }

        return normalized;
    }
}