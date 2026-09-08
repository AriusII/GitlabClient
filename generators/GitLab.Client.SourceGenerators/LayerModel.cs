namespace GitLab.Client.SourceGenerators;

/// <summary>One generated class: what it is called, what it implements, and what it forwards to.</summary>
internal readonly record struct LayerModel(
    string ClassName,
    string NamespaceName,
    string ImplementedInterface,
    string DependencyInterface,
    EquatableArray<MethodModel> Methods,
    EquatableArray<PropertyModel> Properties);