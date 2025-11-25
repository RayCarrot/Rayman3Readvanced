using System;
using System.Linq;
using BinarySerializer.Nintendo.GBA;
using BinarySerializer.Ubisoft.GbaEngine;
using GbaMonoGame.AnimEngine;
using GbaMonoGame.TgxEngine;
using Microsoft.Xna.Framework;
using Action = System.Action;

namespace GbaMonoGame.Rayman3;

// Custom Frame class for viewing animations
public class AnimationViewer : Frame
{
    #region Public Properties

    public Action CurrentStepAction { get; set; }

    public AnimationPlayer AnimationPlayer { get; set; }

    public SpriteTextObject SelectionText { get; set; }
    public SpriteTextObject InfoText { get; set; }
    public ActorResource[] Actors { get; set; }
    public AnimatedObject Animation { get; set; }

    public int SelectedResourceIndex { get; set; }
    public int SelectedActorIndex { get; set; }
    public int SelectedAnimationIndex { get; set; }

    public int HoldButtonTimer { get; set; }

    #endregion

    #region Private Methods

    private void InitSelectResource()
    {
        int resourcesCount = Rom.Loader.GameOffsetTable.Count;
        SetSelectionText($"Resource {SelectedResourceIndex}/{resourcesCount - 1} ({GetCurrentResourceType().Name})");

        CurrentStepAction = Step_SelectResource;
    }

    private void InitSelectActor()
    {
        Scene2DResource resource = Rom.LoadResource<Scene2DResource>(SelectedResourceIndex);

        SelectedActorIndex = 0;
        Actors = resource.Actors.
            Concat(resource.AlwaysActors).
            DistinctBy(x => x.Type).
            OrderBy(x => x.Type).
            ToArray();

        SetSelectionText($"Actor #{Actors[SelectedActorIndex].Type} ({(ActorType)Actors[SelectedActorIndex].Type})");

        CurrentStepAction = Step_SelectActor;
    }

    private void InitSelectAnimation()
    {
        AnimatedObjectResource resource;

        if (GetCurrentResourceType() == typeof(AnimatedObjectResource))
            resource = Rom.LoadResource<AnimatedObjectResource>(SelectedResourceIndex);
        else if (GetCurrentResourceType() == typeof(Scene2DResource))
            resource = Actors[SelectedActorIndex].Model.AnimatedObject;
        else
            throw new Exception("Invalid resource type");

        Animation = new AnimatedObject(resource, resource.IsDynamic)
        {
            BgPriority = 0,
            ObjPriority = 0,
            ScreenPos = Vector2.Zero,
            HorizontalAnchor = HorizontalAnchorMode.Center,
            VerticalAnchor = VerticalAnchorMode.Center,
            RenderContext = Engine.GameRenderContext,
        };
        SelectedAnimationIndex = 0;

        int animationsCount = Animation.Resource.AnimationsCount;
        SetSelectionText($"Animation {SelectedAnimationIndex}/{animationsCount - 1}");

        CurrentStepAction = Step_SelectAnimation;
    }

    private void SetSelectionText(string text)
    {
        SelectionText.Text = text;
        SelectionText.ScreenPos = SelectionText.ScreenPos with { X = -SelectionText.GetStringWidth() / 2f };
    }

    private void SetInfoText(string text)
    {
        InfoText.Text = text;
    }

    private bool IsDirectionalButtonPressed(Rayman3Input input)
    {
        bool left = JoyPad.IsButtonJustPressed(Rayman3Input.MenuLeft);
        bool right = JoyPad.IsButtonJustPressed(Rayman3Input.MenuRight);
        if (!left && !right)
        {
            if (JoyPad.IsButtonPressed(Rayman3Input.MenuLeft))
            {
                if (HoldButtonTimer > 20)
                    left = GameTime.ElapsedFrames % 5 == 0;
                else
                    HoldButtonTimer++;
            }
            else if (JoyPad.IsButtonPressed(Rayman3Input.MenuRight))
            {
                if (HoldButtonTimer > 15)
                    right = GameTime.ElapsedFrames % 5 == 0;
                else
                    HoldButtonTimer++;
            }
            else
            {
                HoldButtonTimer = 0;
            }
        }

        return left && input == Rayman3Input.MenuLeft ||
               right && input == Rayman3Input.MenuRight;
    }

    private Type GetCurrentResourceType()
    {
        GbaEngineSettings settings = Rom.Context.GetRequiredSettings<GbaEngineSettings>();
        return settings.GetDefinedResourceType(SelectedResourceIndex);
    }

    #endregion

    #region Public Methods

