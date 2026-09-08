namespace GitLab.Client.SourceGenerators;

/// <summary>
///     Everything one <c>[GenerateClientLayers]</c>-marked repository interface contributes: the two
///     classes to emit, the diagnostics collected while building them, and the keys the collision check
///     needs. <see cref="HasError" /> suppresses emission so an error-severity diagnostic is not buried
///     under a cascade of compiler errors inside generated files.
/// </summary>
internal readonly record struct ClientLayersModel(
    string ResourceName,
    string RepositoryInterface,
    LayerModel Service,
    LayerModel Controller,
    string HintNamePrefix,
    LocationInfo? AttributeLocation,
    EquatableArray<DiagnosticInfo> Diagnostics,
    bool HasError);