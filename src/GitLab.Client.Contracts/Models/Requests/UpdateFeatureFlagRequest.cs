namespace GitLab.Client.Models.Requests;

/// <summary>
///     Body of <c>PUT /projects/:id/feature_flags/:feature_flag_name</c>. Every member is optional; the
///     library's <c>WhenWritingNull</c> policy omits the ones left unset rather than clearing them.
/// </summary>
public sealed record UpdateFeatureFlagRequest
{
    /// <summary>Renames the flag. The route still takes the flag's current name.</summary>
    public string? Name { get; init; }

    public string? Description { get; init; }

    public bool? Active { get; init; }

    /// <summary>
    ///     Strategies to add, edit or delete. An entry with no <see cref="FeatureFlagStrategyRequest.Id" />
    ///     is added; one carrying an id is edited; one carrying an id and
    ///     <see cref="FeatureFlagStrategyRequest.Destroy" /> is removed. Strategies left out of the array are
    ///     untouched.
    /// </summary>
    public IReadOnlyList<FeatureFlagStrategyRequest>? Strategies { get; init; }
}