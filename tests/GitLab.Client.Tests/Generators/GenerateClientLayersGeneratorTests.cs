namespace GitLab.Client.Tests.Generators;

/// <summary>
///     Drives <c>GenerateClientLayersGenerator</c> over inline compilations. Everything covered here
///     fails at GENERATION time, which the tests that merely instantiate the generated classes can never
///     reach: if the generator drops a member or crashes, the test project itself stops building.
/// </summary>
public sealed class GenerateClientLayersGeneratorTests
{
    [Fact]
    public void Emits_EveryOverload_NotJustTheFirst()
    {
        const string Members = """
                                       int Get(long id);
                                       int Get(string path);
                                       int Get(long id, bool withStatistics);
                               """;
        const string Implementation = """
                                              public int Get(long id) { return 1; }
                                              public int Get(string path) { return 2; }
                                              public int Get(long id, bool withStatistics) { return 3; }
                                      """;

        GeneratorHarnessResult result =
            GeneratorSources.RunLayers(GeneratorSources.ResourceWithMembers(Members, Implementation));

        GeneratorSources.AssertNoDiagnostics(result);
        GeneratorSources.AssertCompiles(result);

        string service = result.Source(GeneratorSources.ServiceHintName);
        Assert.Contains("Get(long id)", service, StringComparison.Ordinal);
        Assert.Contains("Get(string path)", service, StringComparison.Ordinal);
        Assert.Contains("Get(long id, bool withStatistics)", service, StringComparison.Ordinal);
    }

    [Fact]
    public void Emits_TheRealOptionalDefault_NotDefault()
    {
        const string Members = """
                                       int Get(int perPage = 20, bool statistics = true,
                                           System.StringComparison order = System.StringComparison.Ordinal, string? search = null);
                               """;
        const string Implementation = """
                                              public int Get(int perPage = 20, bool statistics = true,
                                                  System.StringComparison order = System.StringComparison.Ordinal, string? search = null)
                                              {
                                                  return perPage;
                                              }
                                      """;

        GeneratorHarnessResult result =
            GeneratorSources.RunLayers(GeneratorSources.ResourceWithMembers(Members, Implementation));

        GeneratorSources.AssertNoDiagnostics(result);
        GeneratorSources.AssertCompiles(result);

        string service = result.Source(GeneratorSources.ServiceHintName);
        Assert.Contains("int perPage = 20", service, StringComparison.Ordinal);
        Assert.Contains("bool statistics = true", service, StringComparison.Ordinal);
        Assert.Contains("(global::System.StringComparison)(4)", service, StringComparison.Ordinal);
        Assert.Contains("string? search = default", service, StringComparison.Ordinal);
    }

    [Fact]
    public void Emits_RefKindAndParamsModifiers()
    {
        const string Members = """
                                       bool TryGet(int id, out int value);
                                       int Sum(params int[] values);
                                       int Measure(in System.DateTime moment);
                               """;
        const string Implementation = """
                                              public bool TryGet(int id, out int value) { value = id; return true; }
                                              public int Sum(params int[] values) { return values.Length; }
                                              public int Measure(in System.DateTime moment) { return moment.Year; }
                                      """;

        GeneratorHarnessResult result =
            GeneratorSources.RunLayers(GeneratorSources.ResourceWithMembers(Members, Implementation));

        GeneratorSources.AssertNoDiagnostics(result);
        GeneratorSources.AssertCompiles(result);

        string service = result.Source(GeneratorSources.ServiceHintName);
        Assert.Contains("out int value", service, StringComparison.Ordinal);
        Assert.Contains("TryGet(id, out value)", service, StringComparison.Ordinal);
        Assert.Contains("params int[] values", service, StringComparison.Ordinal);
        Assert.Contains("in global::System.DateTime moment", service, StringComparison.Ordinal);
    }

    [Fact]
    public void Emits_GenericMethodsWithTheirConstraints()
    {
        const string Members = """
                                       T Echo<T>(T value) where T : class, System.IDisposable, new();
                               """;
        const string Implementation = """
                                              public T Echo<T>(T value) where T : class, System.IDisposable, new() { return value; }
                                      """;

        GeneratorHarnessResult result =
            GeneratorSources.RunLayers(GeneratorSources.ResourceWithMembers(Members, Implementation));

        GeneratorSources.AssertNoDiagnostics(result);
        GeneratorSources.AssertCompiles(result);

        string service = result.Source(GeneratorSources.ServiceHintName);
        Assert.Contains("Echo<T>(T value)", service, StringComparison.Ordinal);
        Assert.Contains("where T : class, global::System.IDisposable, new()", service, StringComparison.Ordinal);
        Assert.Contains("dependency.Echo<T>(value)", service, StringComparison.Ordinal);
    }

