using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace GitLab.Client.OpenApiGen;

/// <summary>
///     Produces a deterministic OpenAPI operation inventory and a deliberately conservative, source-verified
///     coverage ledger. It is a checked-in build-time tool; it does not participate in the SDK runtime path.
/// </summary>
internal static class Program
{
    private const string SummaryFileName = "_summary.json";
    private const int ManifestSchemaVersion = 3;
    private const int CoverageLedgerSchemaVersion = 1;

    private const string DefaultUnmappedReason =
        "No explicit source-verified client mapping is recorded in OperationCoverageLedger.json.";

    public static int Main(string[] args)
    {
        try
        {
            Command command = Command.Parse(args);
            Manifest manifest = BuildManifest(command.IndexDirectory, command.LedgerPath, command.ClientSourceRoot);
            byte[] output = Serialize(manifest);

            if (command.Mode == CommandMode.Verify)
            {
                return Verify(command.OutputPath, output, manifest);
            }

            Write(command.OutputPath, output);
            Console.WriteLine(
                $"Wrote {manifest.OperationCount} operations from GitLab {manifest.SpecVersion} to '{command.OutputPath}'.");
            return 0;
        }
        catch (Exception exception) when (exception is ArgumentException or IOException or JsonException)
        {
            Console.Error.WriteLine(exception.Message);
            return 1;
        }
    }

    private static Manifest BuildManifest(string indexDirectory, string ledgerPath, string clientSourceRoot)
    {
        if (!Directory.Exists(indexDirectory))
        {
            throw new ArgumentException(
                $"OpenAPI index directory '{indexDirectory}' does not exist. Supply --index <spec/index> from the pinned GitLab specification.");
        }

        string summaryPath = Path.Combine(indexDirectory, SummaryFileName);
        using JsonDocument summary = JsonDocument.Parse(File.ReadAllBytes(summaryPath));
        string specVersion = RequiredString(summary.RootElement, "specVersion", summaryPath);
        int declaredOperationCount = RequiredInt32(summary.RootElement, "totalOperations", summaryPath);

        List<string> indexFiles = Directory.EnumerateFiles(indexDirectory, "*.json", SearchOption.TopDirectoryOnly)
            .OrderBy(static path => Path.GetFileName(path), StringComparer.Ordinal)
            .ToList();
        List<Operation> operations = [];
        HashSet<string> operationIds = new(StringComparer.Ordinal);

        foreach (string indexFile in indexFiles)
        {
            if (string.Equals(Path.GetFileName(indexFile), SummaryFileName, StringComparison.Ordinal))
            {
                continue;
            }

            using JsonDocument document = JsonDocument.Parse(File.ReadAllBytes(indexFile));
            JsonElement root = document.RootElement;
            string tag = RequiredString(root, "tag", indexFile);

            foreach (JsonElement operation in RequiredArray(root, "operations", indexFile).EnumerateArray())
            {
                string operationId = RequiredString(operation, "operationId", indexFile);
                if (!operationIds.Add(operationId))
                {
                    throw new JsonException($"Duplicate OpenAPI operationId '{operationId}'.");
                }

                bool hasRequestBody = RequiredNonNullProperty(operation, "requestSchema", indexFile,
                    out JsonElement requestSchema);
                bool hasResponseBody = RequiredNonNullProperty(operation, "responseSchema", indexFile,
                    out JsonElement responseSchema);
                operations.Add(new Operation(
                    operationId,
                    tag,
                    RequiredString(operation, "method", indexFile),
                    RequiredString(operation, "path", indexFile),
                    RequiredBoolean(operation, "deprecated", indexFile),
                    RequiredBoolean(operation, "proseDeprecated", indexFile),
                    hasRequestBody,
                    hasResponseBody,
                    hasRequestBody ? GetSchemaReferences(requestSchema) : [],
                    hasResponseBody ? GetSchemaReferences(responseSchema) : [],
                    null));
            }
        }

        operations.Sort(static (left, right) => string.CompareOrdinal(left.OperationId, right.OperationId));
        if (operations.Count != declaredOperationCount)
        {
            throw new JsonException(
                $"The OpenAPI index declares {declaredOperationCount} operations but contains {operations.Count}.");
        }

        CoverageLedger ledger = LoadCoverageLedger(ledgerPath, specVersion, clientSourceRoot, operations);
        Dictionary<string, CoverageEntry> coverageByOperation = ledger.Entries.ToDictionary(
            static entry => entry.OperationId,
            StringComparer.Ordinal);

        int mappedOperationCount = 0;
        int excludedOperationCount = 0;
        for (int index = 0; index < operations.Count; index++)
        {
            Operation operation = operations[index];
            Coverage coverage;
            if (coverageByOperation.TryGetValue(operation.OperationId, out CoverageEntry? entry))
            {
                coverage = entry.ToManifestCoverage();
                if (entry.State == CoverageState.Mapped)
                {
                    mappedOperationCount++;
                }
                else
                {
                    excludedOperationCount++;
                }
            }
            else
            {
                coverage = Coverage.Unmapped(DefaultUnmappedReason);
            }

            operations[index] = operation with { Coverage = coverage };
        }

        return new Manifest(
            ManifestSchemaVersion,
            specVersion,
            operations.Count,
            ComputeInputHash(indexFiles),
            new CoverageSummary(
                CoverageLedgerSchemaVersion,
                mappedOperationCount,
                excludedOperationCount,
                operations.Count - mappedOperationCount - excludedOperationCount,
                DefaultUnmappedReason),
            operations);
    }

