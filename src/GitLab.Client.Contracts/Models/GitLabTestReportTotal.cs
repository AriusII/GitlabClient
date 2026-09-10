namespace GitLab.Client.Models;

/// <summary>
///     The roll-up block of a pipeline's test report summary
///     (<c>GET /projects/:id/pipelines/:pipeline_id/test_report_summary</c>). It uses shorter wire names than
///     <see cref="GitLabTestReport" /> - <c>count</c> rather than <c>total_count</c> - so it is a separate type.
/// </summary>
public sealed record GitLabTestReportTotal
{
    /// <summary>Total run time across every suite, in seconds. A float on the wire.</summary>
    public double? Time { get; init; }

    public int? Count { get; init; }

    public int? Success { get; init; }

    public int? Failed { get; init; }

    public int? Skipped { get; init; }

    public int? Error { get; init; }

    /// <summary>Set when GitLab could not parse one of the reports at all; null otherwise.</summary>
    public string? SuiteError { get; init; }
}