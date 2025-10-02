// <copyright file="OperatorKindComparerTests.cs">Copyright (c) Peter Rosser. All rights reserved.</copyright>

namespace Ocre.Test.Comparers;

extern alias Analyzers;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Analyzers.Ocre.Comparers;
using Analyzers.Ocre.Configuration;

using Xunit;
using System;

public class OperatorKindComparerTests
{
    [Fact]
    public void OrdersConversionUnaryBinary()
    {
        var cfg = new OcreConfiguration
        {
            OperatorOrder = [OperatorKind.Conversion, OperatorKind.Unary, OperatorKind.Binary]
        };

        ConversionOperatorDeclarationSyntax ParseConversion(string code) =>
            SyntaxFactory.ParseMemberDeclaration(code) as ConversionOperatorDeclarationSyntax
            ?? throw new InvalidOperationException();

        OperatorDeclarationSyntax ParseOperator(string code) =>
            SyntaxFactory.ParseMemberDeclaration(code) as OperatorDeclarationSyntax
            ?? throw new InvalidOperationException();

        ConversionOperatorDeclarationSyntax conv = ParseConversion("public static implicit operator int(C c) => 0;");
        OperatorDeclarationSyntax unary = ParseOperator("public static C operator +(C a) => a;");
        OperatorDeclarationSyntax binary = ParseOperator("public static C operator +(C a, C b) => a;");

        var cmp = new OperatorKindComparer(cfg);

        Assert.True(cmp.Compare(conv, unary) < 0);
        Assert.True(cmp.Compare(unary, binary) < 0);
        Assert.True(cmp.Compare(conv, binary) < 0);
    }
}
