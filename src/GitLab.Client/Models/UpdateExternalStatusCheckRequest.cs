namespace GitLab.Client.Models;

/// <summary>
///     Request body for <c>PUT /projects/:id/external_status_checks/:check_id</c>. Unset members are omitted
///     rather than sent as null, so this is a partial update.
/// </summary>
public sealed record UpdateExternalStatusCheckRequest
{
    public string? Name { get; init; }

    /// <summary>The absolute URL GitLab calls when the check needs to run.</summary>
    public Uri? ExternalUrl { get; init; }

    /// <summary>
    ///     Write-only HMAC secret GitLab signs its callbacks with. It is never returned by the API - the
    ///     response only reports <see cref="GitLabExternalStatusCheck.Hmac" />.
    /// </summary>
    public string? SharedSecret { get; init; }

    /// <summary>Scopes the check to these protected branches.</summary>
    public IReadOnlyList<long>? ProtectedBranchIds { get; init; }

    /// <summary>
    ///     Deliberately opaque. A record's compiler-generated <c>ToString()</c> prints every member, which would
    ///     put <see cref="SharedSecret" /> into any log line or exception message that formats this request.
    /// </summary>
    public override string ToString()
    {
        return nameof(UpdateExternalStatusCheckRequest);
    }
}