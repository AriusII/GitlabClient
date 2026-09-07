using GitLab.Client.Models;

namespace GitLab.Client.Repositories;

/// <summary>
///     Projects <see cref="GitLabProjectTemplateType" /> onto the <c>{type}</c> path segment of the project
///     template endpoints. The mapping is an explicit switch rather than a naming convention so that adding
///     an enum member without giving it a wire name is a compile error (CS8509), not a <c>404</c> at run
///     time - the C# name of one member (<c>GitlabCiYmls</c>) does not snake_case to its wire name either.
/// </summary>
internal static class GitLabProjectTemplateTypeRoutes
{
    /// <summary>
    ///     The GitLab wire name for a template type. Safe to pass to
    ///     <see cref="Infrastructure.Routing.GitLabRouteBuilder.Literal" />: every value is a fixed
    ///     lower-snake-case word from a closed vocabulary, never caller-supplied text.
    /// </summary>
    internal static string ToRouteValue(this GitLabProjectTemplateType type)
    {
        return type switch
        {
            GitLabProjectTemplateType.Dockerfiles => "dockerfiles",
            GitLabProjectTemplateType.Gitignores => "gitignores",
            GitLabProjectTemplateType.GitlabCiYmls => "gitlab_ci_ymls",
            GitLabProjectTemplateType.Licenses => "licenses",
            GitLabProjectTemplateType.Issues => "issues",
            GitLabProjectTemplateType.MergeRequests => "merge_requests",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown GitLab project template type.")
        };
    }
}