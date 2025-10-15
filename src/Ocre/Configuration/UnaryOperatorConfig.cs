// <copyright file="UnaryOperatorTokenType.cs">Copyright (c) Peter Rosser. All rights reserved.</copyright>

namespace Ocre.Configuration;

using Ocre;

public enum UnaryOperatorConfig
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
