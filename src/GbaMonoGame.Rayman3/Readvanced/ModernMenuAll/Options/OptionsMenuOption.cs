﻿using System;
using System.Collections.Generic;
using GbaMonoGame.AnimEngine;

namespace GbaMonoGame.Rayman3.Readvanced;

public abstract class OptionsMenuOption : MenuOption
{
    protected OptionsMenuOption(string text, string infoText, bool isDebugOption = false)
    {
        Text = text;
        InfoText = infoText;
        IsDebugOption = isDebugOption;
    }

    private const float TextScale = 2 / 3f;
    private const float ValueTextScale = 2 / 3f;
    private const float ValueTextXPosition = 165;
    private const float ValueTextPadding = 5;

    public string Text { get; }
    public string InfoText { get; }
    public bool IsDebugOption { get; }

    public SpriteFontTextObject TextObject { get; set; }
    public SpriteFontTextObject ValueTextObject { get; set; }
    
    public abstract bool ShowArrows { get; }
    public Vector2 ArrowLeftPosition { get; set; }
    public Vector2 ArrowRightPosition { get; set; }

    protected void UpdateArrowPositions()
    {
        float valueTextWidth = ValueTextObject.Font.GetWidth(ValueTextObject.Text) * ValueTextScale;
        ArrowLeftPosition = ValueTextObject.ScreenPos + new Vector2(-ValueTextPadding, -2);
        ArrowRightPosition = ValueTextObject.ScreenPos + new Vector2(valueTextWidth + ValueTextPadding, -1);
    }

    public abstract void Reset(IReadOnlyList<OptionsMenuOption> options);
    public virtual bool HasPresetDefined(Enum preset) => false;
    public virtual Enum[] GetUsedPresets() => [];
    public virtual void ApplyFromPreset(IReadOnlyList<OptionsMenuOption> options, Enum preset) { }
    public abstract EditStepResult EditStep(IReadOnlyList<OptionsMenuOption> options);

    public override void Init(int bgPriority, RenderContext renderContext, int index)
    {
        TextObject = new SpriteFontTextObject()
        {
            BgPriority = bgPriority,
            ObjPriority = 0,
            RenderContext = renderContext,
            AffineMatrix = new AffineMatrix(0, new Vector2(TextScale), false, false),
            Text = Text,
            Font = ReadvancedFonts.MenuYellow,
        };

        ValueTextObject = new SpriteFontTextObject()
        {
            BgPriority = bgPriority,
            ObjPriority = 0,
            RenderContext = renderContext,
            AffineMatrix = new AffineMatrix(0, new Vector2(ValueTextScale), false, false),
            Font = ReadvancedFonts.MenuYellow,
        };
    }

    public override void SetPosition(Vector2 position)
    {
        TextObject.ScreenPos = position + new Vector2(0, 13 * TextScale);
        ValueTextObject.ScreenPos = position + new Vector2(ValueTextXPosition, 13 * ValueTextScale);
    }

    public override void ChangeIsSelected(bool isSelected)
    {
        TextObject.Font = isSelected ? ReadvancedFonts.MenuWhite : ReadvancedFonts.MenuYellow;
        ValueTextObject.Font = isSelected ? ReadvancedFonts.MenuWhite : ReadvancedFonts.MenuYellow;
    }

    public override void Draw(AnimationPlayer animationPlayer)
    {
        animationPlayer.Play(TextObject);
        animationPlayer.Play(ValueTextObject);
    }

    public enum EditStepResult
    {
        None,
        Apply,
        Cancel,
    }
}