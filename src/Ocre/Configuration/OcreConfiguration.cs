// <copyright file="OcreConfiguration.cs">Copyright (c) Peter Rosser. All rights reserved.</copyright>

namespace Ocre.Configuration;

using System;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

internal class OcreConfiguration
{
    /// <summary>
    /// The order in which members are sorted according to their accessibility.
    /// </summary>
    /// <remarks>Default = public,internal,protected internal,protected,private protected,private</remarks>
    public Accessibility[] AccessibilityOrder { get; set; } = [];

    /// <summary>
    /// Defines how missing values in sort order settings are handled.
    /// If <see langword="true"/>, any values not explicitly specified in the setting are added to the end of the order in their default order.
    /// If <see langword="false"/>, any values not explicitly specified are treated as equal and sorted by their original order in the source file.
    /// </summary>
    public bool AddMissingOrderValues { get; set; } = true;

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

    /// <summary>The order in which operators are sorted.</summary>
    /// <remarks>Default = conversions,unary,binary</remarks>
    public OperatorKind[] OperatorOrder { get; set; } = [];

    /// <summary>The order in which strategies are applied.</summary>
    /// <remarks>Default = member kind,accessibility,allocation,name</remarks>
    public SortStrategyKind[] SortOrder { get; set; } = [];

    /// <summary>The order in which types are sorted at the file level and when nested inside another type.</summary>
    /// Default = delegate,enum,interface,struct,record,class,name
    public TypeTokenType[] TypeOrder { get; set; } = [];

    /// <summary>The order in which unary operators are sorted.</summary>
    /// <remarks>Default = +, -, !, ~, ++, --, true, false</remarks>
    public UnaryOperatorTokenType[] UnaryOperatorOrder { get; set; } = [];

    public static OcreConfiguration Read(AnalyzerConfigOptionsProvider provider, SyntaxTree tree)
    {
        AnalyzerConfigOptions cfg = provider.GetOptions(tree);
        bool addMissing = cfg.TryGetValue("csharp_style_ocre_add_missing_order_values", out string? rawAddMissing) &&
            bool.TryParse(rawAddMissing, out bool parsedAddMissing) &&
            parsedAddMissing;

        return new OcreConfiguration
        {
            AccessibilityOrder = ReadArraySetting(cfg, "accessibility_order", addMissing, TryParseAccessibility),
            AllocationModifierOrder = ReadArraySetting<AllocationModifierTokenType>(cfg, "allocation_modifier_order", addMissing),
            MemberOrder = ReadArraySetting<MemberKind>(cfg, "member_order", addMissing),
            OperatorOrder = ReadArraySetting<OperatorKind>(cfg, "operator_order", addMissing),
            SortOrder = ReadArraySetting<SortStrategyKind>(cfg, "strategy_order", addMissing),
            TypeOrder = ReadArraySetting<TypeTokenType>(cfg, "type_order", addMissing),
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

    private static T[] ReadArraySetting<T>(AnalyzerConfigOptions cfg, string key, bool addMissing, Func<string, T?>? parse = null)
        where T : struct, Enum
    {
        const string ConfigPrefix = "csharp_style_ocre_";
        parse ??= EnumParser.Parse<T>;

        var defaults = (T[])Enum.GetValues(typeof(T));

        T[]? result = null;
        if (cfg.TryGetValue($"{ConfigPrefix}{key}", out string? raw))
        {
            T[]? specified = raw?
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(parse)
                .Where(x => x.HasValue)
                .Select(x => x!.Value)
                .Concat(addMissing ? defaults : []) // add missing values at the end if configured to do so
                .Distinct() // remove duplicates, preserving order
                .ToArray();

            if (specified is { Length: > 0 })
            {
                result = specified;
            }
        }

        return result is { Length: > 0 }
            ? result
            : defaults;
    }
}

