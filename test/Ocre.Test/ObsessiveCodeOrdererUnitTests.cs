// <copyright file="ObsessiveCodeOrdererUnitTests.cs">Copyright (c) Peter Rosser. All rights reserved.</copyright>

namespace Ocre.Test;

using System.Threading.Tasks;

using Xunit;

using VerifyCS = Ocre.Test.Verifiers.CSharpCodeFixVerifier<OcreAnalyzer, ObsessiveCodeOrdererCodeFixProvider>;

public class ObsessiveCodeOrdererUnitTest
{
    //No diagnostics expected to show up
    [Fact]
    public async Task TestMethod1()
    {
        string test = @"";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    //Diagnostic and CodeFix both triggered and checked for
    [Fact]
    public async Task TestMethod2()
    {
        string test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class {|#0:TypeName|}
        {   
        }
    }";

        string fixtest = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class TYPENAME
        {   
        }
    }";

        Microsoft.CodeAnalysis.Testing.DiagnosticResult expected = VerifyCS.Diagnostic("OCRE1000").WithLocation(0).WithArguments("TypeName");
        await VerifyCS.VerifyCodeFixAsync(test, expected, fixtest);
    }
}
