namespace GitLab.Client.Models;

/// <summary>Request body for <c>POST /projects/:id/external_status_checks</c>.</summary>
public sealed record CreateExternalStatusCheckRequest
{
    public required string Name { get; init; }

    /// <summary>The absolute URL GitLab calls when the check needs to run.</summary>
    public required Uri ExternalUrl { get; init; }

    /// <summary>
    ///     Write-only HMAC secret GitLab signs its callbacks with. It is never returned by the API - the
    ///     response only reports <see cref="GitLabExternalStatusCheck.Hmac" />.
    /// </summary>
    public string? SharedSecret { get; init; }

    /// <summary>Scopes the check to these protected branches; omit to apply it to every branch.</summary>
    public IReadOnlyList<long>? ProtectedBranchIds { get; init; }

    /// <summary>
    ///     Deliberately opaque. A record's compiler-generated <c>ToString()</c> prints every member, which would
    ///     put <see cref="SharedSecret" /> into any log line or exception message that formats this request.
    /// </summary>
    public override string ToString()
    {
        return nameof(CreateExternalStatusCheckRequest);
    }
}