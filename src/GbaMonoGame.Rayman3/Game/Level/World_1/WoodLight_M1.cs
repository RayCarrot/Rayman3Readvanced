using BinarySerializer.Ubisoft.GbaEngine;
using GbaMonoGame.Engine2d;
using GbaMonoGame.Rayman3.Readvanced;
using GbaMonoGame.TgxEngine;

namespace GbaMonoGame.Rayman3;

public class WoodLight_M1 : FrameSideScroller
{
    public WoodLight_M1(MapId mapId) : base(mapId) { }

    // For time attack
    public TextBoxDialog TextBox { get; set; }
    public uint TimeAttackTextBoxTimer { get; set; }
    public bool IsShowingTimeAttackTextBox { get; set; }

    private bool HasDestroyedAnyTimeDecreaseItems()
    {
        foreach (GameObject obj in Scene.KnotManager.GameObjects)
        {
            if (!obj.IsEnabled && obj is BaseActor { Type: (int)ReadvancedActorType.TimeDecreaseItem })
            {
                return true;
            }
        }

        return false;
    }

    public override void Init()
    {
        base.Init();

        TextBox = new TextBoxDialog(Scene);
        Scene.AddDialog(TextBox, false, false);

        TimeAttackTextBoxTimer = 0;
        IsShowingTimeAttackTextBox = false;

        TgxTileLayer cloudsLayer = ((TgxPlayfield2D)Scene.Playfield).TileLayers[0];
        TextureScreenRenderer renderer;
        if (cloudsLayer.Screen.Renderer is MultiScreenRenderer multiScreenRenderer)
            renderer = (TextureScreenRenderer)multiScreenRenderer.Sections[0].ScreenRenderer;
        else
            renderer = (TextureScreenRenderer)cloudsLayer.Screen.Renderer;
        if (Rom.Platform == Platform.GBA || Engine.Settings.Active.Tweaks.UseGbaEffectsOnNGage)
        {
            cloudsLayer.Screen.Renderer = new LevelCloudsRenderer(renderer.Texture, [32, 120, 227]);
        }
        else
        {
            // Need to limit the background to 256 since the rest is just transparent
            renderer.TextureRectangle = renderer.TextureRectangle with { Width = 256 };
            cloudsLayer.Screen.Renderer = renderer;
        }
    }

    public override void Step()
    {
        if (Rayman3.TimeAttack.IsActive && Rayman3.TimeAttack.Mode == TimeAttackMode.Play && CurrentStepAction == _Step_Normal)
            TimeAttackTextBoxTimer++;

        base.Step();

        if (TimeAttackTextBoxTimer == 200 && !HasDestroyedAnyTimeDecreaseItems())
        {
            TextBox.SetCutsceneCharacter(TextBoxCutsceneCharacter.Murfy);
            TextBox.TextBankId = TextBankId.Readvanced;
            TextBox.SetText(0);
            TextBox.MoveInOurOut(true);
            IsShowingTimeAttackTextBox = true;
        }

        if (IsShowingTimeAttackTextBox && TimeAttackTextBoxTimer > 300 && HasDestroyedAnyTimeDecreaseItems())
        {
            TextBox.MoveInOurOut(false);
            IsShowingTimeAttackTextBox = false;
        }
    }
}