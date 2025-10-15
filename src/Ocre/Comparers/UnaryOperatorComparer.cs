// <copyright file="UnaryOperatorComparer.cs">Copyright (c) Peter Rosser. All rights reserved.</copyright>

namespace Ocre.Comparers;
using System;
using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Ocre.Configuration;

internal class UnaryOperatorComparer(OcreConfiguration config, SemanticModel? semanticModel = null) : IComparer<OperatorDeclarationSyntax>
{
    private readonly OperatorKeyCache<UnaryOperatorConfig> cache = new(config, semanticModel);

    private enum SortKind
    {
        Operator,
        ReturnType,
        ParamType0,
    }

    public int Compare(OperatorDeclarationSyntax x, OperatorDeclarationSyntax y)
    {
        OperatorKey<UnaryOperatorConfig> kx = cache.GetOrAdd(x, config => config.UnaryOperators);
        OperatorKey<UnaryOperatorConfig> ky = cache.GetOrAdd(y, config => config.UnaryOperators);

        // Handle missing operator parse symmetrically
        if (!kx.HasOp)
        {
            return ky.HasOp ? 1 : 0;
        }

        if (!ky.HasOp)
        {
            return -1;
        }

        // Handle missing config indices symmetrically
        if (kx.OpIndex == -1 && ky.OpIndex == -1)
        {
            return 0;
        }
        else if (kx.OpIndex == -1)
        {
            return 1;
        }
        else if (ky.OpIndex == -1)
        {
            return -1;
        }

        int opCmp = kx.OpIndex.CompareTo(ky.OpIndex);

        // Compare according to configured priority, using cached semantic keys
        int cmp = 0;
        OcreConfiguration config = cache.Config;
        for (int i = 0; cmp == 0 && i < config.UnaryOperators.Length; i++)
        {
            UnaryOperatorConfig cur = config.UnaryOperators[i];
            SortKind sortKind = GetSortKind(cur);
            if (sortKind == SortKind.Operator && cur != kx.Op && cur != ky.Op)
            {
                continue;
            }

            cmp = GetSortKind(cur) switch
            {
                SortKind.Operator => opCmp,
                SortKind.ReturnType => StringComparer.Ordinal.Compare(kx.ReturnTypeKey, ky.ReturnTypeKey),
                SortKind.ParamType0 => StringComparer.Ordinal.Compare(kx.ParamKeys[0], ky.ParamKeys[0]),
                _ => 0,
            };
        }

        return cmp;
    }

    private static SortKind GetSortKind(UnaryOperatorConfig order) => order switch
    {
        UnaryOperatorConfig.Plus => SortKind.Operator,
        UnaryOperatorConfig.Minus => SortKind.Operator,
        UnaryOperatorConfig.Negate => SortKind.Operator,
        UnaryOperatorConfig.Complement => SortKind.Operator,
        UnaryOperatorConfig.Increment => SortKind.Operator,
        UnaryOperatorConfig.Decrement => SortKind.Operator,
        UnaryOperatorConfig.True => SortKind.Operator,
        UnaryOperatorConfig.False => SortKind.Operator,
        _ => SortKind.Operator,
    };
}
