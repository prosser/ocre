// <copyright file="TypeOrderInFileFixTests.cs">Copyright (c) Peter Rosser. All rights reserved.</copyright>

namespace Ocre.Test.CodeFixes;

extern alias Analyzers;

using System.Threading.Tasks;

using Analyzers.Ocre;

using Microsoft.CodeAnalysis.Testing;

using Ocre.Test.Verifiers;

using Xunit;

public class TypeOrderInFileFixTests
{
    // Canonical default order (mirrors .editorconfig.defaults.txt): delegate,enum,interface,struct,record,class,name
    private static readonly string[] DefaultConfiguredOrder = ["delegate", "enum", "interface", "struct", "record", "class", "name"];
    public static TheoryData<string, string, DiagnosticResult[], string> TestCases()
    {
        TheoryData<string, string, DiagnosticResult[], string> data = new()
        {
            {
                string.Join(",", DefaultConfiguredOrder),
                """
                namespace N;

                /// <summary>Class C summary</summary>
                class C { }
            
                /// <summary>Class B summary</summary>
                class B { }

                /// <summary>Class A summary</summary>
                class A { }
                """,
                [
                    DiagnosticResult.CompilerWarning(OcreAnalyzer.Rules.TypeOrderInFileRule.Id).WithSpan(7,1,7,12).WithArguments("B", "A, B, C"),
                    DiagnosticResult.CompilerWarning(OcreAnalyzer.Rules.TypeOrderInFileRule.Id).WithSpan(10,1,10,12).WithArguments("A", "A, B, C")
                ],
                """
                namespace N;

                /// <summary>Class A summary</summary>
                class A { }

                /// <summary>Class B summary</summary>
                class B { }
                
                /// <summary>Class C summary</summary>
                class C { }
                """
            }
        };

        return data;
    }

    [Theory]
    [MemberData(nameof(TestCases))]
    public async Task TypeOrderInFile_Fix_Works(string configValue, string source, DiagnosticResult[] expected, string fixedSource)
    {
        string editorConfig = $"""
            root = true
            [*.cs]
            csharp_style_ocre_type_order = {configValue}
            """;

        CSharpCodeFixVerifier<OcreAnalyzer, SortingCodeFixProvider>.Test test = new()
        {
            TestState =
            {
                Sources = { source },
                AdditionalFiles = { ("/.editorconfig", editorConfig) },
            },
            FixedState =
            {
                Sources = { fixedSource },
                AdditionalFiles = { ("/.editorconfig", editorConfig) },
            },
        };

        test.ExpectedDiagnostics.AddRange(expected);
        await test.RunAsync();
    }
}
