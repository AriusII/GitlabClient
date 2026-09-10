namespace GitLab.Client.Models.Requests;

/// <summary>
///     The body of <c>POST /license</c>: the Enterprise Edition licence key to activate the instance
///     with.
/// </summary>
/// <remarks>
///     <see cref="License" /> is credential-adjacent - it activates a paid subscription and is as
///     sensitive as a token. It lives on this request record and nowhere else: the library never logs a
///     request body, and no exception thrown by the transport carries one. <c>ToString</c> is overridden
///     for the same reason, because the compiler-generated record <c>ToString</c> would print the key in
///     full the first time someone interpolated the request into a log line.
/// </remarks>
public sealed record CreateLicenseRequest
{
    /// <summary>The licence key, exactly as GitLab issued it.</summary>
    public required string License { get; init; }

    /// <summary>Redacts the licence key. See the remarks on <see cref="CreateLicenseRequest" />.</summary>
    /// <returns>A constant that never contains the key.</returns>
    public override string ToString()
    {
        return "CreateLicenseRequest { License = <redacted> }";
    }
}