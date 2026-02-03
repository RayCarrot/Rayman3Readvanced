using System;
using System.Linq;
using BinarySerializer.Ubisoft.GbaEngine;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.AnimEngine;
using GbaMonoGame.Engine2d;
using GbaMonoGame.FsmSourceGenerator;
using Microsoft.Xna.Framework;

namespace GbaMonoGame.Rayman3.Readvanced;

[GenerateFsmFields]
public partial class TimeAttackScoreDialog : Dialog
{
    public TimeAttackScoreDialog(Scene2D scene) : base(scene)
    {
        CreateGeneratedStates();

        Timer = 0;

        // Get the map
        MapId = TimeAttackInfo.LevelId ?? throw new Exception("Finished time attack with no level set");
        
        // Check if the time is a new record
        TimeAttackTime? recordTime = TimeAttackDataManager.GetRecordTime(MapId);
        NewRecord = recordTime == null || TimeAttackInfo.Timer < recordTime.Value.Time;

        State.SetTo(_Fsm_ShowTargets);
    }

    private const int LineHeight = 16;
    private const float CursorBaseY = 162;
    private const float OptionsBaseY = 168;

    public MapId MapId { get; }
    public bool NewRecord { get; }
    public uint Timer { get; set; }

    public SpriteTimeAttackTimeObject CurrentTimeText { get; set; }
    public float CurrentTimeTextSpeed { get; set; }

    public SpriteTextureObject RecordTimeIcon { get; set; }
    public SpriteTimeAttackTimeObject RecordTimeText { get; set; }
    public SpriteTextObject NewRecordText { get; set; }
    public bool DrawNewRecord { get; set; }

    public TimeAttackScoreDialogTarget[] TimeTargets { get; set; }
    public int TimeTargetTransitionIndex { get; set; }

    public AnimatedObject Cursor { get; set; }
    public float? CursorStartY { get; set; }
    public float? CursorDestY { get; set; }

    public SpriteFontTextObject[] Options { get; set; }
    public int SelectedOption { get; set; }
    public bool DrawOptions { get; set; }

    public CircleTransitionScreenEffect CircleTransitionScreenEffect { get; set; }
    public int CircleTransitionValue { get; set; }
    public bool IsTransitioningOut => CircleTransitionScreenEffect != null;

    private void ManageCursor()
    {
        // Move with a constant speed of 2
        const float speed = 2;

        if (CursorStartY != null && CursorDestY != null)
        {
            float startY = CursorStartY.Value;
            float destY = CursorDestY.Value;

            // Move up
            if (destY < startY && Cursor.ScreenPos.Y > destY)
            {
                Cursor.ScreenPos -= new Vector2(0, speed);
            }
            // Move down
            else if (destY > startY && Cursor.ScreenPos.Y < destY)
            {
                Cursor.ScreenPos += new Vector2(0, speed);
            }
            // Finished moving
            else
            {
                Cursor.ScreenPos = Cursor.ScreenPos with { Y = destY };
                CursorStartY = null;
                CursorDestY = null;
            }
        }
    }

    private void SetSelectedOption(int selectedOption, bool animate = true)
    {
        if (selectedOption < 0)
            selectedOption = Options.Length - 1;
        else if (selectedOption > Options.Length - 1)
            selectedOption = 0;

        SelectedOption = selectedOption;

        CursorStartY = animate ? Cursor.ScreenPos.Y : CursorBaseY + selectedOption * LineHeight;
        CursorDestY = CursorBaseY + selectedOption * LineHeight;

        for (int i = 0; i < Options.Length; i++)
            Options[i].Font = i == SelectedOption ? ReadvancedFonts.MenuWhite : ReadvancedFonts.MenuYellow;
    }

    private void BeginCircleTransition()
    {
        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__SlideOut_Mix01);

        CircleTransitionValue = 252;

        // Create the circle transition
        CircleTransitionScreenEffect = new CircleTransitionScreenEffect()
        {
            RenderContext = Scene.RenderContext,
        };

