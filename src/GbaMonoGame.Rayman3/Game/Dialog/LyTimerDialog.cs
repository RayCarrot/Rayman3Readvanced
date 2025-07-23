using GbaMonoGame.AnimEngine;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public class LyTimerDialog : Dialog
{
    public LyTimerDialog(Scene2D scene) : base(scene) { }

    public TimerBar TimerBar { get; set; }

    protected override bool ProcessMessageImpl(object sender, Message message, object param) => false;

    public override void Load()
    {
        TimerBar = new TimerBar(Scene);
        TimerBar.Load();
        TimerBar.Set();
    }

    public override void Draw(AnimationPlayer animationPlayer)
    {
        TimerBar.DrawTime(animationPlayer, GameInfo.RemainingTime);
    }
}