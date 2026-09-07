using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace GitLab.Client.SourceGenerators;

/// <summary>
///     Turns interface symbols into the value-equatable <see cref="LayerModel" />. Every decision that
///     needs a symbol is made here, in the transform stage; the emitter downstream only ever sees
///     strings, which is what keeps the incremental pipeline cacheable and stops the driver rooting whole
///     compilations in its cache.
/// </summary>
internal static class ClientLayerSymbolReader
{
    private const string CallerAttributeNamespace = "System.Runtime.CompilerServices";

    private const string NullabilityAttributeNamespace = "System.Diagnostics.CodeAnalysis";

    /// <summary>Fully qualified and nullable-annotated: the form generated signatures are written in.</summary>
    public static readonly SymbolDisplayFormat TypeFormat = SymbolDisplayFormat.FullyQualifiedFormat
        .WithMiscellaneousOptions(SymbolDisplayFormat.FullyQualifiedFormat.MiscellaneousOptions
                                  | SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);

    /// <summary>
    ///     Deliberately WITHOUT the nullable-reference modifier: <c>string</c> and <c>string?</c> are one
    ///     CLR signature, so keying overload de-duplication on the annotated form would emit two members
    ///     that differ only in nullability and produce CS0111.
    /// </summary>
    private static readonly SymbolDisplayFormat SignatureKeyFormat = SymbolDisplayFormat.FullyQualifiedFormat;

    public static string Fqn(ITypeSymbol type)
    {
        return type.ToDisplayString(TypeFormat);
    }

    /// <summary>
    ///     Collects the members a generated class must implement, de-duplicated by FULL SIGNATURE rather
    ///     than by name. Keying on the name alone silently drops every overload after the first, which
    ///     surfaces as CS0535 on a generated file with no hint that the generator ate the member - and
    ///     overloads are inevitable across ~1800 operations (ProjectId vs GroupId scoping, id vs iid
    ///     lookups, with and without options).
    /// </summary>
    public static void CollectMembers(
        INamedTypeSymbol interfaceSymbol,
        List<IMethodSymbol> methods,
        List<IPropertySymbol> properties,
        List<DiagnosticInfo> diagnostics,
        LocationInfo? fallbackLocation,
        CancellationToken cancellationToken)
    {
        HashSet<string> seen = new(StringComparer.Ordinal);

        foreach (INamedTypeSymbol candidate in EnumerateInterfaces(interfaceSymbol))
        {
            cancellationToken.ThrowIfCancellationRequested();

            foreach (ISymbol member in candidate.GetMembers())
            {
                if (member.DeclaredAccessibility != Accessibility.Public)
                {
                    continue;
                }

                if (member.IsStatic)
                {
                    // A static abstract member DOES have to be implemented, so it cannot be ignored;
                    // an ordinary static helper on the interface is not part of the instance contract
                    // and needs no forwarder at all.
                    if (member.IsAbstract)
                    {
                        Report(diagnostics, interfaceSymbol, member, "static abstract member", fallbackLocation);
                    }

                    continue;
                }

                switch (member)
                {
                    case IMethodSymbol { MethodKind: MethodKind.Ordinary } method:
                        CollectMethod(interfaceSymbol, method, methods, diagnostics, seen, fallbackLocation);
                        break;
                    case IMethodSymbol:
                        // Accessors, constructors and operators: covered by the owning member, or not
                        // implementable at all.
                        break;
                    case IPropertySymbol property:
                        CollectProperty(interfaceSymbol, property, properties, diagnostics, seen, fallbackLocation);
                        break;
                    case IEventSymbol:
                        Report(diagnostics, interfaceSymbol, member, "event", fallbackLocation);
                        break;
                }
            }
        }
    }

