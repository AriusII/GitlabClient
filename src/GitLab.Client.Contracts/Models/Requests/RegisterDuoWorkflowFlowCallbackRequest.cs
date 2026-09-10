namespace GitLab.Client.Models.Requests;

/// <summary>
///     Body of <c>POST /ai/duo_workflows/flow_callbacks</c>. Neither secret is ever returned by GitLab,
///     so record them on your side before sending.
/// </summary>
public sealed record RegisterDuoWorkflowFlowCallbackRequest
{
    /// <summary>The HTTPS URL that receives callbacks. GitLab caps this at 8192 characters.</summary>
    public required Uri Url { get; init; }

    /// <summary>A label for this endpoint, at most 255 characters.</summary>
    public string? Name { get; init; }

    /// <summary>
    ///     HMAC signing secret in <c>whsec_&lt;base64-of-32-bytes&gt;</c> form, used to compute the
    ///     <c>webhook-signature</c> header so deliveries can be verified. Never returned.
    /// </summary>
    public string? SigningToken { get; init; }

    /// <summary>Optional shared secret sent verbatim as the <c>X-Gitlab-Token</c> header. Never returned.</summary>
    public string? Token { get; init; }
}