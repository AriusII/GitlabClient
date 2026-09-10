using System.Diagnostics.CodeAnalysis;

namespace GitLab.Client.Composition;

/// <summary>
///     Pure project-composition operations exposed through <c>gitLab.Mappers.Projects</c>.
/// </summary>
/// <remarks>
///     This façade has no transport, cache, scheduler, serialization, reflection, or mutable state. It delegates to
///     <see cref="GitLabProjectCompositionMapper" /> so consumers can use the fluent client entry point without
///     giving up the static mapper for dependency-free code paths.
/// </remarks>
public sealed class GitLabProjectMappers
{
    /// <summary>Assembles the selected project route results into a rich composition.</summary>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="input" />, its project, or <paramref name="loadPlan" /> is null.
    /// </exception>
    /// <exception cref="ArgumentException">A selected collection route result was not supplied.</exception>
    [SuppressMessage(
        "Performance",
        "CA1822:Mark members as static",
        Justification =
            "The instance method is required for the allocation-free gitLab.Mappers.Projects fluent façade.")]
    public GitLabProjectComposition Map(
        GitLabProjectCompositionInput input,
        GitLabProjectCompositionLoadPlan loadPlan)
    {
        return GitLabProjectCompositionMapper.Map(input, loadPlan);
    }
}