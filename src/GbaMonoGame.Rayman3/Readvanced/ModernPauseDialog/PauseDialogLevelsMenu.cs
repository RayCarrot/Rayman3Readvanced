using System.Collections.Generic;
using System.Linq;
using BinarySerializer.Ubisoft.GbaEngine;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.AnimEngine;
using GbaMonoGame.Engine2d;
using Microsoft.Xna.Framework.Graphics;

namespace GbaMonoGame.Rayman3.Readvanced;

public class PauseDialogLevelsMenu
{
    public PauseDialogLevelsMenu(Scene2D scene)
    {
        Scene = scene;
        RenderContext = new FixedResolutionRenderContext(Resolution.Modern, verticalAlignment: VerticalAlignment.Top);
    }

    private const float TransitionHeight = 220;
    private const float TransitionSpeed = 5;

    private const float TextScale = 5 / 6f;

    private const float CanvasBaseY = 0;
    private const float CursorBaseY = 43;

    private const float LineHeight = 16;

    public Scene2D Scene { get; }
    public RenderContext RenderContext { get; }

    public float? CursorStartY { get; set; }
    public float? CursorDestY { get; set; }

    public LevelsMenuOption[] Options { get; set; }
    public int SelectedOption { get; set; }
    public bool HasSelectedLevel { get; set; }

    public SpriteTextureObject Canvas { get; set; }
    public AnimatedObject Cursor { get; set; }

    public float OffsetY { get; set; }
    public float CursorOffsetY { get; set; }
    public PauseDialogDrawStep DrawStep { get; set; }

    private Vector2 GetOptionPosition(int index) => new(75, 30 + LineHeight * index - OffsetY);

    private void SetCursorMovement(float startY, float endY)
    {
        CursorStartY = startY;
        CursorDestY = endY;
    }

    private void ManageCursor()
    {
        // Move with a constant speed of 4
        const float speed = 4;

        if (CursorStartY != null && CursorDestY != null)
        {
            float startY = CursorStartY.Value;
            float destY = CursorDestY.Value;

            // Move up
            if (destY < startY && CursorOffsetY > destY)
            {
                CursorOffsetY -= speed;
            }
            // Move down
            else if (destY > startY && CursorOffsetY < destY)
            {
                CursorOffsetY += speed;
            }
            // Finished moving
            else
            {
                CursorOffsetY = destY;
                CursorStartY = null;
                CursorDestY = null;
            }
        }
    }

