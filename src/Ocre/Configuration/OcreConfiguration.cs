// <copyright file="OcreSettings.cs">Copyright (c) Peter Rosser. All rights reserved.</copyright>

namespace Ocre.Configuration;

using System;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using Ocre;

internal class OcreConfiguration
{
    /// <summary>
    /// The order in which members are sorted according to their accessibility.
    /// </summary>
    /// <remarks>Default = public,internal,protected internal,protected,private protected,private</remarks>
    public Accessibility[] AccessibilityOrder { get; set; } = [];

    /// <summary>
    /// The order in which members are sorted according to their allocation modifier.
    /// "instance" = no modifier (i.e., the default).
    /// </summary>
    /// <remarks>Default = const,static,instance</remarks>
    public AllocationModifierTokenType[] AllocationModifierOrder { get; set; } = [];

    /// <summary>The order in which binary operators are sorted.</summary>
    /// <remarks>Default = +, -, *, /, %, &, |, ^, <<, >>, return type, arg</remarks>
    public BinaryOperatorTokenType[] BinaryOperatorOrder { get; set; } = [];

    /// <summary>
    /// The order in which kinds of members are sorted.
    /// </summary>
    /// <remarks>Default = field,constructor,event,property,operator,method,type</remarks>
    public MemberKind[] MemberOrder { get; set; } = [];

    /// <summary>The severity of diagnostics raised for ordering violations of nested types.</summary>
    public DiagnosticSeverity NestedTypeOrderSeverity { get; set; } = DiagnosticSeverity.Warning;

    /// <summary>The order in which operators are sorted.</summary>
    /// <remarks>Default = conversions,unary,binary</remarks>
    public OperatorKind[] OperatorOrder { get; set; } = [];

    /// <summary>The order in which strategies are applied.</summary>
    /// <remarks>Default = member kind,accessibility,allocation,name</remarks>
    public SortStrategyKind[] SortOrder { get; set; } = [];

    /// <summary>The order in which types are sorted at the file level and when nested inside another type.</summary>
    /// Default = delegate,enum,interface,struct,record,class,name
    public TypeTokenType[] TypeOrder { get; set; } = [];

    /// <summary>The severity of diagnostics raised for ordering violations of types in a file.</summary>
    public DiagnosticSeverity TypeOrderInFileSeverity { get; set; } = DiagnosticSeverity.Warning;

    /// <summary>The severity of diagnostics raised for ordering violations of members within a type.</summary>
    public DiagnosticSeverity MemberOrderSeverity { get; set; } = DiagnosticSeverity.Warning;

    /// <summary>The severity of diagnostics raised for ordering violations of operator overloads within a type.</summary>
    public DiagnosticSeverity OperatorOrderSeverity { get; set; } = DiagnosticSeverity.Warning;

    /// <summary>The order in which unary operators are sorted.</summary>
    /// <remarks>Default = +, -, !, ~, ++, --, true, false</remarks>
    public UnaryOperatorTokenType[] UnaryOperatorOrder { get; set; } = [];

    public static OcreConfiguration Read(AnalyzerConfigOptionsProvider provider, SyntaxTree tree)
    {
        AnalyzerConfigOptions cfg = provider.GetOptions(tree);

        return new OcreConfiguration
        {
            AccessibilityOrder = ReadArraySetting<Accessibility>(cfg, "accessibility_order", TryParseAccessibility),
            AllocationModifierOrder = ReadArraySetting<AllocationModifierTokenType>(cfg, "allocation_modifier_order"),
            MemberOrder = ReadArraySetting<MemberKind>(cfg, "member_order"),
            OperatorOrder = ReadArraySetting<OperatorKind>(cfg, "operator_order"),
            SortOrder = ReadArraySetting<SortStrategyKind>(cfg, "strategy_order"),
            TypeOrder = ReadArraySetting<TypeTokenType>(cfg, "type_order"),

            TypeOrderInFileSeverity = TryReadSeveritySetting(cfg, OcreAnalyzer.Rules.TypeOrderInFileRule.Id),
            NestedTypeOrderSeverity = TryReadSeveritySetting(cfg, OcreAnalyzer.Rules.NestedTypeOrderRule.Id),
            MemberOrderSeverity = TryReadSeveritySetting(cfg, OcreAnalyzer.Rules.MemberOrderRule.Id),
            OperatorOrderSeverity = TryReadSeveritySetting(cfg, OcreAnalyzer.Rules.OperatorOrderRule.Id),
        };
    }

    private static Accessibility? TryParseAccessibility(string s)
    {
        return s.Replace(" ", "").ToLowerInvariant() switch
        {
            "public" => Accessibility.Public,
            "internal" => Accessibility.Internal,
            "protectedinternal" => Accessibility.ProtectedOrInternal,
            "protected" => Accessibility.Protected,
            "privateprotected" => Accessibility.ProtectedAndInternal,
            "private" => Accessibility.Private,
            _ => default,
        };
    }

    private static DiagnosticSeverity TryReadSeveritySetting(AnalyzerConfigOptions cfg, string diagnosticId)
    {
        const string SeverityPrefix = "dotnet_diagnostic.";
        const string SeveritySuffix = ".severity";

        if (cfg.TryGetValue($"{SeverityPrefix}{diagnosticId.ToLowerInvariant()}{SeveritySuffix}", out string? raw) &&
            Enum.TryParse(raw, true, out DiagnosticSeverity severity))
        {
            return severity;
        }

        return DiagnosticSeverity.Warning;
    }

    private static T[] ReadArraySetting<T>(AnalyzerConfigOptions cfg, string key, Func<string, T?>? parse = null)
        where T : struct, Enum
    {
        const string ConfigPrefix = "csharp_style_ocre_";
        parse ??= EnumParser.Parse<T>;

        T[]? result = null;
        if (cfg.TryGetValue($"{ConfigPrefix}{key}", out string? raw))
        {
            T[]? specified = raw?
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(parse)
                .Distinct()
                .Where(x => x.HasValue)
                .Select(x => x!.Value)
                .ToArray();

            if (specified is { Length: > 0 })
            {
                result = specified;
            }
        }

        return result is { Length: > 0 }
            ? result
            : (T[])Enum.GetValues(typeof(T));
    }
}

