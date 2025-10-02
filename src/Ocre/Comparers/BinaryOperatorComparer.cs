// <copyright file="BinaryOperatorComparer.cs">Copyright (c) Peter Rosser. All rights reserved.</copyright>

namespace Ocre.Comparers;

using System;
using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Ocre.Configuration;

internal class BinaryOperatorComparer(OcreConfiguration config, SemanticModel? semanticModel = null) : IComparer<CSharpSyntaxNode>, IComparer<OperatorDeclarationSyntax>
{
    private readonly OperatorKeyCache<BinaryOperatorTokenType> cache = new(config, semanticModel);

    private enum SortKind
    {
        Operator,
        ReturnType,
        ParamType0,
        ParamType1,
    }

    public int Compare(CSharpSyntaxNode x, CSharpSyntaxNode y)
    {
        return Compare((OperatorDeclarationSyntax)x, (OperatorDeclarationSyntax)y);
    }

    public int Compare(OperatorDeclarationSyntax x, OperatorDeclarationSyntax y)
    {
        OperatorKey<BinaryOperatorTokenType> kx = cache.GetOrAdd(x, config => config.BinaryOperatorOrder);
        OperatorKey<BinaryOperatorTokenType> ky = cache.GetOrAdd(y, config => config.BinaryOperatorOrder);

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
        for (int i = 0; cmp == 0 && i < config.BinaryOperatorOrder.Length; i++)
        {
            BinaryOperatorTokenType cur = config.BinaryOperatorOrder[i];
            SortKind sortKind = GetSortKind(cur);
            if (sortKind == SortKind.Operator && cur != kx.Op && cur != ky.Op)
            {
                continue;
            }

            cmp = sortKind switch
            {
                SortKind.Operator => opCmp,
                SortKind.ReturnType => StringComparer.Ordinal.Compare(kx.ReturnTypeKey, ky.ReturnTypeKey),
                SortKind.ParamType0 => StringComparer.Ordinal.Compare(kx.ParamKeys[0], ky.ParamKeys[0]),
                SortKind.ParamType1 => StringComparer.Ordinal.Compare(kx.ParamKeys[1], ky.ParamKeys[1]),
                _ => 0,
            };
        }

        return cmp;
    }

    private static SortKind GetSortKind(BinaryOperatorTokenType order) => order switch
    {
        BinaryOperatorTokenType.ReturnType => SortKind.ReturnType,
        BinaryOperatorTokenType.ParamType0 => SortKind.ParamType0,
        BinaryOperatorTokenType.ParamType1 => SortKind.ParamType1,
        _ => SortKind.Operator,
    };
}
