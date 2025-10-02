// <copyright file="ObsessiveCodeOrdererCodeFixProvider.cs">Copyright (c) Peter Rosser. All rights reserved.</copyright>

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
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ObsessiveCodeOrdererCodeFixProvider)), Shared]
public class ObsessiveCodeOrdererCodeFixProvider : CodeFixProvider
{
    public sealed override ImmutableArray<string> FixableDiagnosticIds { get; } =
        ImmutableArray.Create(
        OcreAnalyzer.Rules.TypeOrderInFileRule.Id,
        OcreAnalyzer.Rules.NestedTypeOrderRule.Id,
        OcreAnalyzer.Rules.MemberOrderRule.Id,
        OcreAnalyzer.Rules.OperatorOrderRule.Id);

    public sealed override FixAllProvider GetFixAllProvider()
    {
        // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
        return WellKnownFixAllProviders.BatchFixer;
    }

    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        Document doc = context.Document;
        SyntaxNode root = await doc.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

        // TODO: Replace the following code with your own analysis, generating a CodeAction for each fix to suggest
        Diagnostic diagnostic = context.Diagnostics[0];
        TextSpan diagnosticSpan = diagnostic.Location.SourceSpan;

        // Find the type declaration identified by the diagnostic.
        TypeDeclarationSyntax node = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<TypeDeclarationSyntax>().First();

        // Register a code action that will invoke the fix.
        context.RegisterCodeFix(
            CodeAction.Create(
                title: CodeFixResources.CodeFixTitle,
                createChangedSolution: c => MakeUppercaseAsync(context.Document, node, c),
                equivalenceKey: nameof(CodeFixResources.CodeFixTitle)),
            diagnostic);

        context.RegisterCodeFix(
            CodeAction.Create(
                "Sort members",
                ct => SortMembersAsync(doc, node, ct),
                equivalenceKey: "SortMembers"),
            context.Diagnostics);
    }

    private static async Task<Document> SortMembersAsync(Document doc, TypeDeclarationSyntax typeDecl, CancellationToken ct)
    {
        DocumentEditor editor = await DocumentEditor.CreateAsync(doc, ct).ConfigureAwait(false);

        //SyntaxList<MemberDeclarationSyntax> members = typeDecl.Members;
        //var segments = SegmentByDirectives(members);
        //var comparer = BuildMemberComparer(doc, typeDecl); // from settings

        //var newSegments = segments.Select(seg =>
        //{
        //    var arr = seg.ToArray();
        //    Array.Sort(arr, comparer); // stable if comparer returns 0 for equals; or use OrderBy with original index
        //    return SyntaxFactory.List(arr);
        //});

        //var newMembers = ConcatSegments(newSegments);
        //TypeDeclarationSyntax newType = typeDecl.WithMembers(newMembers).WithAdditionalAnnotations(Formatter.Annotation);
        //editor.ReplaceNode(typeDecl, newType);
        return editor.GetChangedDocument();
    }

    private static SyntaxTokenList SortModifiers(SyntaxTokenList modifiers, IComparer<SyntaxToken> cmp)
    {
        SyntaxToken[] arr = [.. modifiers];
        Array.Sort(arr, cmp);
        return SyntaxFactory.TokenList(arr);
    }

    private async Task<Solution> MakeUppercaseAsync(Document document, TypeDeclarationSyntax typeDecl, CancellationToken cancellationToken)
    {
        // Compute new uppercase name.
        SyntaxToken identifierToken = typeDecl.Identifier;
        string newName = identifierToken.Text.ToUpperInvariant();

        // Get the symbol representing the type to be renamed.
        SemanticModel semanticModel = await document.GetSemanticModelAsync(cancellationToken);
        INamedTypeSymbol typeSymbol = semanticModel.GetDeclaredSymbol(typeDecl, cancellationToken);

        // Produce a new solution that has all references to that type renamed, including the declaration.
        SymbolRenameOptions options = new(RenameOverloads: false, RenameInStrings: false, RenameInComments: false, RenameFile: false);
        Solution newSolution = await Renamer.RenameSymbolAsync(document.Project.Solution, typeSymbol, options, newName, cancellationToken).ConfigureAwait(false);

        // Return the new solution with the now-uppercase type name.
        return newSolution;
    }
}
