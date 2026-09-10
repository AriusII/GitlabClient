using System.Text.Json;

namespace GitLab.Client.Models;

/// <summary>The association between an epic and one of its issues.</summary>
public sealed record GitLabEpicIssueLink
{
    public long? Id { get; init; }

    public int? RelativePosition { get; init; }

    /// <summary>The specification leaves this embedded epic object open-ended.</summary>
    public JsonElement? Epic { get; init; }

    /// <summary>The specification's basic-issue projection has no stable closed schema in this response.</summary>
    public JsonElement? Issue { get; init; }
}