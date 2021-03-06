﻿namespace StyleCop.Analyzers.SpacingRules
{
    using System.Collections.Immutable;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.CodeAnalysis.Text;

    /// <summary>
    /// Discovers any C# lines of code with trailing whitespace.
    /// </summary>
    /// <remarks>
    /// <para>A violation of this rule occurs whenever the code contains whitespace at the end of the line.</para>
    ///
    /// <para>Trailing whitespace causes unnecessary diffs in source control,
    /// looks tacky in editors that show invisible whitespace as visible characters,
    /// and is highlighted as an error in some configurations of git.</para>
    ///
    /// <para>For these reasons, trailing whitespace should be avoided.</para>
    /// </remarks>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SA1028CodeMustNotContainTrailingWhitespace : DiagnosticAnalyzer
    {
        /// <summary>
        /// The ID for diagnostics produced by the <see cref="SA1028CodeMustNotContainTrailingWhitespace"/> analyzer.
        /// </summary>
        public const string DiagnosticId = "SA1028";
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(SpacingResources.SA1028Title), SpacingResources.ResourceManager, typeof(SpacingResources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(SpacingResources.SA1028MessageFormat), SpacingResources.ResourceManager, typeof(SpacingResources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(SpacingResources.SA1028Description), SpacingResources.ResourceManager, typeof(SpacingResources));
        private static readonly string HelpLink = "https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1028.md";

        private static readonly DiagnosticDescriptor Descriptor =
            new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, AnalyzerCategory.SpacingRules, DiagnosticSeverity.Warning, AnalyzerConstants.EnabledByDefault, Description, HelpLink, WellKnownDiagnosticTags.Unnecessary);

        private static readonly ImmutableArray<DiagnosticDescriptor> SupportedDiagnosticsValue =
            ImmutableArray.Create(Descriptor);

        /// <inheritdoc />
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => SupportedDiagnosticsValue;

        /// <inheritdoc />
        public override void Initialize(AnalysisContext context)
        {
            context.RegisterCompilationStartAction(HandleCompilationStart);
        }

        private static void HandleCompilationStart(CompilationStartAnalysisContext context)
        {
            context.RegisterSyntaxTreeActionHonorExclusions(HandleSyntaxTree);
        }

        /// <summary>
        /// Scans an entire document for lines with trailing whitespace.
        /// </summary>
        /// <param name="context">The context that provides the document to scan.</param>
        private static void HandleSyntaxTree(SyntaxTreeAnalysisContext context)
        {
            var root = context.Tree.GetRoot(context.CancellationToken);
            var text = context.Tree.GetText(context.CancellationToken);

            foreach (var trivia in root.DescendantTrivia(descendIntoTrivia: true))
            {
                switch (trivia.Kind())
                {
                case SyntaxKind.WhitespaceTrivia:
                    bool reportWarning = false;
                    var token = trivia.Token;
                    SyntaxTriviaList triviaList;
                    if (token.LeadingTrivia.Contains(trivia))
                    {
                        triviaList = token.LeadingTrivia;
                    }
                    else
                    {
                        triviaList = token.TrailingTrivia;
                    }

                    bool foundWhitespace = false;
                    foreach (var innerTrivia in triviaList)
                    {
                        if (!foundWhitespace)
                        {
                            if (innerTrivia.Equals(trivia))
                            {
                                foundWhitespace = true;
                            }

                            continue;
                        }

                        if (innerTrivia.IsKind(SyntaxKind.EndOfLineTrivia))
                        {
                            reportWarning = true;
                        }

                        break;
                    }

                    if (reportWarning)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(Descriptor, trivia.GetLocation()));
                    }

                    break;
                case SyntaxKind.SingleLineCommentTrivia:
                    TextSpan trailingWhitespace = FindTrailingWhitespace(text, trivia.Span);
                    if (!trailingWhitespace.IsEmpty)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(Descriptor, Location.Create(context.Tree, trailingWhitespace)));
                    }

                    break;
                case SyntaxKind.MultiLineCommentTrivia:
                case SyntaxKind.SingleLineDocumentationCommentTrivia:
                    var line = text.Lines.GetLineFromPosition(trivia.Span.Start);
                    do
                    {
                        trailingWhitespace = FindTrailingWhitespace(text, line.Span);
                        if (!trailingWhitespace.IsEmpty)
                        {
                            context.ReportDiagnostic(Diagnostic.Create(Descriptor, Location.Create(context.Tree, trailingWhitespace)));
                        }

                        if (line.EndIncludingLineBreak == text.Length)
                        {
                            // We've reached the end of the document.
                            break;
                        }

                        line = text.Lines.GetLineFromPosition(line.EndIncludingLineBreak + 1);
                    }
                    while (line.End <= trivia.Span.End);

                    break;
                case SyntaxKind.IfDirectiveTrivia:
                case SyntaxKind.ElifDirectiveTrivia:
                case SyntaxKind.ElseDirectiveTrivia:
                case SyntaxKind.EndIfDirectiveTrivia:
                case SyntaxKind.DefineDirectiveTrivia:
                case SyntaxKind.UndefDirectiveTrivia:
                case SyntaxKind.WarningDirectiveTrivia:
                case SyntaxKind.ErrorDirectiveTrivia:
                case SyntaxKind.RegionDirectiveTrivia:
                case SyntaxKind.EndRegionDirectiveTrivia:
                    trailingWhitespace = FindTrailingWhitespace(text, trivia.Span);
                    if (!trailingWhitespace.IsEmpty)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(Descriptor, Location.Create(context.Tree, trailingWhitespace)));
                    }

                    break;
                default:
                    break;
                }
            }
        }

        private static TextSpan FindTrailingWhitespace(SourceText text, TextSpan within)
        {
            for (int i = within.End - 1; i >= within.Start; i--)
            {
                if (!char.IsWhiteSpace(text[i]))
                {
                    return TextSpan.FromBounds(i + 1, within.End);
                }
            }

            return within;
        }
    }
}
