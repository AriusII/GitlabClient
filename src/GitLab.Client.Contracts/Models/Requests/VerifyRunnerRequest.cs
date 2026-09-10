namespace GitLab.Client.Models.Requests;

/// <summary>
///     Request body for <c>POST /runners/verify</c>, which verifies a registered runner's authentication
///     token and optionally associates the request with one runner manager.
/// </summary>
public sealed record VerifyRunnerRequest
{
    /// <summary>The runner authentication token to verify.</summary>
    public required string Token { get; init; }

    /// <summary>The optional system identifier of the runner manager making the request.</summary>
    public string? SystemId { get; init; }
}