    private static CoverageLedger LoadCoverageLedger(string ledgerPath, string specVersion, string clientSourceRoot,
        IReadOnlyList<Operation> operations)
    {
        if (!File.Exists(ledgerPath))
        {
            throw new ArgumentException(
                $"OpenAPI coverage ledger '{ledgerPath}' is missing. Add an explicit ledger before generating the manifest.");
        }

        using JsonDocument document = JsonDocument.Parse(File.ReadAllBytes(ledgerPath));
        JsonElement root = document.RootElement;
        EnsureObject(root, ledgerPath);
        EnsureOnlyProperties(root, ledgerPath, "schemaVersion", "specVersion", "entries");
        int schemaVersion = RequiredInt32(root, "schemaVersion", ledgerPath);
        if (schemaVersion != CoverageLedgerSchemaVersion)
        {
            throw new JsonException(
                $"OpenAPI coverage ledger '{ledgerPath}' has schema version {schemaVersion}; expected {CoverageLedgerSchemaVersion}.");
        }

        string ledgerSpecVersion = RequiredString(root, "specVersion", ledgerPath);
        if (!string.Equals(ledgerSpecVersion, specVersion, StringComparison.Ordinal))
        {
            throw new JsonException(
                $"OpenAPI coverage ledger '{ledgerPath}' targets GitLab {ledgerSpecVersion}, but the index targets GitLab {specVersion}.");
        }

        Dictionary<string, Operation> operationsById = operations.ToDictionary(
            static operation => operation.OperationId,
            StringComparer.Ordinal);
        HashSet<string> recordedOperationIds = new(StringComparer.Ordinal);
        List<CoverageEntry> entries = [];
        int entryIndex = 0;
        foreach (JsonElement element in RequiredArray(root, "entries", ledgerPath).EnumerateArray())
        {
            string entryPath = $"{ledgerPath} entries[{entryIndex++}]";
            CoverageEntry entry = ParseCoverageEntry(element, entryPath);
            if (!operationsById.TryGetValue(entry.OperationId, out Operation operation))
            {
                throw new JsonException($"{entryPath} references unknown OpenAPI operationId '{entry.OperationId}'.");
            }

            if (!recordedOperationIds.Add(entry.OperationId))
            {
                throw new JsonException(
                    $"OpenAPI coverage ledger contains duplicate operationId '{entry.OperationId}'.");
            }

            if (entry.State == CoverageState.Mapped)
            {
                ValidateMappedEntry(entry, operation, clientSourceRoot, entryPath);
            }

            entries.Add(entry);
        }

        return new CoverageLedger(entries);
    }

    private static CoverageEntry ParseCoverageEntry(JsonElement element, string entryPath)
    {
        EnsureObject(element, entryPath);
        string stateName = RequiredString(element, "state", entryPath);
        string operationId = RequiredString(element, "operationId", entryPath);
        string reason = RequiredString(element, "reason", entryPath);

        return stateName switch
        {
            "mapped" => ParseMappedCoverageEntry(element, entryPath, operationId, reason),
            "excluded" => ParseExcludedCoverageEntry(element, entryPath, operationId, reason),
            _ => throw new JsonException($"{entryPath} has unsupported coverage state '{stateName}'.")
        };
    }

    private static CoverageEntry ParseMappedCoverageEntry(JsonElement element, string entryPath, string operationId,
        string reason)
    {
        EnsureOnlyProperties(element, entryPath, "operationId", "state", "reason", "clientInterface", "endpointType",
            "transportCall", "routeSegments", "methods");
        return new CoverageEntry(
            operationId,
            CoverageState.Mapped,
            reason,
            RequiredIdentifier(element, "clientInterface", entryPath),
            RequiredIdentifier(element, "endpointType", entryPath),
            RequiredIdentifier(element, "transportCall", entryPath),
            RequiredRouteSegments(element, entryPath),
            RequiredCoverageMethods(element, entryPath));
    }

