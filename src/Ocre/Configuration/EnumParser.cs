// <copyright file="EnumParser.cs">Copyright (c) Peter Rosser. All rights reserved.</copyright>

namespace Ocre.Configuration;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

using Microsoft.CodeAnalysis;

using Ocre;

internal static class EnumParser
{
    public static T? Parse<T>(string value)
        where T : struct, Enum
        => Typed<T>.Parse(value);
    public static bool TryParse<T>(string value, out T result)
        where T : struct, Enum
        => Typed<T>.TryParse(value, out result);

    private static class Typed<T>
        where T : struct, Enum
    {
        private static readonly Lazy<Dictionary<string, T>> Aliases = new(GetAliases, LazyThreadSafetyMode.ExecutionAndPublication);

        public static T? Parse(string value)
        {
            string normalized = value.Replace("_", "").Replace(" ", "").Trim();
            return Enum.TryParse(normalized, true, out T result) ||
                Aliases.Value.TryGetValue(normalized, out result)
                ? result
                : (T?)default;
        }

        public static bool TryParse(string value, out T result)
        {
            T? parsed = Parse(value);
            if (parsed.HasValue)
            {
                result = parsed.Value;
                return true;
            }

            result = default!;
            return false;
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
