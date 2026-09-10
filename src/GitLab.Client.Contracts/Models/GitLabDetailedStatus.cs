namespace GitLab.Client.Models;

/// <summary>
///     The declared <c>DetailedStatusEntity</c> shared by the detailed pipeline response shapes.
/// </summary>
public sealed record GitLabDetailedStatus
{
    public string? Icon { get; init; }

    public string? Text { get; init; }

    public string? Label { get; init; }

    public string? Group { get; init; }

    public string? Tooltip { get; init; }

    public bool? HasDetails { get; init; }

    public string? DetailsPath { get; init; }

    public string? Favicon { get; init; }

    /// <summary>The declared action shape, when GitLab offers an action for the pipeline status.</summary>
    public GitLabDetailedStatusAction? Action { get; init; }
}