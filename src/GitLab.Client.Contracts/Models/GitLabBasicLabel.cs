namespace GitLab.Client.Models;

/// <summary>The <c>APIEntitiesLabelBasic</c> projection embedded by a board or board-list response.</summary>
public sealed record GitLabBasicLabel
{
    public long? Id { get; init; }

    public string? Name { get; init; }

    public string? Description { get; init; }

    public string? TextColor { get; init; }

    public string? DescriptionHtml { get; init; }

    public string? Color { get; init; }

    public bool? Archived { get; init; }
}