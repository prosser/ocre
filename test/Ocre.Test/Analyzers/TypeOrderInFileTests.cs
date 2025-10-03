// <copyright file="TypeOrderInFileTests.cs">Copyright (c) Peter Rosser. All rights reserved.</copyright>

namespace Ocre.Test.Analyzers;

extern alias Analyzers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Analyzers.Ocre;
using Analyzers.Ocre.Comparers;
using Analyzers.Ocre.Configuration;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;

using Ocre.Test.Verifiers;

using Xunit;

public class TypeOrderInFileTests
{
    // Canonical default order (mirrors .editorconfig.defaults.txt): delegate,enum,interface,struct,record,class,name
    private static readonly string[] DefaultConfiguredOrder = ["delegate", "enum", "interface", "struct", "record", "class", "name"];

    public static TheoryData<string, string> InOrderCases()
    {
        var data = new TheoryData<string, string>
        {
            {
                string.Join(',', DefaultConfiguredOrder),
                """
                namespace N
                {
                    delegate void D();
                    enum E { }
                    interface I { }
                    struct S { }
                    record R();
                    class C { }
                }
                """
            },
            {
                "class,name",
                """
                namespace N
                {
                    class A { }
                    class B { }
                }
                """
            },
            {
                string.Join(',', DefaultConfiguredOrder),
                """
                namespace N;

                delegate void D();
                enum E { }
                interface I { }
                struct S { }
                record R();
                class C { }
                """
            },
            {
                "class,name",
                """
                namespace N;

                class A { }
                class B { }
                """
            }
        };
        return data;
    }

    public static TheoryData<string, string> OutOfOrderCases()
    {
        var data = new TheoryData<string, string>
        {
            {
                string.Join(',', DefaultConfiguredOrder),
                """
                namespace N
                {
                    enum E { }
                    delegate void D();
                }
                """
            },
            {
                string.Join(',', DefaultConfiguredOrder),
                """
                namespace N
                {
                    class C { }
                    record R();
                    interface I { }
                }
                """
            },
            {
                "class,name",
                """
                namespace N
                {
                    class B { }
                    class A { }
                }
                """
            }
        };
        return data;
    }

    [Theory]
    [MemberData(nameof(InOrderCases))]
    public async Task ProducesNoDiagnostics_WhenOrdered(string config, string source)
    {
        await VerifyAsync(config, source, []);
    }

    [Theory]
    [MemberData(nameof(OutOfOrderCases))]
    public async Task ProducesDiagnostics_WhenOutOfOrder(string config, string source)
    {
        // Compute expected diagnostics by mimicking analyzer logic
        (OcreConfiguration settings, CSharpSyntaxNode[] types) = ParseTypesWithConfig(config, source);
        TypeDeclarationComparer comparer = new(settings, semanticModel: null);
        string expectedOrder = string.Join(", ", settings.TypeOrder.Select(t => t.ToString().ToLowerInvariant()));

        List<DiagnosticResult> expected = [];
        for (int i = 1; i < types.Length; i++)
        {
            CSharpSyntaxNode previous = types[i - 1];
            CSharpSyntaxNode current = types[i];
            if (comparer.Compare(previous, current) > 0)
            {
                FileLinePositionSpan span = current.GetLocation().GetLineSpan();
                int sl = span.StartLinePosition.Line + 1;
                int sc = span.StartLinePosition.Character + 1;
                int el = span.EndLinePosition.Line + 1;
                int ec = span.EndLinePosition.Character + 1;

                string typeName = GetDisplayName(current);
                expected.Add(
                    CSharpAnalyzerVerifier<OcreAnalyzer>.Diagnostic(OcreAnalyzer.Rules.TypeOrderInFileRule)
                        .WithSpan(sl, sc, el, ec)
                        .WithArguments(typeName, expectedOrder));
            }
        }

        await VerifyAsync(config, source, [.. expected]);
    }