    private static CoverageEntry ParseExcludedCoverageEntry(JsonElement element, string entryPath, string operationId,
        string reason)
    {
        EnsureOnlyProperties(element, entryPath, "operationId", "state", "reason");
        return new CoverageEntry(operationId, CoverageState.Excluded, reason, null, null, null, [], []);
    }

    private static List<string> RequiredRouteSegments(JsonElement element, string entryPath)
    {
        JsonElement segments = RequiredArray(element, "routeSegments", entryPath);
        if (segments.GetArrayLength() == 0)
        {
            throw new JsonException($"{entryPath} has no route segments.");
        }

        List<string> values = [];
        int index = 0;
        foreach (JsonElement segment in segments.EnumerateArray())
        {
            if (segment.ValueKind != JsonValueKind.String || string.IsNullOrWhiteSpace(segment.GetString()))
            {
                throw new JsonException($"{entryPath} routeSegments[{index}] must be a non-empty string.");
            }

            string value = segment.GetString()!;
            if (value.Contains('/', StringComparison.Ordinal) || value.Contains('{', StringComparison.Ordinal) ||
                value.Contains('}', StringComparison.Ordinal))
            {
                throw new JsonException(
                    $"{entryPath} routeSegments[{index}] must be one fully literal route segment, not '{value}'.");
            }

            values.Add(value);
            index++;
        }

        return values;
    }

    private static List<CoverageMethod> RequiredCoverageMethods(JsonElement element, string entryPath)
    {
        JsonElement methods = RequiredArray(element, "methods", entryPath);
        if (methods.GetArrayLength() == 0)
        {
            throw new JsonException($"{entryPath} has no client methods.");
        }

        HashSet<string> clientMethods = new(StringComparer.Ordinal);
        HashSet<string> endpointMethods = new(StringComparer.Ordinal);
        List<CoverageMethod> values = [];
        int index = 0;
        foreach (JsonElement method in methods.EnumerateArray())
        {
            string methodPath = $"{entryPath} methods[{index++}]";
            EnsureObject(method, methodPath);
            EnsureOnlyProperties(method, methodPath, "clientMethod", "endpointMethod");
            string clientMethod = RequiredIdentifier(method, "clientMethod", methodPath);
            string endpointMethod = RequiredIdentifier(method, "endpointMethod", methodPath);
            if (!clientMethods.Add(clientMethod))
            {
                throw new JsonException($"{methodPath} repeats client method '{clientMethod}'.");
            }

            if (!endpointMethods.Add(endpointMethod))
            {
                throw new JsonException($"{methodPath} repeats endpoint method '{endpointMethod}'.");
            }

            values.Add(new CoverageMethod(clientMethod, endpointMethod));
        }

        return values;
    }

    private static void ValidateMappedEntry(CoverageEntry entry, Operation operation, string clientSourceRoot,
        string entryPath)
    {
        ArgumentNullException.ThrowIfNull(entry.ClientInterface);
        ArgumentNullException.ThrowIfNull(entry.EndpointType);
        ArgumentNullException.ThrowIfNull(entry.TransportCall);
        if (!IsExpectedTransportCall(operation.Method, entry.TransportCall))
        {
            throw new JsonException(
                $"{entryPath} transport '{entry.TransportCall}' cannot issue {operation.Method} for '{operation.OperationId}'.");
        }

        string expectedPath = "/api/v4/" + string.Join('/', entry.RouteSegments);
        if (!string.Equals(operation.Path, expectedPath, StringComparison.Ordinal))
        {
            throw new JsonException(
                $"{entryPath} literal route '{expectedPath}' does not match OpenAPI path '{operation.Path}'.");
        }

        string interfacePath = Path.Combine(clientSourceRoot, "Abstractions", entry.ClientInterface + ".cs");
        string endpointPath = Path.Combine(clientSourceRoot, "Endpoints", entry.EndpointType + ".cs");
        string interfaceSource = ReadSourceFile(interfacePath, entryPath);
        string endpointSource = ReadSourceFile(endpointPath, entryPath);
        RequireTypeDeclaration(interfaceSource, "interface", entry.ClientInterface, interfacePath, entryPath);
        RequireTypeDeclaration(endpointSource, "class", entry.EndpointType, endpointPath, entryPath);
        if (!endpointSource.Contains(": " + entry.ClientInterface, StringComparison.Ordinal))
        {
            throw new JsonException(
                $"{entryPath} endpoint '{entry.EndpointType}' does not implement '{entry.ClientInterface}'.");
        }

        string expectedRouteExpression = BuildRouteExpression(entry.RouteSegments);
        foreach (CoverageMethod method in entry.Methods)
        {
            if (!ContainsMethodDeclaration(interfaceSource, method.ClientMethod))
            {
                throw new JsonException(
                    $"{entryPath} client method '{entry.ClientInterface}.{method.ClientMethod}' is not declared in '{interfacePath}'.");
            }

            string endpointMethodBody =
                ExtractMethodBody(endpointSource, method.EndpointMethod, endpointPath, entryPath);
            string normalizedMethodBody = RemoveWhitespace(endpointMethodBody);
            if (!normalizedMethodBody.Contains("connection." + entry.TransportCall + "(", StringComparison.Ordinal))
            {
                throw new JsonException(
                    $"{entryPath} endpoint method '{entry.EndpointType}.{method.EndpointMethod}' does not call connection.{entry.TransportCall}.");
            }

            if (!ContainsExpectedLiteralRoute(normalizedMethodBody, expectedRouteExpression))
            {
                throw new JsonException(
                    $"{entryPath} endpoint method '{entry.EndpointType}.{method.EndpointMethod}' does not contain the expected fully literal route expression.");
            }
        }
    }

