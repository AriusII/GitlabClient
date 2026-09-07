using System.Text.Json;
using System.Text.Json.Serialization;

using GitLab.Client.Infrastructure.Serialization;
using GitLab.Client.Models;

namespace GitLab.Client.Tests.Infrastructure;

/// <summary>
///     Guards the one piece of per-resource wiring the <c>[GenerateClientLayers]</c> generator cannot
///     own. A Roslyn generator cannot contribute <c>[JsonSerializable]</c> attributes to
///     <see cref="GitLabJsonContext" />: System.Text.Json's own generator runs against the original
///     compilation and never observes another generator's normal source output, and the attributes
///     cannot be spread across several partial declarations of the context either (that crashes the
///     STJ generator with CS8785). These tests are the substitute for that missing compile-time check.
/// </summary>
/// <remarks>
///     Forgetting a registration is normally already a <c>CS1061</c> compile error, because every call
///     site goes through the strongly typed <c>GitLabJsonContext.Default.&lt;Type&gt;</c> property.
///     What the compiler cannot catch is a DTO that is registered nowhere and not yet consumed by a
///     repository, or a registration dropped while hand-merging this file after a worktree fan-out.
///     The remaining runtime-resolved escape hatch -
///     <c>
///         JsonSerializer.Serialize(value, typeof(X),
///         context)
///     </c>
///     - is made unwritable by <c>src/GitLab.Client/BannedSymbols.txt</c>.
/// </remarks>
public sealed class GitLabJsonContextRegistrationTests
{
    /// <summary>
    ///     Suffix of the Models types that describe query strings rather than JSON bodies. They are
    ///     decomposed by <c>GitLabRouteBuilder.Query(...)</c> and must never carry JSON metadata.
    /// </summary>
    private const string QueryOptionsSuffix = "ListOptions";

    /// <summary>
    ///     The definitive marker for a query-string model: <c>[GitLabQuery]</c> is what makes the query
    ///     projection generated, and it also covers the query models of endpoints that are not lists (CI Lint's
    ///     <c>CiLintOptions</c>, for one), which the <see cref="QueryOptionsSuffix" /> convention alone misses.
    ///     Matched by name because the attribute is compiler-embedded in <c>GitLab.Client</c> and therefore
    ///     cannot be referenced from another assembly.
    /// </summary>
    private const string QueryAttributeName = "GitLabQueryAttribute";

    [Fact]
    public void EveryPayloadModel_HasJsonMetadata()
    {
        List<Type> missing = GetPayloadModelTypes()
            .Where(static type => GitLabJsonContext.Default.GetTypeInfo(type) is null)
            .ToList();

        Assert.True(missing.Count == 0, BuildMissingMessage(missing));
    }

    [Fact]
    public void QueryOptionModels_AreNeverRegistered()
    {
        List<Type> registered = GetModelTypes()
            .Where(IsQueryOptionsModel)
            .Where(static type => GitLabJsonContext.Default.GetTypeInfo(type) is not null)
            .ToList();

        Assert.True(
            registered.Count == 0,
            $"Query-option models ([GitLabQuery], or the {QueryOptionsSuffix} suffix) describe query "
            + "strings, not JSON bodies, and must not be "
            + "registered in GitLabJsonContext (registering one would also silently exempt it from "
            + $"{nameof(EveryPayloadModel_HasJsonMetadata)}): "
            + string.Join(", ", registered.Select(static type => type.Name)));
    }

    [Fact]
    public void SourceGenerationOptions_MatchTheGitLabWireFormat()
    {
        JsonSerializerOptions options = GitLabJsonContext.Default.Options;

        // GitLab is consistently snake_case on the wire.
        Assert.Same(JsonNamingPolicy.SnakeCaseLower, options.PropertyNamingPolicy);

        // Unset properties on Create*/Update* requests must be omitted, not sent as null - sending
        // null would clear the field server-side on a PUT.
        Assert.Equal(JsonIgnoreCondition.WhenWritingNull, options.DefaultIgnoreCondition);

        // Several GitLab endpoints return numerics as JSON strings.
        Assert.Equal(JsonNumberHandling.AllowReadingFromString, options.NumberHandling);

        // The DTOs deliberately model a subset of each GitLab payload, so unmapped members must be
        // skipped. Disallow would throw on essentially every response.
        Assert.Equal(JsonUnmappedMemberHandling.Skip, options.UnmappedMemberHandling);

        // The library's public surface is nullable-annotated; enforce that contract on the wire so a
        // null from GitLab is a named JsonException rather than an NRE deep in consumer code.
        Assert.True(options.RespectNullableAnnotations);

        // Case-insensitive matching is slower and unnecessary against a consistent snake_case API.
        Assert.False(options.PropertyNameCaseInsensitive);

        // No $id/$ref/$type anywhere in this library; enabling it only costs buffering.
        Assert.False(options.AllowOutOfOrderMetadataProperties);
    }

    private static List<Type> GetModelTypes()
    {
        return typeof(GitLabProject).Assembly
            .GetExportedTypes()
            .Where(static type => type.Namespace == typeof(GitLabProject).Namespace)
            .Where(static type => type is { IsClass: true, IsAbstract: false })
            .OrderBy(static type => type.Name, StringComparer.Ordinal)
            .ToList();
    }

    private static List<Type> GetPayloadModelTypes()
    {
        return GetModelTypes()
            .Where(static type => !IsQueryOptionsModel(type))
            .ToList();
    }

    private static bool IsQueryOptionsModel(Type type)
    {
        return type.Name.EndsWith(QueryOptionsSuffix, StringComparison.Ordinal)
               || Array.Exists(
                   type.GetCustomAttributes(false),
                   attribute => string.Equals(attribute.GetType().Name, QueryAttributeName, StringComparison.Ordinal));
    }

    private static string BuildMissingMessage(List<Type> missing)
    {
        IEnumerable<string> lines =
            missing.Select(static type => $"[JsonSerializable(typeof({type.Name}))]");

        return $"""
                {missing.Count} model type(s) under GitLab.Client.Models have no System.Text.Json
                metadata. Add the following to
                src/GitLab.Client/Infrastructure/Serialization/GitLabJsonContext.cs (plus the matching
                [] array entry for any type a List/paged endpoint returns):

                {string.Join(Environment.NewLine, lines)}
                """;
    }
}