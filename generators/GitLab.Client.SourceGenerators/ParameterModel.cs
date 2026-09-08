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