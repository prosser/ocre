// <copyright file="OcreAnalyzer.cs">Copyright (c) Peter Rosser. All rights reserved.</copyright>

namespace Ocre;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

using Ocre.Comparers;
using Ocre.Configuration;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class OcreAnalyzer : DiagnosticAnalyzer
{
    public static class Rules
    {
        private const string IdPrefix = "OC";

        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localizable, you can use regular strings for Title and MessageFormat.
        // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/Localizing%20Analyzers.md for more on localization

        public static readonly DiagnosticDescriptor TracingRule = new(
                   id: IdPrefix + "0001",
                   title: "Trace",
                   messageFormat: "{0}",
                   category: "Style",
                   defaultSeverity: DiagnosticSeverity.Info,
                   isEnabledByDefault: true,
                   description: "Trace.");

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
        Rules.TracingRule,
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
            Compilation compilation = cStart.Compilation;
            AnalyzerConfigOptionsProvider provider = cStart.Options.AnalyzerConfigOptionsProvider;

            // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/Analyzer%20Actions%20Semantics.md for more information

            //cStart.RegisterSyntaxNodeAction(ctx =>
            //{
            //    var settings = OcreConfiguration.Read(provider, ctx.Node.SyntaxTree);
            //    AnalyzeIntraTypeSorting(settings, ctx);
            //}, SyntaxKind.ClassDeclaration, SyntaxKind.StructDeclaration, SyntaxKind.RecordDeclaration, SyntaxKind.RecordStructDeclaration);

            cStart.RegisterSemanticModelAction(ctx =>
            {
                SemanticModel semanticModel = ctx.SemanticModel;

                if (semanticModel.SyntaxTree.TryGetRoot(out SyntaxNode? root))
                {
                    var settings = OcreConfiguration.Read(provider, semanticModel.SyntaxTree);
                    AnalyzeTypesInFileOrdering(settings, ctx, semanticModel, root);
                }
            });
        });
    }

    private void AnalyzeTypesInFileOrdering(OcreConfiguration settings, SemanticModelAnalysisContext context, SemanticModel model, SyntaxNode root)
    {
        CSharpSyntaxNode[] types = [.. root.GetTypeDeclaractionsInFile()];
        //context.ReportDiagnostic(Diagnostic.Create(Rules.TracingRule, root.GetLocation(), $"Types={types.Length} [{string.Join(",", types.Select(t => t.GetDisplayName()))}]"));

        if (types.Length < 2)
        {
            return; // trivially ordered
        }

        TypeDeclarationComparer comparer = new(settings, model);

        // Pre-compute expected order string for message (configuration order tokens)
        List<CSharpSyntaxNode> sorted = [.. types];
        sorted.Sort(comparer);

        string expectedOrder = string.Join(", ", sorted.Select(t => t.GetDisplayName()));

        // Find all out-of-order types (report the later node in each violating pair)
        for (int i = 1; i < types.Length; i++)
        {
            CSharpSyntaxNode current = types[i];
            CSharpSyntaxNode previous = types[i - 1];
            if (comparer.Compare(previous, current) > 0)
            {
                string typeName = current.GetDisplayName();
                context.ReportDiagnostic(Diagnostic.Create(Rules.TypeOrderInFileRule, current.GetLocation(), typeName, expectedOrder));
            }
        }
    }
}
