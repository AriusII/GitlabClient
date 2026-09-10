namespace GitLab.Client.Composition;

/// <summary>
///     The independently loadable route slices that can enrich a <see cref="GitLabProjectComposition" />.
/// </summary>
/// <remarks>
///     The values deliberately describe route results, rather than client implementations. A future orchestrator
///     can therefore fetch the selected slices through REST, GraphQL, a cache, or a test fixture while producing
///     the same composition contract.
/// </remarks>
[Flags]
public enum GitLabProjectCompositionSection
{
    None = 0,

    /// <summary><c>GET /projects/:id</c>.</summary>
    Project = 1,

    /// <summary><c>GET /projects/:id/pipelines</c>, reduced by the caller to its selected pipeline.</summary>
    LatestPipeline = 2,

    /// <summary><c>GET /projects/:id/pipelines/:pipeline_id/jobs</c>.</summary>
    LatestPipelineJobs = 4,

    /// <summary><c>GET /projects/:id/merge_requests</c>.</summary>
    MergeRequests = 8,

    /// <summary><c>GET /projects/:id/issues</c>.</summary>
    Issues = 16,

    /// <summary><c>GET /projects/:id/environments</c>.</summary>
    Environments = 32,

    /// <summary>Every currently supported project-composition slice.</summary>
    All = Project | LatestPipeline | LatestPipelineJobs | MergeRequests | Issues | Environments
}