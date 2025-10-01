// <copyright file="OperatorKindComparer.cs">Copyright (c) Peter Rosser. All rights reserved.</copyright>

namespace Ocre.Comparers;

using System;
using System.Collections.Generic;

using Microsoft.CodeAnalysis.CSharp.Syntax;

using Ocre.Configuration;

internal class OperatorKindComparer(OcreConfiguration config) : IComparer<BaseMethodDeclarationSyntax>
{
    public int Compare(BaseMethodDeclarationSyntax x, BaseMethodDeclarationSyntax y)
    {
        static OperatorKind KindOf(BaseMethodDeclarationSyntax node)
        {
            if (node is ConversionOperatorDeclarationSyntax)
            {
                return OperatorKind.Conversion;
            }

            if (node is OperatorDeclarationSyntax op)
            {
                // unary operators have 1 parameter, binary have 2
                int paramCount = op.ParameterList?.Parameters.Count ?? 0;
                return paramCount == 1 ? OperatorKind.Unary : OperatorKind.Binary;
            }

            // Fallback: treat unknown member as binary (or choose a default)
            return OperatorKind.Binary;
        }

        OperatorKind kx = KindOf(x);
        OperatorKind ky = KindOf(y);

        if (kx == ky)
        {
            return 0;
        }

        int ix = Array.IndexOf(config.OperatorOrder, kx);
        int iy = Array.IndexOf(config.OperatorOrder, ky);

        return ix == -1 ? 1 : iy == -1 ? -1 : ix.CompareTo(iy);
    }
}
