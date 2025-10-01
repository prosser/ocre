// <copyright file="UnaryOperatorComparer.cs">Copyright (c) Peter Rosser. All rights reserved.</copyright>

namespace Ocre.Comparers;
using System;
using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Ocre.Configuration;

internal class UnaryOperatorComparer(OcreConfiguration config, SemanticModel? semanticModel = null) : IComparer<OperatorDeclarationSyntax>
{
    private readonly OperatorKeyCache<UnaryOperatorTokenType> cache = new(config, semanticModel);

    private enum SortKind
    {
        Operator,
        ReturnType,
        ParamType0,
    }

    public int Compare(OperatorDeclarationSyntax x, OperatorDeclarationSyntax y)
    {
        OperatorKey<UnaryOperatorTokenType> kx = cache.GetOrAdd(x, config => config.UnaryOperatorOrder);
        OperatorKey<UnaryOperatorTokenType> ky = cache.GetOrAdd(y, config => config.UnaryOperatorOrder);

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
        for (int i = 0; cmp == 0 && i < config.UnaryOperatorOrder.Length; i++)
        {
            UnaryOperatorTokenType cur = config.UnaryOperatorOrder[i];
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

    private static SortKind GetSortKind(UnaryOperatorTokenType order) => order switch
    {
        UnaryOperatorTokenType.Plus => SortKind.Operator,
        UnaryOperatorTokenType.Minus => SortKind.Operator,
        UnaryOperatorTokenType.Negate => SortKind.Operator,
        UnaryOperatorTokenType.Complement => SortKind.Operator,
        UnaryOperatorTokenType.Increment => SortKind.Operator,
        UnaryOperatorTokenType.Decrement => SortKind.Operator,
        UnaryOperatorTokenType.True => SortKind.Operator,
        UnaryOperatorTokenType.False => SortKind.Operator,
        _ => SortKind.Operator,
    };
}
