namespace GitLab.Client.Models.Requests;

/// <summary>Creates a new child epic and relates it to its parent in one operation.</summary>
public sealed record CreateChildEpicRequest
{
    public required string Title { get; init; }

    /// <summary>Defaults to the parent epic's confidentiality when omitted.</summary>
    public bool? Confidential { get; init; }
}