    [Fact]
    public void Emits_PropertyForwarders()
    {
        const string Members = """
                                       int Count { get; }
                                       string Name { get; set; }
                               """;
        const string Implementation = """
                                              public int Count { get { return 0; } }
                                              public string Name { get; set; } = "";
                                      """;

        GeneratorHarnessResult result =
            GeneratorSources.RunLayers(GeneratorSources.ResourceWithMembers(Members, Implementation));

        GeneratorSources.AssertNoDiagnostics(result);
        GeneratorSources.AssertCompiles(result);

        string service = result.Source(GeneratorSources.ServiceHintName);
        Assert.Contains("public int Count => dependency.Count;", service, StringComparison.Ordinal);
        Assert.Contains("get => dependency.Name;", service, StringComparison.Ordinal);
        Assert.Contains("set => dependency.Name = value;", service, StringComparison.Ordinal);
    }

    [Fact]
    public void Emits_InheritedBaseInterfaceMembers()
    {
        const string Source = """
                              namespace Sample.Abstractions
                              {
                                  public interface IThingsBase { int Get(int id); }

                                  public interface IThingsClient : IThingsBase { int Extra(); }
                              }

                              namespace Sample.Services
                              {
                                  internal interface IThingsService { int Get(int id); int Extra(); }
                              }

                              namespace Sample.Repositories
                              {
                                  [GitLab.Client.SourceGenerators.GenerateClientLayers(
                                      typeof(Sample.Services.IThingsService), typeof(Sample.Abstractions.IThingsClient))]
                                  internal interface IThingsRepository { int Get(int id); int Extra(); }
                              }
                              """;

        GeneratorHarnessResult result = GeneratorSources.RunLayers(Source);

        GeneratorSources.AssertNoDiagnostics(result);
        GeneratorSources.AssertCompiles(result);

        string controller = result.Source(GeneratorSources.ControllerHintName);
        Assert.Contains("Get(int id)", controller, StringComparison.Ordinal);
        Assert.Contains("Extra()", controller, StringComparison.Ordinal);
    }

    [Fact]
    public void DerivesLayerNamespacesFromTheRepositoryNamespace()
    {
        GeneratorHarnessResult result = GeneratorSources.RunLayers(GeneratorSources.Resource());

        Assert.Contains("namespace Sample.Services", result.Source(GeneratorSources.ServiceHintName),
            StringComparison.Ordinal);
        Assert.Contains("namespace Sample.Controllers", result.Source(GeneratorSources.ControllerHintName),
            StringComparison.Ordinal);
    }

    [Fact]
    public void HonoursExplicitNamespaceOverrides()
    {
        GeneratorHarnessResult result = GeneratorSources.RunLayers(
            GeneratorSources.Resource(", ServiceNamespace = \"Custom.Svc\", ControllerNamespace = \"Custom.Ctl\""));

        Assert.Contains("namespace Custom.Svc", result.Source(GeneratorSources.ServiceHintName),
            StringComparison.Ordinal);
        Assert.Contains("namespace Custom.Ctl", result.Source(GeneratorSources.ControllerHintName),
            StringComparison.Ordinal);
    }

    [Fact]
    public void Reports_GLC0001_WhenAnAttributeArgumentIsNotAnInterface()
    {
        const string Source = """
                              namespace Sample.Abstractions
                              {
                                  public sealed class NotAnInterface { }

                                  public interface IThingsClient { int Get(int id); }
                              }

                              namespace Sample.Repositories
                              {
                                  [GitLab.Client.SourceGenerators.GenerateClientLayers(
                                      typeof(Sample.Abstractions.NotAnInterface), typeof(Sample.Abstractions.IThingsClient))]
                                  internal interface IThingsRepository { int Get(int id); }
                              }
                              """;

        GeneratorHarnessResult result = GeneratorSources.RunLayers(Source);

        GeneratorSources.AssertDiagnostic(result, "GLC0001");
        Assert.DoesNotContain(GeneratorSources.ServiceHintName, result.GeneratedSources.Keys,
            StringComparer.Ordinal);
    }

