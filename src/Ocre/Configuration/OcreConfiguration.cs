// <copyright file="OcreConfiguration.cs">Copyright (c) Peter Rosser. All rights reserved.</copyright>

namespace Ocre.Configuration;

using System;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

public class OcreConfiguration
{
    /// <summary>
    /// .editorconfig prefix for all OCRE settings.
    /// </summary>
    internal const string ConfigPrefix = "csharp_style_oc_";

    /// <summary>
    /// Order of member accessibility
    /// </summary>
    public AccessibilityConfig[] Accessibility { get; set; } = [];

    /// <summary>
    /// Whether unspecified values are appended
    /// </summary>
    public bool AddMissingOrderValues { get; set; } = true;

    /// <summary>
    /// Allocation modifier order
    /// </summary>
    public AllocationModifierConfig[] AllocationModifiers { get; set; } = [];

    /// <summary>
    /// Member kind ordering
    /// </summary>
    public MemberKindsConfig[] MemberKinds { get; set; } = [];

    /// <summary>
    /// Operator category order (conversion, unary, binary)
    /// </summary>
    public OperatorsConfig[] Operators { get; set; } = [];

    /// <summary>
    /// Binary operator ordering config
    /// </summary>
    public BinaryOperatorsConfig[] BinaryOperators { get; set; } = [];

    /// <summary>
    /// Unary operator ordering config
    /// </summary>
    public UnaryOperatorConfig[] UnaryOperators { get; set; } = [];

    /// <summary>
    /// Conversion operator ordering config
    /// </summary>
    public ConversionOperatorConfig[] ConversionOperators { get; set; } = [];

    /// <summary>
    /// Strategy order for type members
    /// </summary>
    public MembersConfig[] Members { get; set; } = [];

    /// <summary>
    /// Strategy order for method overrides based on parameters
    /// </summary>
    public ParameterSortKind[] Parameters { get; set; } = [];

    /// <summary>
    /// Type order (file / nested)
    /// </summary>
    public TypesConfig[] Types { get; set; } = [];

    public static OcreConfiguration Read(AnalyzerConfigOptionsProvider provider, SyntaxTree tree)
    {
        AnalyzerConfigOptions cfg = provider.GetOptions(tree);
        bool addMissing = cfg.TryGetValue(ConfigPrefix + "add_missing_order_values", out string? rawAddMissing) &&
            bool.TryParse(rawAddMissing, out bool parsedAddMissing) &&
            parsedAddMissing;

        return new OcreConfiguration
        {
            Accessibility = ReadArraySetting<AccessibilityConfig>(cfg, ConfigPrefix + "accessibility", addMissing),
            AllocationModifiers = ReadArraySetting<AllocationModifierConfig>(cfg, ConfigPrefix + "allocation_modifiers", addMissing),
            MemberKinds = ReadArraySetting<MemberKindsConfig>(cfg, ConfigPrefix + "member_kinds", addMissing),
            Operators = ReadArraySetting<OperatorsConfig>(cfg, ConfigPrefix + "operators", addMissing),
            BinaryOperators = ReadArraySetting<BinaryOperatorsConfig>(cfg, ConfigPrefix + "binary_operators", addMissing),
            UnaryOperators = ReadArraySetting<UnaryOperatorConfig>(cfg, ConfigPrefix + "unary_operators", addMissing),
            ConversionOperators = ReadArraySetting<ConversionOperatorConfig>(cfg, ConfigPrefix + "conversion_operators", addMissing),
            Members = ReadArraySetting<MembersConfig>(cfg, ConfigPrefix + "members", addMissing),
            Types = ReadArraySetting<TypesConfig>(cfg, ConfigPrefix + "types", addMissing),
        };
    }

    private static T[] ReadArraySetting<T>(AnalyzerConfigOptions cfg, string key, bool addMissing)
        where T : struct, Enum
    {
        if (cfg.TryGetValue(key, out string? raw))
        {
            T[]? specified = raw?
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(ConfigEnumHelper.Parse<T>)
                .Where(x => x.HasValue)
                .Select(x => x!.Value)
                .Concat(addMissing ? ConfigEnumHelper.GetDefaults<T>() : [])
                .Distinct()
                .ToArray();

            if (specified is { Length: > 0 })
            {
                return specified;
            }
        }

        return addMissing ? ConfigEnumHelper.GetDefaults<T>() : [];
    }
}

