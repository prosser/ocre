// <copyright file="SortingCodeFixProvider.cs">Copyright (c) Peter Rosser. All rights reserved.</copyright>

namespace Ocre;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;

using Ocre.Comparers;
using Ocre.Configuration;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(SortingCodeFixProvider)), Shared]
public class SortingCodeFixProvider : CodeFixProvider
{
    public sealed override ImmutableArray<string> FixableDiagnosticIds { get; } =
        [
            OcreAnalyzer.Rules.TypeOrderInFileRule.Id,
            OcreAnalyzer.Rules.NestedTypeOrderRule.Id,
            OcreAnalyzer.Rules.MemberOrderRule.Id,
            OcreAnalyzer.Rules.OperatorOrderRule.Id,
        ];

    public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        Document doc = context.Document;
        SyntaxNode? root = await doc.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root is null)
        {
            return;
        }

        Diagnostic[] oc1000 = [.. context.Diagnostics.Where(d => d.Id == OcreAnalyzer.Rules.TypeOrderInFileRule.Id)];
        if (oc1000.Length == 0)
        {
            return;
        }

        context.RegisterCodeFix(
            CodeAction.Create(
                title: CodeFixResources.SortTypesInFileTitle,
                createChangedDocument: c => ApplySortTypesFixAsync(doc, root, c),
                equivalenceKey: nameof(CodeFixResources.SortTypesInFileTitle)),
            oc1000);

        // Legacy sample fix (can remove later)
        Diagnostic first = context.Diagnostics[0];
        TextSpan span = first.Location.SourceSpan;
        TypeDeclarationSyntax? typeDecl = root.FindToken(span.Start).Parent?.AncestorsAndSelf().OfType<TypeDeclarationSyntax>().FirstOrDefault();
        if (typeDecl != null)
        {
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: CodeFixResources.CodeFixTitle,
                    createChangedSolution: c => MakeUppercaseAsync(doc, typeDecl, c),
                    equivalenceKey: nameof(CodeFixResources.CodeFixTitle)),
                first);
        }
    }

    private static async Task<Document> ApplySortTypesFixAsync(Document document, SyntaxNode root, CancellationToken ct)
    {
        SemanticModel? model = await document.GetSemanticModelAsync(ct).ConfigureAwait(false);
        if (model is null)
        {
            return document;
        }

        // Obtain analyzer config options provider from the project
        AnalyzerConfigOptionsProvider provider = document.Project.AnalyzerOptions.AnalyzerConfigOptionsProvider;
        var settings = OcreConfiguration.Read(provider, model.SyntaxTree);

        // Collect all top-level types from compilation unit, block namespaces, and file-scoped namespaces
        List<(SyntaxNode container, MemberDeclarationSyntax member)> collected = [];

        if (root is CompilationUnitSyntax cu)
        {
            foreach (MemberDeclarationSyntax m in cu.Members)
            {
                if (IsTopLevelType(m))
                {
                    collected.Add((cu, m));
                }

                if (m is NamespaceDeclarationSyntax nd)
                {
                    foreach (MemberDeclarationSyntax nm in nd.Members.Where(IsTopLevelType))
                    {
                        collected.Add((nd, nm));
                    }
                }
                else if (m is FileScopedNamespaceDeclarationSyntax fns)
                {
                    foreach (MemberDeclarationSyntax nm in fns.Members.Where(IsTopLevelType))
                    {
                        collected.Add((fns, nm));
                    }
                }
            }
        }

        if (collected.Count < 2)
        {
            return document; // nothing to reorder
        }

        // Build comparer
        TypeDeclarationComparer comparer = new(settings, model);

        // Group by container (CU / namespace) and sort within each group
        var byContainer = collected
            .GroupBy(x => x.container)
            .ToDictionary(g => g.Key, g => g.Select(x => x.member).ToList());

        Dictionary<MemberDeclarationSyntax, MemberDeclarationSyntax> replacements = [];

        foreach (KeyValuePair<SyntaxNode, List<MemberDeclarationSyntax>> kvp in byContainer)
        {
            List<MemberDeclarationSyntax> original = kvp.Value;
            if (original.Count < 2)
            {
                continue;
            }

            var sorted = original.OrderBy(m => m, comparer).ToList();
            if (sorted.SequenceEqual(original))
            {
                continue; // already ordered
            }

            // Preserve leading trivia with their type declarations (already attached)
            // Only need to map originals to sorted sequence order
            for (int i = 0; i < original.Count; i++)
            {
                replacements[original[i]] = sorted[i];
            }
        }

        if (replacements.Count == 0)
        {
            return document; // no changes
        }

        SyntaxNode newRoot = root.ReplaceNodes(replacements.Keys, (orig, _) => replacements[orig]);
        return document.WithSyntaxRoot(newRoot);
    }

    private static bool IsTopLevelType(MemberDeclarationSyntax m) => m is TypeDeclarationSyntax or EnumDeclarationSyntax or DelegateDeclarationSyntax;

    private static async Task<Solution> MakeUppercaseAsync(Document document, TypeDeclarationSyntax typeDecl, CancellationToken cancellationToken)
    {
        SyntaxToken identifierToken = typeDecl.Identifier;
        string newName = identifierToken.Text.ToUpperInvariant();

        SemanticModel? semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
        if (semanticModel is null)
        {
            return document.Project.Solution;
        }

        if (semanticModel.GetDeclaredSymbol(typeDecl, cancellationToken) is not INamedTypeSymbol typeSymbol)
        {
            return document.Project.Solution;
        }

        SymbolRenameOptions options = new(RenameOverloads: false, RenameInStrings: false, RenameInComments: false, RenameFile: false);
        Solution newSolution = await Renamer.RenameSymbolAsync(document.Project.Solution, typeSymbol, options, newName, cancellationToken).ConfigureAwait(false);
        return newSolution;
    }
}
