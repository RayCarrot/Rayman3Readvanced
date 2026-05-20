using GbaMonoGame.Rayman3.Readvanced;
using Microsoft.Xna.Framework;

namespace GbaMonoGame.Rayman3;

public class MissileRace1 : FrameSingleMode7
{
    public MissileRace1(MapId mapId) : base(mapId, [60, 55, 50]) { }

    public TextBoxDialog TextBox { get; set; }
    public uint TextBoxTimer { get; set; }
    public byte TextBoxCountdown { get; set; }

    public override void Init()
    {
        base.Init();

        // Custom textbox for strafing hint
        if (Engine.Settings.Active.Tweaks.ShowAdditionalGameplayHints)
        {
            TextBox = new(Scene);
            Scene.AddDialog(TextBox, false, false);
            TextBox.SetCutsceneCharacter(TextBoxCutsceneCharacter.Murfy);
            TextBox.TextBankId = TextBankId.Readvanced;
            TextBox.SetText(1);
            TextBoxCountdown = 0;
            TextBoxTimer = 0;
        }

        AddWalls(new Point(1, 22), new Point(3, 3));

        ExtendMap(
        [
            new(2), new(3), new(4),
            new(6), new(1), new(7),
            new(8), new(5), new(9)
        ], 3, 3);

        Rayman3Achievements.MissileRace1_HasStrafed = false;
    }

    public override void Step()
    {
        TextBoxTimer++;

        base.Step();

        if (TextBoxTimer == 60)
        {
            TextBox?.MoveInOurOut(true);
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