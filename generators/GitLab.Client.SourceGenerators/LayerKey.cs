namespace GitLab.Client.SourceGenerators;

/// <summary>
///     The identity of one generated layer pair, collected across the whole compilation so two
///     repository interfaces that would generate the same class can be reported (GLC0002) rather than
///     colliding as CS0101 inside generated code.
/// </summary>
internal readonly record struct LayerKey(
    string QualifiedServiceName,
    string QualifiedControllerName,
    string RepositoryInterface,
    LocationInfo? Location);