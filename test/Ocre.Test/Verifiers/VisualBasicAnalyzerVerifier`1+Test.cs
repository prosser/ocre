// <copyright file="VisualBasicAnalyzerVerifier`1+Test.cs">Copyright (c) Peter Rosser. All rights reserved.</copyright>

namespace Ocre.Test.Verifiers;

using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.VisualBasic.Testing;

public static partial class VisualBasicAnalyzerVerifier<TAnalyzer>
    where TAnalyzer : DiagnosticAnalyzer, new()
{
    public class Test : VisualBasicAnalyzerTest<TAnalyzer, DefaultVerifier>
    {
        public Test()
        {
        }
    }
}
