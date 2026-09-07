using System.Text;

namespace GitLab.Client.Models;

/// <summary>The body of <c>POST /import/github/gists</c>.</summary>
/// <remarks>
///     Carries a credential for github.com, so the record's synthesized formatting is replaced: a record
///     prints every property through <c>ToString()</c> by default, and this one is a token.
/// </remarks>
public sealed record ImportGitHubGistsRequest
{
    /// <summary>A GitHub personal access token with the <c>gist</c> scope.</summary>
    public required string PersonalAccessToken { get; init; }

    private bool PrintMembers(StringBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        // Reports only whether a token was supplied - enough to debug an empty request, and never the
        // token itself.
        builder.Append("PersonalAccessToken = ")
            .Append(string.IsNullOrEmpty(PersonalAccessToken) ? "[none]" : "[redacted]");

        return true;
    }
}