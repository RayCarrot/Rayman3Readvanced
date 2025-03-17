using System;
using System.Collections.Generic;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.AnimEngine;
using GbaMonoGame.TgxEngine;

namespace GbaMonoGame.Rayman3;

public abstract class MenuPage
{
    protected MenuPage(ModernMenuAll menu)
    {
        Menu = menu;
    }

    public ModernMenuAll Menu { get; }
    public RenderContext RenderContext => Menu.Playfield.RenderContext;
    public List<MenuOption> Options { get; } = [];
    public MenuPageState State { get; set; }
    public int TransitionValue { get; set; } // 0-160
    public Action ClickCallback { get; set; }
    public Action FadeOutCallback { get; set; }

    public int SelectedOption { get; set; }

    public abstract bool UsesCursor { get; }
    public abstract int BackgroundPalette { get; }
    public abstract int LineHeight { get; }

    protected virtual void Init() { }
    protected virtual void Step_TransitionIn() { }
    protected virtual void Step_Active() { }
    protected virtual void Step_TransitionOut() { }
    protected virtual void UnInit() { }
    protected virtual void Draw(AnimationPlayer animationPlayer) { }

    protected void DrawOptions(AnimationPlayer animationPlayer)
    {
        foreach (MenuOption option in Options)
            option.Draw(animationPlayer);
    }

    protected void ClearOptions()
    {
        SelectedOption = 0;
        Options.Clear();
    }

    protected void AddOption(MenuOption option)
    {
        int index = Options.Count;
        Options.Add(option);

        if (!option.IsInitialized)
        {
            option.Init(Menu, RenderContext, new Vector2(75, 54 + LineHeight * index), index);
            option.IsInitialized = true;
        }

        option.ChangeIsSelected(index == SelectedOption);
    }

    protected virtual bool SetSelectedOption(int selectedOption, bool playSound = true, bool forceUpdate = false)
    {
        int prevSelectedOption = SelectedOption;

        int newSelectedOption = selectedOption;
        if (newSelectedOption > Options.Count - 1)
            newSelectedOption = 0;
        else if (newSelectedOption < 0)
            newSelectedOption = Options.Count - 1;

        bool changed = Menu.SetCursorTarget(newSelectedOption);

        if (changed || forceUpdate)
        {
            SelectedOption = newSelectedOption;
            Options[prevSelectedOption].ChangeIsSelected(false);
            Options[newSelectedOption].ChangeIsSelected(true);

            if (playSound)
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__MenuMove);
        }

        return changed || forceUpdate;
    }

    protected void CursorClick(Action callback)
    {
        Menu.CursorClick();
        ClickCallback = callback;
    }

    protected void InvalidCursorClick()
    {
        Menu.InvalidCursorClick();
    }

    protected void FadeOut(float stepSize, Action callback)
    {
        Menu.TransitionsFX.FadeOutInit(stepSize);
        FadeOutCallback = callback;
    }

    public void Step()
    {
        switch (State)
        {
            case MenuPageState.Init:
                ClearOptions();
                Init();
                
                if (UsesCursor)
                    Menu.ResetStem();

                Menu.SetBackgroundPalette(BackgroundPalette);
                
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Store02_Mix02);
                State = MenuPageState.TransitionIn;
                ClickCallback = null;
                FadeOutCallback = null;
                break;
            
            case MenuPageState.TransitionIn:
                // 40 frames total
                TransitionValue += 4;

                // 20 frames to move up the curtains
                if (TransitionValue < 80)
                {
                    TgxCluster cluster = Menu.Playfield.Camera.GetCluster(1);
                    cluster.Position += new Vector2(0, cluster.RenderContext.Resolution.Y / (80 / 4f));
                }

                Step_TransitionIn();

                if (TransitionValue >= 160)
                {
                    TransitionValue = 0;
                    State = MenuPageState.Active;
                }

                Draw(Menu.AnimationPlayer);
                break;

            case MenuPageState.Active:
                if (FadeOutCallback == null && ClickCallback == null)
                {
                    Step_Active();
                }
                else if (FadeOutCallback != null && Menu.TransitionsFX.IsFadeOutFinished)
                {
                    FadeOutCallback();
                    FadeOutCallback = null;
                }
                else if (Menu.HasFinishedCursorClick())
                {
                    Menu.SetCursorToIdle();

                    if (ClickCallback != null)
                    {
                        ClickCallback();
                        ClickCallback = null;
                    }
                }

                Draw(Menu.AnimationPlayer);
                break;

            case MenuPageState.TransitionOut:
                // 69 frames total (55 in the original game)
                TransitionValue += 4;

                TgxCluster curtainsCluster = Menu.Playfield.Camera.GetCluster(1);
                if (TransitionValue <= curtainsCluster.RenderContext.Resolution.Y)
                {
                    curtainsCluster.Position -= new Vector2(0, 4);
                    Step_TransitionOut();
                }
                else if (TransitionValue >= curtainsCluster.RenderContext.Resolution.Y + 60)
                {
                    TransitionValue = 0;
                    State = MenuPageState.Inactive;
                    UnInit();
                }

                Draw(Menu.AnimationPlayer);
                break;
        }

        if (UsesCursor)
            Menu.ManageCursorAndStem();
    }

    public enum MenuPageState
    {
        Inactive,
        Init,
        TransitionIn,
        Active,
        TransitionOut,
    }
}