    /// <summary>
    ///     Verifies that every member the generated class must implement actually exists on the layer
    ///     below it. Without this, drift between the three hand-written interfaces of a resource surfaces
    ///     as CS1061 inside a generated file the developer cannot edit - and with the three-interface
    ///     pattern repeated once per resource, that drift is the single most likely mistake.
    /// </summary>
    public static void VerifyForwardable(
        INamedTypeSymbol implementedInterface,
        INamedTypeSymbol dependencyInterface,
        List<IMethodSymbol> methods,
        List<IPropertySymbol> properties,
        List<DiagnosticInfo> diagnostics,
        LocationInfo? fallbackLocation,
        CancellationToken cancellationToken)
    {
        Dictionary<string, ISymbol> available = new(StringComparer.Ordinal);

        foreach (INamedTypeSymbol candidate in EnumerateInterfaces(dependencyInterface))
        {
            cancellationToken.ThrowIfCancellationRequested();

            foreach (ISymbol member in candidate.GetMembers())
            {
                if (member.IsStatic || member.DeclaredAccessibility != Accessibility.Public)
                {
                    continue;
                }

                switch (member)
                {
                    case IMethodSymbol { MethodKind: MethodKind.Ordinary } method:
                        AddIfAbsent(available, MethodSignatureKey(method), method);
                        break;
                    case IPropertySymbol { IsIndexer: false } property:
                        AddIfAbsent(available, PropertySignatureKey(property), property);
                        break;
                }
            }
        }

        foreach (IMethodSymbol method in methods)
        {
            if (available.TryGetValue(MethodSignatureKey(method), out ISymbol? match) &&
                match is IMethodSymbol candidate &&
                string.Equals(Fqn(candidate.ReturnType), Fqn(method.ReturnType), StringComparison.Ordinal))
            {
                continue;
            }

            ReportMissing(diagnostics, implementedInterface, dependencyInterface, method, fallbackLocation);
        }

        foreach (IPropertySymbol property in properties)
        {
            if (available.TryGetValue(PropertySignatureKey(property), out ISymbol? match) &&
                match is IPropertySymbol candidate &&
                string.Equals(Fqn(candidate.Type), Fqn(property.Type), StringComparison.Ordinal) &&
                (property.GetMethod is null || candidate.GetMethod is not null) &&
                (property.SetMethod is null || candidate.SetMethod is not null))
            {
                continue;
            }

            ReportMissing(diagnostics, implementedInterface, dependencyInterface, property, fallbackLocation);
        }
    }

    public static MethodModel CreateMethodModel(IMethodSymbol method, List<DiagnosticInfo> diagnostics,
        LocationInfo? fallbackLocation)
    {
        List<ParameterModel> parameters = new();

        foreach (IParameterSymbol parameter in method.Parameters)
        {
            parameters.Add(CreateParameterModel(method, parameter, diagnostics, fallbackLocation));
        }

        List<string> constraints = new();

        foreach (ITypeParameterSymbol typeParameter in method.TypeParameters)
        {
            string? clause = RenderConstraintClause(typeParameter);
            if (clause is not null)
            {
                constraints.Add(clause);
            }
        }

        return new MethodModel(
            RenderAttributes(method.GetReturnTypeAttributes(), "return: "),
            Fqn(method.ReturnType),
            ClientLayerNaming.EscapeIdentifier(method.Name),
            RenderTypeParameterList(method),
            EquatableArray<string>.From(constraints),
            EquatableArray<ParameterModel>.From(parameters));
    }

    public static PropertyModel CreatePropertyModel(IPropertySymbol property)
    {
        return new PropertyModel(
            Fqn(property.Type),
            ClientLayerNaming.EscapeIdentifier(property.Name),
            property.GetMethod is not null,
            property.SetMethod is not null);
    }

    private static void CollectMethod(INamedTypeSymbol interfaceSymbol, IMethodSymbol method,
        List<IMethodSymbol> methods, List<DiagnosticInfo> diagnostics, HashSet<string> seen,
        LocationInfo? fallbackLocation)
    {
        if (!method.IsAbstract && !method.IsVirtual)
        {
            Report(diagnostics, interfaceSymbol, method, "sealed default implementation", fallbackLocation);
            return;
        }

        if (seen.Add(MethodSignatureKey(method)))
        {
            methods.Add(method);
        }
    }

    private static void CollectProperty(INamedTypeSymbol interfaceSymbol, IPropertySymbol property,
        List<IPropertySymbol> properties, List<DiagnosticInfo> diagnostics, HashSet<string> seen,
        LocationInfo? fallbackLocation)
    {
        if (property.IsIndexer)
        {
            Report(diagnostics, interfaceSymbol, property, "indexer", fallbackLocation);
            return;
        }

        if (property.SetMethod is { IsInitOnly: true })
        {
            // An init accessor can only be assigned during construction, so a forwarder body has
            // nowhere to send the value.
            Report(diagnostics, interfaceSymbol, property, "init-only property", fallbackLocation);
            return;
        }

        if (!property.IsAbstract && !property.IsVirtual)
        {
            Report(diagnostics, interfaceSymbol, property, "sealed default implementation", fallbackLocation);
            return;
        }

        if (seen.Add(PropertySignatureKey(property)))
        {
            properties.Add(property);
        }
    }

