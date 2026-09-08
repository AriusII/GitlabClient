namespace GitLab.Client.SourceGenerators;

/// <summary>
///     One forwarded method, pre-rendered down to strings.
/// </summary>
/// <param name="DocCommentLines">
///     The original Repository interface member's XML doc-comment, one entry per source line, with the
///     leading <c>///</c> marker already stripped (everything after it - including its own hanging
///     indentation - kept verbatim). Empty when the Repository member carries no doc comment. Re-run
///     through the same forwarding lookup for both the Service and the Controller layer, so a consumer
///     calling through the generated Controller sees the documentation authored one layer down instead of
///     nothing at all.
/// </param>
internal readonly record struct MethodModel(
    string ReturnAttributes,
    string ReturnType,
    string Name,
    string TypeParameterList,
    EquatableArray<string> ConstraintClauses,
    EquatableArray<ParameterModel> Parameters,
    EquatableArray<string> DocCommentLines);