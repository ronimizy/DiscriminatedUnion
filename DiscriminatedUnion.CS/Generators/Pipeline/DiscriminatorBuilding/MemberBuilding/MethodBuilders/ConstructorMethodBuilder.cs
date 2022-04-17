using DiscriminatedUnion.CS.Extensions;
using DiscriminatedUnion.CS.Generators.Pipeline.DiscriminatorBuilding.MemberBuilding.Models;
using DiscriminatedUnion.CS.Generators.Pipeline.DiscriminatorBuilding.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace DiscriminatedUnion.CS.Generators.Pipeline.DiscriminatorBuilding.MemberBuilding.MethodBuilders;

public class ConstructorMethodBuilder : MethodBuilderBase
{
    protected override MethodMemberBuilderResponse BuildMemberSyntaxComponent(
        MemberBuilderContext<IMethodSymbol> context)
    {
        var (memberSymbol, _, syntax) = context;

        if (!context.WrappedSymbol.Constructors.Contains(memberSymbol))
            return new MethodMemberBuilderResponse(MethodMemberBuilderResult.NotBuilt, syntax);

        IEnumerable<ParameterSyntax> parameters = memberSymbol.Parameters.ToParameterSyntax();
        IEnumerable<ArgumentSyntax> arguments = memberSymbol.Parameters.ToArgumentSyntax();

        var wrappedCreation = ObjectCreationExpression(IdentifierName(context.WrappedTypeName))
            .WithArgumentList(ArgumentList(SeparatedList(arguments)));

        var discriminatorCreation = ObjectCreationExpression(IdentifierName(context.DiscriminatorTypeName))
            .WithArgumentList(ArgumentList(SingletonSeparatedList(Argument(wrappedCreation))));

        var method = MethodDeclaration(IdentifierName(context.DiscriminatorTypeName), Identifier("Create"))
            .WithModifiers(memberSymbol.DeclaredAccessibility.ToSyntaxTokenList())
            .AddModifiers(Token(SyntaxKind.StaticKeyword))
            .WithParameterList(ParameterList(SeparatedList(parameters)))
            .WithBody(Block(SingletonList(ReturnStatement(discriminatorCreation))));

        return new MethodMemberBuilderResponse(MethodMemberBuilderResult.Built, syntax.AddMembers(method));
    }
}