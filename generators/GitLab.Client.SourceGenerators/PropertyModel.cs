namespace GitLab.Client.SourceGenerators;

/// <summary>
///     One forwarded property. Read-only, write-only and read/write properties all forward 1:1;
///     indexers, events and <c>init</c>-only setters have no meaningful forwarding shape and are
///     rejected with GLC0004 instead of being silently skipped.
/// </summary>
/// <param name="DocCommentLines">
///     See <see cref="MethodModel.DocCommentLines" /> - the same verbatim copy of the original Repository
///     interface member's XML doc-comment, sourced by matching signature rather than by declaration order.
/// </param>
internal readonly record struct PropertyModel(
    string Type,
    string Name,
    bool HasGetter,
    bool HasSetter,
    EquatableArray<string> DocCommentLines);