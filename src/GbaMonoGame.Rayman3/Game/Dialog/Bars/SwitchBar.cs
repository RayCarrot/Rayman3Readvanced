using System;
using BinarySerializer.Ubisoft.GbaEngine;
using GbaMonoGame.AnimEngine;
using Microsoft.Xna.Framework;

namespace GbaMonoGame.Rayman3;

public class SwitchBar : Bar
{
    public SwitchBar(Scene2D scene) : base(scene) { }

    public int OffsetY { get; set; } = 35;
    public int ActivatedSwitches { get; set; }
    public int PaletteShiftValue { get; set; }
    public int PaletteShiftDirection { get; set; }

    public AnimatedObject Switches { get; set; }

    public void ActivateSwitch()
    {
        ActivatedSwitches++;
        Switches.CurrentAnimation = ActivatedSwitches;
    }

    public override void Load()
    {
        AnimatedObjectResource resource = Rom.LoadResource<AnimatedObjectResource>(Rayman3DefinedResource.SwitchBarAnimations);

        Switches = new AnimatedObject(resource, false)
        {
            IsFramed = true,
            CurrentAnimation = ActivatedSwitches,
            ScreenPos = new Vector2(-40, -4),
            HorizontalAnchor = HorizontalAnchorMode.Right,
            VerticalAnchor = VerticalAnchorMode.Bottom,
            BgPriority = 0,
            ObjPriority = 0,
            RenderContext = Scene.HudRenderContext,
        };

        // The original game dynamically modifies the loaded palette in the Draw method to make
        // the switches appear like they're glowing. The easiest way to replicate it here is to
        // create separate palettes that we cycle between.
        Color[] originalPalette = Switches.Palettes.Palettes[0].Colors;
        SpritePalettes newPalettes = new(
            palettes: new Palette[9],
            // Set the pointer as the original plus 1 so it gets cached differently
            cachePointer: Switches.Palettes.CachePointer + 1);

        for (int i = 0; i < newPalettes.Palettes.Length; i++)
        {
            Color[] colors = new Color[originalPalette.Length];
            Array.Copy(originalPalette, colors, originalPalette.Length);

            colors[2] = ColorHelpers.FromRGB555((i + 23) * 0x20);
            colors[9] = ColorHelpers.FromRGB555((i + 4) * 0x400 | 0x160 | i + 23);

            newPalettes.Palettes[i] = new Palette(colors, null);
        }

        // Override the palettes
        Switches.Palettes = newPalettes;

        PaletteShiftValue = 0;
        PaletteShiftDirection = 0;
    }

    public override void Set() { }

    public override void Draw(AnimationPlayer animationPlayer)
    {
        if (((FrameSideScroller)Frame.Current).CurrentStepAction == ((FrameSideScroller)Frame.Current).Step_Normal)
            Switches.BasePaletteIndex = PaletteShiftValue;

        if ((GameTime.ElapsedFrames & 3) == 3)
        {
            if (PaletteShiftDirection == 0)
            {
                PaletteShiftValue++;
                if (PaletteShiftValue == 8)
                    PaletteShiftDirection = 1;
            }
            else
            {
                PaletteShiftValue--;
                if (PaletteShiftValue == 0)
                    PaletteShiftDirection = 0;
            }
        }
        if (Mode is BarMode.StayHidden or BarMode.Disabled)
            return;

        switch (DrawStep)
        {
            case BarDrawStep.Hide:
                OffsetY = 35;
                break;

            case BarDrawStep.MoveIn:
                if (OffsetY > 0)
                    OffsetY -= 2;
                else
                    DrawStep = BarDrawStep.Wait;
                break;

            case BarDrawStep.MoveOut:
                if (OffsetY < 35)
                {
                    OffsetY++;
                }
                else
                {
                    OffsetY = 35;
                    DrawStep = BarDrawStep.Hide;
                }
                break;
        }

        if (DrawStep != BarDrawStep.Hide)
        {
            Switches.ScreenPos = Switches.ScreenPos with { Y = -4 + OffsetY };
            
            animationPlayer.PlayFront(Switches);
        }
    }
}