namespace GitLab.Client.Batching;

/// <summary>Configures one finite, client-side GitLab batch plan.</summary>
/// <remarks>
///     <para>
///         This limits concurrent logical SDK operations; it is not a GitLab quota, a global rate limiter, or an
///         HTTP request merger. GitLab's endpoint-specific batch routes remain separate typed API operations.
///     </para>
///     <para>
///         The conservative default of four concurrent operations is intentionally explicit rather than derived
///         from processor count: GitLab calls are network I/O and instance quotas vary by deployment.
///     </para>
/// </remarks>
public sealed class GitLabBatchOptions
{
    /// <summary>Gets the maximum number of operations that the plan may have in flight at once.</summary>
    public int MaxConcurrency { get; init; } = 4;

    /// <summary>Gets the policy applied after one operation faults or is canceled independently.</summary>
    public GitLabBatchFailureMode FailureMode { get; init; } = GitLabBatchFailureMode.CollectAll;
}