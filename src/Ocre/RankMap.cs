// <copyright file="RankMap.cs">Copyright (c) Peter Rosser. All rights reserved.</copyright>

namespace Ocre;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Microsoft.CodeAnalysis;

internal static class RankMap
{
    public static ImmutableDictionary<TEnum, int> Build<TEnum>(IReadOnlyList<TEnum> order)
        where TEnum : struct, Enum
        => order.Select((v, i) => (v, i)).ToImmutableDictionary(p => p.v, p => p.i);
}