        // Initialize and add as a screen effect
        CircleTransitionScreenEffect.Init(CircleTransitionValue, Scene.RenderContext.Resolution / 2);
        Gfx.SetScreenEffect(CircleTransitionScreenEffect);
    }

    private bool StepCircleTransition()
    {
        CircleTransitionValue -= 6;

        if (CircleTransitionValue < 0)
        {
            CircleTransitionValue = 0;
            CircleTransitionScreenEffect = null;

            return true;
        }
        else
        {
            CircleTransitionScreenEffect.Radius = CircleTransitionValue;
            return false;
        }
    }

    public override void Load()
    {
        CurrentTimeText = new SpriteTimeAttackTimeObject()
        {
            BgPriority = 0,
            ObjPriority = 0,
            ScreenPos = new Vector2(164, 9),
            RenderContext = Scene.HudRenderContext,
            Time = new TimeAttackTime(TimeAttackTimeType.Record, TimeAttackInfo.Timer),
        };

        if (NewRecord)
        {
            NewRecordText = new SpriteTextObject
            {
                ScreenPos = new Vector2(155, 45),
                RenderContext = Scene.HudRenderContext,
                Text = "NEW RECORD",
                Color = Color.White,
                FontSize = FontSize.Font16,
            };
        }
        else
        {
            RecordTimeIcon = new SpriteTextureObject
            {
                BgPriority = 0,
                ObjPriority = 0,
                BlendMode = BlendMode.AlphaBlend,
                Alpha = 0.7f,
                RenderContext = Scene.HudRenderContext,
            };

            RecordTimeText = new SpriteTimeAttackTimeObject()
            {
                BgPriority = 0,
                ObjPriority = 0,
                ScreenPos = new Vector2(164, 47),
                BlendMode = BlendMode.AlphaBlend,
                Alpha = 0.7f,
                RenderContext = Scene.HudRenderContext,
                Time = TimeAttackDataManager.GetRecordTime(MapId) ?? default,
            };

            RecordTimeIcon.Texture = RecordTimeText.Time.LoadIcon(true);
            RecordTimeIcon.ScreenPos = RecordTimeText.ScreenPos + new Vector2(-18, -2);
        }

        TimeTargets = TimeAttackInfo.TargetTimes.
            Where(x => x.Type != TimeAttackTimeType.Record).
            Select((x, i) => new TimeAttackScoreDialogTarget(x, Scene.HudRenderContext, new Vector2(80 + i * 96, 80))).
            ToArray();

        AnimatedObjectResource canvasResource = Rom.LoadResource<AnimatedObjectResource>(Rayman3DefinedResource.PauseCanvasAnimations);
        Cursor = new AnimatedObject(canvasResource, false)
        {
            IsFramed = true,
            CurrentAnimation = 1,
            ScreenPos = new Vector2(138, CursorBaseY),
            RenderContext = Scene.HudRenderContext,
        };

        Options =
        [
            new SpriteFontTextObject
            {
                ScreenPos = new Vector2(160, OptionsBaseY),
                RenderContext = Scene.HudRenderContext,
                Text = "RESTART",
            },
            new SpriteFontTextObject
            {
                ScreenPos = new Vector2(160, OptionsBaseY + LineHeight),
                RenderContext = Scene.HudRenderContext,
                Text = "CONTINUE",
            },
        ];
    }

    public override void Draw(AnimationPlayer animationPlayer)
    {
        animationPlayer.Play(CurrentTimeText);

        if (DrawNewRecord)
        {
            if (NewRecord)
            {
                animationPlayer.Play(NewRecordText);
            }
            else
            {
                animationPlayer.Play(RecordTimeIcon);
                animationPlayer.Play(RecordTimeText);
            }
        }

        foreach (TimeAttackScoreDialogTarget timeTarget in TimeTargets)
            timeTarget.Draw(animationPlayer);

        if (DrawOptions)
        {
            animationPlayer.Play(Cursor);

            foreach (SpriteFontTextObject option in Options)
                animationPlayer.Play(option);
        }
    }
}