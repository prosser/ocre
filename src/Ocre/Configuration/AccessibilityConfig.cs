// <copyright file="UnaryOperatorTokenType.cs">Copyright (c) Peter Rosser. All rights reserved.</copyright>

namespace Ocre.Configuration;

using Ocre;

public enum AccessibilityConfig
{
    // DO NOT CHANGE ORDER OF THESE ENUM VALUES
    [SettingAlias("private")]
    Private = 1,

    /// <summary>
    /// Only accessible where both protected and internal members are accessible
    /// (more restrictive than <see cref="Protected"/>, <see cref="Internal"/> and <see cref="ProtectedOrInternal"/>).
    /// </summary>
    [SettingAlias("private internal")]
    ProtectedAndInternal = 2,

    /// <summary>
    /// Only accessible where both protected and friend members are accessible
    /// (more restrictive than <see cref="Protected"/>, <see cref="Friend"/> and <see cref="ProtectedOrFriend"/>).
    /// </summary>
    ProtectedAndFriend = ProtectedAndInternal,

    [SettingAlias("protected")]
    Protected = 3,

    [SettingAlias("internal")]
    Internal = 4,
    Friend = Internal,

    /// <summary>
    /// Accessible wherever either protected or internal members are accessible
    /// (less restrictive than <see cref="Protected"/>, <see cref="Internal"/> and <see cref="ProtectedAndInternal"/>).
    /// </summary>
    [SettingAlias("protected internal")]
    ProtectedOrInternal = 5,

    /// <summary>
    /// Accessible wherever either protected or friend members are accessible
    /// (less restrictive than <see cref="Protected"/>, <see cref="Friend"/> and <see cref="ProtectedAndFriend"/>).
    /// </summary>
    ProtectedOrFriend = ProtectedOrInternal,

    [SettingAlias("public")]
    Public = 6
}