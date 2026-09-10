namespace GitLab.Client.Models.Requests;

/// <summary>
///     Request body for <c>POST /runners/reset_authentication_token</c>. This runner-protocol endpoint
///     rotates the token presented in the body; it is distinct from the administrator endpoint that resets
///     a token by runner id.
/// </summary>
public sealed record ResetRunnerAuthenticationTokenRequest
{
    /// <summary>The runner's current authentication token.</summary>
    public required string Token { get; init; }
}