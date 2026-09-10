using System.Text.Json;
using System.Text.Json.Serialization;

namespace GitLab.Client.Models.Requests;

/// <summary>
///     Body of <c>POST /mcp</c> - one JSON-RPC 2.0 envelope addressed to GitLab's Model Context Protocol
///     server.
/// </summary>
public sealed record McpJsonRpcRequest
{
    /// <summary>
    ///     The JSON-RPC protocol version. GitLab's schema admits exactly one value, so it is fixed here
    ///     rather than exposed as a single-member enum.
    /// </summary>
    [JsonPropertyName("jsonrpc")]
    public string JsonRpc { get; } = "2.0";

    /// <summary>The MCP method to invoke - <c>initialize</c>, <c>tools/list</c>, <c>tools/call</c>.</summary>
    public required string Method { get; init; }

    /// <summary>
    ///     The correlation id echoed back on the response. JSON-RPC allows a number or a string, so this is
    ///     a raw <see cref="JsonElement" /> rather than a guess at one of them; build one with
    ///     <c>JsonDocument.Parse("1").RootElement.Clone()</c>.
    /// </summary>
    public JsonElement? Id { get; init; }

    /// <summary>
    ///     The method's parameters, an object or an array depending on the method. Untyped in the spec and
    ///     defined by the MCP method being called, so it stays a raw <see cref="JsonElement" />.
    /// </summary>
    [JsonPropertyName("params")]
    public JsonElement? Parameters { get; init; }
}