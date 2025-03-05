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
    public MenuPageState State { get; set; }
    public int TransitionValue { get; set; }

    public int SelectedOption { get; set; }

    public abstract bool UsesCursor { get; }
    public abstract int BackgroundPalette { get; }

    protected abstract void Init();
    protected abstract void Step_TransitionIn();
    protected abstract void Step_Active();
    protected abstract void Step_TransitionOut();
    protected abstract void Draw(AnimationPlayer animationPlayer);

    public void Step()
    {
        switch (State)
        {
            case MenuPageState.Init:
                Init();
                
                if (UsesCursor)
                    Menu.ResetStem();

                SelectedOption = 0;
                Menu.SetBackgroundPalette(BackgroundPalette);
                
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Store02_Mix02);
                State = MenuPageState.TransitionIn;
                break;
            
            case MenuPageState.TransitionIn:
                TransitionValue += 4;

                if (TransitionValue <= Menu.Playfield.RenderContext.Resolution.Y / 2)
                {
                    TgxCluster cluster = Menu.Playfield.Camera.GetCluster(1);
                    cluster.Position += new Vector2(0, 8);
                }

                if (TransitionValue >= Menu.Playfield.RenderContext.Resolution.Y)
                {
                    TransitionValue = 0;
                    State = MenuPageState.Active;
                }

                Step_TransitionIn();
                Draw(Menu.AnimationPlayer);
                break;

            case MenuPageState.Active:
                Step_Active();
                Draw(Menu.AnimationPlayer);
                break;

            case MenuPageState.TransitionOut:
                TransitionValue += 4;

                if (TransitionValue <= Menu.Playfield.RenderContext.Resolution.Y)
                {
                    TgxCluster cluster = Menu.Playfield.Camera.GetCluster(1);
                    cluster.Position -= new Vector2(0, 4);
                }
                else if (TransitionValue >= Menu.Playfield.RenderContext.Resolution.Y + 60)
                {
                    TransitionValue = 0;
                    State = MenuPageState.Inactive;
                }

                Step_TransitionOut();
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