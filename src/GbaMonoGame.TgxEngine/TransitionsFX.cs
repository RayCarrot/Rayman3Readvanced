namespace GbaMonoGame.TgxEngine;

public class TransitionsFX
{
    public TransitionsFX(bool clear)
    {
        if (clear)
            Gfx.FadeControl = FadeControl.None;
    }

    public float FadeCoefficient { get; set; }
    public float BrightnessCoefficient { get; set; } = 1;

    // TODO: Should maybe be renamed and reversed to "IsFadingIn" and "IsFadingOut". Code in FrameNewPower is more readable then.
    public bool IsFadeOutFinished => BrightnessCoefficient == 1;
    public bool IsFadeInFinished => FadeCoefficient == 0;

    public float StepSize { get; set; }

    public void StepAll()
    {
        // The game only runs this in 15 fps (every 4 frames), but we want to do it every frame
        float stepSize = StepSize / 4;

        if (BrightnessCoefficient < 1)
        {
            BrightnessCoefficient += stepSize;

            if (FadeCoefficient > 1)
                FadeCoefficient = 1;

            Gfx.Fade = BrightnessCoefficient;
        }
        else if (FadeCoefficient == 0)
        {
            // TODO: Implement
            Logger.NotImplemented("Not implemented transition when fade coefficient is 0");
        }
        else
        {
            FadeCoefficient -= stepSize;

            if (FadeCoefficient < 0)
                FadeCoefficient = 0;

            Gfx.Fade = FadeCoefficient;

            if (FadeCoefficient == 0)
                Gfx.FadeControl = FadeControl.None;
        }
    }

    public void StepFadeIn()
    {
        // The game only runs this in 30 fps (every 2 frames), but we want to do it every frame
        float stepSize = StepSize / 2;

        if (!IsFadeInFinished)
        {
            FadeCoefficient -= stepSize;

            if (FadeCoefficient < 0)
                FadeCoefficient = 0;

            Gfx.Fade = FadeCoefficient;
        }
    }

    public void StepFadeOut()
    {
        // The game only runs this in 30 fps (every 2 frames), but we want to do it every frame
        float stepSize = StepSize / 2;

        if (!IsFadeOutFinished)
        {
            BrightnessCoefficient += stepSize;

            if (BrightnessCoefficient > 1)
                BrightnessCoefficient = 1;

            Gfx.Fade = BrightnessCoefficient;
        }
    }

    // TODO: Pass in step size as 0-16 value like game does
    public void FadeInInit(float stepSize)
    {
        Gfx.FadeControl = new FadeControl(FadeMode.BrightnessDecrease);
        Gfx.Fade = 1;

        FadeCoefficient = 1;
        StepSize = stepSize;
    }

    public void FadeOutInit(float stepSize)
    {
        Gfx.FadeControl = new FadeControl(FadeMode.BrightnessDecrease);
        Gfx.Fade = 0;
        
        BrightnessCoefficient = 0;
        StepSize = stepSize;
    }
}