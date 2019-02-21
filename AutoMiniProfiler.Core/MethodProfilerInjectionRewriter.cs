using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AutoMiniProfiler.Core
{
	internal sealed class MethodProfilerInjectionRewriter
		: CSharpSyntaxRewriter
	{
		private readonly SemanticModel model;

		public MethodProfilerInjectionRewriter(SemanticModel model, bool visitIntoStructuredTrivia = false)
			: base(visitIntoStructuredTrivia) => this.model = model ?? throw new ArgumentNullException(nameof(model));

		public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node)
		{
			if (!node.ContainsDiagnostics)
			{
				var expressionBodied = node.DescendantNodes().SingleOrDefault(
					_ => typeof(ArrowExpressionClauseSyntax).IsAssignableFrom(_.GetType()));

				if (expressionBodied != null)
				{
					var expression = expressionBodied.ChildNodes().First() as ExpressionSyntax;
					var statement = SyntaxFactory.ExpressionStatement(expression);
					var newBlock = this.CreateBlock(node, new[] { statement });
					node = node.RemoveNode(expressionBodied, SyntaxRemoveOptions.KeepDirectives);
					var semicolons = node.DescendantTokens(_ => true).Where(_ => _.Kind() == SyntaxKind.SemicolonToken);
					return node.ReplaceTokens(semicolons, (_, __) => new SyntaxToken())
						.WithBody(newBlock);
				}
				else
				{
					var blockNode = node.ChildNodes().First(_ => _.Kind() == SyntaxKind.Block) as BlockSyntax;
					var statements = blockNode.ChildNodes().Cast<StatementSyntax>();
					var newBlock = this.CreateBlock(node, statements);
					return node.RemoveNode(blockNode, SyntaxRemoveOptions.KeepDirectives).WithBody(newBlock);
				}
			}

			return base.VisitMethodDeclaration(node);
		}

		private BlockSyntax CreateBlock(MethodDeclarationSyntax method, IEnumerable<StatementSyntax> statements)
		{
			var methodModel = this.model.GetDeclaredSymbol(method) as IMethodSymbol;
			var methodName = $"{methodModel.ContainingType.Name}.{methodModel.Name}";

			var getTimingArgument = SyntaxFactory.LiteralExpression(
				SyntaxKind.StringLiteralExpression,
				SyntaxFactory.Literal(methodName));

			var getTimingMemberAccess = SyntaxFactory.MemberAccessExpression(
				SyntaxKind.SimpleMemberAccessExpression,
				SyntaxFactory.IdentifierName("TimingCreator"),
				SyntaxFactory.IdentifierName("GetTiming"));

			var getTimingInvocation = SyntaxFactory.InvocationExpression(getTimingMemberAccess)
				.WithArgumentList(
					SyntaxFactory.ArgumentList(
						SyntaxFactory.SingletonSeparatedList(
							SyntaxFactory.Argument(getTimingArgument))));

			return SyntaxFactory.Block(
				SyntaxFactory.SingletonList<StatementSyntax>(
					SyntaxFactory.UsingStatement(SyntaxFactory.Block(statements)).WithExpression(getTimingInvocation)))
				.NormalizeWhitespace();
		}
	}
}