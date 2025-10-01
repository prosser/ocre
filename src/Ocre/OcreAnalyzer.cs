// <copyright file="OcreAnalyzer.cs">Copyright (c) Peter Rosser. All rights reserved.</copyright>

namespace Ocre;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

using Ocre.Configuration;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class OcreAnalyzer : DiagnosticAnalyzer
{
    public static class Rules
    {
        private const string IdPrefix = "OCRE";

        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localizable, you can use regular strings for Title and MessageFormat.
        // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/Localizing%20Analyzers.md for more on localization

        public static readonly DiagnosticDescriptor TypeOrderInFileRule = new(
            id: IdPrefix + "1000",
            title: new LocalizableResourceString(nameof(Resources.TypeOrderInFileTitle), Resources.ResourceManager, typeof(Resources)),
            messageFormat: new LocalizableResourceString(nameof(Resources.TypeOrderInFileMessageFormat), Resources.ResourceManager, typeof(Resources)),
            category: "Style",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: new LocalizableResourceString(nameof(Resources.TypeOrderInFileDescription), Resources.ResourceManager, typeof(Resources)));

        public static readonly DiagnosticDescriptor NestedTypeOrderRule = new(
            id: IdPrefix + "1001",
            title: new LocalizableResourceString(nameof(Resources.NestedTypeOrderTitle), Resources.ResourceManager, typeof(Resources)),
            messageFormat: new LocalizableResourceString(nameof(Resources.NestedTypeOrderMessageFormat), Resources.ResourceManager, typeof(Resources)),
            category: "Style",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: new LocalizableResourceString(nameof(Resources.NestedTypeOrderDescription), Resources.ResourceManager, typeof(Resources)));

        public static readonly DiagnosticDescriptor MemberOrderRule = new(
            id: IdPrefix + "1002",
            title: new LocalizableResourceString(nameof(Resources.TypeMemberOrderTitle), Resources.ResourceManager, typeof(Resources)),
            messageFormat: new LocalizableResourceString(nameof(Resources.TypeMemberOrderMessageFormat), Resources.ResourceManager, typeof(Resources)),
            category: "Style",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: new LocalizableResourceString(nameof(Resources.TypeMemberOrderDescription), Resources.ResourceManager, typeof(Resources)));

        public static readonly DiagnosticDescriptor OperatorOrderRule = new(
            id: IdPrefix + "1003",
            title: new LocalizableResourceString(nameof(Resources.OperatorOrderTitle), Resources.ResourceManager, typeof(Resources)),
            messageFormat: new LocalizableResourceString(nameof(Resources.OperatorOrderMessageFormat), Resources.ResourceManager, typeof(Resources)),
            category: "Style",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: new LocalizableResourceString(nameof(Resources.OperatorOrderDescription), Resources.ResourceManager, typeof(Resources)));
    }

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
    [
        Rules.TypeOrderInFileRule,
        Rules.NestedTypeOrderRule,
        Rules.MemberOrderRule,
        Rules.OperatorOrderRule
    ];

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
                var settings = OcreConfiguration.Read(provider, ctx.Node.SyntaxTree);
                AnalyzeIntraTypeSorting(settings, ctx);
            }, SyntaxKind.ClassDeclaration, SyntaxKind.StructDeclaration, SyntaxKind.RecordDeclaration, SyntaxKind.RecordStructDeclaration);

            cStart.RegisterSyntaxNodeAction(ctx =>
            {
                var settings = OcreConfiguration.Read(provider, ctx.Node.SyntaxTree);
                AnalyzeTypesInFileOrdering(settings, ctx);
            }, SyntaxKind.NamespaceDeclaration);
        });
    }

    private void AnalyzeIntraTypeSorting(OcreConfiguration settings, SyntaxNodeAnalysisContext context)
    {
        //context.ReportDiagnostic(Diagnostic.Create(Rules.MemberOrderRule, context.Node.GetLocation(), ))
    }

    private void AnalyzeTypesInFileOrdering(OcreConfiguration settings, SyntaxNodeAnalysisContext context)
    {
        SyntaxNode nsNode = context.Node;

        Queue<SortStrategyKind> topQueue = new(settings.SortOrder);
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
            //var diagnostic = Diagnostic.Create(Rule, namedTypeSymbol.Locations[0], namedTypeSymbol.Name);

            //context.ReportDiagnostic(diagnostic);
        }
    }
}