    private static bool IsExpectedTransportCall(string httpMethod, string transportCall)
    {
        return httpMethod switch
        {
            "GET" => string.Equals(transportCall, "GetAsync", StringComparison.Ordinal),
            "PUT" => string.Equals(transportCall, "PutAsync", StringComparison.Ordinal) ||
                     string.Equals(transportCall, "PutFileAsync", StringComparison.Ordinal) ||
                     string.Equals(transportCall, "PutMultipartFormAsync", StringComparison.Ordinal) ||
                     string.Equals(transportCall, "PutUrlEncodedFormAsync", StringComparison.Ordinal),
            "POST" => string.Equals(transportCall, "PostAsync", StringComparison.Ordinal) ||
                      string.Equals(transportCall, "PostFileAsync", StringComparison.Ordinal) ||
                      string.Equals(transportCall, "PostMultipartFormAsync", StringComparison.Ordinal) ||
                      string.Equals(transportCall, "PostUrlEncodedFormAsync", StringComparison.Ordinal),
            "DELETE" => string.Equals(transportCall, "DeleteAsync", StringComparison.Ordinal),
            "HEAD" => string.Equals(transportCall, "HeadAsync", StringComparison.Ordinal),
            _ => false
        };
    }

    private static string BuildRouteExpression(IReadOnlyList<string> routeSegments)
    {
        StringBuilder builder = new("GitLabRouteBuilder.Create(\"");
        builder.Append(routeSegments[0]);
        builder.Append("\")");
        for (int index = 1; index < routeSegments.Count; index++)
        {
            builder.Append(".Literal(\"");
            builder.Append(routeSegments[index]);
            builder.Append("\")");
        }

        return builder.ToString();
    }

    private static bool ContainsExpectedLiteralRoute(string normalizedMethodBody, string expectedRouteExpression)
    {
        int routeStart = normalizedMethodBody.IndexOf(expectedRouteExpression, StringComparison.Ordinal);
        if (routeStart < 0)
        {
            return false;
        }

        int buildStart = normalizedMethodBody.IndexOf(".Build()", routeStart + expectedRouteExpression.Length,
            StringComparison.Ordinal);
        if (buildStart < 0)
        {
            return false;
        }

        ReadOnlySpan<char> continuation = normalizedMethodBody.AsSpan(
            routeStart + expectedRouteExpression.Length,
            buildStart - routeStart - expectedRouteExpression.Length);
        return !continuation.Contains(".Literal(", StringComparison.Ordinal) &&
               !continuation.Contains(".Escaped(", StringComparison.Ordinal) &&
               !continuation.Contains(".Segment(", StringComparison.Ordinal) &&
               !continuation.Contains("GitLabRouteBuilder.Create(", StringComparison.Ordinal);
    }

    private static string ReadSourceFile(string path, string entryPath)
    {
        if (!File.Exists(path))
        {
            throw new JsonException($"{entryPath} expects source file '{path}', but it does not exist.");
        }

        return File.ReadAllText(path);
    }

