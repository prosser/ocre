// <copyright file="OcreConfigurationTests.cs">Copyright (c) Peter Rosser. All rights reserved.</copyright>

namespace Ocre.Test.Configuration;

using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

using Ocre.Configuration;

using Xunit;

public class OcreConfigurationTests
{
    [Fact]
    public void Read_ParsesArraySettingsFromAnalyzerConfigOptions()
    {
        var dict = new Dictionary<string, string>
        {
            ["csharp_style_ocre_accessibility_order"] = "public,protected internal,private",
            ["csharp_style_ocre_allocation_modifier_order"] = "const,static,instance",
            ["csharp_style_ocre_member_order"] = "field,constructor,property,method",
            ["csharp_style_ocre_operator_order"] = "conversion,unary,binary",
            ["csharp_style_ocre_strategy_order"] = "member kind,accessibility,allocation,name",
            ["csharp_style_ocre_type_order"] = "class,interface,struct"
        };

        var provider = new TestAnalyzerConfigOptionsProvider(dict);
        SyntaxTree tree = CSharpSyntaxTree.ParseText("namespace N { class C { } }");

        var cfg = OcreConfiguration.Read(provider, tree);

        Assert.Equal([Accessibility.Public, Accessibility.ProtectedOrInternal, Accessibility.Private], cfg.AccessibilityOrder);
        Assert.Equal([AllocationModifierTokenType.Const, AllocationModifierTokenType.Static, AllocationModifierTokenType.Instance], cfg.AllocationModifierOrder);
        Assert.Equal([MemberKind.Field, MemberKind.Constructor, MemberKind.Property, MemberKind.Method], cfg.MemberOrder);
        Assert.Equal([OperatorKind.Conversion, OperatorKind.Unary, OperatorKind.Binary], cfg.OperatorOrder);
        Assert.Equal([SortStrategyKind.MemberKind, SortStrategyKind.Accessibility, SortStrategyKind.Allocation, SortStrategyKind.Name], cfg.SortOrder);
        Assert.Equal([TypeTokenType.Class, TypeTokenType.Interface, TypeTokenType.Struct], cfg.TypeOrder);
    }

    [Fact]
    public void Read_UsesDefaultsWhenSettingMissing()
    {
        var dict = new Dictionary<string, string>();
        var provider = new TestAnalyzerConfigOptionsProvider(dict);
        SyntaxTree tree = CSharpSyntaxTree.ParseText("class C { }");

        var cfg = OcreConfiguration.Read(provider, tree);

        // When settings missing, arrays should be populated with enum defaults (all values)
        Assert.NotEmpty(cfg.AccessibilityOrder);
        Assert.NotEmpty(cfg.AllocationModifierOrder);
        Assert.NotEmpty(cfg.MemberOrder);
        Assert.NotEmpty(cfg.OperatorOrder);
        Assert.NotEmpty(cfg.SortOrder);
        Assert.NotEmpty(cfg.TypeOrder);
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

        public override bool TryGetValue(string key, out string value)
        {
            return values.TryGetValue(key, out value);
        }
    }
}
