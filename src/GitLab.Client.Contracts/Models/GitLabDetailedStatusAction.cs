namespace GitLab.Client.Models;

/// <summary>The action object embedded in a <see cref="GitLabDetailedStatus" />.</summary>
public sealed record GitLabDetailedStatusAction
{
    public string? Icon { get; init; }

    public string? Title { get; init; }

    public string? Path { get; init; }

    public string? Method { get; init; }

    public string? ButtonTitle { get; init; }

    public string? ConfirmationMessage { get; init; }
}