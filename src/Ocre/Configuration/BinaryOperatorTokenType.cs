// <copyright file="BinaryOperatorTokenType.cs">Copyright (c) Peter Rosser. All rights reserved.</copyright>

namespace Ocre.Configuration;

using Ocre;

public enum BinaryOperatorTokenType
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
    ParamType0,
    ParamType1,
}

