namespace GitLab.Client.SourceGenerators;

/// <summary>
///     One forwarded parameter, pre-rendered down to strings. Everything the emitter needs is decided in
///     the transform stage, where symbols are legitimately available; nothing symbolic crosses into the
///     model, which is what lets the pipeline cache on value equality.
/// </summary>
/// <param name="Attributes">
///     Pre-rendered parameter attributes, trailing space included, or empty. Nullable-contract and
///     caller-info attributes participate in the C# language contract, so dropping them would silently
///     degrade flow analysis for anyone calling the generated concrete type.
/// </param>
/// <param name="Modifiers">Declaration-side modifiers such as <c>params </c>, <c>out </c>, <c>in </c>.</param>
/// <param name="ArgumentPrefix">Call-site prefix: <c>ref </c>, <c>out </c> or empty.</param>
/// <param name="Type">Fully qualified, nullable-annotated parameter type.</param>
/// <param name="Name">Parameter name, already escaped against C# keywords.</param>
/// <param name="DefaultValue">Rendered default, or <see langword="null" /> for no default at all.</param>
internal readonly record struct ParameterModel(
    string Attributes,
    string Modifiers,
    string ArgumentPrefix,
    string Type,
    string Name,
    string? DefaultValue);

/// <summary>One forwarded method, pre-rendered down to strings.</summary>
internal readonly record struct MethodModel(
    string ReturnAttributes,
    string ReturnType,
    string Name,
    string TypeParameterList,
    EquatableArray<string> ConstraintClauses,
    EquatableArray<ParameterModel> Parameters);

/// <summary>
///     One forwarded property. Read-only, write-only and read/write properties all forward 1:1;
///     indexers, events and <c>init</c>-only setters have no meaningful forwarding shape and are
///     rejected with GLC0004 instead of being silently skipped.
/// </summary>
internal readonly record struct PropertyModel(
    string Type,
    string Name,
    bool HasGetter,
    bool HasSetter);

/// <summary>One generated class: what it is called, what it implements, and what it forwards to.</summary>
internal readonly record struct LayerModel(
    string ClassName,
    string NamespaceName,
    string ImplementedInterface,
    string DependencyInterface,
    EquatableArray<MethodModel> Methods,
    EquatableArray<PropertyModel> Properties);

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