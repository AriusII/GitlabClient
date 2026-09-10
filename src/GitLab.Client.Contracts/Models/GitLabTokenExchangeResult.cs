namespace GitLab.Client.Models;

/// <summary>
///     The JWT issued by <c>POST /token_exchange</c>.
///     <para>
///         GitLab's OpenAPI spec documents only the <c>201 Created</c> status for this operation and no
///         response schema at all, so the <c>token</c> wire name here is inferred from the operation's own
///         description ("Issues a short-lived JWT") rather than read off a declared schema - verify
///         against a live instance before depending on the exact field name. This whole area
///         (<c>x-gitlab-lifecycle: experiment</c>) may change shape upstream faster than most of this
///         package.
///     </para>
///     <para>
///         <see cref="Token" /> is a bearer credential for the target modular service - do not log it, do
///         not put it in an exception message, and do not let it reach a compiler-generated
///         <c>ToString()</c> (the override below deliberately redacts it, matching
///         <see cref="GitLabAccessTokenWithSecret" /> and <see cref="GitLabDeployTokenWithSecret" />).
///     </para>
/// </summary>
public sealed record GitLabTokenExchangeResult
{
    /// <summary>The issued short-lived JWT, scoped to the requested audience.</summary>
    public string? Token { get; init; }

    /// <summary>Renders the result without its secret. See the type-level remarks.</summary>
    public override string ToString()
    {
        return "GitLabTokenExchangeResult { Token = <redacted> }";
    }
}