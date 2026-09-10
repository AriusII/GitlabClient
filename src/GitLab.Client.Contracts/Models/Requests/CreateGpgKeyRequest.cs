namespace GitLab.Client.Models.Requests;

/// <summary>Request body for <c>POST /user/gpg_keys</c> and <c>POST /users/:id/gpg_keys</c>.</summary>
public sealed record CreateGpgKeyRequest
{
    /// <summary>
    ///     The ASCII-armored public key, beginning <c>-----BEGIN PGP PUBLIC KEY BLOCK-----</c>. It spans
    ///     several lines and so travels in the JSON body, never in the URL.
    /// </summary>
    public required string Key { get; init; }
}