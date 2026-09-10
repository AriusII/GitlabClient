namespace GitLab.Client.Composition;

/// <summary>
///     Pure mapper that assembles a <see cref="GitLabProjectComposition" /> from existing GitLab route DTOs.
/// </summary>
/// <remarks>
///     The mapper performs no I/O, scheduling, serialization, reflection or lazy loading. It is intentionally a
///     static function so a future REST/GraphQL orchestration layer can use it without a new abstraction or a DI
///     registration.
/// </remarks>
public static class GitLabProjectCompositionMapper
{
    /// <summary>Assembles the route data selected by <paramref name="loadPlan" />.</summary>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="input" />, its project, or
    ///     <paramref name="loadPlan" /> is null.
    /// </exception>
    /// <exception cref="ArgumentException">A selected collection route result was not supplied.</exception>
    public static GitLabProjectComposition Map(
        GitLabProjectCompositionInput input,
        GitLabProjectCompositionLoadPlan loadPlan)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(input.Project);
        ArgumentNullException.ThrowIfNull(loadPlan);

        return new GitLabProjectComposition
        {
            LoadPlan = loadPlan,
            Project = input.Project,
            LatestPipeline = loadPlan.Includes(GitLabProjectCompositionSection.LatestPipeline)
                ? input.LatestPipeline
                : null,
            LatestPipelineJobs = Select(
                input.LatestPipelineJobs,
                loadPlan,
                GitLabProjectCompositionSection.LatestPipelineJobs,
                nameof(GitLabProjectCompositionInput.LatestPipelineJobs)),
            MergeRequests = Select(
                input.MergeRequests,
                loadPlan,
                GitLabProjectCompositionSection.MergeRequests,
                nameof(GitLabProjectCompositionInput.MergeRequests)),
            Issues = Select(
                input.Issues,
                loadPlan,
                GitLabProjectCompositionSection.Issues,
                nameof(GitLabProjectCompositionInput.Issues)),
            Environments = Select(
                input.Environments,
                loadPlan,
                GitLabProjectCompositionSection.Environments,
                nameof(GitLabProjectCompositionInput.Environments))
        };
    }

    private static IReadOnlyList<T> Select<T>(
        IReadOnlyList<T>? routeResult,
        GitLabProjectCompositionLoadPlan loadPlan,
        GitLabProjectCompositionSection section,
        string memberName)
    {
        return !loadPlan.Includes(section)
            ? []
            : routeResult ?? throw new ArgumentException(
                $"The selected {section} route result must be supplied through {memberName}.",
                nameof(routeResult));
    }
}