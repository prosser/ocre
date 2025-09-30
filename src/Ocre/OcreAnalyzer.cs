// <copyright file="OcreAnalyzer.cs">Copyright (c) Peter Rosser. All rights reserved.</copyright>

namespace Ocre;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class OcreAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "OCRE1000";

    // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
    // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/Localizing%20Analyzers.md for more on localization
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
    private const string Category = "Naming";

    private static readonly DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterCompilationStartAction(cStart =>
        {
            AnalyzerConfigOptionsProvider provider = cStart.Options.AnalyzerConfigOptionsProvider;

            // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/Analyzer%20Actions%20Semantics.md for more information

            cStart.RegisterSyntaxNodeAction(ctx =>
            {
                var settings = OcreSettings.Read(provider, ctx.Node.SyntaxTree);
                AnalyzeIntraTypeSorting(settings, ctx);
            }, SyntaxKind.ClassDeclaration, SyntaxKind.StructDeclaration, SyntaxKind.RecordDeclaration, SyntaxKind.RecordStructDeclaration);

            cStart.RegisterSyntaxNodeAction(ctx =>
            {
                var settings = OcreSettings.Read(provider, ctx.Node.SyntaxTree);
                AnalyzeTypesInFileOrdering(settings, ctx);
            }, SyntaxKind.NamespaceDeclaration);
        });
    }

    private void AnalyzeIntraTypeSorting(OcreSettings settings, SyntaxNodeAnalysisContext context)
    {

    }

    private void AnalyzeTypesInFileOrdering(OcreSettings settings, SyntaxNodeAnalysisContext context)
    {
        SyntaxNode nsNode = context.Node;

        Queue<SortPriority> topQueue = new(settings.SortOrder);
        while (topQueue.Count > 0)
        {

        }
    }

    private static void AnalyzeSymbol(SymbolAnalysisContext context)
    {
        // TODO: Replace the following code with your own analysis, generating Diagnostic objects for any issues you find
        var namedTypeSymbol = (INamedTypeSymbol)context.Symbol;

        // Find just those named type symbols with names containing lowercase letters.
        if (namedTypeSymbol.Name.ToCharArray().Any(char.IsLower))
        {
            // For all such symbols, produce a diagnostic.
            var diagnostic = Diagnostic.Create(Rule, namedTypeSymbol.Locations[0], namedTypeSymbol.Name);

            context.ReportDiagnostic(diagnostic);
        }
    }
}
