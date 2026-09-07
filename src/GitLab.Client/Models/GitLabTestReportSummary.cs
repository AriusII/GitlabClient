namespace GitLab.Client.Models;

/// <summary>
///     A pipeline's test report summary
///     (<c>GET /projects/:id/pipelines/:pipeline_id/test_report_summary</c>) - the same suites as
///     <see cref="GitLabTestReport" /> but without the individual cases, which makes it the cheap call for a
///     pipeline with a large report.
/// </summary>
public sealed record GitLabTestReportSummary
{
    /// <summary>The counts rolled up across every suite.</summary>
    public GitLabTestReportTotal? Total { get; init; }

    /// <summary>
    ///     The per-suite summaries, each carrying <see cref="GitLabTestSuite.BuildIds" /> rather than
    ///     <see cref="GitLabTestSuite.TestCases" />.
    /// </summary>
    public IReadOnlyList<GitLabTestSuite>? TestSuites { get; init; }
}