    [Fact]
    public async Task RandomizedOrderings()
    {
        var rnd = new Random(42);
        string[] identifiers = ["D", "E", "I", "S", "R", "C"];
        string config = string.Join(',', DefaultConfiguredOrder);

        for (int i = 0; i < 5; i++)
        {
            string[] shuffled = [.. identifiers.OrderBy(_ => rnd.Next())];
            string body = string.Join("\n", shuffled.Select(id => id switch
            {
                "D" => "    delegate void D();",
                "E" => "    enum E { }",
                "I" => "    interface I { }",
                "S" => "    struct S { }",
                "R" => "    record R();",
                "C" => "    class C { }",
                _ => string.Empty
            }));
            string source = "namespace N {\n" + body + "\n}";

            (OcreConfiguration settings, CSharpSyntaxNode[] types) = ParseTypesWithConfig(config, source);
            TypeDeclarationComparer comparer = new(settings, null);
            bool ordered = true;
            for (int t = 1; t < types.Length && ordered; t++)
            {
                if (comparer.Compare(types[t - 1], types[t]) > 0)
                {
                    ordered = false;
                }
            }

            if (ordered)
            {
                await VerifyAsync(config, source, []);
            }
            else
            {
                // At least one diagnostic expected; compute them precisely
                string expectedOrder = string.Join(", ", settings.TypeOrder.Select(t => t.ToString().ToLowerInvariant()));
                List<DiagnosticResult> expected = [];
                for (int t = 1; t < types.Length; t++)
                {
                    if (comparer.Compare(types[t - 1], types[t]) > 0)
                    {
                        FileLinePositionSpan span = types[t].GetLocation().GetLineSpan();
                        int sl = span.StartLinePosition.Line + 1;
                        int sc = span.StartLinePosition.Character + 1;
                        int el = span.EndLinePosition.Line + 1;
                        int ec = span.EndLinePosition.Character + 1;
                        string typeName = GetDisplayName(types[t]);
                        expected.Add(
                            CSharpAnalyzerVerifier<OcreAnalyzer>.Diagnostic(OcreAnalyzer.Rules.TypeOrderInFileRule)
                                .WithSpan(sl, sc, el, ec)
                                .WithArguments(typeName, expectedOrder));
                    }
                }

                await VerifyAsync(config, source, [.. expected]);
            }
        }
    }

    private static async Task VerifyAsync(string config, string source, DiagnosticResult[] expected)
    {
        string editorConfig = $"root = true\n[*.cs]\ncsharp_style_ocre_type_order = {config}\n";

        var test = new CSharpAnalyzerVerifier<OcreAnalyzer>.Test
        {
            TestCode = source,
        };

        test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", editorConfig));
        test.ExpectedDiagnostics.AddRange(expected);
        await test.RunAsync();
    }

    private static (OcreConfiguration settings, CSharpSyntaxNode[] types) ParseTypesWithConfig(string config, string source)
    {
        SyntaxTree tree = CSharpSyntaxTree.ParseText(source);
        var dict = new Dictionary<string, string>
        {
            ["csharp_style_ocre_type_order"] = config
        };
        AnalyzerConfigOptionsProvider provider = new LocalTestAnalyzerConfigOptionsProvider(dict);
        var settings = OcreConfiguration.Read(provider, tree);

        SyntaxNode root = tree.GetRoot();
        NamespaceDeclarationSyntax? ns = root.DescendantNodes().OfType<NamespaceDeclarationSyntax>().FirstOrDefault();
        SyntaxNode container = (SyntaxNode?)ns ?? root;

        CSharpSyntaxNode[] types = [.. container.ChildNodes()
            .Where(n => n is TypeDeclarationSyntax or EnumDeclarationSyntax or DelegateDeclarationSyntax)
            .Cast<CSharpSyntaxNode>()];

        return (settings, types);
    }

    private sealed class LocalTestAnalyzerConfigOptionsProvider(Dictionary<string, string> values) : AnalyzerConfigOptionsProvider
    {
        private readonly AnalyzerConfigOptions options = new LocalTestAnalyzerConfigOptions(values);
        public override AnalyzerConfigOptions GetOptions(SyntaxTree tree) => options;
        public override AnalyzerConfigOptions GetOptions(AdditionalText textFile) => options;
        public override AnalyzerConfigOptions GlobalOptions => options;
    }

    private sealed class LocalTestAnalyzerConfigOptions(Dictionary<string, string> values) : AnalyzerConfigOptions
    {
        private readonly Dictionary<string, string> values = values ?? [];
        public override bool TryGetValue(string key, out string value) => values.TryGetValue(key, out value!);
    }

    private static string GetDisplayName(CSharpSyntaxNode node) => node switch
    {
        ClassDeclarationSyntax c => c.Identifier.Text,
        StructDeclarationSyntax s => s.Identifier.Text,
        InterfaceDeclarationSyntax i => i.Identifier.Text,
        EnumDeclarationSyntax e => e.Identifier.Text,
        RecordDeclarationSyntax r => r.Identifier.Text,
        DelegateDeclarationSyntax d => d.Identifier.Text,
        _ => node.Kind().ToString()
    };
}
