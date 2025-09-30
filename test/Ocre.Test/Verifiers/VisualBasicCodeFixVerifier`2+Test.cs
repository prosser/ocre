// <copyright file="VisualBasicCodeFixVerifier`2+Test.cs">Copyright (c) Peter Rosser. All rights reserved.</copyright>

namespace Ocre.Test.Verifiers;

using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.VisualBasic.Testing;

public static partial class VisualBasicCodeFixVerifier<TAnalyzer, TCodeFix>
    where TAnalyzer : DiagnosticAnalyzer, new()
    where TCodeFix : CodeFixProvider, new()
{
    public class Test : VisualBasicCodeFixTest<TAnalyzer, TCodeFix, DefaultVerifier>
    {
    }
}
