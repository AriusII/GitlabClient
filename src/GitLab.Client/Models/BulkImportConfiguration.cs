using System.Text;

namespace GitLab.Client.Models;

/// <summary>
///     How to reach the source GitLab instance a direct-transfer migration reads from, supplied on
///     <c>POST /bulk_imports</c>.
/// </summary>
/// <remarks>
///     <para>
///         This is the only place in the library where a credential for a <em>different</em> instance is
///         supplied. It is request-only: no migration response ever echoes it back, and the record's
///         synthesized formatting is replaced so the token cannot reach a log line, a test output or an
///         exception message through <c>ToString()</c>.
///     </para>
///     <para>
///         The token must belong to a user with the Owner role on the source group, and needs the
///         <c>api</c> scope.
///     </para>
/// </remarks>
public sealed record BulkImportConfiguration
{
    /// <summary>The base URL of the source GitLab instance, for example <c>https://gitlab.example.com</c>.</summary>
    public required Uri Url { get; init; }

    /// <summary>
    ///     A personal, group or project access token on the SOURCE instance, with the <c>api</c> scope.
    ///     Never logged, and never echoed back by GitLab.
    /// </summary>
    public required string AccessToken { get; init; }

    /// <summary>
    ///     Replaces the record's synthesized member list so the access token cannot leak through
    ///     <c>ToString()</c>. Records print every property by default, and this one is a credential.
    /// </summary>
    private bool PrintMembers(StringBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Append("Url = ").Append(Url).Append(", AccessToken = [redacted]");

        return true;
    }
}