    private static void RequireTypeDeclaration(string source, string typeKeyword, string typeName, string sourcePath,
        string entryPath)
    {
        if (!source.Contains(typeKeyword + " " + typeName, StringComparison.Ordinal))
        {
            throw new JsonException($"{entryPath} expects '{typeKeyword} {typeName}' in '{sourcePath}'.");
        }
    }

    private static bool ContainsMethodDeclaration(string source, string methodName)
    {
        int index = source.IndexOf(methodName, StringComparison.Ordinal);
        while (index >= 0)
        {
            int afterName = index + methodName.Length;
            int openingParenthesis = SkipWhitespace(source, afterName);
            if (IsIdentifierBoundary(source, index - 1) && IsIdentifierBoundary(source, afterName) &&
                openingParenthesis < source.Length && source[openingParenthesis] == '(')
            {
                return true;
            }

            index = source.IndexOf(methodName, afterName, StringComparison.Ordinal);
        }

        return false;
    }

    private static string ExtractMethodBody(string source, string methodName, string sourcePath, string entryPath)
    {
        int index = source.IndexOf(methodName, StringComparison.Ordinal);
        while (index >= 0)
        {
            int afterName = index + methodName.Length;
            int openingParenthesis = SkipWhitespace(source, afterName);
            if (!IsIdentifierBoundary(source, index - 1) || !IsIdentifierBoundary(source, afterName) ||
                openingParenthesis >= source.Length || source[openingParenthesis] != '(')
            {
                index = source.IndexOf(methodName, afterName, StringComparison.Ordinal);
                continue;
            }

            int closingParenthesis = FindMatchingDelimiter(source, openingParenthesis, '(', ')');
            if (closingParenthesis < 0)
            {
                throw new JsonException(
                    $"{entryPath} cannot parse endpoint method '{methodName}' in '{sourcePath}': unbalanced parameter list.");
            }

            int openingBrace = FindNextCodeCharacter(source, closingParenthesis + 1, '{');
            if (openingBrace >= 0)
            {
                int closingBrace = FindMatchingDelimiter(source, openingBrace, '{', '}');
                if (closingBrace < 0)
                {
                    throw new JsonException(
                        $"{entryPath} cannot parse endpoint method '{methodName}' in '{sourcePath}': unbalanced method body.");
                }

                return source.Substring(openingBrace, closingBrace - openingBrace + 1);
            }

            index = source.IndexOf(methodName, afterName, StringComparison.Ordinal);
        }

        throw new JsonException($"{entryPath} endpoint method '{methodName}' is not implemented in '{sourcePath}'.");
    }

    private static int FindMatchingDelimiter(string source, int openingIndex, char opening, char closing)
    {
        int depth = 0;
        for (int index = openingIndex; index < source.Length; index++)
        {
            char current = source[index];
            if (current == '/' && index + 1 < source.Length)
            {
                if (source[index + 1] == '/')
                {
                    index = FindLineEnd(source, index + 2);
                    continue;
                }

                if (source[index + 1] == '*')
                {
                    index = FindBlockCommentEnd(source, index + 2);
                    continue;
                }
            }

            if (current == '\"' || current == '\'')
            {
                index = FindStringEnd(source, index, current);
                continue;
            }

            if (current == opening)
            {
                depth++;
            }
            else if (current == closing && --depth == 0)
            {
                return index;
            }
        }

        return -1;
    }

    private static int FindNextCodeCharacter(string source, int startIndex, char expected)
    {
        for (int index = startIndex; index < source.Length; index++)
        {
            char current = source[index];
            if (current == '/' && index + 1 < source.Length)
            {
                if (source[index + 1] == '/')
                {
                    index = FindLineEnd(source, index + 2);
                    continue;
                }

                if (source[index + 1] == '*')
                {
                    index = FindBlockCommentEnd(source, index + 2);
                    continue;
                }
            }

            if (current == '\"' || current == '\'')
            {
                index = FindStringEnd(source, index, current);
                continue;
            }

            if (current == expected)
            {
                return index;
            }
        }

        return -1;
    }

    private static int FindLineEnd(string source, int startIndex)
    {
        int index = source.IndexOf('\n', startIndex);
        return index < 0 ? source.Length : index;
    }

    private static int FindBlockCommentEnd(string source, int startIndex)
    {
        int index = source.IndexOf("*/", startIndex, StringComparison.Ordinal);
        return index < 0 ? source.Length : index + 1;
    }

    private static int FindStringEnd(string source, int openingIndex, char quote)
    {
        for (int index = openingIndex + 1; index < source.Length; index++)
        {
            if (source[index] == '\\')
            {
                index++;
                continue;
            }

            if (source[index] == quote)
            {
                return index;
            }
        }

        return source.Length;
    }

