namespace GitLab.Client.Models;

/// <summary>
///     The project fragment embedded by CI jobs and bridges - the inline <c>project</c> projection in
///     <c>APIEntitiesCiJob</c> and <c>APIEntitiesCiBridge</c>.
/// </summary>
public sealed record GitLabJobProject
{
    /// <summary>Whether CI job tokens from this project are constrained by a job-token scope.</summary>
    public bool? CiJobTokenScopeEnabled { get; init; }
}