    public override void Init()
    {
        TransitionsFX.Init(true);
        TransitionsFX.FadeInInit(2);
        AnimationPlayer = new AnimationPlayer(false, SoundEventsManager.ProcessEvent);
        Gfx.ClearColor = Color.Fuchsia;

        SelectionText = new SpriteTextObject()
        {
            Text = String.Empty,
            FontSize = FontSize.Font32,
            Color = Color.White,
            ScreenPos = new Vector2(0, 10),
            HorizontalAnchor = HorizontalAnchorMode.Center,
            RenderContext = new FixedResolutionRenderContext(Engine.InternalGameResolution * 2),
        };

        InfoText = new SpriteTextObject()
        {
            Text = String.Empty,
            FontSize = FontSize.Font32,
            Color = Color.White,
            ScreenPos = new Vector2(30, 30),
            RenderContext = new FixedResolutionRenderContext(Engine.InternalGameResolution * 6),
        };

        HoldButtonTimer = 0;

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

        if (IsDirectionalButtonPressed(Rayman3Input.MenuLeft))
        {
            Type resourceType;
            do
            {
                SelectedResourceIndex--;

                if (SelectedResourceIndex < 0)
                    SelectedResourceIndex = resourcesCount - 1;

                resourceType = GetCurrentResourceType();
            } while (resourceType != typeof(Scene2DResource) && resourceType != typeof(AnimatedObjectResource));

            SetSelectionText($"Resource {SelectedResourceIndex}/{resourcesCount - 1} ({resourceType.Name})");
        }
        else if (IsDirectionalButtonPressed(Rayman3Input.MenuRight))
        {
            Type resourceType;
            do
            {
                SelectedResourceIndex++;

                if (SelectedResourceIndex > resourcesCount - 1)
                    SelectedResourceIndex = 0;

                resourceType = GetCurrentResourceType();
            } while (resourceType != typeof(Scene2DResource) && resourceType != typeof(AnimatedObjectResource));

            SetSelectionText($"Resource {SelectedResourceIndex}/{resourcesCount - 1} ({resourceType.Name})");
        }
        else if (JoyPad.IsButtonJustPressed(Rayman3Input.MenuConfirm))
        {
            if (GetCurrentResourceType() == typeof(AnimatedObjectResource))
                InitSelectAnimation();
            else if (GetCurrentResourceType() == typeof(Scene2DResource))
                InitSelectActor();
        }
    }

    public void Step_SelectActor()
    {
        int actorsCount = Actors.Length;

        if (IsDirectionalButtonPressed(Rayman3Input.MenuLeft))
        {
            SelectedActorIndex--;

            if (SelectedActorIndex < 0)
                SelectedActorIndex = actorsCount - 1;

            SetSelectionText($"Actor #{Actors[SelectedActorIndex].Type} ({(ActorType)Actors[SelectedActorIndex].Type})");
        }
        else if (IsDirectionalButtonPressed(Rayman3Input.MenuRight))
        {
            SelectedActorIndex++;

            if (SelectedActorIndex > actorsCount - 1)
                SelectedActorIndex = 0;

            SetSelectionText($"Actor #{Actors[SelectedActorIndex].Type} ({(ActorType)Actors[SelectedActorIndex].Type})");
        }
        else if (JoyPad.IsButtonJustPressed(Rayman3Input.MenuConfirm))
        {
            InitSelectAnimation();
        }
        else if (JoyPad.IsButtonJustPressed(Rayman3Input.MenuBack))
        {
            InitSelectResource();
        }
    }

    public void Step_SelectAnimation()
    {
        int animationsCount = Animation.Resource.AnimationsCount;

        if (IsDirectionalButtonPressed(Rayman3Input.MenuLeft))
        {
            SelectedAnimationIndex--;

            if (SelectedAnimationIndex < 0)
                SelectedAnimationIndex = animationsCount - 1;

            SetSelectionText($"Animation {SelectedAnimationIndex}/{animationsCount - 1}");
            Animation.CurrentAnimation = SelectedAnimationIndex;
        }
        else if (IsDirectionalButtonPressed(Rayman3Input.MenuRight))
        {
            SelectedAnimationIndex++;

            if (SelectedAnimationIndex > animationsCount - 1)
                SelectedAnimationIndex = 0;

            SetSelectionText($"Animation {SelectedAnimationIndex}/{animationsCount - 1}");
            Animation.CurrentAnimation = SelectedAnimationIndex;
        }
        else if (JoyPad.IsButtonJustPressed(Rayman3Input.MenuConfirm))
        {
            Animation.CurrentFrame = 0;
        }
        else if (JoyPad.IsButtonJustPressed(Rayman3Input.MenuBack))
        {
            if (GetCurrentResourceType() == typeof(Scene2DResource))
                InitSelectActor();
            else
                InitSelectResource();
        }

        SetInfoText($"Frame: {Animation.CurrentFrame}/{Animation.GetAnimation().FramesCount - 1}\n" +
                    $"DelayMode: {Animation.IsDelayMode}\n" +
                    $"Timer: {Animation.Timer}\n" +
                    $"ChannelIndex: {Animation.ChannelIndex}\n\n" +
                    $"{String.Join("\n", Animation.EnumerateCurrentChannels().
                        Select((x, i) => 
                        {
                            string str = $"{i}: {x.ChannelType}";

                            if (x.ChannelType == AnimationChannelType.Sprite)
                            {
                                str += $" {x.XPosition} x {x.YPosition}";

                                if (x.ObjectMode is OBJ_ATTR_ObjectMode.AFF or OBJ_ATTR_ObjectMode.AFF_DBL)
                                {
                                    AffineMatrixResource matrix = Animation.GetAnimation().AffineMatrices.Matrices[x.AffineMatrixIndex];
                                    AffineMatrix affineMatrix = new(
                                        pa: matrix.Pa,
                                        pb: matrix.Pb,
                                        pc: matrix.Pc,
                                        pd: matrix.Pd);

                                    str += $" | Scale: {affineMatrix.Scale.X} x {affineMatrix.Scale.Y} | Rot: {affineMatrix.Rotation}";
                                }
                            }

                            return str;
                        }))}");

        AnimationPlayer.Play(Animation);
        AnimationPlayer.Play(InfoText);
    }

    #endregion
}