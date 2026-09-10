using System.Diagnostics.CodeAnalysis;

namespace GitLab.Client.Models;

/// <summary>The <c>APIEntitiesBasicGroupDetails</c> projection embedded by a board response.</summary>
public sealed record GitLabBasicGroupDetails
{
    public long? Id { get; init; }

    /// <summary>The group's web URL as the unconstrained string returned by GitLab.</summary>
    [SuppressMessage("Design", "CA1056",
        Justification =
            "APIEntitiesBasicGroupDetails declares web_url as string without a URI format; accepting a "
            + "relative or instance-specific value is required to preserve the GitLab 19.4 wire contract.")]
    public string? WebUrl { get; init; }

    public string? Name { get; init; }
}