    /// <summary>
    ///     The declaring interface first, then its base interfaces in a stable ordinal order.
    ///     <c>AllInterfaces</c> order is a compiler implementation detail, and letting it drive member
    ///     order would churn the generated text - and therefore every downstream cache - for no semantic
    ///     reason.
    /// </summary>
    private static IEnumerable<INamedTypeSymbol> EnumerateInterfaces(INamedTypeSymbol interfaceSymbol)
    {
        return interfaceSymbol.AllInterfaces
            .OrderBy(static candidate => candidate.ToDisplayString(SignatureKeyFormat), StringComparer.Ordinal)
            .Prepend(interfaceSymbol);
    }

    private static void AddIfAbsent(Dictionary<string, ISymbol> available, string key, ISymbol member)
    {
        if (!available.ContainsKey(key))
        {
            available.Add(key, member);
        }
    }

    private static string MethodSignatureKey(IMethodSymbol method)
    {
        StringBuilder key = new();
        key.Append("M:").Append(method.Name).Append('`').Append(method.Arity).Append('(');

        for (int index = 0; index < method.Parameters.Length; index++)
        {
            if (index > 0)
            {
                key.Append(',');
            }

            IParameterSymbol parameter = method.Parameters[index];
            key.Append(parameter.RefKind).Append(':')
                .Append(parameter.Type.ToDisplayString(SignatureKeyFormat));
        }

        return key.Append(')').ToString();
    }

    private static string PropertySignatureKey(IPropertySymbol property)
    {
        return "P:" + property.Name;
    }

    private static ParameterModel CreateParameterModel(IMethodSymbol method, IParameterSymbol parameter,
        List<DiagnosticInfo> diagnostics, LocationInfo? fallbackLocation)
    {
        string? defaultValue = RenderDefaultValue(parameter);

        if (defaultValue is null && parameter.HasExplicitDefaultValue && !parameter.IsParams)
        {
            diagnostics.Add(DiagnosticInfo.Create(
                Descriptors.UnrepresentableDefaultValue,
                LocationInfo.CreateFrom(parameter) ?? fallbackLocation,
                parameter.Name,
                method.ToDisplayString()));
        }

        return new ParameterModel(
            RenderAttributes(parameter.GetAttributes(), string.Empty),
            RenderParameterModifiers(parameter),
            RenderArgumentPrefix(parameter),
            Fqn(parameter.Type),
            ClientLayerNaming.EscapeIdentifier(parameter.Name),
            defaultValue);
    }

    private static string RenderParameterModifiers(IParameterSymbol parameter)
    {
        StringBuilder builder = new();

        if (parameter.ScopedKind == ScopedKind.ScopedRef)
        {
            builder.Append("scoped ");
        }

        switch (parameter.RefKind)
        {
            case RefKind.Ref:
                builder.Append("ref ");
                break;
            case RefKind.Out:
                builder.Append("out ");
                break;
            case RefKind.In:
                builder.Append("in ");
                break;
            case RefKind.RefReadOnlyParameter:
                builder.Append("ref readonly ");
                break;
        }

        if (parameter.IsParams)
        {
            builder.Append("params ");
        }

        return builder.ToString();
    }

    private static string RenderArgumentPrefix(IParameterSymbol parameter)
    {
        switch (parameter.RefKind)
        {
            case RefKind.Ref:
                return "ref ";
            case RefKind.Out:
                return "out ";
            default:
                // `in` and `ref readonly` arguments are passed with value syntax; `ref readonly` is not
                // even valid argument syntax.
                return string.Empty;
        }
    }

    /// <summary>
    ///     Renders the parameter's REAL default. Substituting <c>= default</c> for every default (as the
    ///     first version of this generator did) turns <c>int perPage = 20</c> into a silent 0 for anyone
    ///     calling the concrete class - the only failure mode here that produces wrong runtime behaviour
    ///     rather than a compile break. Where the value cannot be reproduced faithfully this returns
    ///     <see langword="null" />, and the caller omits the default entirely so the mistake is a
    ///     compile-time break at the call site rather than a wrong value.
    /// </summary>
    private static string? RenderDefaultValue(IParameterSymbol parameter)
    {
        if (!parameter.HasExplicitDefaultValue || parameter.IsParams)
        {
            return null;
        }

        object? value = parameter.ExplicitDefaultValue;

        if (value is null)
        {
            // Covers "= null" on reference and nullable-value parameters, and "= default" on structs
            // (CancellationToken included).
            return "default";
        }

        ITypeSymbol type = parameter.Type;

        if (type is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } nullable)
        {
            type = nullable.TypeArguments[0];
        }

