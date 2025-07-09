using System;
using System.Collections.Generic;

namespace GbaMonoGame.TgxEngine;

public static class TransitionsFX
{
    public static void Init(bool clear)
    {
        Screns = new List<int>();
        AlphaStep = null;
        BrightnessCoefficient = MaxAlpha;
        FadeCoefficient = MinAlpha;

        if (clear)
            Gfx.FadeControl = FadeControl.None;
    }

    private const float MinAlpha = 0;
    private const float MaxAlpha = 16;

    // Screen alpha blending
    public static float FadeStepSize { get; set; }

    public static float FadeCoefficient { get; set; }
    public static float BrightnessCoefficient { get; set; }

    public static bool IsFadingOut => BrightnessCoefficient != MaxAlpha;
    public static bool IsFadingIn => FadeCoefficient != MinAlpha;

    // Background alpha blending
    public static float? AlphaStep { get; set; }
    public static float AlphaCoefficient { get; set; }
    public static List<int> Screns { get; set; }

    public static void StepAll()
    {
        // The game only runs this in 15 fps (every 4 frames), but we want to do it every frame
        float stepSize = FadeStepSize / 4;

        // Fade out
        if (BrightnessCoefficient < MaxAlpha)
        {
            BrightnessCoefficient += stepSize;

            if (BrightnessCoefficient > MaxAlpha)
                BrightnessCoefficient = MaxAlpha;

            Gfx.GbaFade = BrightnessCoefficient;
        }
        // Fade in
        else if (FadeCoefficient != MinAlpha)
        {
            FadeCoefficient -= stepSize;

            if (FadeCoefficient < MinAlpha)
                FadeCoefficient = MinAlpha;

            Gfx.GbaFade = FadeCoefficient;

            if (FadeCoefficient == MinAlpha)
                Gfx.FadeControl = FadeControl.None;
        }
        else
        {
            if (Screns.Count != 0 && AlphaStep != null)
                AlphaBlendingStep();
        }
    }

    public static void StepFadeIn()
    {
        // The game only runs this in 30 fps (every 2 frames), but we want to do it every frame
        float stepSize = FadeStepSize / 2;

        if (IsFadingIn)
        {
            FadeCoefficient -= stepSize;

            if (FadeCoefficient < MinAlpha)
                FadeCoefficient = MinAlpha;

            Gfx.GbaFade = FadeCoefficient;
        }
    }

    public static void StepFadeOut()
    {
        // The game only runs this in 30 fps (every 2 frames), but we want to do it every frame
        float stepSize = FadeStepSize / 2;

        if (IsFadingOut)
        {
            BrightnessCoefficient += stepSize;

            if (BrightnessCoefficient > MaxAlpha)
                BrightnessCoefficient = MaxAlpha;

            Gfx.GbaFade = BrightnessCoefficient;
        }
    }

    public static void SetBGAlphaBlending(GfxScreen screen, float alphaCoefficient)
    {
        AlphaStep = 0;
        AlphaCoefficient = alphaCoefficient;
        screen.IsEnabled = false;
        Screns.Add(screen.Id);
    }

    public static void ApplyAlphaSettings(float coefficient)
    {
        foreach (int screen in Screns)
            Gfx.GetScreen(screen).GbaAlpha = coefficient;
    }

    public static void AlphaBlendingStep()
    {
        // The game only runs this every 8 frames, but we want to do it every frame
        float speed = 1 / 8f;

        if (AlphaStep == null)
        {
            throw new Exception("Incorrect use of this function");
        }
        // Fade in
        else if (AlphaStep < 127)
        {
            if (AlphaStep > AlphaCoefficient)
            {
                AlphaStep = null;
            }
            else
            {
                ApplyAlphaSettings(AlphaStep.Value);

                if (AlphaStep == 0)
                    foreach (int screen in Screns)
                        Gfx.GetScreen(screen).IsEnabled = true;

                AlphaStep += speed;
            }
        }
        // Fade out
        else
        {
            if (AlphaCoefficient < 255 - AlphaStep)
            {
                AlphaStep = null;

                foreach (int screen in Screns)
                    Gfx.GetScreen(screen).IsEnabled = false;

                if (BrightnessCoefficient > MaxAlpha)
                {
                    FadeStepSize = BrightnessCoefficient - MaxAlpha;
                    BrightnessCoefficient = MinAlpha;
                    AlphaStep = 0;

                    Gfx.FadeControl = new FadeControl(FadeMode.BrightnessDecrease);
                    Gfx.GbaFade = MinAlpha;
                }
            }
            else
            {
                ApplyAlphaSettings((AlphaStep.Value + AlphaCoefficient + 1) % 256);
                AlphaStep -= speed;
            }
        }
    }

    public static void FadeInInit(float stepSize)
    {
        Gfx.FadeControl = new FadeControl(FadeMode.BrightnessDecrease);
        Gfx.GbaFade = MaxAlpha;

        FadeCoefficient = MaxAlpha;
        FadeStepSize = stepSize;
    }

    public static void FadeOutInit(float stepSize)
    {
        if (Screns.Count == 0)
        {
            Gfx.FadeControl = new FadeControl(FadeMode.BrightnessDecrease);
            Gfx.GbaFade = MinAlpha;

            BrightnessCoefficient = MinAlpha;
            FadeStepSize = stepSize;
        }
        else
        {
            AlphaStep = 255;
            BrightnessCoefficient = MaxAlpha + stepSize;
        }
    }
}