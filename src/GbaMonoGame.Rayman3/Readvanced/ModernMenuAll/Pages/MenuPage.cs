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
    public int TransitionValue { get; set; }

    public int SelectedOption { get; set; }

    public abstract bool UsesCursor { get; }
    public abstract int BackgroundPalette { get; }
    public abstract int LineHeight { get; }

    protected virtual void Init() { }
    protected virtual void Step_TransitionIn() { }
    protected virtual void Step_Active() { }
    protected virtual void Step_TransitionOut() { }
    protected virtual void Draw(AnimationPlayer animationPlayer) { }

    protected void DrawOptions(AnimationPlayer animationPlayer)
    {
        foreach (MenuOption option in Options)
            option.Draw(animationPlayer);
    }

    protected void AddOption(MenuOption option)
    {
        int index = Options.Count;
        Options.Add(option);
        option.Init(Menu, RenderContext, new Vector2(80, 54 + LineHeight * index), index);
        option.ChangeIsSelected(index == SelectedOption);
    }

    protected void ChangeSelectedOption(int delta)
    {
        Options[SelectedOption].ChangeIsSelected(false);

        SelectedOption += delta;

        if (SelectedOption > Options.Count - 1)
            SelectedOption = 0;
        else if (SelectedOption < 0)
            SelectedOption = Options.Count - 1;
        
        Options[SelectedOption].ChangeIsSelected(true);
        
        Menu.SelectOption(SelectedOption, true);
    }

    public void Step()
    {
        switch (State)
        {
            case MenuPageState.Init:
                Options.Clear();
                SelectedOption = 0;

                Init();
                
                if (UsesCursor)
                    Menu.ResetStem();

                Menu.SetBackgroundPalette(BackgroundPalette);
                
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Store02_Mix02);
                State = MenuPageState.TransitionIn;
                break;
            
            case MenuPageState.TransitionIn:
                TransitionValue += 6;

                if (TransitionValue <= 140)
                {
                    TgxCluster cluster = Menu.Playfield.Camera.GetCluster(1);
                    cluster.Position += new Vector2(0, 10);
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
                Step_Active();
                Draw(Menu.AnimationPlayer);
                break;

            case MenuPageState.TransitionOut:
                TransitionValue += 6;

                if (TransitionValue <= RenderContext.Resolution.Y)
                {
                    TgxCluster cluster = Menu.Playfield.Camera.GetCluster(1);
                    cluster.Position -= new Vector2(0, 6);
                    Step_TransitionOut();
                }
                else if (TransitionValue >= RenderContext.Resolution.Y + 60)
                {
                    TransitionValue = 0;
                    State = MenuPageState.Inactive;
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