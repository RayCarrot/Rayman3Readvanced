using GbaMonoGame.AnimEngine;

namespace GbaMonoGame.Rayman3;

public abstract class Bar
{
    protected Bar(Scene2D scene)
    {
        Scene = scene;
        DrawStep = BarDrawStep.Wait;
    }

    protected static readonly int[] BounceData =
    [
        0, -3, -5, -6, -5, -3,
        0, 2, 4, 5, 4, 2,
        0, -2, -3, -4, -3, -2,
        0, 2, 3, 2,
        0, -1, -2, -1,
        0, 1,
        0, -1,
        0, 1,
        0, -1,
        0,
        0
    ];

    public Scene2D Scene { get; }

    public BarMode Mode { get; set; }
    public BarDrawStep DrawStep { get; set; }

    public void SetToStayHidden()
    {
        if (Mode != BarMode.Disabled)
            Mode = BarMode.StayHidden;
    }

    public void SetToStayVisible()
    {
        if (Mode != BarMode.Disabled)
            Mode = BarMode.StayVisible;
    }

    public void SetToDefault()
    {
        if (Mode != BarMode.Disabled)
            Mode = BarMode.Default;
    }

    public void Disable()
    {
        Mode = BarMode.Disabled;
    }

    public void MoveIn()
    {
        if (DrawStep != BarDrawStep.Bounce && Mode != BarMode.StayHidden)
            DrawStep = BarDrawStep.MoveIn;
    }

    public void MoveOut()
    {
        DrawStep = BarDrawStep.MoveOut;
    }

    public abstract void Load();
    public abstract void Set();
    public abstract void Draw(AnimationPlayer animationPlayer);
}