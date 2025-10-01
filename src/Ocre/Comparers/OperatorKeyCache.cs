// <copyright file="OperatorKeyCache.cs">Copyright (c) Peter Rosser. All rights reserved.</copyright>

namespace Ocre.Comparers;

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Ocre.Configuration;

internal sealed class OperatorKeyCache<TOp>(OcreConfiguration config, SemanticModel? semanticModel)
    where TOp : struct, Enum
{
    private readonly ConditionalWeakTable<OperatorDeclarationSyntax, OperatorKey<TOp>> cache = new();
    private Dictionary<TOp, int>? indexMap;

    public OcreConfiguration Config { get; } = config ?? throw new ArgumentNullException(nameof(config));
    public SemanticModel? SemanticModel { get; } = semanticModel;

    public OperatorKey<TOp> GetOrAdd(OperatorDeclarationSyntax node, Func<OcreConfiguration, TOp[]> getConfiguredOrder)
    {
        // compute index map once per cache for this operator type
        if (indexMap is null)
        {
            TOp[] arr = getConfiguredOrder(Config);
            var map = new Dictionary<TOp, int>(arr.Length);
            for (int i = 0; i < arr.Length; i++)
            {
                map[arr[i]] = i;
            }

            indexMap = map;
        }

        return GetOrAdd(node, n =>
        {
            OperatorKey<TOp> key = new();
            if (EnumParser.TryParse(n.OperatorToken.Text, out TOp op))
            {
                key.HasOp = true;
                key.Op = op;
                key.OpIndex = indexMap!.TryGetValue(op, out int idx) ? idx : -1;
            }
            else
            {
                key.HasOp = false;
                key.OpIndex = -1;
                key.Op = default;
            }

            // Initialize param keys array length; actual contents will be filled lazily
            key.ParamKeys = new string[n.ParameterList.Parameters.Count];

            // Use semantic model: store symbols, but don't allocate display strings until needed
            if (SemanticModel is not null)
            {
                TypeInfo rtInfo = SemanticModel.GetTypeInfo(n.ReturnType);
                key.ReturnTypeSymbol = rtInfo.Type;

                key.ParamTypeSymbols = new ITypeSymbol?[key.ParamKeys.Length];
                for (int i = 0; i < key.ParamKeys.Length; i++)
                {
                    key.ParamTypeSymbols[i] = n.ParameterList.Parameters[i].Type is not null ? SemanticModel.GetTypeInfo(n.ParameterList.Parameters[i].Type!).Type : null;
                }

                // leave ReturnTypeKey and ParamKeys empty for lazy population
            }
            else
            {
                // Fallback to syntactic representation immediately
                key.ReturnTypeKey = n.ReturnType?.ToString() ?? string.Empty;

                for (int i = 0; i < key.ParamKeys.Length; i++)
                {
                    key.ParamKeys[i] = n.ParameterList.Parameters[i].Type?.ToString() ?? string.Empty;
                }
            }

            return key;
        });
    }

    public OperatorKey<TOp> GetOrAdd(OperatorDeclarationSyntax node, Func<OperatorDeclarationSyntax, OperatorKey<TOp>> factory)
    {
        if (node is null)
        {
            throw new ArgumentNullException(nameof(node));
        }

        if (factory is null)
        {
            throw new ArgumentNullException(nameof(factory));
        }

        if (cache.TryGetValue(node, out OperatorKey<TOp>? existing))
        {
            return existing;
        }

        OperatorKey<TOp> key = factory(node);
        cache.Add(node, key);
        return key;
    }

    // Ensure the readable display strings are populated from symbols if necessary.
    public void EnsureDisplayStrings(OperatorKey<TOp> key)
    {
        if (key is null)
        {
            throw new ArgumentNullException(nameof(key));
        }
        // If already populated, nothing to do
        if (!string.IsNullOrEmpty(key.ReturnTypeKey) && key.ParamKeys.Length > 0 && Array.TrueForAll(key.ParamKeys, s => s != string.Empty))
        {
            return;
        }

        if (SemanticModel is null)
        {
            // Nothing to do; syntactic path should have populated strings already
            return;
        }

        if (key.ReturnTypeSymbol is not null && string.IsNullOrEmpty(key.ReturnTypeKey))
        {
            key.ReturnTypeKey = key.ReturnTypeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        }

        if (key.ParamTypeSymbols is not null)
        {
            if (key.ParamKeys is null || key.ParamKeys.Length != key.ParamTypeSymbols.Length)
            {
                key.ParamKeys = new string[key.ParamTypeSymbols.Length];
            }

            for (int i = 0; i < key.ParamTypeSymbols.Length; i++)
            {
                if (!string.IsNullOrEmpty(key.ParamKeys[i]))
                {
                    continue;
                }

                ITypeSymbol? sym = key.ParamTypeSymbols[i];
                key.ParamKeys[i] = sym is not null ? sym.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) : string.Empty;
            }
        }
    }
}
