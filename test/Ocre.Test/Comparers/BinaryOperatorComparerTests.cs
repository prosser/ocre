// <copyright file="BinaryOperatorComparerTests.cs">Copyright (c) Peter Rosser. All rights reserved.</copyright>

namespace Ocre.Test.Comparers;

extern alias Analyzers;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Analyzers.Ocre.Comparers;
using Analyzers.Ocre.Configuration;

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

        var opPlus = SyntaxFactory.ParseMemberDeclaration("public static int operator +(C a, C b) => 0;") as OperatorDeclarationSyntax;
        var opMinus = SyntaxFactory.ParseMemberDeclaration("public static int operator -(C a, C b) => 0;") as OperatorDeclarationSyntax;

        var cmp = new BinaryOperatorComparer(cfg);

        Assert.True(cmp.Compare(opPlus!, opMinus!) < 0);

        // tie-breaker by return type
        var plusInt = SyntaxFactory.ParseMemberDeclaration("public static int operator +(C a, C b) => 0;") as OperatorDeclarationSyntax;
        var plusLong = SyntaxFactory.ParseMemberDeclaration("public static long operator +(C a, C b) => 0;") as OperatorDeclarationSyntax;

        Assert.True(cmp.Compare(plusInt!, plusLong!) < 0, "return type `int` should sort before `long`");
    }
}
