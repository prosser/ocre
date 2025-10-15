// <copyright file="OcreConfigurationTests.cs">Copyright (c) Peter Rosser. All rights reserved.</copyright>

namespace Ocre.Test.Configuration;

extern alias Analyzers;

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using Analyzers.Ocre.Configuration;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

using Xunit;

public class OcreConfigurationTests
{
    [Fact]
    public void Read_ParsesArraySettingsFromAnalyzerConfigOptions()
    {
        OcreConfiguration expected = new()
        {
            AddMissingOrderValues = false,
            Accessibility = [AccessibilityConfig.Public, AccessibilityConfig.ProtectedOrInternal, AccessibilityConfig.Private],
            BinaryOperators = [BinaryOperatorsConfig.Plus, BinaryOperatorsConfig.Minus, BinaryOperatorsConfig.Multiply, BinaryOperatorsConfig.Divide],
            AllocationModifiers = [AllocationModifierConfig.Const, AllocationModifierConfig.Static, AllocationModifierConfig.Instance],
            ConversionOperators = [ConversionOperatorConfig.Implicit, ConversionOperatorConfig.Explicit],
            Members = [MembersConfig.MemberKinds, MembersConfig.Accessibility, MembersConfig.AllocationModifiers, MembersConfig.Name, MembersConfig.Parameters, MembersConfig.ReturnType],
            MemberKinds = [MemberKindsConfig.Field, MemberKindsConfig.Constructor, MemberKindsConfig.Property, MemberKindsConfig.Method],
            Operators = [OperatorsConfig.Conversion, OperatorsConfig.Unary, OperatorsConfig.Binary],
            Types = [TypesConfig.Class, TypesConfig.Interface, TypesConfig.Struct],
            UnaryOperators = [UnaryOperatorConfig.Plus, UnaryOperatorConfig.Minus, UnaryOperatorConfig.Negate, UnaryOperatorConfig.Complement],
        };

        Dictionary<string, string> dict = expected.ToStringDictionary();

        var provider = new TestAnalyzerConfigOptionsProvider(dict);
        SyntaxTree tree = CSharpSyntaxTree.ParseText("namespace N { class C { } }");

        var cfg = OcreConfiguration.Read(provider, tree);

        Assert.Equal([AccessibilityConfig.Public, AccessibilityConfig.ProtectedOrInternal, AccessibilityConfig.Private], cfg.Accessibility);
        Assert.Equal([AllocationModifierConfig.Const, AllocationModifierConfig.Static, AllocationModifierConfig.Instance], cfg.AllocationModifiers);
        Assert.Equal([MemberKindsConfig.Field, MemberKindsConfig.Constructor, MemberKindsConfig.Property, MemberKindsConfig.Method], cfg.MemberKinds);
        Assert.Equal([OperatorsConfig.Conversion, OperatorsConfig.Unary, OperatorsConfig.Binary], cfg.Operators);
        Assert.Equal([MembersConfig.MemberKinds, MembersConfig.Accessibility, MembersConfig.AllocationModifiers, MembersConfig.Name, MembersConfig.Parameters, MembersConfig.ReturnType], cfg.Members);
        Assert.Equal([TypesConfig.Class, TypesConfig.Interface, TypesConfig.Struct], cfg.Types);
        Assert.Equal([BinaryOperatorsConfig.Plus, BinaryOperatorsConfig.Minus, BinaryOperatorsConfig.Multiply, BinaryOperatorsConfig.Divide], cfg.BinaryOperators);
        Assert.Equal([UnaryOperatorConfig.Plus, UnaryOperatorConfig.Minus, UnaryOperatorConfig.Negate, UnaryOperatorConfig.Complement], cfg.UnaryOperators);
        Assert.Equal([ConversionOperatorConfig.Implicit, ConversionOperatorConfig.Explicit], cfg.ConversionOperators);
    }

    [Fact]
    public void Read_UsesDefaultsWhenSettingMissing()
    {
        OcreConfiguration expected = new()
        {
            AddMissingOrderValues = true,
        };

        Dictionary<string, string> dict = expected.ToStringDictionary();

        var provider = new TestAnalyzerConfigOptionsProvider(dict);
        SyntaxTree tree = CSharpSyntaxTree.ParseText("class C { }");

        var cfg = OcreConfiguration.Read(provider, tree);

        // When settings missing, arrays should be populated with enum defaults (all values)
        Assert.NotEmpty(cfg.Accessibility);
        Assert.NotEmpty(cfg.AllocationModifiers);
        Assert.NotEmpty(cfg.MemberKinds);
        Assert.NotEmpty(cfg.Operators);
        Assert.NotEmpty(cfg.Members);
        Assert.NotEmpty(cfg.Types);
        Assert.NotEmpty(cfg.BinaryOperators);
        Assert.NotEmpty(cfg.UnaryOperators);
        Assert.NotEmpty(cfg.ConversionOperators);
    }

    private sealed class TestAnalyzerConfigOptionsProvider(Dictionary<string, string> values) : AnalyzerConfigOptionsProvider
    {
        private readonly AnalyzerConfigOptions options = new TestAnalyzerConfigOptions(values);

        public override AnalyzerConfigOptions GetOptions(SyntaxTree tree) => options;
        public override AnalyzerConfigOptions GetOptions(AdditionalText textFile) => options;

        public override AnalyzerConfigOptions GlobalOptions => options;
    }

    private sealed class TestAnalyzerConfigOptions(Dictionary<string, string> values) : AnalyzerConfigOptions
    {
        private readonly Dictionary<string, string> values = values ?? [];

        public override bool TryGetValue(string key, [NotNullWhen(true)] out string? value)
        {
            return values.TryGetValue(key, out value);
        }
    }
}