    [Fact]
    public void Reports_GLC0002_WhenTwoRepositoriesGenerateTheSameLayer()
    {
        const string Source = """
                              namespace Sample.Abstractions
                              {
                                  public interface IThingsClient { }
                              }

                              namespace Sample.Services
                              {
                                  internal interface IThingsService { }
                              }

                              namespace Sample.Repositories
                              {
                                  [GitLab.Client.SourceGenerators.GenerateClientLayers(
                                      typeof(Sample.Services.IThingsService), typeof(Sample.Abstractions.IThingsClient))]
                                  internal interface IThingsRepository { }

                                  [GitLab.Client.SourceGenerators.GenerateClientLayers(
                                      typeof(Sample.Services.IThingsService), typeof(Sample.Abstractions.IThingsClient))]
                                  internal interface IThings { }
                              }
                              """;

        GeneratorHarnessResult result = GeneratorSources.RunLayers(Source);

        GeneratorSources.AssertDiagnostic(result, "GLC0002");
    }

    [Fact]
    public void Reports_GLC0003_WhenTheForwardingTargetLacksTheMember()
    {
        const string Source = """
                              namespace Sample.Abstractions
                              {
                                  public interface IThingsClient { int Get(int id); int Missing(); }
                              }

                              namespace Sample.Services
                              {
                                  internal interface IThingsService { int Get(int id); }
                              }

                              namespace Sample.Repositories
                              {
                                  [GitLab.Client.SourceGenerators.GenerateClientLayers(
                                      typeof(Sample.Services.IThingsService), typeof(Sample.Abstractions.IThingsClient))]
                                  internal interface IThingsRepository { int Get(int id); }
                              }
                              """;

        GeneratorHarnessResult result = GeneratorSources.RunLayers(Source);

        GeneratorSources.AssertDiagnostic(result, "GLC0003");
        // The Controller is the broken layer, so nothing is emitted for this resource at all rather than
        // a file that would fail with CS1061 somewhere the developer cannot edit.
        Assert.DoesNotContain(GeneratorSources.ControllerHintName, result.GeneratedSources.Keys,
            StringComparer.Ordinal);
    }

    [Fact]
    public void Reports_GLC0003_WhenOnlyTheReturnTypeDiffers()
    {
        const string Source = """
                              namespace Sample.Abstractions
                              {
                                  public interface IThingsClient { int Get(int id); }
                              }

                              namespace Sample.Services
                              {
                                  internal interface IThingsService { long Get(int id); }
                              }

                              namespace Sample.Repositories
                              {
                                  [GitLab.Client.SourceGenerators.GenerateClientLayers(
                                      typeof(Sample.Services.IThingsService), typeof(Sample.Abstractions.IThingsClient))]
                                  internal interface IThingsRepository { long Get(int id); }
                              }
                              """;

        GeneratorHarnessResult result = GeneratorSources.RunLayers(Source);

        GeneratorSources.AssertDiagnostic(result, "GLC0003");
    }

    [Theory]
    [InlineData("        event System.Action Changed;", "event")]
    [InlineData("        int this[int index] { get; }", "indexer")]
    public void Reports_GLC0004_ForMembersThatCannotBeForwarded(string member, string expectedReason)
    {
        string source = $$"""
                          namespace Sample.Abstractions
                          {
                              public interface IThingsClient
                              {
                          {{member}}
                              }
                          }

                          namespace Sample.Services
                          {
                              internal interface IThingsService { }
                          }

                          namespace Sample.Repositories
                          {
                              [GitLab.Client.SourceGenerators.GenerateClientLayers(
                                  typeof(Sample.Services.IThingsService), typeof(Sample.Abstractions.IThingsClient))]
                              internal interface IThingsRepository { }
                          }
                          """;

        GeneratorHarnessResult result = GeneratorSources.RunLayers(source);

        GeneratorSources.AssertDiagnostic(result, "GLC0004");
        Assert.Contains(result.GeneratorDiagnostics,
            diagnostic => diagnostic.GetMessage().Contains(expectedReason, StringComparison.Ordinal));
    }

