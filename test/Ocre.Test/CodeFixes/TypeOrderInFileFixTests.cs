// <copyright file="TypeOrderInFileFixTests.cs">Copyright (c) Peter Rosser. All rights reserved.</copyright>

namespace Ocre.Test.CodeFixes;

extern alias Analyzers;

using System.Linq;
using System.Threading.Tasks;

using Analyzers.Ocre;
using Analyzers.Ocre.Configuration;

using Microsoft.CodeAnalysis.Testing;

using Ocre.Test.Configuration;
using Ocre.Test.Verifiers;

using Xunit;
using Xunit.Abstractions;

public class TypeOrderInFileFixTests
{
    // Canonical default order (mirrors .editorconfig.defaults.txt): delegate,enum,interface,struct,record,class,name
    private static readonly string DefaultConfiguredOrder = ConfigEnumHelper.GetDefaults<TypesConfig>().ToSettingString();
    public static TheoryData<string, string, Expected[], string> TestCases()
    {
        TheoryData<string, string, Expected[], string> data = new()
        {
            {
                $"{nameof(OcreConfiguration.Types).ToSettingKey()} = {DefaultConfiguredOrder}",
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
                    new(OcreAnalyzer.Rules.TypeOrderInFileRule.Id, 7, 1, 7, 12, "B", "A, B, C"),
                    new(OcreAnalyzer.Rules.TypeOrderInFileRule.Id, 10, 1, 10, 12,"A", "A, B, C"),
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
            },
            {
                $"{nameof(OcreConfiguration.Types).ToSettingKey()} = {DefaultConfiguredOrder}",
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
                    new(OcreAnalyzer.Rules.TypeOrderInFileRule.Id, 7, 1, 7, 12, "B", "A, B, C"),
                    new(OcreAnalyzer.Rules.TypeOrderInFileRule.Id, 10, 1, 10, 12,"A", "A, B, C"),
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
    public async Task TypeOrderInFile_Fix_Works(string configLines, string source, Expected[] expected, string fixedSource)
    {
        string editorConfig = $"""
            root = true
            [*.cs]
            {configLines}
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

        test.ExpectedDiagnostics.AddRange(expected.Select(x => (DiagnosticResult)x));
        await test.RunAsync();
    }

    public record Expected : IXunitSerializable
    {
        public Expected()
        {
            Id = string.Empty;
            Arguments = [];
        }

        public Expected(string id, int startLine, int startColumn, int endLine, int endColumn, params string[] arguments)
            : this()
        {
            Id = id;
            StartLine = startLine;
            StartColumn = startColumn;
            EndLine = endLine;
            EndColumn = endColumn;
            Arguments = arguments ?? [];
        }

        public string Id { get; set; }
        public int StartLine { get; set; }
        public int StartColumn { get; set; }
        public int EndLine { get; set; }
        public int EndColumn { get; set; }
        public string[] Arguments { get; set; }

        public void Serialize(IXunitSerializationInfo info)
        {
            info.AddValue(nameof(Id), Id);
            info.AddValue(nameof(StartLine), StartLine);
            info.AddValue(nameof(StartColumn), StartColumn);
            info.AddValue(nameof(EndLine), EndLine);
            info.AddValue(nameof(EndColumn), EndColumn);
            info.AddValue(nameof(Arguments), Arguments);
        }

        public void Deserialize(IXunitSerializationInfo info)
        {
            Id = info.GetValue<string>(nameof(Id));
            StartLine = info.GetValue<int>(nameof(StartLine));
            StartColumn = info.GetValue<int>(nameof(StartColumn));
            EndLine = info.GetValue<int>(nameof(EndLine));
            EndColumn = info.GetValue<int>(nameof(EndColumn));
            Arguments = info.GetValue<string[]>(nameof(Arguments)) ?? [];
        }

        public static explicit operator DiagnosticResult(Expected expected)
            => DiagnosticResult.CompilerWarning(expected.Id)
                .WithSpan(expected.StartLine, expected.StartColumn, expected.EndLine, expected.EndColumn)
                .WithArguments(expected.Arguments);
    }
}
