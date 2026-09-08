using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Models;

/// <summary>Query options for <c>GET /application/plan_limits</c>.</summary>
[GitLabQuery]
public readonly record struct PlanLimitsOptions
{
    /// <summary>The plan to read limits from. Defaults to <see cref="GitLabPlanName.Default" /> when omitted.</summary>
    public GitLabPlanName? PlanName { get; init; }
}