    private static string RemoveWhitespace(string value)
    {
        StringBuilder builder = new(value.Length);
        foreach (char character in value)
        {
            if (!char.IsWhiteSpace(character))
            {
                builder.Append(character);
            }
        }

        return builder.ToString();
    }

    private static int SkipWhitespace(string source, int startIndex)
    {
        int index = startIndex;
        while (index < source.Length && char.IsWhiteSpace(source[index]))
        {
            index++;
        }

        return index;
    }

    private static bool IsIdentifierBoundary(string source, int index)
    {
        return index < 0 || index >= source.Length || (!char.IsLetterOrDigit(source[index]) && source[index] != '_');
    }

    private static void EnsureObject(JsonElement element, string sourcePath)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            throw new JsonException($"'{sourcePath}' must be a JSON object.");
        }
    }

    private static void EnsureOnlyProperties(JsonElement element, string sourcePath, params string[] allowedProperties)
    {
        foreach (JsonProperty property in element.EnumerateObject())
        {
            if (!allowedProperties.Contains(property.Name, StringComparer.Ordinal))
            {
                throw new JsonException($"'{sourcePath}' contains unsupported property '{property.Name}'.");
            }
        }
    }

    private static string RequiredIdentifier(JsonElement element, string propertyName, string sourcePath)
    {
        string value = RequiredString(element, propertyName, sourcePath);
        if (!IsIdentifier(value))
        {
            throw new JsonException($"'{sourcePath}' property '{propertyName}' must be a C# identifier.");
        }

        return value;
    }

    private static bool IsIdentifier(string value)
    {
        if (value.Length == 0 || !IsIdentifierStart(value[0]))
        {
            return false;
        }

        for (int index = 1; index < value.Length; index++)
        {
            if (!char.IsLetterOrDigit(value[index]) && value[index] != '_')
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsIdentifierStart(char value)
    {
        return char.IsLetter(value) || value == '_';
    }

    private static string RequiredString(JsonElement element, string propertyName, string sourcePath)
    {
        if (!element.TryGetProperty(propertyName, out JsonElement property) ||
            property.ValueKind != JsonValueKind.String ||
            string.IsNullOrWhiteSpace(property.GetString()))
        {
            throw new JsonException($"'{sourcePath}' does not have a non-empty string '{propertyName}'.");
        }

        return property.GetString()!;
    }

    private static int RequiredInt32(JsonElement element, string propertyName, string sourcePath)
    {
        if (!element.TryGetProperty(propertyName, out JsonElement property) ||
            property.ValueKind != JsonValueKind.Number ||
            !property.TryGetInt32(out int value))
        {
            throw new JsonException($"'{sourcePath}' does not have an Int32 '{propertyName}'.");
        }

        return value;
    }

    private static bool RequiredBoolean(JsonElement element, string propertyName, string sourcePath)
    {
        if (!element.TryGetProperty(propertyName, out JsonElement property) ||
            (property.ValueKind != JsonValueKind.True && property.ValueKind != JsonValueKind.False))
        {
            throw new JsonException($"'{sourcePath}' does not have a Boolean '{propertyName}'.");
        }

        return property.GetBoolean();
    }

    private static JsonElement RequiredArray(JsonElement element, string propertyName, string sourcePath)
    {
        if (!element.TryGetProperty(propertyName, out JsonElement property) ||
            property.ValueKind != JsonValueKind.Array)
        {
            throw new JsonException($"'{sourcePath}' does not have an array '{propertyName}'.");
        }

        return property;
    }

    private static bool RequiredNonNullProperty(JsonElement element, string propertyName, string sourcePath,
        out JsonElement property)
    {
        if (!element.TryGetProperty(propertyName, out property))
        {
            throw new JsonException($"'{sourcePath}' has an operation without '{propertyName}'.");
        }

        return property.ValueKind is not JsonValueKind.Null;
    }

    private static List<string> GetSchemaReferences(JsonElement schema)
    {
        HashSet<string> references = new(StringComparer.Ordinal);
        CollectSchemaReferences(schema, references);
        return references.OrderBy(static reference => reference, StringComparer.Ordinal).ToList();
    }

    private static void CollectSchemaReferences(JsonElement element, HashSet<string> references)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (JsonProperty property in element.EnumerateObject())
                {
                    if (property.NameEquals("ref") && property.Value.ValueKind == JsonValueKind.String)
                    {
                        string? reference = property.Value.GetString();
                        if (!string.IsNullOrEmpty(reference))
                        {
                            references.Add(reference);
                        }
                    }

                    CollectSchemaReferences(property.Value, references);
                }

                break;
            case JsonValueKind.Array:
                foreach (JsonElement item in element.EnumerateArray())
                {
                    CollectSchemaReferences(item, references);
                }

                break;
        }
    }

    private static string ComputeInputHash(List<string> indexFiles)
    {
        using IncrementalHash hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        foreach (string file in indexFiles)
        {
            hash.AppendData(Encoding.UTF8.GetBytes(Path.GetFileName(file)));
            hash.AppendData([0]);
            hash.AppendData(File.ReadAllBytes(file));
            hash.AppendData([0]);
        }

        return Convert.ToHexStringLower(hash.GetHashAndReset());
    }

    private static byte[] Serialize(Manifest manifest)
    {
        using MemoryStream stream = new();
        using (Utf8JsonWriter writer = new(stream, new JsonWriterOptions { Indented = true }))
        {
            writer.WriteStartObject();
            writer.WriteNumber("schemaVersion", manifest.SchemaVersion);
            writer.WriteString("specVersion", manifest.SpecVersion);
            writer.WriteNumber("operationCount", manifest.OperationCount);
            writer.WriteString("indexSha256", manifest.IndexSha256);
            WriteCoverageSummary(writer, manifest.CoverageSummary);
            writer.WriteStartArray("operations");
            foreach (Operation operation in manifest.Operations)
            {
                writer.WriteStartObject();
                writer.WriteString("operationId", operation.OperationId);
                writer.WriteString("tag", operation.Tag);
                writer.WriteString("method", operation.Method);
                writer.WriteString("path", operation.Path);
                writer.WriteBoolean("deprecated", operation.Deprecated);
                writer.WriteBoolean("proseDeprecated", operation.ProseDeprecated);
                writer.WriteBoolean("hasRequestBody", operation.HasRequestBody);
                writer.WriteBoolean("hasResponseBody", operation.HasResponseBody);
                WriteSchemaReferences(writer, "requestSchemaRefs", operation.RequestSchemaRefs);
                WriteSchemaReferences(writer, "responseSchemaRefs", operation.ResponseSchemaRefs);
                WriteCoverage(writer,
                    operation.Coverage ?? throw new InvalidOperationException("Coverage was not resolved."));
                writer.WriteEndObject();
            }

            writer.WriteEndArray();
            writer.WriteEndObject();
        }

        return stream.ToArray();
    }

    private static void WriteCoverageSummary(Utf8JsonWriter writer, CoverageSummary summary)
    {
        writer.WritePropertyName("coverageSummary");
        writer.WriteStartObject();
        writer.WriteNumber("ledgerSchemaVersion", summary.LedgerSchemaVersion);
        writer.WriteNumber("mappedOperationCount", summary.MappedOperationCount);
        writer.WriteNumber("excludedOperationCount", summary.ExcludedOperationCount);
        writer.WriteNumber("unmappedOperationCount", summary.UnmappedOperationCount);
        writer.WriteString("unmappedReason", summary.UnmappedReason);
        writer.WriteEndObject();
    }

    private static void WriteCoverage(Utf8JsonWriter writer, Coverage coverage)
    {
        writer.WritePropertyName("coverage");
        writer.WriteStartObject();
        writer.WriteString("state", coverage.State);
        writer.WriteString("reason", coverage.Reason);
        if (coverage.ClientInterface is not null)
        {
            writer.WriteString("clientInterface", coverage.ClientInterface);
            writer.WriteString("endpointType", coverage.EndpointType);
            writer.WriteStartArray("clientMethods");
            foreach (CoverageMethod method in coverage.Methods)
            {
                writer.WriteStringValue(method.ClientMethod);
            }

            writer.WriteEndArray();
            writer.WriteStartArray("endpointMethods");
            foreach (CoverageMethod method in coverage.Methods)
            {
                writer.WriteStringValue(method.EndpointMethod);
            }

            writer.WriteEndArray();
        }

        writer.WriteEndObject();
    }

    private static void WriteSchemaReferences(Utf8JsonWriter writer, string propertyName,
        IReadOnlyList<string> references)
    {
        writer.WritePropertyName(propertyName);
        writer.WriteStartArray();
        foreach (string reference in references)
        {
            writer.WriteStringValue(reference);
        }

        writer.WriteEndArray();
    }

    private static int Verify(string outputPath, byte[] expected, Manifest manifest)
    {
        if (!File.Exists(outputPath))
        {
            Console.Error.WriteLine($"OpenAPI manifest '{outputPath}' is missing. Run with --write.");
            return 1;
        }

        if (!File.ReadAllBytes(outputPath).AsSpan().SequenceEqual(expected))
        {
            Console.Error.WriteLine(
                $"OpenAPI manifest '{outputPath}' is stale for GitLab {manifest.SpecVersion}. Run with --write and review the operation and coverage-ledger diff.");
            return 1;
        }

        Console.WriteLine(
            $"Verified {manifest.OperationCount} operations from GitLab {manifest.SpecVersion} ({manifest.CoverageSummary.MappedOperationCount} source-verified mappings).");
        return 0;
    }

    private static void Write(string outputPath, byte[] content)
    {
        string? directory = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        string temporaryPath = outputPath + ".tmp";
        File.WriteAllBytes(temporaryPath, content);
        File.Move(temporaryPath, outputPath, true);
    }

    private sealed record Manifest(
        int SchemaVersion,
        string SpecVersion,
        int OperationCount,
        string IndexSha256,
        CoverageSummary CoverageSummary,
        List<Operation> Operations);

    private sealed record CoverageSummary(
        int LedgerSchemaVersion,
        int MappedOperationCount,
        int ExcludedOperationCount,
        int UnmappedOperationCount,
        string UnmappedReason);

    private sealed record CoverageLedger(IReadOnlyList<CoverageEntry> Entries);

    private sealed record CoverageEntry(
        string OperationId,
        CoverageState State,
        string Reason,
        string? ClientInterface,
        string? EndpointType,
        string? TransportCall,
        IReadOnlyList<string> RouteSegments,
        IReadOnlyList<CoverageMethod> Methods)
    {
        public Coverage ToManifestCoverage()
        {
            return new Coverage(State == CoverageState.Mapped ? "mapped" : "excluded", Reason, ClientInterface,
                EndpointType, Methods);
        }
    }

    private sealed record Coverage(
        string State,
        string Reason,
        string? ClientInterface,
        string? EndpointType,
        IReadOnlyList<CoverageMethod> Methods)
    {
        public static Coverage Unmapped(string reason)
        {
            return new Coverage("unmapped", reason, null, null, []);
        }
    }

    private sealed record CoverageMethod(string ClientMethod, string EndpointMethod);

    private enum CoverageState
    {
        Mapped,
        Excluded
    }

    private readonly record struct Operation(
        string OperationId,
        string Tag,
        string Method,
        string Path,
        bool Deprecated,
        bool ProseDeprecated,
        bool HasRequestBody,
        bool HasResponseBody,
        IReadOnlyList<string> RequestSchemaRefs,
        IReadOnlyList<string> ResponseSchemaRefs,
        Coverage? Coverage);

    private sealed record Command(
        CommandMode Mode,
        string IndexDirectory,
        string OutputPath,
        string LedgerPath,
        string ClientSourceRoot)
    {
        public static Command Parse(string[] arguments)
        {
            CommandMode? mode = null;
            string indexDirectory = Path.Combine("spec", "index");
            string outputPath = Path.Combine("tools", "GitLab.Client.OpenApiGen", "OpenApiManifest.json");
            string ledgerPath = Path.Combine("tools", "GitLab.Client.OpenApiGen", "OperationCoverageLedger.json");
            string clientSourceRoot = Path.Combine("src", "GitLab.Client");

            for (int index = 0; index < arguments.Length; index++)
            {
                switch (arguments[index])
                {
                    case "--write": mode = SetMode(mode, CommandMode.Write); break;
                    case "--verify": mode = SetMode(mode, CommandMode.Verify); break;
                    case "--index" when index + 1 < arguments.Length: indexDirectory = arguments[++index]; break;
                    case "--output" when index + 1 < arguments.Length: outputPath = arguments[++index]; break;
                    case "--ledger" when index + 1 < arguments.Length: ledgerPath = arguments[++index]; break;
                    case "--client-source-root"
                        when index + 1 < arguments.Length: clientSourceRoot = arguments[++index]; break;
                    default:
                        throw new ArgumentException(
                            "Usage: GitLab.Client.OpenApiGen (--write|--verify) [--index <spec/index>] [--output <manifest.json>] [--ledger <coverage.json>] [--client-source-root <src/GitLab.Client>].");
                }
            }

            return new Command(mode ?? throw new ArgumentException("Specify --write or --verify."), indexDirectory,
                outputPath, ledgerPath, clientSourceRoot);
        }

        private static CommandMode SetMode(CommandMode? existing, CommandMode requested)
        {
            if (existing is not null && existing != requested)
            {
                throw new ArgumentException("Specify exactly one of --write or --verify.");
            }

            return requested;
        }
    }

    private enum CommandMode
    {
        Write,
        Verify
    }
}