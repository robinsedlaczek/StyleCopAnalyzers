﻿namespace StyleCop.Analyzers.Test.LayoutRules
{
    using System.Threading;
    using System.Threading.Tasks;

    using StyleCop.Analyzers.LayoutRules;
    using TestHelper;
    using Xunit;

    /// <summary>
    /// Unit tests for <see cref="SA1500CurlyBracketsForMultiLineStatementsMustNotShareLine"/>.
    /// </summary>
    public partial class SA1500UnitTests : DiagnosticVerifier
    {
        /// <summary>
        /// Verifies that no diagnostics are reported for the valid enums defined in this test.
        /// </summary>
        /// <remarks>
        /// These are valid for SA1500 only, some will report other diagnostics from the layout (SA15xx) series.
        /// </remarks>
        [Fact]
        public async Task TestEnumValid()
        {
            var testCode = @"public class Foo
{
    public enum ValidEnum1
    {
    }

    public enum ValidEnum2
    {
        Test
    }

    public enum ValidEnum3 { } /* Valid only for SA1500 */

    public enum ValidEnum4 { Test }  /* Valid only for SA1500 */
}";

            await this.VerifyCSharpDiagnosticAsync(testCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
        }

        /// <summary>
        /// Verifies that diagnostics will be reported for all invalid enum definitions.
        /// </summary>
        [Fact]
        public async Task TestEnumInvalid()
        {
            var testCode = @"public class Foo
{
    public enum InvalidEnum1 {
    }

    public enum InvalidEnum2 {
        Test 
    }

    public enum InvalidEnum3 {
        Test }

    public enum InvalidEnum4 { Test 
    }

    public enum InvalidEnum5
    { 
        Test }

    public enum InvalidEnum6
    { Test 
    }

    public enum InvalidEnum7
    { Test }
}";

            var expectedDiagnostics = new[]
            {
                // InvalidEnum1
                this.CSharpDiagnostic().WithLocation(3, 30),
                // InvalidEnum2
                this.CSharpDiagnostic().WithLocation(6, 30),
                // InvalidEnum3
                this.CSharpDiagnostic().WithLocation(10, 30),
                this.CSharpDiagnostic().WithLocation(11, 14),
                // InvalidEnum4
                this.CSharpDiagnostic().WithLocation(13, 30),
                // InvalidEnum5
                this.CSharpDiagnostic().WithLocation(18, 14),
                // InvalidEnum6
                this.CSharpDiagnostic().WithLocation(21, 5),
                // InvalidEnum7
                this.CSharpDiagnostic().WithLocation(25, 5),
                this.CSharpDiagnostic().WithLocation(25, 12)
            };

            await this.VerifyCSharpDiagnosticAsync(testCode, expectedDiagnostics, CancellationToken.None).ConfigureAwait(false);
        }
    }
}