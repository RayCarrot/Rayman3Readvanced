using System;
using GbaMonoGame.AnimEngine;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public class AObjectChain : AnimatedObject
{
    public AObjectChain(AnimatedObjectResource resource, bool isDynamic) : base(resource, isDynamic) { }

    private const int BufferLength = 32;

    public int ChildrenCount { get; set; }
    public int ChildrenDrawCount { get; set; }

    public int FirstRunBufferIndex { get; set; }
    public int BufferIndex { get; set; }
    public bool FinishedFirstRun => FirstRunBufferIndex >= BufferIndex;

    public int BaseAnimation { get; set; }
    
    public bool EnablePriorityManagement { get; set; }
    public bool DisableLinks { get; set; }

    public Vector2[] PositionsBuffer { get; set; }
    public Vector2[] ChildrenScreenPositions { get; set; }
    
    public bool[] PriosBuffer { get; set; }
    public bool[] Prios { get; set; }

    public void SetCurrentPriority(bool prio)
    {
        PriosBuffer[BufferIndex] = prio;
    }

    public void SetPosition(Vector2 position)
    {
        BufferIndex = 0;
        for (int i = 0; i < PositionsBuffer.Length; i++)
            PositionsBuffer[i] = position;
    }

    public void Init(int linksCount, Vector2 position, int baseAnimation, bool enablePriorityManagement)
    {
        ChildrenCount = linksCount - 1;
        ChildrenDrawCount = 0;
        FirstRunBufferIndex = 0;
        BaseAnimation = baseAnimation;
        EnablePriorityManagement = enablePriorityManagement;

        PositionsBuffer = new Vector2[BufferLength];
        ChildrenScreenPositions = new Vector2[ChildrenCount];

        DisableLinks = false;

        BufferIndex = 0;
        PositionsBuffer[BufferIndex] = position;
        for (int i = 1; i < PositionsBuffer.Length; i++)
            PositionsBuffer[i] = PositionsBuffer[BufferIndex]; 

        if (EnablePriorityManagement)
        {
            PriosBuffer = new bool[BufferLength];

            // NOTE: In the original game this is bugged and will allocate 1 less than needed. This
            //       however won't cause any overflow issues because the count is always 6 and when
            //       allocating it aligns it to 4, meaning it allocates 2 additional bytes (8 total).
            Prios = new bool[ChildrenCount + 1];
        }
        else
        {
            PriosBuffer = null;
            Prios = null;
        }

        CurrentAnimation = baseAnimation;
    }

    public override void Execute(Action<short> soundEventCallback)
    {
        if (DisableLinks)
        {
            base.Execute(soundEventCallback);
            return;
        }

        // Get the current state
        BoxTable boxTable = BoxTable;
        int currentFrame = CurrentFrame;
        int timer = Timer;
        bool isDelayMode = IsDelayMode;
        
        // Set the animation, keeping the same state
        CurrentAnimation = BaseAnimation;
        CurrentFrame = currentFrame;
        Timer = timer;
        IsDelayMode = isDelayMode;

        IsBackSprite = EnablePriorityManagement && Prios[0];

        base.Execute(soundEventCallback);

        BoxTable = null;

        for (int i = 0; i < ChildrenDrawCount; i++)
        {
            // Set the animation, keeping the same state
            CurrentAnimation = BaseAnimation + i + 1;
            CurrentFrame = currentFrame;
            Timer = timer;
            IsDelayMode = isDelayMode;

            ScreenPos = ChildrenScreenPositions[i];

            IsBackSprite = EnablePriorityManagement && Prios[i + 1];

            base.Execute(soundEventCallback);
        }

        BoxTable = boxTable;
    }

    public void Draw(BaseActor actor, AnimationPlayer animationPlayer, bool forceDraw)
    {
        ChildrenDrawCount = 0;

        if (DisableLinks)
        { 
            actor.DrawDefault(animationPlayer, forceDraw);
            return;
        }

        if (actor.Scene.Camera.IsActorFramed(actor) || forceDraw)
        {
            if (EnablePriorityManagement)
                Prios[0] = PriosBuffer[BufferIndex];

            int value = BufferIndex + 26;

            for (int i = 0; i < ChildrenCount; i++)
            {
                value %= BufferLength;

                if (FirstRunBufferIndex > value)
                    ChildrenDrawCount++;

                ChildrenScreenPositions[i] = PositionsBuffer[value] - actor.Scene.Playfield.Camera.Position;

                if (EnablePriorityManagement)
                    Prios[i + 1] = PriosBuffer[value];

                value += i switch
                {
                    0 => 27,
                    1 => 28,
                    2 => 29,
                    _ => 30
                };
            }

            IsFramed = true;
            animationPlayer.Play(this);

            PositionsBuffer[BufferIndex] = actor.Position;

            BufferIndex++;
            BufferIndex %= BufferLength;

            if (!FinishedFirstRun)
                FirstRunBufferIndex++;
        }
        else
        {
            IsFramed = false;
            ComputeNextFrame();

            if (FinishedFirstRun)
            {
                PositionsBuffer[BufferIndex] = actor.Position;

                BufferIndex++;
                BufferIndex %= BufferLength;
            }
        }
    }
}