    private void CursorClick()
    {
        Cursor.CurrentAnimation = 16;

        if (!SoundEventsManager.IsSongPlaying(Rayman3SoundEvent.Play__Valid01_Mix01))
            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Valid01_Mix01);
    }

    private void InvalidCursorClick()
    {
        Cursor.CurrentAnimation = 16;
        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Back01_Mix01);
    }

    private void SetSelectedOption(int selectedOption, bool playSound = true)
    {
        int prevSelectedOption = SelectedOption;

        int newSelectedOption = selectedOption;

        if (newSelectedOption > Options.Length - 1)
            newSelectedOption = 0;
        else if (newSelectedOption < 0)
            newSelectedOption = Options.Length - 1;

        SetCursorMovement(CursorOffsetY, newSelectedOption * LineHeight);

        SelectedOption = newSelectedOption;
        Options[prevSelectedOption].ChangeIsSelected(false);
        Options[newSelectedOption].ChangeIsSelected(true);

        if (playSound)
            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__MenuMove);
    }

    public void MoveIn()
    {
        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Store01_Mix01);

        OffsetY = TransitionHeight;
        DrawStep = PauseDialogDrawStep.MoveIn;
    }

    public void MoveOut()
    {
        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Store01_Mix01);
        
        OffsetY = 0;
        DrawStep = PauseDialogDrawStep.MoveOut;
    }

    public void Load()
    {
        // Create animations
        AnimatedObjectResource propsAnimations = Rom.LoadResource<AnimatedObjectResource>(Rayman3DefinedResource.MenuPropAnimations);
        Texture2D canvasTexture = Engine.FixContentManager.Load<Texture2D>(Assets.Menu.OptionsDialogBoard);

        Canvas = new SpriteTextureObject
        {
            BgPriority = 0,
            ObjPriority = 0,
            ScreenPos = new Vector2(0, CanvasBaseY),
            RenderContext = RenderContext,
            Texture = canvasTexture,
        };

        Cursor = new AnimatedObject(propsAnimations, propsAnimations.IsDynamic)
        {
            IsFramed = true,
            BgPriority = 0,
            ObjPriority = 0,
            ScreenPos = new Vector2(33, CursorBaseY),
            CurrentAnimation = 0,
            RenderContext = RenderContext,
        };

        // Add the levels from the level curtains
        SelectedOption = 0;
        List<LevelsMenuOption> options = [];
        foreach (BaseActor actor in Scene.Iterate<BaseActor>(IteratorFlags.Actor))
        {
            if (actor.Type == (int)ActorType.LevelCurtain)
            {
                // Get the level curtain ID
                int levelCurtainId = (int)((LevelCurtain)actor).InitialActionId;

                // Determine if any lums or cages have been collected
                bool hasCollectedLumsOrCages = GameInfo.LevelMaps[levelCurtainId].
                    Any(x => GameInfo.GetDeadLumsForCurrentMap(x) > 0 || GameInfo.GetDeadCagesForCurrentMap(x) > 0);

                // Determine if the level is available in the menu
                MapId mapId = GameInfo.LevelMaps[levelCurtainId][0];
                bool isAvailable = mapId switch
                {
                    MapId.Bonus1 or MapId.Bonus2 or MapId.Bonus3 or MapId.Bonus4 => hasCollectedLumsOrCages,
                    MapId._1000Lums => GameInfo.AreAllLumsDead(),
                    MapId.ChallengeLy1 => GameInfo.PersistentInfo.FinishedLyChallenge1,
                    MapId.ChallengeLy2 => GameInfo.PersistentInfo.FinishedLyChallenge2,
                    MapId.ChallengeLyGCN => GameInfo.PersistentInfo.FinishedLyChallengeGCN,
                    _ => GameInfo.PersistentInfo.LastCompletedLevel >= (int)mapId || hasCollectedLumsOrCages
                };

                options.Add(new LevelsMenuOption(levelCurtainId, isAvailable, TextScale));
            }
        }
        Options = options.OrderBy(x => GameInfo.LevelMaps[x.LevelCurtainId][0]).ToArray();

        for (int i = 0; i < Options.Length; i++)
        {
            LevelsMenuOption option = Options[i];
            option.Init(0, RenderContext, i);
            option.IsInitialized = true;
        }

        SetSelectedOption(0, false);
    }

    public void Step()
    {
        if (DrawStep != PauseDialogDrawStep.Wait)
            return;

        if (!HasSelectedLevel)
        {
            if (JoyPad.IsButtonJustPressed(Rayman3Input.MenuUp))
            {
                SetSelectedOption(SelectedOption - 1);
            }
            else if (JoyPad.IsButtonJustPressed(Rayman3Input.MenuDown))
            {
                SetSelectedOption(SelectedOption + 1);
            }
            else if (JoyPad.IsButtonJustPressed(Rayman3Input.MenuConfirm))
            {
                LevelsMenuOption option = Options[SelectedOption];
                if (option.IsAvailable)
                {
                    HasSelectedLevel = true;
                    CursorClick();
                }
                else
                {
                    InvalidCursorClick();
                }
            }
            else if (JoyPad.IsButtonJustPressed(Rayman3Input.MenuBack))
            {
                // Go back
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Back01_Mix01);
                MoveOut();
            }
        }

        // End cursor click animation
        if (Cursor.CurrentAnimation == 16 && Cursor.EndOfAnimation)
        {
            Cursor.CurrentAnimation = 0;

            // Go to level if selected
            if (HasSelectedLevel)
            {
                LevelsMenuOption option = Options[SelectedOption];
                GameInfo.PersistentInfo.LastPlayedLevel = (byte)GameInfo.LevelMaps[option.LevelCurtainId][0];
                GameTime.Resume();
                SoundEventsManager.StopAllSongs();
                FrameManager.ReloadCurrentFrame();
            }
        }

        ManageCursor();
    }

    public void Draw(AnimationPlayer animationPlayer)
    {
        switch (DrawStep)
        {
            case PauseDialogDrawStep.Hide:
                OffsetY = TransitionHeight;
                break;

            case PauseDialogDrawStep.MoveIn:
                if (OffsetY > 0)
                    OffsetY -= TransitionSpeed;
                else
                    OffsetY = 0;

                if (OffsetY <= 0)
                    DrawStep = PauseDialogDrawStep.Wait;
                break;

            case PauseDialogDrawStep.MoveOut:
                if (OffsetY < TransitionHeight)
                    OffsetY += TransitionSpeed;
                else
                    OffsetY = TransitionHeight;

                if (Frame.Current is not FrameWorldSideScroller frameWorldSideScroller ||
                    frameWorldSideScroller.UserInfo.HasFinishedMovingInCurtains())
                {
                    if (OffsetY >= TransitionHeight)
                        DrawStep = PauseDialogDrawStep.Hide;
                }
                break;
        }

        if (DrawStep != PauseDialogDrawStep.Hide)
        {
            // Transition
            Canvas.ScreenPos = Canvas.ScreenPos with { Y = CanvasBaseY - OffsetY };
            Cursor.ScreenPos = Cursor.ScreenPos with { Y = CursorBaseY + CursorOffsetY - OffsetY };

            int index = 0;
            foreach (LevelsMenuOption option in Options)
            {
                option.SetPosition(GetOptionPosition(index));
                index++;
            }

            // Draw
            animationPlayer.Play(Canvas);
            animationPlayer.Play(Cursor);
            foreach (LevelsMenuOption option in Options)
                option.Draw(animationPlayer);
        }
    }
}