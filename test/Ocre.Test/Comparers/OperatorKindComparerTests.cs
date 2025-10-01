// <copyright file="OperatorKindComparerTests.cs">Copyright (c) Peter Rosser. All rights reserved.</copyright>

namespace Ocre.Test.Comparers;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Ocre.Comparers;
using Ocre.Configuration;

using Xunit;

public class OperatorKindComparerTests
{
    [Fact]
    public void OrdersConversionUnaryBinary()
    {
        var cfg = new OcreConfiguration
        {
            OperatorOrder = [OperatorKind.Conversion, OperatorKind.Unary, OperatorKind.Binary]
        };

        var conv = (ConversionOperatorDeclarationSyntax)SyntaxFactory.ParseMemberDeclaration("public static implicit operator int(C c) => 0;");
        var unary = (OperatorDeclarationSyntax)SyntaxFactory.ParseMemberDeclaration("public static C operator +(C a) => a;");
        var binary = (OperatorDeclarationSyntax)SyntaxFactory.ParseMemberDeclaration("public static C operator +(C a, C b) => a;");

        var cmp = new OperatorKindComparer(cfg);

        Assert.True(cmp.Compare(conv, unary) < 0);
        Assert.True(cmp.Compare(unary, binary) < 0);
        Assert.True(cmp.Compare(conv, binary) < 0);
    }
}
