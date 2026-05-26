using Microsoft.CodeAnalysis;

namespace GbaMonoGame.SourceGenerators;

[Generator]
public sealed class FsmFieldsGenerator : BaseDelegateFieldsGenerator
{
    protected override string StateMethodPrefix => "Fsm_";
    protected override string GenerateMethodName => "CreateGeneratedStates";
    protected override string DelegateTypeName => "GbaMonoGame.FiniteStateMachine.Fsm";
    protected override string AttributeName => "GenerateFsmFieldsAttribute";
}