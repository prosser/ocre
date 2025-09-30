// <copyright file="OcreSettings.cs">Copyright (c) Peter Rosser. All rights reserved.</copyright>

namespace Ocre;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

internal enum AccessibilityOrderKind
{
    Public = 0,
    Internal,
    ProtectedInternal,
    Protected,
    Private,
}

internal enum AllocationModifierKind
{
    Const = 0,
    Static,
    Instance,
}

internal enum MemberOrder
{
    Field = 0,
    Constructor,
    Event,
    Property,
    Operator,
    Method,
    Type,
}

internal enum OperatorOrder
{
    Conversions = 0,
    Explicit,
    Unary,
    Binary,
    Arithmetic,
}

internal enum SortPriority
{
    MemberKind = 0,
    Accessibility,
    Allocation,
    Name,
}

internal enum TypeOrder
{
    Delegate = 0,
    Enum,
    Interface,
    Struct,
    Record,
    Class,
    Name,
}

internal enum BinaryOperatorOrder
{
    [SettingAlias("+")]
    Plus,
    [SettingAlias("-")]
    Minus,
    [SettingAlias("*")]
    Multiply,
    [SettingAlias("/")]
    Divide,
    [SettingAlias("%")]
    Modulus,
    [SettingAlias("&")]
    And,
    [SettingAlias("|")]
    Or,
    [SettingAlias("^")]
    Xor,
    [SettingAlias("<<")]
    LeftShift,
    [SettingAlias(">>")]
    RightShift,
    ReturnType,
    ArgType,
}

internal enum ConversionOperatorOrder
{
    Implicit,
    Explicit,
    ReturnType,
    Arg1Type,
    Arg2Type,
}

internal enum UnaryOperatorOrder
{
    [SettingAlias("+")]
    Plus,
    [SettingAlias("-")]
    Minus,
    [SettingAlias("!")]
    Negate,
    [SettingAlias("^")]
    Complement,
    [SettingAlias("++")]
    Increment,
    [SettingAlias("--")]
    Decrement,
    True,
    False,
}

internal class OcreSettings
{
    /// <summary>
    /// The order in which members are sorted according to their accessibility.
    /// </summary>
    /// <remarks>Default = public,internal,protected internal,protected,private</remarks>
    public IReadOnlyList<AccessibilityOrderKind> AccessibilityOrder { get; set; } = [];

    /// <summary>
    /// The order in which members are sorted according to their allocation modifier.
    /// "instance" = no modifier (i.e., the default).
    /// </summary>
    /// <remarks>Default = const,static,instance</remarks>
    public IReadOnlyList<AllocationModifierKind> AllocationModifierOrder { get; set; } = [];

    /// <summary>The order in which binary operators are sorted.</summary>
    /// <remarks>Default = +, -, *, /, %, &, |, ^, <<, >>, return type, arg</remarks>
    public IReadOnlyList<BinaryOperatorOrder> BinaryOperatorOrder { get; set; } = [];

    /// <summary>
    /// The order in which kinds of members are sorted.
    /// </summary>
    /// <remarks>Default = field,constructor,event,property,operator,method,type</remarks>
    public IReadOnlyList<MemberOrder> MemberOrder { get; set; } = [];

    /// <summary>The order in which operators are sorted.</summary>
    /// <remarks>Default = conversions,unary,binary</remarks>
    public IReadOnlyList<OperatorOrder> OperatorOrder { get; set; } = [];

    /// <summary>The order in which categories of sorts are applied.</summary>
    /// <remarks>Default = member kind,accessibility,allocation,name</remarks>
    public IReadOnlyList<SortPriority> SortOrder { get; set; } = [];

    /// <summary>The order in which types are sorted at the file level and when nested inside another type.</summary>
    /// Default = delegate,enum,interface,struct,record,class,name
    public IReadOnlyList<TypeOrder> TypeOrder { get; set; } = [];

    /// <summary>The order in which unary operators are sorted.</summary>
    /// <remarks>Default = +, -, !, ~, ++, --, true, false</remarks>
    public IReadOnlyList<UnaryOperatorOrder> UnaryOperatorOrder { get; set; } = [];

    public static OcreSettings Read(AnalyzerConfigOptionsProvider provider, SyntaxTree tree)
    {
        AnalyzerConfigOptions cfg = provider.GetOptions(tree);
        const string KeyPrefix = "dotnet_code_quality.ocre.";

        return new OcreSettings
        {
            AccessibilityOrder = ReadArraySetting<AccessibilityOrderKind>(cfg, $"{KeyPrefix}accessibility_order"),
            AllocationModifierOrder = ReadArraySetting<AllocationModifierKind>(cfg, $"{KeyPrefix}allocation_modifier_order"),
            MemberOrder = ReadArraySetting<MemberOrder>(cfg, $"{KeyPrefix}member_order"),
            OperatorOrder = ReadArraySetting<OperatorOrder>(cfg, $"{KeyPrefix}operator_order"),
            SortOrder = ReadArraySetting<SortPriority>(cfg, $"{KeyPrefix}sort_order"),
        };
    }

    private static T[] ReadArraySetting<T>(AnalyzerConfigOptions cfg, string key)
        where T : struct, Enum
    {
        T[]? result = null;
        if (cfg.TryGetValue(key, out string? raw))
        {
            T[]? specified = raw?
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(EnumParser<T>.Parse)
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

    private static class EnumParser<T>
        where T : struct, Enum
    {
        private static readonly Lazy<Dictionary<string, T>> Aliases = new(GetAliases, LazyThreadSafetyMode.ExecutionAndPublication);

        public static T? Parse(string value)
        {
            string normalized = value.Replace("_", "").Replace(" ", "").Trim();
            return Enum.TryParse(normalized, out T result) ||
                Aliases.Value.TryGetValue(normalized, out result)
                ? result
                : (T?)default;
        }

        public static Dictionary<string, T> GetAliases()
        {
            return ((T[])Enum.GetValues(typeof(T)))
                .Select(x => TryGetAlias(x, out string alias)
                    ? new AliasResult(x, alias)
                    : (AliasResult?)null)
                .Where(x => x is not null)
                .ToDictionary(
                    r => r!.Value.Alias,
                    r => r!.Value.Value,
                    StringComparer.OrdinalIgnoreCase);

            static bool TryGetAlias(T value, out string alias)
            {
                FieldInfo? field = typeof(T).GetField(value.ToString(), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

                if (field is null)
                {
                    alias = "";
                    return false;
                }

                SettingAliasAttribute? attr = field.GetCustomAttribute<SettingAliasAttribute>(false);
                if (attr is null || string.IsNullOrWhiteSpace(attr.Alias))
                {
                    alias = "";
                    return false;
                }

                alias = attr.Alias;
                return true;
            }
        }

        private readonly record struct AliasResult(T Value, string Alias);
    }
}

