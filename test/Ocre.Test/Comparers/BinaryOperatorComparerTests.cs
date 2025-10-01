// <copyright file="BinaryOperatorComparerTests.cs">Copyright (c) Peter Rosser. All rights reserved.</copyright>

namespace Ocre.Test.Comparers;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Ocre.Comparers;
using Ocre.Configuration;

using Xunit;

public class BinaryOperatorComparerTests
{
    [Fact]
    public void OrdersByOperatorThenReturnType()
    {
        var cfg = new OcreConfiguration
        {
            BinaryOperatorOrder = [BinaryOperatorTokenType.Plus, BinaryOperatorTokenType.Minus, BinaryOperatorTokenType.ReturnType, BinaryOperatorTokenType.ParamType0, BinaryOperatorTokenType.ParamType1]
        };

        var opPlus = (OperatorDeclarationSyntax)SyntaxFactory.ParseMemberDeclaration("public static int operator +(C a, C b) => 0;");
        var opMinus = (OperatorDeclarationSyntax)SyntaxFactory.ParseMemberDeclaration("public static int operator -(C a, C b) => 0;");

        var cmp = new BinaryOperatorComparer(cfg);

        Assert.True(cmp.Compare(opPlus, opMinus) < 0);

        // tie-breaker by return type
        var plusInt = (OperatorDeclarationSyntax)SyntaxFactory.ParseMemberDeclaration("public static int operator +(C a, C b) => 0;");
        var plusLong = (OperatorDeclarationSyntax)SyntaxFactory.ParseMemberDeclaration("public static long operator +(C a, C b) => 0;");

        Assert.True(cmp.Compare(plusInt, plusLong) < 0, "return type `int` should sort before `long`");
    }
}
