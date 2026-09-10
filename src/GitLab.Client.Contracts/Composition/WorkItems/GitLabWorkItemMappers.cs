using System.Diagnostics.CodeAnalysis;

namespace GitLab.Client.Composition.WorkItems;

/// <summary>
///     Pure Work Item composition operations exposed through <c>gitLab.Mappers.WorkItems</c>.
/// </summary>
/// <remarks>
///     This façade performs no HTTP operation, cache lookup, scheduling, serialization, reflection, or mutable-state
///     access. It delegates to <see cref="GitLabWorkItemCompositionMapper" /> after the caller has independently
///     acquired REST and GraphQL DTOs.
/// </remarks>
public sealed class GitLabWorkItemMappers
{
    /// <summary>Assembles the selected REST and GraphQL Work Item DTOs into a rich composition.</summary>
    /// <exception cref="ArgumentNullException"><paramref name="input" /> or <paramref name="loadPlan" /> is null.</exception>
    [SuppressMessage(
        "Performance",
        "CA1822:Mark members as static",
        Justification =
            "The instance method is required for the allocation-free gitLab.Mappers.WorkItems fluent façade.")]
    public GitLabWorkItemComposition Map(
        GitLabWorkItemCompositionInput input,
        GitLabWorkItemCompositionLoadPlan loadPlan)
    {
        return GitLabWorkItemCompositionMapper.Map(input, loadPlan);
    }
}