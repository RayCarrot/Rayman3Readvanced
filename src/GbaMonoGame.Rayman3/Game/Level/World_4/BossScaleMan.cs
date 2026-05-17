using GbaMonoGame.Engine2d;
using GbaMonoGame.Rayman3.Readvanced;

namespace GbaMonoGame.Rayman3;

public class BossScaleMan : FrameSideScroller
{
    public BossScaleMan(MapId mapId) : base(mapId) { }

    public TextBoxDialog TextBox { get; set; }
    public bool ShowAttackHint { get; set; }
    public byte TextBoxCountdown { get; set; }

    public override void Init()
    {
        base.Init();

        if (!Rayman3.TimeAttack.IsActive)
        {
            Scene.Camera.LinkedObject = Scene.GetGameObject<MovableActor>(1);
            Scene.MainActor.ProcessMessage(this, Message.Rayman_Stop);
        }

        // Custom textbox for strafing hint
        if (Engine.ActiveConfig.Tweaks.ShowAdditionalGameplayHints)
        {
            TextBox = new(Scene);
            Scene.AddDialog(TextBox, false, false);
            TextBox.SetCutsceneCharacter(TextBoxCutsceneCharacter.Murfy);
            TextBox.TextBankId = TextBankId.Readvanced;
            TextBox.SetText(2);
            TextBoxCountdown = 0;
        }
    }

    public override void Step()
    {
        base.Step();

        if (ShowAttackHint && TextBoxCountdown == 0)
        {
            TextBox?.MoveInOurOut(true);
            ShowAttackHint = false;
            TextBoxCountdown = 240;
        }

        if (TextBoxCountdown != 0)
        {
            TextBoxCountdown--;

            if (TextBoxCountdown == 0)
                TextBox?.MoveInOurOut(false);
        }
    }

}