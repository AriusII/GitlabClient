namespace GitLab.Client.Tests.Repositories;

/// <summary>
///     Stands in for the typed settings record an external caller (or a later wave of per-slug setters)
///     would bring to <c>SetAsync&lt;TSettings&gt;</c>. Deliberately declared in the test assembly with
///     its own context: the point of the generic overload is that the settings type does not have to be
///     one the library knows about.
/// </summary>
internal sealed record TestSlackSettings
{
    public required Uri Webhook { get; init; }

    public bool? NotifyOnlyBrokenPipelines { get; init; }
}