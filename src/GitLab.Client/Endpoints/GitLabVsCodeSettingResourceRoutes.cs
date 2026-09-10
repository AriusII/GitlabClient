using GitLab.Client.Models;

namespace GitLab.Client.Endpoints;

/// <summary>
///     Projects <see cref="GitLabVsCodeSettingResource" /> onto the <c>{resource_name}</c> path segment of
///     the Settings Sync endpoints. The mapping is an explicit switch rather than a naming convention so
///     that adding an enum member without giving it a wire name is a compile error (CS8509), not a
///     <c>400</c> at run time - and because one of the wire names is camelCase, which no convention over
///     PascalCase member names would produce.
/// </summary>
internal static class GitLabVsCodeSettingResourceRoutes
{
    /// <summary>
    ///     The GitLab wire name for a settings resource. Safe to pass to
    ///     <see cref="Infrastructure.Routing.GitLabRouteBuilder.Literal" />: every value is a fixed word
    ///     from a closed vocabulary, never caller-supplied text.
    /// </summary>
    internal static string ToRouteValue(this GitLabVsCodeSettingResource resourceName)
    {
        return resourceName switch
        {
            GitLabVsCodeSettingResource.Settings => "settings",
            GitLabVsCodeSettingResource.Extensions => "extensions",
            GitLabVsCodeSettingResource.GlobalState => "globalState",
            GitLabVsCodeSettingResource.Machines => "machines",
            GitLabVsCodeSettingResource.Keybindings => "keybindings",
            GitLabVsCodeSettingResource.Snippets => "snippets",
            GitLabVsCodeSettingResource.Tasks => "tasks",
            GitLabVsCodeSettingResource.Profiles => "profiles",
            _ => throw new ArgumentOutOfRangeException(nameof(resourceName), resourceName,
                "Unknown VS Code Settings Sync resource name.")
        };
    }
}