using System;
using BinarySerializer.Ubisoft.GbaEngine;
using GbaMonoGame.AnimEngine;
using GbaMonoGame.TgxEngine;
using Microsoft.Xna.Framework;
using Action = System.Action;

namespace GbaMonoGame.Rayman3;

// TODO: This could be improved a lot, adding options to show actor animations, set the palette index etc.

// Custom Frame class for viewing animations
public class AnimationViewer : Frame
{
    #region Public Properties

    public Action CurrentStepAction { get; set; }

    public TransitionsFX TransitionsFX { get; set; }
    public AnimationPlayer AnimationPlayer { get; set; }

    public SpriteTextObject SelectionText { get; set; }
    public AnimatedObject Animation { get; set; }

    public int SelectedResourceIndex { get; set; }
    public int SelectedAnimationIndex { get; set; }

    #endregion

    #region Private Methods

    private void InitSelectResource()
    {
        int resourcesCount = Rom.Loader.GameOffsetTable.Count;
        SetText($"Resource {SelectedResourceIndex}/{resourcesCount - 1}");

        CurrentStepAction = Step_SelectResource;
    }

    private void InitSelectAnimation()
    {
        AnimatedObjectResource resource = Rom.LoadResource<AnimatedObjectResource>(SelectedResourceIndex);
        Animation = new AnimatedObject(resource, resource.IsDynamic)
        {
            BgPriority = 0,
            ObjPriority = 0,
            ScreenPos = Vector2.Zero,
            HorizontalAnchor = HorizontalAnchorMode.Center,
            VerticalAnchor = VerticalAnchorMode.Center,
        };
        SelectedAnimationIndex = 0;

        int animationsCount = Animation.Resource.AnimationsCount;
        SetText($"Animation {SelectedAnimationIndex}/{animationsCount - 1}");

        CurrentStepAction = Step_SelectAnimation;
    }

    private void SetText(string text)
    {
        SelectionText.Text = text;
        SelectionText.ScreenPos = SelectionText.ScreenPos with { X = -SelectionText.GetStringWidth() / 2f };
    }

    #endregion

    #region Public Methods

    public override void Init()
    {
        TransitionsFX = new TransitionsFX(true);
        TransitionsFX.FadeInInit(2 / 16f);
        AnimationPlayer = new AnimationPlayer(false, SoundEventsManager.ProcessEvent);
        Gfx.ClearColor = Color.Black;

        SelectionText = new SpriteTextObject()
        {
            Text = String.Empty,
            Color = Color.White,
            FontSize = FontSize.Font16,
            ScreenPos = new Vector2(0, 10),
            HorizontalAnchor = HorizontalAnchorMode.Center,
        };

        InitSelectResource();
    }

    public override void Step()
    {
        CurrentStepAction();

        AnimationPlayer.Play(SelectionText);

        TransitionsFX.StepAll();
        AnimationPlayer.Execute();
    }

    #endregion

    #region Steps

    public void Step_SelectResource()
    {
        int resourcesCount = Rom.Loader.GameOffsetTable.Count;

        if (JoyPad.IsButtonJustPressed(GbaInput.Left))
        {
            SelectedResourceIndex--;

            if (SelectedResourceIndex < 0)
                SelectedResourceIndex = resourcesCount - 1;

            SetText($"Resource {SelectedResourceIndex}/{resourcesCount - 1}");
        }
        else if (JoyPad.IsButtonJustPressed(GbaInput.Right))
        {
            SelectedResourceIndex++;

            if (SelectedResourceIndex > resourcesCount - 1)
                SelectedResourceIndex = 0;

            SetText($"Resource {SelectedResourceIndex}/{resourcesCount - 1}");
        }
        else if (JoyPad.IsButtonJustPressed(GbaInput.A))
        {
            InitSelectAnimation();
        }
    }

    public void Step_SelectAnimation()
    {
        int animationsCount = Animation.Resource.AnimationsCount;

        if (JoyPad.IsButtonJustPressed(GbaInput.Left))
        {
            SelectedAnimationIndex--;

            if (SelectedAnimationIndex < 0)
                SelectedAnimationIndex = animationsCount - 1;

            SetText($"Animation {SelectedAnimationIndex}/{animationsCount - 1}");
            Animation.CurrentAnimation = SelectedAnimationIndex;
        }
        else if (JoyPad.IsButtonJustPressed(GbaInput.Right))
        {
            SelectedAnimationIndex++;

            if (SelectedAnimationIndex > animationsCount - 1)
                SelectedAnimationIndex = 0;

            SetText($"Animation {SelectedAnimationIndex}/{animationsCount - 1}");
            Animation.CurrentAnimation = SelectedAnimationIndex;
        }
        else if (JoyPad.IsButtonJustPressed(GbaInput.B))
        {
            InitSelectResource();
        }

        AnimationPlayer.Play(Animation);
    }

    #endregion
}