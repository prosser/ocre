// <copyright file="OperatorKey.cs">Copyright (c) Peter Rosser. All rights reserved.</copyright>

namespace Ocre.Comparers;

using System;

using Microsoft.CodeAnalysis;

internal sealed class OperatorKey<T>
    where T : struct, Enum
{
    public bool HasOp { get; set; }
    public T Op { get; set; }
    public int OpIndex { get; set; }

    // Semantic symbols, populated when a SemanticModel is available
    public ITypeSymbol? ReturnTypeSymbol { get; set; }
    public ITypeSymbol?[]? ParamTypeSymbols { get; set; }

    // Cached display strings (lazy): filled either from syntax when no semantic model, or from symbols on demand
    public string ReturnTypeKey { get; set; } = string.Empty;
    public string[] ParamKeys { get; set; } = [];
}