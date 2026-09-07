namespace GitLab.Client.Models;

/// <summary>
///     One version of a registered model in the GitLab model registry, in MLflow's
///     <c>ModelVersion</c> shape.
/// </summary>
public sealed record GitLabMlModelVersion
{
    /// <summary>The name of the registered model this version belongs to.</summary>
    public required string Name { get; init; }

    /// <summary>
    ///     The version string. GitLab requires semantic versions here, so the value routinely contains
    ///     dots and may carry a pre-release suffix - never assume it parses as an integer.
    /// </summary>
    public string? Version { get; init; }

    /// <summary>When the version was created, as a Unix timestamp in milliseconds.</summary>
    public long? CreationTimestamp { get; init; }

    /// <summary>When the version was last modified, as a Unix timestamp in milliseconds.</summary>
    public long? LastUpdatedTimestamp { get; init; }

    /// <summary>The MLflow user identifier, which GitLab reports as a string rather than a numeric id.</summary>
    public string? UserId { get; init; }

    /// <summary>
    ///     The MLflow stage of this version - <c>development</c> in the spec's example. The spec types it
    ///     as a bare string with no enumerated vocabulary, so it stays a string here: a stage GitLab adds
    ///     later must not turn a healthy response into a deserialization failure.
    /// </summary>
    public string? CurrentStage { get; init; }

    /// <summary>The version's description, as free text.</summary>
    public string? Description { get; init; }

    /// <summary>The artifact source URI the version was registered from.</summary>
    public string? Source { get; init; }

    /// <summary>The MLflow run - a GitLab candidate - that produced this version, if any.</summary>
    public string? RunId { get; init; }

    /// <summary>
    ///     The version's status - <c>READY</c> in the spec's example. Left as a string for the same
    ///     reason as <see cref="CurrentStage" />: the spec enumerates no vocabulary for it.
    /// </summary>
    public string? Status { get; init; }

    /// <summary>Detail accompanying <see cref="Status" />, typically set only for a failed version.</summary>
    public string? StatusMessage { get; init; }

    /// <summary>User-defined key/value metadata attached to this version.</summary>
    public IReadOnlyList<GitLabMlModelTag>? Tags { get; init; }

    /// <summary>A link back to the run that produced the version.</summary>
    public string? RunLink { get; init; }

    /// <summary>
    ///     The aliases pointing at this version. An alias is caller-chosen free text - the spec's own
    ///     example is the semantic version <c>1.0.0</c> - and is what
    ///     <c>IMlModelsClient.GetVersionByAliasAsync</c> resolves.
    /// </summary>
    public IReadOnlyList<string>? Aliases { get; init; }
}