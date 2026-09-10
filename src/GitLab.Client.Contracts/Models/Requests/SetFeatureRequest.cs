using System.Globalization;
using System.Text.Json;

namespace GitLab.Client.Models.Requests;

/// <summary>
///     Body of <c>POST /features/:name</c>, which creates the instance feature if it does not exist yet
///     and then sets one gate on it.
///     <para>
///         <see cref="Key" /> and the actor members (<see cref="FeatureGroup" />, <see cref="User" />,
///         <see cref="Group" />, <see cref="Namespace" />, <see cref="Project" />,
///         <see cref="Organization" />, <see cref="Repository" />, <see cref="Runner" />,
///         <see cref="Endpoint" />) are mutually exclusive: <c>key</c> selects a percentage gate, an actor
///         member scopes a boolean gate. Setting both is a <c>400</c> from GitLab.
///     </para>
/// </summary>
public sealed record SetFeatureRequest
{
    private static readonly JsonElement EnabledValue = ParseValue("true");

    private static readonly JsonElement DisabledValue = ParseValue("false");

    /// <summary>
    ///     The gate value: <c>true</c> or <c>false</c> to switch the feature on or off, or an integer for a
    ///     percentage. The spec types it as an untyped value because it is genuinely polymorphic, so it is a
    ///     raw <see cref="JsonElement" /> here - use <see cref="Enable" />, <see cref="Disable" /> or
    ///     <see cref="ForPercentage" /> rather than building one by hand.
    /// </summary>
    public required JsonElement Value { get; init; }

    /// <summary>
    ///     <c>percentage_of_actors</c> or <c>percentage_of_time</c> (GitLab's default), naming which
    ///     percentage gate <see cref="Value" /> feeds. Mutually exclusive with every actor member.
    /// </summary>
    public string? Key { get; init; }

    /// <summary>A Flipper feature group name to scope the gate to.</summary>
    public string? FeatureGroup { get; init; }

    /// <summary>A username, or several separated by commas.</summary>
    public string? User { get; init; }

    /// <summary>A group path such as <c>gitlab-org</c>, or several separated by commas.</summary>
    public string? Group { get; init; }

    /// <summary>A user or group namespace path, or several separated by commas.</summary>
    public string? Namespace { get; init; }

    /// <summary>A project path such as <c>gitlab-org/gitlab</c>, or several separated by commas.</summary>
    public string? Project { get; init; }

    /// <summary>An organization ID or path, or several separated by commas.</summary>
    public string? Organization { get; init; }

    /// <summary>A repository path such as <c>gitlab-org/gitlab.git</c>, or several separated by commas.</summary>
    public string? Repository { get; init; }

    /// <summary>A runner ID, or several separated by commas.</summary>
    public string? Runner { get; init; }

    /// <summary>
    ///     A caller ID identifying a code path, such as <c>GET /api/v4/projects/:id</c>, or several
    ///     separated by commas.
    /// </summary>
    public string? Endpoint { get; init; }

    /// <summary>Skip GitLab's own validation, such as the check that a YAML definition exists for the flag.</summary>
    public bool? Force { get; init; }

    /// <summary>Switches the feature fully on (<c>value=true</c>).</summary>
    public static SetFeatureRequest Enable()
    {
        return new SetFeatureRequest { Value = EnabledValue };
    }

    /// <summary>Switches the feature fully off (<c>value=false</c>).</summary>
    public static SetFeatureRequest Disable()
    {
        return new SetFeatureRequest { Value = DisabledValue };
    }

    /// <summary>
    ///     Rolls the feature out to a percentage. Which percentage gate is fed depends on <see cref="Key" />,
    ///     which GitLab defaults to <c>percentage_of_time</c>.
    /// </summary>
    /// <param name="percentage">A whole percentage between 0 and 100.</param>
    public static SetFeatureRequest ForPercentage(int percentage)
    {
        return new SetFeatureRequest { Value = ParseValue(percentage.ToString(CultureInfo.InvariantCulture)) };
    }

    // JsonElement has no standalone constructor: it is always a window onto a JsonDocument. Clone()
    // detaches the element from the document so the value survives disposing it here.
    private static JsonElement ParseValue(string json)
    {
        using JsonDocument document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }
}