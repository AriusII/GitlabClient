namespace GitLab.Client.Models.Requests;

/// <summary>Request body for <c>POST /user/keys</c> and <c>POST /users/:user_id/keys</c>.</summary>
public sealed record CreateSshKeyRequest
{
    /// <summary>The public key, in <c>authorized_keys</c> form.</summary>
    public required string Key { get; init; }

    /// <summary>The human-readable label for the key.</summary>
    public required string Title { get; init; }

    /// <summary>
    ///     When the key stops being usable. GitLab types this as a full date-time here, unlike the plain
    ///     date the access-token request bodies take.
    /// </summary>
    public DateTimeOffset? ExpiresAt { get; init; }

    /// <summary>
    ///     What the key may be used for: <c>auth_and_signing</c>, <c>auth</c> or <c>signing</c>. Left a
    ///     <see cref="string" /> rather than an enum, matching how the other GitLab status/kind fields in
    ///     this library are modelled.
    /// </summary>
    public string? UsageType { get; init; }
}