        if (type.TypeKind == TypeKind.Enum)
        {
            // The cast form survives values with no single named member, such as flag combinations.
            return "(" + Fqn(type) + ")(" +
                   (SymbolDisplay.FormatPrimitive(value, false, false) ?? "0") + ")";
        }

        if (SymbolDisplay.FormatPrimitive(value, true, false) is not { } literal)
        {
            return null;
        }

        switch (type.SpecialType)
        {
            case SpecialType.System_Boolean:
            case SpecialType.System_Char:
            case SpecialType.System_String:
            case SpecialType.System_SByte:
            case SpecialType.System_Byte:
            case SpecialType.System_Int16:
            case SpecialType.System_UInt16:
            case SpecialType.System_Int32:
                return literal;
            case SpecialType.System_UInt32:
                return literal + "U";
            case SpecialType.System_Int64:
                return literal + "L";
            case SpecialType.System_UInt64:
                return literal + "UL";
            case SpecialType.System_Decimal:
                return literal + "M";
            case SpecialType.System_Single:
                return value is float single && !float.IsNaN(single) && !float.IsInfinity(single)
                    ? literal + "F"
                    : null;
            case SpecialType.System_Double:
                return value is double sized && !double.IsNaN(sized) && !double.IsInfinity(sized)
                    ? literal + "D"
                    : null;
            default:
                return null;
        }
    }

    private static string RenderTypeParameterList(IMethodSymbol method)
    {
        if (!method.IsGenericMethod)
        {
            return string.Empty;
        }

        StringBuilder builder = new();
        builder.Append('<');

        for (int index = 0; index < method.TypeParameters.Length; index++)
        {
            if (index > 0)
            {
                builder.Append(", ");
            }

            builder.Append(method.TypeParameters[index].Name);
        }

        return builder.Append('>').ToString();
    }

    /// <summary>
    ///     An implicit implementation of a generic interface method must restate its constraints
    ///     identically or the compiler reports CS0425, so emitting the type-parameter list alone is not
    ///     enough. Order matters: class/struct/unmanaged/notnull, then constraint types, then new() last.
    /// </summary>
    private static string? RenderConstraintClause(ITypeParameterSymbol typeParameter)
    {
        List<string> constraints = new();

        if (typeParameter.HasReferenceTypeConstraint)
        {
            constraints.Add(typeParameter.ReferenceTypeConstraintNullableAnnotation == NullableAnnotation.Annotated
                ? "class?"
                : "class");
        }
        else if (typeParameter.HasUnmanagedTypeConstraint)
        {
            constraints.Add("unmanaged");
        }
        else if (typeParameter.HasValueTypeConstraint)
        {
            constraints.Add("struct");
        }
        else if (typeParameter.HasNotNullConstraint)
        {
            constraints.Add("notnull");
        }

        foreach (ITypeSymbol constraintType in typeParameter.ConstraintTypes)
        {
            constraints.Add(Fqn(constraintType));
        }

        if (typeParameter.HasConstructorConstraint)
        {
            constraints.Add("new()");
        }

        if (constraints.Count == 0)
        {
            return null;
        }

        return "where " + typeParameter.Name + " : " + string.Join(", ", constraints);
    }

    /// <summary>
    ///     Copies the attributes that participate in the C# language contract onto the forwarder:
    ///     nullable flow analysis (<c>[NotNullWhen]</c>, <c>[MaybeNull]</c>), trimming annotations
    ///     (<c>[DynamicallyAccessedMembers]</c>) and caller info. The allow-list is deliberate - copying
    ///     everything would drag along attributes such as <c>[EnumeratorCancellation]</c> that are
    ///     meaningless on a non-iterator forwarder and would produce a warning inside generated code.
    /// </summary>
    private static string RenderAttributes(IEnumerable<AttributeData> attributes, string target)
    {
        StringBuilder builder = new();

        foreach (AttributeData attribute in attributes)
        {
            if (attribute.AttributeClass is not { } attributeClass || !IsContractAttribute(attributeClass))
            {
                continue;
            }

            builder.Append('[').Append(target).Append(Fqn(attributeClass));
            AppendAttributeArguments(builder, attribute);
            builder.Append("] ");
        }

        return builder.ToString();
    }

    private static bool IsContractAttribute(INamedTypeSymbol attributeClass)
    {
        string containingNamespace = attributeClass.ContainingNamespace.ToDisplayString();

        if (string.Equals(containingNamespace, NullabilityAttributeNamespace, StringComparison.Ordinal))
        {
            return true;
        }

        if (!string.Equals(containingNamespace, CallerAttributeNamespace, StringComparison.Ordinal))
        {
            return false;
        }

        switch (attributeClass.Name)
        {
            case "CallerMemberNameAttribute":
            case "CallerFilePathAttribute":
            case "CallerLineNumberAttribute":
            case "CallerArgumentExpressionAttribute":
                return true;
            default:
                return false;
        }
    }

    private static void AppendAttributeArguments(StringBuilder builder, AttributeData attribute)
    {
        if (attribute.ConstructorArguments.Length == 0 && attribute.NamedArguments.Length == 0)
        {
            return;
        }

        builder.Append('(');
        bool first = true;

        foreach (TypedConstant argument in attribute.ConstructorArguments)
        {
            if (!first)
            {
                builder.Append(", ");
            }

            first = false;
            builder.Append(RenderTypedConstant(argument));
        }

        foreach (KeyValuePair<string, TypedConstant> argument in attribute.NamedArguments)
        {
            if (!first)
            {
                builder.Append(", ");
            }

            first = false;
            builder.Append(argument.Key).Append(" = ").Append(RenderTypedConstant(argument.Value));
        }

        builder.Append(')');
    }

    /// <summary>
    ///     Renders one attribute argument as C# source. Roslyn keeps its own
    ///     <c>TypedConstant.ToCSharpString</c> internal, and the shape needed here is small and closed:
    ///     the attributes copied onto a forwarder take literals, enum members, <c>typeof</c> and arrays.
    /// </summary>
    private static string RenderTypedConstant(TypedConstant constant)
    {
        if (constant.IsNull)
        {
            return "null";
        }

        switch (constant.Kind)
        {
            case TypedConstantKind.Type:
                return constant.Value is ITypeSymbol type ? "typeof(" + Fqn(type) + ")" : "null";
            case TypedConstantKind.Enum:
                return "(" + Fqn(constant.Type!) + ")(" +
                       (SymbolDisplay.FormatPrimitive(constant.Value!, false, false) ?? "0") + ")";
            case TypedConstantKind.Array:
                return RenderTypedConstantArray(constant);
            case TypedConstantKind.Primitive:
                return SymbolDisplay.FormatPrimitive(constant.Value!, true, false) ?? "default";
            default:
                return "default";
        }
    }

    private static string RenderTypedConstantArray(TypedConstant constant)
    {
        StringBuilder builder = new();
        builder.Append("new ").Append(Fqn(constant.Type!)).Append(" { ");

        for (int index = 0; index < constant.Values.Length; index++)
        {
            if (index > 0)
            {
                builder.Append(", ");
            }

            builder.Append(RenderTypedConstant(constant.Values[index]));
        }

        return builder.Append(" }").ToString();
    }

    private static void Report(List<DiagnosticInfo> diagnostics, INamedTypeSymbol interfaceSymbol, ISymbol member,
        string reason, LocationInfo? fallbackLocation)
    {
        diagnostics.Add(DiagnosticInfo.Create(
            Descriptors.UnsupportedMember,
            LocationInfo.CreateFrom(member) ?? fallbackLocation,
            interfaceSymbol.ToDisplayString(),
            member.ToDisplayString(),
            reason));
    }

    private static void ReportMissing(List<DiagnosticInfo> diagnostics, INamedTypeSymbol implementedInterface,
        INamedTypeSymbol dependencyInterface, ISymbol member, LocationInfo? fallbackLocation)
    {
        diagnostics.Add(DiagnosticInfo.Create(
            Descriptors.ForwardingTargetMissingMember,
            LocationInfo.CreateFrom(member) ?? fallbackLocation,
            implementedInterface.ToDisplayString(),
            member.ToDisplayString(),
            dependencyInterface.ToDisplayString()));
    }
}