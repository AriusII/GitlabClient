using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     One stored VS Code Settings Sync resource
///     (<c>GET /vscode/settings_sync/v1/resource/:resource_name/:id</c>).
///     <para>
///         Every member is nullable and every member is a <see langword="string" />, deliberately: the
///         GitLab spec types this whole tag's fields as strings from Grape's defaults rather than from
///         declared documentation, and <see cref="Content" /> and <see cref="Machines" /> are opaque
///         payloads VS Code encodes itself - GitLab stores and returns them without interpreting them, so
///         this library does not interpret them either.
///     </para>
/// </summary>
public sealed record GitLabVsCodeSetting
{
    /// <summary>The stored payload for this resource, exactly as the editor uploaded it.</summary>
    public string? Content { get; init; }

    /// <summary>The machines this resource is associated with, as VS Code's own encoding of them.</summary>
    public string? Machines { get; init; }

    /// <summary>The resource version. VS Code compares it against the manifest to decide whether to pull.</summary>
    public string? Version { get; init; }

    /// <summary>
    ///     The id of the machine that last wrote this resource. Spelled <c>machineId</c> on the wire -
    ///     camelCase, unlike GitLab's usual snake_case, because it comes straight from VS Code's protocol.
    /// </summary>
    [JsonPropertyName("machineId")]
    public string? MachineId { get; init; }
}