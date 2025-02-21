using BinarySerializer.Ubisoft.GbaEngine.Rayman3;

namespace GbaMonoGame.Rayman3;

public class MarshAwakening1 : FrameWaterSkiMode7
{
    public MarshAwakening1(MapId mapId) : base(mapId)
    {
        HasShownTextBox = false;
        CanShowTextBox = false;
    }

    public TextBoxDialog TextBox { get; set; }
    public uint TextBoxTimer { get; set; }
    public byte TextBoxCountdown { get; set; }
    public bool CanShowTextBox { get; set; }
    public bool HasShownTextBox { get; set; }

    private void ShowTextBox()
    {
        TextBox.MoveInOurOut(true);
        TextBoxCountdown = 240;
    }

    public override void Init()
    {
        base.Init();
        
        CameraMode7 cam = (CameraMode7)Scene.Camera;
        cam.IsWaterSki = true;
        cam.MainActorDistance = 85;

        TextBox = new TextBoxDialog(Scene);
        Scene.AddDialog(TextBox, false, false);
        
        TextBoxCountdown = 0;
        TextBoxTimer = 0;

        TextBox.SetCutsceneCharacter(TextBoxCutsceneCharacter.Murfy);
        TextBox.SetText(6);
    }

    public override void UnInit()
    {
        base.UnInit();
        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Stop__SkiLoop1);
    }

    public override void Step()
    {
        TextBoxTimer++;
        
        base.Step();
        
        if (TextBoxTimer == 60 && !HasShownTextBox && CanShowTextBox)
        {
            ShowTextBox();
            HasShownTextBox = true;
        }

        if (TextBoxCountdown != 0)
        {
            TextBoxCountdown--;

            if (TextBoxCountdown == 0)
                TextBox.MoveInOurOut(false);
        }
    }
}