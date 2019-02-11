using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace AutoMiniProfiler.Core
{
	internal sealed class MethodProfilerInjectionRewriter
		: CSharpSyntaxRewriter
	{
		private readonly SemanticModel model;

		public MethodProfilerInjectionRewriter(SemanticModel model, bool visitIntoStructuredTrivia = false)
			: base(visitIntoStructuredTrivia) => this.model = model;

		public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node)
		{
			if (!node.ContainsDiagnostics)
			{
				var expressionBodied = node.DescendantNodes().SingleOrDefault(_ => typeof(ArrowExpressionClauseSyntax).IsAssignableFrom(_.GetType()));

				if (expressionBodied != null)
				{
					var expression = expressionBodied.ChildNodes().First() as ExpressionSyntax;
					var statement = SyntaxFactory.ExpressionStatement(expression);
					var newBlock = this.CreateBlock(node, new[] { statement });
					return node.RemoveNode(expressionBodied, SyntaxRemoveOptions.KeepDirectives).WithBody(newBlock);
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

			return SyntaxFactory.Block(
				SyntaxFactory.SingletonList<StatementSyntax>(
					SyntaxFactory.UsingStatement(
						SyntaxFactory.Block(statements))
					.WithExpression(
						SyntaxFactory.InvocationExpression(
							SyntaxFactory.MemberAccessExpression(
								SyntaxKind.SimpleMemberAccessExpression,
								SyntaxFactory.IdentifierName("TimingCreator"),
								SyntaxFactory.IdentifierName("GetTiming")))
						.WithArgumentList(
							SyntaxFactory.ArgumentList(
								SyntaxFactory.SingletonSeparatedList<ArgumentSyntax>(
									SyntaxFactory.Argument(
										SyntaxFactory.LiteralExpression(
											SyntaxKind.StringLiteralExpression,
												SyntaxFactory.Literal(methodName)))))))))
				.NormalizeWhitespace();
		}
	}

	class C
	{
		int q;
		void Foo() => System.Console.Out.WriteLine("Foo");

		void Bar() => this.q = 10;

		void FooWithBody()
		{
			var x = this.q;
			System.Console.Out.WriteLine("Foo");
		}
	}
}
