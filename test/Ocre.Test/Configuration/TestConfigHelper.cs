// <copyright file="TestConfigHelper.cs">Copyright (c) Peter Rosser. All rights reserved.</copyright>

namespace Ocre.Test.Configuration;

extern alias Analyzers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using Analyzers.Ocre;
using Analyzers.Ocre.Configuration;

internal static class TestConfigHelper
{
    public static Dictionary<string, string> ToStringDictionary(this OcreConfiguration config)
    {
        Dictionary<string, string> dict = new()
        {
            [nameof(config.AddMissingOrderValues).ToSettingKey()] = config.AddMissingOrderValues.ToString().ToLowerInvariant(),
        };

        foreach (PropertyInfo pi in typeof(OcreConfiguration)
            .GetProperties()
            .Where(pi => pi.PropertyType.IsArray && (pi.PropertyType.GetElementType()?.IsEnum ?? false)))
        {
            Type pt = pi.PropertyType.GetElementType()!;
            Type enumerableType = typeof(IEnumerable<>).MakeGenericType(pt);
            Type[] types = [enumerableType, typeof(char)];
            MethodInfo toStringMethod = typeof(TestConfigHelper)
                .GetMethods(BindingFlags.Static | BindingFlags.Public)
                .FirstOrDefault(m => m.Name == nameof(ToSettingString)
                    && m.IsGenericMethodDefinition
                    && m.GetParameters().Length == 2
                    && m.GetParameters()[0].ParameterType.GetGenericTypeDefinition() == typeof(IEnumerable<>)
                    && m.GetParameters()[1].ParameterType == typeof(char))
                ?.MakeGenericMethod(pt)
                ?? throw new InvalidOperationException($"Could not find method {nameof(ToSettingString)}<{pt.Name}>(...)");

            var arr = pi.GetValue(config) as Array;
            if (arr is not null && arr.Length > 0)
            {
                string settingValue = (string)toStringMethod.Invoke(null, [arr, ' '])!;
                dict[pi.Name.ToSettingKey()] = settingValue;
            }
        }

        return dict;
    }

    public static string ToSettingKey(this string memberName)
    {
        return OcreConfiguration.ConfigPrefix + SplitAndRejoinWords(memberName, '_');
    }

    public static string ToSettingString<T>(this IEnumerable<T> values, char wordDelimiter = ' ')
        where T : Enum
    {
        return string.Join(',', values.Select(v => GetSettingString(v, wordDelimiter)));
    }

    private static string GetSettingString<T>(T value, char wordDelimiter = ' ')
        where T : Enum
    {
        Type type = typeof(T);

        // get first matching member
        MemberInfo? member = null;
        MemberInfo[] members = type.GetMember(value.ToString());
        if (members?.Length > 0)
        {
            member = members[0];
        }

        // get first SettingAliasAttribute
        SettingAliasAttribute? attr = null;
        if (member is not null)
        {
            object[] attrs = member.GetCustomAttributes(typeof(SettingAliasAttribute), false);
            if (attrs != null)
            {
                for (int i = 0; i < attrs.Length; i++)
                {
                    if (attrs[i] is SettingAliasAttribute a)
                    {
                        attr = a;
                        break;
                    }
                }
            }
        }

        return attr?.Alias ?? SplitAndRejoinWords(value.ToString(), wordDelimiter);
    }

    private static string SplitAndRejoinWords(string name, char wordDelimiter)
    {
        // split by pascal case, joining with spaces, and lower-casing
        if (name.Length > 1)
        {
            var sb = new StringBuilder(name.Length * 2);
            for (int i = 0; i < name.Length; i++)
            {
                char c = name[i];
                _ = i > 0 && char.IsUpper(c)
                    ? sb.Append(wordDelimiter).Append(char.ToLowerInvariant(c))
                    : sb.Append(c);
            }

            name = sb.ToString();
        }

        return name.ToLowerInvariant();
    }
}
