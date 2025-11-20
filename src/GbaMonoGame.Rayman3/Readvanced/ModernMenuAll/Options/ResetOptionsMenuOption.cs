using System;
using System.Collections.Generic;

namespace GbaMonoGame.Rayman3.Readvanced;

public class ResetOptionsMenuOption : OptionsMenuOption
{
    public ResetOptionsMenuOption(string text, string infoText, Action resetAction, bool isDebugOption = false) 
        : base(text, infoText, isDebugOption)
    {
        ResetAction = resetAction;
    }

    public Action ResetAction { get; }
    public override bool ShowArrows => false;

    public override void Reset(IReadOnlyList<OptionsMenuOption> options) { }

    public override EditStepResult EditStep(IReadOnlyList<OptionsMenuOption> options)
    {
        ResetAction();
        foreach (OptionsMenuOption option in options)
            option.Reset(options);
        return EditStepResult.Apply;
    }
}