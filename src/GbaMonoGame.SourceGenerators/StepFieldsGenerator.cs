using Microsoft.CodeAnalysis;

namespace GbaMonoGame.SourceGenerators;

[Generator]
public sealed class StepFieldsGenerator : BaseDelegateFieldsGenerator
{
    protected override string StateMethodPrefix => "Step_";
    protected override string GenerateMethodName => "CreateGeneratedSteps";
    protected override string DelegateTypeName => "System.Action";
    protected override string AttributeName => "GenerateStepFieldsAttribute";
}