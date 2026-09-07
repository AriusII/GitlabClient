namespace GitLab.Client.Models;

/// <summary>
///     A pipeline's aggregated test report
///     (<c>GET /projects/:id/pipelines/:pipeline_id/test_report</c>), assembled from the JUnit artifacts its
///     jobs published. Every count is zero and <see cref="TestSuites" /> is empty when no job published one.
/// </summary>
public sealed record GitLabTestReport
{
    /// <summary>Total run time across every suite, in seconds. A float on the wire.</summary>
    public double? TotalTime { get; init; }

    public int? TotalCount { get; init; }

    public int? SuccessCount { get; init; }

    public int? FailedCount { get; init; }

    public int? SkippedCount { get; init; }

    public int? ErrorCount { get; init; }

    public IReadOnlyList<GitLabTestSuite>? TestSuites { get; init; }
}