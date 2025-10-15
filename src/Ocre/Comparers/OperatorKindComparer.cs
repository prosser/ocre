// <copyright file="OperatorKindComparer.cs">Copyright (c) Peter Rosser. All rights reserved.</copyright>

namespace Ocre.Comparers;

using System;
using System.Collections.Generic;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Ocre.Configuration;

internal class OperatorKindComparer(OcreConfiguration config) : IComparer<CSharpSyntaxNode>, IComparer<BaseMethodDeclarationSyntax>
{
    public int Compare(CSharpSyntaxNode x, CSharpSyntaxNode y)
    {
        if (x is BaseMethodDeclarationSyntax xMethod && y is BaseMethodDeclarationSyntax yMethod)
        {
            return Compare(xMethod, yMethod);
        }

        // If either node is not a method declaration, consider them ordered equally
        return 0;
    }

    public int Compare(BaseMethodDeclarationSyntax x, BaseMethodDeclarationSyntax y)
    {
        static OperatorsConfig KindOf(BaseMethodDeclarationSyntax node)
        {
            if (node is ConversionOperatorDeclarationSyntax)
            {
                return OperatorsConfig.Conversion;
            }

            if (node is OperatorDeclarationSyntax op)
            {
                // unary operators have 1 parameter, binary have 2
                int paramCount = op.ParameterList?.Parameters.Count ?? 0;
                return paramCount == 1 ? OperatorsConfig.Unary : OperatorsConfig.Binary;
            }

            // Fallback: treat unknown member as binary (or choose a default)
            return OperatorsConfig.Binary;
        }

        OperatorsConfig kx = KindOf(x);
        OperatorsConfig ky = KindOf(y);

        if (kx == ky)
        {
            return 0;
        }

        int ix = Array.IndexOf(config.Operators, kx);
        int iy = Array.IndexOf(config.Operators, ky);

        return ix == -1 ? 1 : iy == -1 ? -1 : ix.CompareTo(iy);
    }
}