    [Fact]
    public void Reports_GLC0005_WhenTheRepositoryInterfaceIsMisnamed()
    {
        const string Source = """
                              namespace Sample.Abstractions
                              {
                                  public interface IThingsClient { int Get(int id); }
                              }

                              namespace Sample.Services
                              {
                                  internal interface IThingsService { int Get(int id); }
                              }

                              namespace Sample.Repositories
                              {
                                  [GitLab.Client.SourceGenerators.GenerateClientLayers(
                                      typeof(Sample.Services.IThingsService), typeof(Sample.Abstractions.IThingsClient))]
                                  internal interface IThingsStore { int Get(int id); }
                              }
                              """;

        GeneratorHarnessResult result = GeneratorSources.RunLayers(Source);

        GeneratorSources.AssertDiagnostic(result, "GLC0005");
    }

    [Fact]
    public void Reports_GLC0006_ForAGenericRepositoryInterface()
    {
        const string Source = """
                              namespace Sample.Abstractions
                              {
                                  public interface IThingsClient { int Get(int id); }
                              }

                              namespace Sample.Services
                              {
                                  internal interface IThingsService { int Get(int id); }
                              }

                              namespace Sample.Repositories
                              {
                                  [GitLab.Client.SourceGenerators.GenerateClientLayers(
                                      typeof(Sample.Services.IThingsService), typeof(Sample.Abstractions.IThingsClient))]
                                  internal interface IThingsRepository<T> { int Get(int id); }
                              }
                              """;

        GeneratorHarnessResult result = GeneratorSources.RunLayers(Source);

        GeneratorSources.AssertDiagnostic(result, "GLC0006");
    }

    [Fact]
    public void Reports_GLC0007_AndStillEmitsACompilableEmptyForwarder()
    {
        const string Source = """
                              namespace Sample.Abstractions
                              {
                                  public interface IThingsClient { }
                              }

                              namespace Sample.Services
                              {
                                  internal interface IThingsService { }
                              }

                              namespace Sample.Repositories
                              {
                                  [GitLab.Client.SourceGenerators.GenerateClientLayers(
                                      typeof(Sample.Services.IThingsService), typeof(Sample.Abstractions.IThingsClient))]
                                  internal interface IThingsRepository { }
                              }
                              """;

        GeneratorHarnessResult result = GeneratorSources.RunLayers(Source);

        GeneratorSources.AssertDiagnostic(result, "GLC0007");
        // CS9113 ("parameter is unread") is a core compiler warning that the auto-generated header does
        // NOT suppress, and TreatWarningsAsErrors would turn it into a break on an uneditable file.
        Assert.DoesNotContain(result.Compilation.GetDiagnostics(TestContext.Current.CancellationToken),
            diagnostic => string.Equals(diagnostic.Id, "CS9113", StringComparison.Ordinal));
        GeneratorSources.AssertCompiles(result);
    }

    [Fact]
    public void Reports_GLC0008_AndOmitsADefaultItCannotReproduce()
    {
        const string Members = """
                                       double Get(double ratio = double.NaN);
                               """;
        const string Implementation = """
                                              public double Get(double ratio = double.NaN) { return ratio; }
                                      """;

        GeneratorHarnessResult result =
            GeneratorSources.RunLayers(GeneratorSources.ResourceWithMembers(Members, Implementation));

        GeneratorSources.AssertDiagnostic(result, "GLC0008");
        Assert.Contains("double ratio)", result.Source(GeneratorSources.ServiceHintName), StringComparison.Ordinal);
    }

    [Fact]
    public void CopiesNullabilityContractAttributesOntoTheForwarder()
    {
        const string Members = """
                                       bool TryGet(int id, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out string? value);
                               """;
        const string Implementation = """
                                              public bool TryGet(int id, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out string? value)
                                              {
                                                  value = null;
                                                  return false;
                                              }
                                      """;

        GeneratorHarnessResult result =
            GeneratorSources.RunLayers(GeneratorSources.ResourceWithMembers(Members, Implementation));

        GeneratorSources.AssertNoDiagnostics(result);
        GeneratorSources.AssertCompiles(result);
        Assert.Contains("[global::System.Diagnostics.CodeAnalysis.NotNullWhenAttribute(true)]",
            result.Source(GeneratorSources.ServiceHintName), StringComparison.Ordinal);
    }

