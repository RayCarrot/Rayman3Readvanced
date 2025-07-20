using System;
using System.Collections.Generic;
using System.Linq;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.AnimEngine;
using GbaMonoGame.TgxEngine;

namespace GbaMonoGame.Rayman3.Readvanced;

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
    public int ScrollOffset { get; set; }
    public bool HasScrollableContent => Options.Count > MaxOptions;
    public int MaxScrollOffset => Math.Max(Options.Count - MaxOptions, 0);

    public abstract bool UsesCursor { get; }
    public abstract int BackgroundPalette { get; }
    public abstract int LineHeight { get; }
    public virtual int MaxOptions => 6;
    public virtual bool HasScrollBar => false;
    public virtual MenuScrollBarSize ScrollBarSize => MenuScrollBarSize.Big;
    
    private Vector2 GetOptionPosition(int index) => new(75, 54 + LineHeight * index);

    private void UpdateOptionPositions()
    {
        int index = 0;
        foreach (MenuOption option in Options.Skip(ScrollOffset).Take(MaxOptions))
        {
            option.SetPosition(GetOptionPosition(index));
            index++;
        }
    }

    protected virtual void Init() { }
    protected virtual void Step_TransitionIn() { }
    protected virtual void Step_Active() { }
    protected virtual void Step_TransitionOut() { }
    protected virtual void UnInit() { }
    protected virtual void Draw(AnimationPlayer animationPlayer) { }

    protected void DrawOptions(AnimationPlayer animationPlayer)
    {
        foreach (MenuOption option in Options.Skip(ScrollOffset).Take(MaxOptions))
            option.Draw(animationPlayer);
    }

    protected void ClearOptions()
    {
        SelectedOption = 0;
        ScrollOffset = 0;
        Options.Clear();
    }

    protected void AddOption(MenuOption option)
    {
        int index = Options.Count;
        Options.Add(option);

        if (!option.IsInitialized)
        {
            option.Init(3, RenderContext, index);
            option.IsInitialized = true;
        }
        
        option.SetPosition(GetOptionPosition(index - ScrollOffset));
        option.ChangeIsSelected(index == SelectedOption);
    }

    protected virtual bool SetSelectedOption(int selectedOption, bool playSound = true, bool forceUpdate = false)
    {
        int prevSelectedOption = SelectedOption;

        int newSelectedOption = selectedOption;

        int newScrollOffset = ScrollOffset;
        if (newSelectedOption > prevSelectedOption)
        {
            if (newSelectedOption >= ScrollOffset + MaxOptions)
                newScrollOffset++;
        }
        else if (newSelectedOption < prevSelectedOption)
        {
            if (newSelectedOption < ScrollOffset)
                newScrollOffset--;
        }

        if (newSelectedOption > Options.Count - 1)
        {
            newSelectedOption = 0;
            newScrollOffset = 0;
        }
        else if (newSelectedOption < 0)
        {
            newSelectedOption = Options.Count - 1;
            newScrollOffset = MaxScrollOffset;
        }

        bool changed = Menu.SetCursorTarget(newSelectedOption - newScrollOffset);

        if (changed || forceUpdate)
        {
            SelectedOption = newSelectedOption;
            Options[prevSelectedOption].ChangeIsSelected(false);
            Options[newSelectedOption].ChangeIsSelected(true);

            if (newScrollOffset != ScrollOffset)
            {
                ScrollOffset = newScrollOffset;
                UpdateOptionPositions();
            }

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
        TransitionsFX.FadeOutInit(stepSize);
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
                bool finishedCursorClick = Menu.HasFinishedCursorClick();
                if (finishedCursorClick)
                    Menu.SetCursorToIdle();

                if (FadeOutCallback == null && ClickCallback == null)
                {
                    Step_Active();
                }
                else if (FadeOutCallback != null && !TransitionsFX.IsFadingOut)
                {
                    FadeOutCallback();
                    FadeOutCallback = null;
                }
                else if (ClickCallback != null && finishedCursorClick)
                {
                    ClickCallback();
                    ClickCallback = null;
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