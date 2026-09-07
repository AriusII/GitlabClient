namespace GitLab.Client.SourceGenerators;

/// <summary>
///     One <c>[GenerateClientLayers]</c>-marked resource reduced to strings, bools and a
///     <see cref="LocationInfo" /> - nothing symbolic, so the collected pipeline below it compares by
///     value and only re-renders when something that actually changes the emitted text changed.
/// </summary>
/// <param name="RepositoryImplementation">
///     Empty when the conventional <c>&lt;Resource&gt;Repository</c> class could not be resolved; the
///     resource is then reported as GLC0103 and dropped from the registrations rather than emitting a
///     line that would fail as CS0246 inside a generated file.
/// </param>
/// <param name="ServiceImplementation">
///     Composed from <see cref="ClientLayerNaming" />, never resolved: the Service class is emitted by
///     the other generator and therefore does not exist in the compilation this one reads.
/// </param>
internal readonly record struct ResourceWiringModel(
    string ResourceName,
    string RootPropertyName,
    string RepositoryInterface,
    string RepositoryImplementation,
    string ServiceInterface,
    string ServiceImplementation,
    string ClientInterface,
    string ControllerImplementation,
    bool Register,
    bool ExposeOnRootClient,
    bool ClientInterfaceIsPublic,
    LocationInfo? Location);