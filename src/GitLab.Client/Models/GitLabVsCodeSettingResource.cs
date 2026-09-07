namespace GitLab.Client.Models;

/// <summary>
///     The kind of VS Code Settings Sync resource a request addresses - the <c>{resource_name}</c> path
///     segment of <c>/vscode/settings_sync/v1/resource/:resource_name</c>.
///     <para>
///         This is a closed vocabulary the spec enumerates on the path parameter, so it is modelled as an
///         enum rather than a free-text segment: an unknown resource name is a <c>400</c> from GitLab,
///         and a compile error is a cheaper way to find that out. It never appears in a JSON payload -
///         the repository projects it onto the route, and the member names differ from the wire words in
///         casing only (<c>globalState</c> is camelCase on the wire because VS Code's own sync protocol
///         spells it that way).
///     </para>
/// </summary>
public enum GitLabVsCodeSettingResource
{
    /// <summary>The user's <c>settings.json</c>.</summary>
    Settings,

    /// <summary>The list of installed extensions.</summary>
    Extensions,

    /// <summary>VS Code's synchronized global state - UI state and per-extension stored values.</summary>
    GlobalState,

    /// <summary>The machines participating in this user's sync, and their display names.</summary>
    Machines,

    /// <summary>The user's <c>keybindings.json</c>.</summary>
    Keybindings,

    /// <summary>The user's code snippets.</summary>
    Snippets,

    /// <summary>The user's <c>tasks.json</c>.</summary>
    Tasks,

    /// <summary>The user's editor profiles.</summary>
    Profiles
}