    [Fact]
    public void HintNamesAreQualifiedByTheRepositoryInterface()
    {
        GeneratorHarnessResult result = GeneratorSources.RunLayers(GeneratorSources.Resource());

        // Qualifying by the repository interface's fully qualified name makes hint-name collisions
        // impossible, which is what stops a same-named resource in two namespaces from crashing the
        // generator (CS8785) and taking every other resource's output down with it.
        Assert.Contains(GeneratorSources.ServiceHintName, result.GeneratedSources.Keys, StringComparer.Ordinal);
        Assert.Contains(GeneratorSources.ControllerHintName, result.GeneratedSources.Keys, StringComparer.Ordinal);
    }

    [Fact]
    public void GeneratedTypesCarryGeneratedCodeMetadata()
    {
        GeneratorHarnessResult result = GeneratorSources.RunLayers(GeneratorSources.Resource());
        string service = result.Source(GeneratorSources.ServiceHintName);

        Assert.Contains("[global::System.CodeDom.Compiler.GeneratedCode(", service, StringComparison.Ordinal);
        Assert.Contains("[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]", service,
            StringComparison.Ordinal);
        Assert.Contains("[global::System.Diagnostics.DebuggerNonUserCode]", service, StringComparison.Ordinal);
    }

    /// <summary>
    ///     The doc comment must come from the REPOSITORY member specifically, not from whichever interface
    ///     the forwarder happens to implement one layer down: the Service interface here carries its own,
    ///     different doc comment, and the Client interface carries none at all, yet both the generated
    ///     Service AND the generated Controller must show the Repository's text.
    /// </summary>
    [Fact]
    public void CopiesTheRepositoryMemberDocCommentOntoBothGeneratedForwarders()
    {
        const string Source = """
                              namespace Sample.Abstractions
                              {
                                  public interface IThingsClient
                                  {
                                      int Get(int id);
                                  }
                              }

                              namespace Sample.Services
                              {
                                  internal interface IThingsService
                                  {
                                      /// <summary>Service-layer doc that must NOT leak onto the forwarders.</summary>
                                      int Get(int id);
                                  }
                              }

                              namespace Sample.Repositories
                              {
                                  [GitLab.Client.SourceGenerators.GenerateClientLayers(
                                      typeof(Sample.Services.IThingsService), typeof(Sample.Abstractions.IThingsClient))]
                                  internal interface IThingsRepository
                                  {
                                      /// <summary>
                                      ///     Gets a thing by its id.
                                      /// </summary>
                                      /// <param name="id">The thing's id.</param>
                                      /// <returns>The thing.</returns>
                                      int Get(int id);
                                  }
                              }
                              """;

        GeneratorHarnessResult result = GeneratorSources.RunLayers(Source);

        GeneratorSources.AssertNoDiagnostics(result);
        GeneratorSources.AssertCompiles(result);

        string service = result.Source(GeneratorSources.ServiceHintName);
        string controller = result.Source(GeneratorSources.ControllerHintName);

        foreach (string generated in new[] { service, controller })
        {
            Assert.Contains("/// <summary>", generated, StringComparison.Ordinal);
            Assert.Contains("///     Gets a thing by its id.", generated, StringComparison.Ordinal);
            Assert.Contains("""/// <param name="id">The thing's id.</param>""", generated, StringComparison.Ordinal);
            Assert.Contains("/// <returns>The thing.</returns>", generated, StringComparison.Ordinal);
            Assert.DoesNotContain("Service-layer doc that must NOT leak", generated, StringComparison.Ordinal);

            // The doc comment must sit directly above the method it documents, correctly re-indented to
            // the generated class's own 8-space member indentation - not just present anywhere in the file.
            Assert.Contains(
                "        /// <summary>\n        ///     Gets a thing by its id.\n        /// </summary>\n",
                generated.Replace("\r\n", "\n", StringComparison.Ordinal), StringComparison.Ordinal);
        }
    }

    /// <summary>A Repository member with no doc comment forwards exactly as it does today: undocumented.</summary>
    [Fact]
    public void EmitsNoDocComment_WhenTheRepositoryMemberHasNone()
    {
        GeneratorHarnessResult result = GeneratorSources.RunLayers(GeneratorSources.Resource());

        GeneratorSources.AssertNoDiagnostics(result);
        GeneratorSources.AssertCompiles(result);

        Assert.DoesNotContain("///", result.Source(GeneratorSources.ServiceHintName), StringComparison.Ordinal);
        Assert.DoesNotContain("///", result.Source(GeneratorSources.ControllerHintName), StringComparison.Ordinal);
    }
}