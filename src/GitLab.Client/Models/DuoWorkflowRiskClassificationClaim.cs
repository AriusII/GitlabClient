namespace GitLab.Client.Models;

/// <summary>
///     One categorical claim about a merge request, submitted to
///     <c>POST /ai/duo_workflows/tools/risk_classification/results</c>.
/// </summary>
public sealed record DuoWorkflowRiskClassificationClaim
{
    /// <summary>
    ///     Name of the claim - for example <c>touches_auth</c>. GitLab requires <c>[a-z0-9_]+</c>, at most
    ///     64 characters.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    ///     The categorical answer - for example <c>true</c>, <c>false</c> or <c>behavioral</c>. Never a
    ///     score. At most 256 characters.
    /// </summary>
    public required string Value { get; init; }

    /// <summary>Where the claim was observed, as a <c>path:line</c> reference. At most 256 characters.</summary>
    public string? Evidence { get; init; }
}