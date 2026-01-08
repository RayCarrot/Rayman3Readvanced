using System;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.AnimEngine;
using GbaMonoGame.Engine2d;
using Microsoft.Xna.Framework;

namespace GbaMonoGame.Rayman3;

public partial class CheatDialog : Dialog
{
    public CheatDialog(Scene2D scene) : base(scene)
    {
        State.SetTo(Fsm_NavigateItem);
    }

    public DebugBoxAObject BoxObject { get; set; }
    public CheatItem[] CheatItems { get; set; }

    public int SelectedIndex { get; set; }
    public bool PendingClose { get; set; }

    public void SetSelectedIndex(int selectedIndex)
    {
        CheatItems[SelectedIndex].SetIsSelected(false);
        SelectedIndex = selectedIndex;
        CheatItems[SelectedIndex].SetIsSelected(true);
    }

    protected override bool ProcessMessageImpl(object sender, Message message, object param) => false;

    public override void Load()
    {
        BoxObject = new DebugBoxAObject
        {
            RenderContext = Scene.RenderContext,
            Color = Color.Black,
            IsFilled = true
        };

        CheatItems =
        [
            new CheatItem(
                renderContext: Scene.RenderContext,
                text: "COMPLETE LEVEL",
                action: _ =>
                {
                    if (Scene.MainActor.Type == (int)ActorType.MissileMode7)
                    {
                        FrameSingleMode7 frame = (FrameSingleMode7)Frame.Current;
                        MissileMode7 actor = (MissileMode7)Scene.MainActor;
                        frame.SaveLums();
                        actor.State.MoveTo(actor.Fsm_FinishedRace);
                        SoundEventsManager.ReplaceAllSongs(Rayman3SoundEvent.Play__win3, 0);
                        LevelMusicManager.HasOverridenLevelMusic = false;
                    }
                    else if (Scene.MainActor.Type == (int)ActorType.RaymanMode7)
                    {
                        RaymanMode7 actor = (RaymanMode7)Scene.MainActor;
                        SamMode7 sam = Scene.GetGameObject<SamMode7>(actor.SamActorId);
                        sam.State.MoveTo(sam.Fsm_End);
                    }
                    else
                    {
                        Scene.MainActor.ProcessMessage(this, Message.Rayman_FinishLevel);
                    }
                },
                isEnabled: null),
            new CheatItem(
                renderContext: Scene.RenderContext,
                text: "RESTORE HEALTH",
                action: _ =>
                {
                    Scene.MainActor.HitPoints = Scene.MainActor.ActorModel.HitPoints;
                },
                isEnabled: null),
            new CheatItem(
                renderContext: Scene.RenderContext,
                text: "SHOW OBJECT BOXES",
                action: item =>
                {
                    item.ToggleIsEnabled();
                    Scene.ShowDebugBoxes = item.IsEnabled == true;
                },
                isEnabled: Scene.ShowDebugBoxes),
            new CheatItem(
                renderContext: Scene.RenderContext,
                text: "SHOW PHYSICAL COLLISION",
                action: item =>
                {
                    item.ToggleIsEnabled();
                    Scene.Playfield.PhysicalLayer.EnsureDebugScreenIsCreated();
                    Scene.Playfield.PhysicalLayer.DebugScreen.IsEnabled = item.IsEnabled == true;
                },
                isEnabled: Scene.Playfield.PhysicalLayer.DebugScreen?.IsEnabled ?? false),
        ];

        Vector2 pos = new(30, 30);
        const int lineHeight = 16;
        const int boxPadding = 8;
        
        int boxWidth = 0;
        foreach (CheatItem cheatItem in CheatItems)
        {
            int width = cheatItem.TextObject.GetStringWidth();
            if (width > boxWidth)
                boxWidth = width;
        }

        BoxObject.ScreenPos = pos - new Vector2(boxPadding);
        BoxObject.Size = new Vector2(boxPadding + boxWidth + boxPadding, boxPadding + CheatItems.Length * lineHeight + boxPadding);

        foreach (CheatItem cheatItem in CheatItems)
        {
            cheatItem.SetPosition(pos);
            pos += new Vector2(0, lineHeight);
        }
    }

    public override void Init()
    {
        SetSelectedIndex(0);
        PendingClose = false;
    }

    public override void Draw(AnimationPlayer animationPlayer)
    {
        animationPlayer.PlayFront(BoxObject);

        foreach (CheatItem cheatItem in CheatItems)
            cheatItem.Draw(animationPlayer);
    }

    public class CheatItem
    {
        public CheatItem(RenderContext renderContext, string text, Action<CheatItem> action, bool? isEnabled)
        {
            Text = text;
            Action = action;
            IsEnabled = isEnabled;
            TextObject = new SpriteTextObject
            {
                RenderContext = renderContext
            };
            SetIsEnabled(isEnabled);
            SetIsSelected(false);
        }

        public string Text { get; }
        public Action<CheatItem> Action { get; }
        public bool? IsEnabled { get; set; }
        public SpriteTextObject TextObject { get; }

        public void SetIsEnabled(bool? isEnabled)
        {
            IsEnabled = isEnabled;
            TextObject.Text = IsEnabled switch
            {
                true => $"{Text}: ON",
                false => $"{Text}: OFF",
                null => Text,
            };
        }

        public void ToggleIsEnabled()
        {
            SetIsEnabled(!IsEnabled);
        }

        public void SetIsSelected(bool isSelected)
        {
            TextObject.Color = isSelected ? Color.Yellow : Color.White;
        }

        public void SetPosition(Vector2 position)
        {
            TextObject.ScreenPos = position;
        }

        public void Invoke()
        {
            Action(this);
        }

        public void Draw(AnimationPlayer animationPlayer)
        {
            animationPlayer.PlayFront(TextObject);
        }
    }
}