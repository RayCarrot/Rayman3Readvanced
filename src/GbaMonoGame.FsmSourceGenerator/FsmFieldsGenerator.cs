using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace GbaMonoGame.FsmSourceGenerator;

[Generator]
public class FsmFieldsGenerator : IIncrementalGenerator
{
    private const string StateMethodPrefix = "Fsm_";
    private const string AttributeNamespace = "GbaMonoGame.FsmSourceGenerator";
    private const string AttributeName = "GenerateFsmFieldsAttribute";
    private const string AttributeStr = $$"""
                                        namespace {{AttributeNamespace}};
                                        
                                        [System.AttributeUsage(System.AttributeTargets.Class)]
                                        public class {{AttributeName}} : System.Attribute { }
                                        """;

    private static FsmClass? GetFsmClass(SemanticModel semanticModel, SyntaxNode declarationSyntaxNode)
    {
        // Get the semantic representation of the class
        if (semanticModel.GetDeclaredSymbol(declarationSyntaxNode) is not INamedTypeSymbol classSymbol)
            return null;

        // Get the class name and namespace
        string className = classSymbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
        string classNamespace = classSymbol.ContainingNamespace.ToDisplayString(
            new(typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces));

        // Get all state methods
        List<string> stateMethodNames = new();
        foreach (ISymbol classMember in classSymbol.GetMembers())
        {
            // NOTE: For now we're finding the methods based on the naming prefix, but we might want to make this more flexible in the future
            if (classMember is IMethodSymbol field && field.Name.StartsWith(StateMethodPrefix))
                stateMethodNames.Add(field.Name);
        }

        return new FsmClass(classNamespace, className, stateMethodNames);
    }

    private static string GenerateClassString(FsmClass fsmClass)
    {
        StringBuilder sb = new();

        // Add initial declaration
        sb.Append($$"""
                    namespace {{fsmClass.ClassNamespace}};

                    partial class {{fsmClass.ClassName}}
                    {
                    """);

        // Add fields for each state method
        sb.AppendLine();
        foreach (string methodName in fsmClass.StateMethodNames)
            sb.AppendLine($"    private GbaMonoGame.FiniteStateMachine.Fsm _{methodName};");

        // Add the method to create the states
        sb.AppendLine();
        sb.Append("""
                      private void CreateGeneratedStates()
                      {
                  """);

        // Add field initializations
        sb.AppendLine();
        foreach (string methodName in fsmClass.StateMethodNames)
            sb.AppendLine($"        _{methodName} = {methodName};");

        // Close the method and class
        sb.Append("""
                      }
                  }
                  """);

        return sb.ToString();
    }

    private static void Execute(FsmClass? fsmClass, SourceProductionContext context)
    {
        if (fsmClass is { } fsmClassValue)
        {
            string classString = GenerateClassString(fsmClassValue);
            context.AddSource($"{fsmClassValue.ClassName}.g.cs", SourceText.From(classString, Encoding.UTF8));
        }
    }

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Add the attribute
        context.RegisterPostInitializationOutput(static ctx => 
            ctx.AddSource($"{AttributeName}.g.cs", SourceText.From(AttributeStr, Encoding.UTF8)));

        // Find the classes to generate the fields for
        IncrementalValuesProvider<FsmClass?> fsmClasses = context.SyntaxProvider.
            ForAttributeWithMetadataName(
                 $"{AttributeNamespace}.{AttributeName}",
                 predicate: static (_, _) => true,
                 transform: static (ctx, _) => GetFsmClass(ctx.SemanticModel, ctx.TargetNode)).
            Where(static m => m is not null);

        // Generate the partial class implementation for each found class
        context.RegisterSourceOutput(fsmClasses,
            static (spc, source) => Execute(source, spc));
    }

    private readonly record struct FsmClass(string ClassNamespace, string ClassName, List<string> StateMethodNames)
    {
        public string ClassNamespace { get; } = ClassNamespace;
        public string ClassName { get; } = ClassName;
        public List<string> StateMethodNames { get; } = StateMethodNames;
    }
}