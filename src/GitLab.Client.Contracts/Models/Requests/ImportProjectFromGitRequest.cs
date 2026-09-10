using System.Text;
using System.Text.Json.Serialization;

namespace GitLab.Client.Models.Requests;

/// <summary>
///     Body for <c>POST /projects/:id/import/git</c>, which pulls a repository into an existing project
///     from a Git URL.
/// </summary>
/// <remarks>
///     <see cref="ImportUsername" /> and <see cref="ImportPassword" /> are credentials for the source
///     repository. Keep them on this record: never log a populated request, and never fold one into a
///     message a user or a log sink will see.
/// </remarks>
public sealed record ImportProjectFromGitRequest
{
    /// <summary>The repository to clone from. Prefer the credential members over embedding userinfo in the URL.</summary>
    public required Uri ImportUrl { get; init; }

    /// <summary>
    ///     Username for <see cref="ImportUrl" />, when it needs authentication. GitLab calls the field
    ///     <c>import_url_user</c>; a string property whose name contains "Url" is CA1056, and this one is a
    ///     credential rather than a URL, hence the explicit wire name.
    /// </summary>
    [JsonPropertyName("import_url_user")]
    public string? ImportUsername { get; init; }

    /// <summary>
    ///     Password or token for <see cref="ImportUrl" />. A secret. GitLab calls the field
    ///     <c>import_url_password</c>; see <see cref="ImportUsername" /> for why the C# name differs.
    /// </summary>
    [JsonPropertyName("import_url_password")]
    public string? ImportPassword { get; init; }

    private bool PrintMembers(StringBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        // import_url can legitimately contain userinfo or a short-lived access token. Its value therefore
        // must not appear in record diagnostics even when callers chose the dedicated credential members.
        builder.Append("ImportUrl = [redacted], ImportUsername = ")
            .Append(string.IsNullOrEmpty(ImportUsername) ? "[none]" : "[redacted]")
            .Append(", ImportPassword = ")
            .Append(string.IsNullOrEmpty(ImportPassword) ? "[none]" : "[redacted]");

        return true;
    }
}