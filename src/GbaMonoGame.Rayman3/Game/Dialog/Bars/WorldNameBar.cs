using System;
using BinarySerializer.Ubisoft.GbaEngine;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.AnimEngine;

namespace GbaMonoGame.Rayman3;

public class WorldNameBar : Bar
{
    public WorldNameBar(Scene2D scene) : base(scene)
    {
        WorldNameBarDrawStep = BarDrawStep.Hide;
        OffsetY = 40;
        CanMoveIn = false;
    }

    public BarDrawStep WorldNameBarDrawStep { get; set; }
    public int OffsetY { get; set; }
    public bool CanMoveIn { get; set; }

    public AnimatedObject Canvas { get; set; }
    public SpriteTextObject WorldName { get; set; }

    public void SetWorldId(WorldId worldId)
    {
        string worldNameText = Localization.GetText(TextBankId.LevelNames, worldId switch
        {
            WorldId.World1 => 31,
            WorldId.World2 => 32,
            WorldId.World3 => 33,
            WorldId.World4 => 34,
            _ => throw new ArgumentOutOfRangeException(nameof(worldId), worldId, "Invalid world id"),
        })[0];

        WorldName.Text = worldNameText;

        // NOTE: The original game sets the y pos to 64 here for some reason
        WorldName.ScreenPos = WorldName.ScreenPos with { X = -WorldName.GetStringWidth() / 2f };

        WorldNameBarDrawStep = BarDrawStep.MoveIn;
    }

    public void MoveInWorldNameBar()
    {
        if (CanMoveIn)
            WorldNameBarDrawStep = BarDrawStep.MoveIn;
    }

    public void MoveOutWorldNameBar()
    {
        if (WorldNameBarDrawStep == BarDrawStep.Wait)
            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__PannelDw_Mix01);

        WorldNameBarDrawStep = BarDrawStep.MoveOut;
    }

    public override void Load()
    {
        // NOTE: Game has it set up so Load can be called multiple times. Dynamic objects don't get recreated after the first time, but instead
        //       reloaded into VRAM. We don't need to do that though due to how the graphics system works here, so just always create everything.

        AnimatedObjectResource resource = Rom.LoadResource<AnimatedObjectResource>(Rayman3DefinedResource.WorldDashboardAnimations);

        Canvas = new AnimatedObject(resource, false)
        {
            IsFramed = true,
            BgPriority = 0,
            ObjPriority = 0,
            ScreenPos = new Vector2(0, -1),
            HorizontalAnchor = HorizontalAnchorMode.Center,
            VerticalAnchor = VerticalAnchorMode.Bottom,
            CurrentAnimation = 0,
            RenderContext = Scene.HudRenderContext,
        };

        WorldName = new SpriteTextObject()
        {
            Color = TextColor.WorldName,
            FontSize = FontSize.Font16,
            ScreenPos = new Vector2(0, -16),
            HorizontalAnchor = HorizontalAnchorMode.Center,
            VerticalAnchor = VerticalAnchorMode.Bottom,
            RenderContext = Scene.HudRenderContext,
        };

        SetWorldId(0);
    }

    public override void Set() { }

    public override void Draw(AnimationPlayer animationPlayer)
    {
        if (Mode is BarMode.StayHidden or BarMode.Disabled)
            return;

        switch (WorldNameBarDrawStep)
        {
            case BarDrawStep.Hide:
                OffsetY = 40;
                break;

            case BarDrawStep.MoveIn:
                if (OffsetY != 0)
                {
                    if (OffsetY == 36)
                        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__PannelUp_Mix01);

                    OffsetY -= 2;
                }
                else
                {
                    WorldNameBarDrawStep = BarDrawStep.Wait;
                }
                break;

            case BarDrawStep.MoveOut:
                if (OffsetY < 40)
                {
                    OffsetY += 2;
                }
                else
                {
                    OffsetY = 40;
                    WorldNameBarDrawStep = BarDrawStep.Hide;
                }
                break;
        }

        if (WorldNameBarDrawStep != BarDrawStep.Hide)
        {
            Canvas.ScreenPos = Canvas.ScreenPos with { Y = -1 + OffsetY };
            WorldName.ScreenPos = new Vector2(
                x: -WorldName.GetStringWidth() / 2f,
                y: -16 + OffsetY);

            animationPlayer.PlayFront(Canvas);
            animationPlayer.PlayFront(WorldName);
        }
    }
}