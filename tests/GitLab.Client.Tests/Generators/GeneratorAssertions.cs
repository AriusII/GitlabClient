namespace GitLab.Client.Tests.Generators;

internal static class GeneratorAssertions
{
    public static void AssertNoDiagnostics(GeneratorHarnessResult result)
    {
        Assert.True(result.GeneratorDiagnostics.IsEmpty,
            $"Expected no generator diagnostics; got: {Describe(result)}");
    }

    public static void AssertDiagnostic(GeneratorHarnessResult result, string id)
    {
        Assert.Contains(result.GeneratorDiagnostics,
            diagnostic => string.Equals(diagnostic.Id, id, StringComparison.Ordinal));
    }

    public static void AssertCompiles(GeneratorHarnessResult result)
    {
        Assert.True(result.CompilationErrors.IsEmpty,
            "Generated code did not compile: " +
            string.Join(" | ", result.CompilationErrors.Select(static error => error.ToString())));
    }

    private static string Describe(GeneratorHarnessResult result)
    {
        return string.Join(", ", result.GeneratorDiagnostics.Select(static diagnostic =>
            diagnostic.Id + ": " + diagnostic.GetMessage()));
    }
}