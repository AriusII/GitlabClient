namespace GitLab.Client.Models;

/// <summary>
///     Runtime metadata a GitLab Runner supplies while it registers with the legacy
///     <c>POST /runners</c> protocol endpoint.
///     <para>
///         This is deliberately separate from <see cref="GitLabRunner" />. It is the runner process's
///         self-reported build identity, not a persisted runner resource returned by the administration
///         APIs. Every member is optional because runners register progressively as they bring their
///         executors online.
///     </para>
/// </summary>
public sealed record GitLabRunnerMetadata
{
    /// <summary>The runner process name reported by its host.</summary>
    public string? Name { get; init; }

    /// <summary>The GitLab Runner version, for example <c>18.7.0</c>.</summary>
    public string? Version { get; init; }

    /// <summary>The Git revision of the GitLab Runner binary.</summary>
    public string? Revision { get; init; }

    /// <summary>The operating system platform hosting the runner.</summary>
    public string? Platform { get; init; }

    /// <summary>The CPU architecture hosting the runner, for example <c>amd64</c>.</summary>
    public string? Architecture { get; init; }
}