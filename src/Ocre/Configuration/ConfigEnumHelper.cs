// <copyright file="ConfigEnumHelper.cs">Copyright (c) Peter Rosser. All rights reserved.</copyright>

namespace Ocre.Configuration;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

using Ocre;

internal static class ConfigEnumHelper
{
    public static T[] ParseMany<T>(string value, char delimiter = ',')
        where T : struct, Enum
    {
        string[] parts = value.Split([delimiter], StringSplitOptions.RemoveEmptyEntries);
        return Array.ConvertAll(parts, part => Parse<T>(part) ?? throw new ArgumentException($"Invalid {typeof(T).Name} value '{part}'"));
    }

    public static T[] GetDefaults<T>()
        where T : struct, Enum
    {
        return [.. ((T[])Enum.GetValues(typeof(T))).Distinct()];
    }

    public static T? Parse<T>(string value)
        where T : struct, Enum
        => Typed<T>.Parse(value);
    public static bool TryParse<T>(string value, out T result)
        where T : struct, Enum
        => Typed<T>.TryParse(value, out result);

    private static class Typed<T>
        where T : struct, Enum
    {
        private static readonly Lazy<Dictionary<string, T>> Cache = new(CreateCache, LazyThreadSafetyMode.ExecutionAndPublication);

        public static T? Parse(string value)
        {
            return Cache.Value.TryGetValue(value, out T result)
                ? result
                : null;
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

        private static Dictionary<string, T> CreateCache()
        {
            // Build the map by inspecting enum fields once. Avoid repeated GetField(value.ToString()) calls.
            Dictionary<string, T> dict = new(CaseInsensitiveOrdinalComparer.Instance);

            FieldInfo[] fields = typeof(T).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

            foreach (FieldInfo field in fields)
            {
                // Only consider the named enum members
                if (!field.IsLiteral)
                {
                    continue;
                }

                object? raw = field.GetValue(null);
                if (raw is null)
                {
                    continue;
                }

                var value = (T)raw;

                // Try alias attribute first
                SettingAliasAttribute? attr = field.GetCustomAttribute<SettingAliasAttribute>(false);
                if (attr is not null && !string.IsNullOrWhiteSpace(attr.Alias))
                {
                    string alias = attr.Alias;
                    if (!dict.ContainsKey(alias))
                    {
                        dict[alias] = value;
                    }
                }

                // Always ensure name is present (fallback). Check duplicates as before.
                string name = field.Name;
                if (dict.TryGetValue(name, out T existing) && !existing.Equals(value))
                {
                    throw new InvalidOperationException($"Duplicate enum name '{name}' in enum {typeof(T).FullName}");
                }

                dict[name] = value;
            }

            return dict;
        }

        private readonly record struct AliasResult(T Value, string Alias);
    }
}
