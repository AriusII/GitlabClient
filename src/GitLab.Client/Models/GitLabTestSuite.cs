namespace GitLab.Client.Models;

/// <summary>
///     One test suite inside a pipeline's test report.
///     <para>
///         GitLab has two shapes here and both are modelled by this type: the full form returned by
///         <c>/test_report</c>, and the summary form returned by <c>/test_report_summary</c>, which adds
///         <see cref="BuildIds" /> and omits <see cref="TestCases" />. Every member is therefore nullable.
///     </para>
/// </summary>
public sealed record GitLabTestSuite
{
    /// <summary>The suite name, which GitLab takes from the job that published the report.</summary>
    public string? Name { get; init; }

    /// <summary>Total run time of the suite in seconds. A float on the wire.</summary>
    public double? TotalTime { get; init; }

    public int? TotalCount { get; init; }

    public int? SuccessCount { get; init; }

    public int? FailedCount { get; init; }

    public int? SkippedCount { get; init; }

    public int? ErrorCount { get; init; }

    /// <summary>
    ///     Why GitLab could not parse the suite's report at all - for example
    ///     "JUnit XML parsing failed: 1:1: FATAL: Document is empty". Null when parsing succeeded.
    /// </summary>
    public string? SuiteError { get; init; }

    /// <summary>
    ///     The individual cases. Part of the <c>/test_report</c> shape only; null on the summary form.
    /// </summary>
    public IReadOnlyList<GitLabTestCase>? TestCases { get; init; }

    /// <summary>
    ///     The jobs whose artifacts the suite was assembled from. Part of the <c>/test_report_summary</c>
    ///     shape only; null on the full form.
    /// </summary>
    public IReadOnlyList<long>? BuildIds { get; init; }
}