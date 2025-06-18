using System;
using System.IO;
using BinarySerializer.Ubisoft.GbaEngine;
using GbaMonoGame.AnimEngine;
using Microsoft.Xna.Framework.Input;

namespace GbaMonoGame.Rayman3.Readvanced;

public class FrameFatalError : Frame
{
    public FrameFatalError(Exception exception)
    {
        Exception = exception;
    }

    public Exception Exception { get; }

    public AnimationPlayer AnimationPlayer { get; set; }
    public SpriteFontTextObject Text { get; set; }

    public override void Init()
    {
        try
        {
            string crashLogFilePath = FileManager.GetDataFile(Engine.CrashlogFileName);
            File.WriteAllText(crashLogFilePath, Exception?.ToString());
        }
        catch
        {
            // Ignore
        }

        Gfx.FadeControl = FadeControl.None;
        Gfx.Fade = 0;

        if (SoundEventsManager.IsLoaded)
            SoundEventsManager.StopAllSongs();

        AnimationPlayer = new AnimationPlayer(false, null);

        Text = new SpriteFontTextObject
        {
            ScreenPos = new Vector2(5, 5),
            RenderContext = Engine.GameRenderContext,
            Text = $"FATAL ERROR - Press any button to quit\n\n\n{Exception}",
            Font = ReadvancedFonts.MenuYellow,
            AffineMatrix = new AffineMatrix(0, new Vector2(0.3f))
        };

        Text.WrapText(Text.RenderContext.Resolution.X - Text.ScreenPos.X - 50);
    }

    public override void Step()
    {
        if (JoyPad.IsButtonJustPressed(GbaInput.A) ||
            JoyPad.IsButtonJustPressed(GbaInput.B) ||
            JoyPad.IsButtonJustPressed(GbaInput.Start) ||
            JoyPad.IsButtonJustPressed(GbaInput.Select) ||
            InputManager.IsButtonJustPressed(Keys.Escape))
        {
            Engine.GbaGame.Exit();
        }

        AnimationPlayer.Play(Text);

        AnimationPlayer.Execute();
    }
}