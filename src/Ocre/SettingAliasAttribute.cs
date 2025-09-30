// <copyright file="SettingAliasAttribute.cs">Copyright (c) Peter Rosser. All rights reserved.</copyright>

namespace Ocre;

using System;

[AttributeUsage(AttributeTargets.Field)]
internal sealed class SettingAliasAttribute(string alias) : Attribute
{
    public string Alias { get; } = alias;
}

