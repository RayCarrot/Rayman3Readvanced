using BinarySerializer.Ubisoft.GbaEngine;
using GbaMonoGame.AnimEngine;
using Microsoft.Xna.Framework.Graphics;

namespace GbaMonoGame.Rayman3.Readvanced;

public class SlotMenuOption : MenuOption
{
    public SlotMenuOption(ModernMenuAll.Slot slot)
    {
        Slot = slot;
    }

    public ModernMenuAll.Slot Slot { get; set; }

    public AnimatedObject Icon { get; set; }
    public SpriteTextureObject ExtIcon { get; set; } // For custom icons 4 and 5
    public AnimatedObject LumIcon { get; set; }
    public SpriteTextObject LumText { get; set; }
    public AnimatedObject CageIcon { get; set; }
    public SpriteTextObject CageText { get; set; }
    public SpriteTextureObject TimeIcon { get; set; }
    public SpriteTextObject TimeText { get; set; }
    public SpriteFontTextObject EmptyText { get; set; }

    public override void Init(int bgPriority, RenderContext renderContext, Vector2 position, int index)
    {
        AnimatedObjectResource propsAnimations = Rom.LoadResource<AnimatedObjectResource>(GameResource.MenuPropAnimations);

        if (index < 3)
        {
            Icon = new AnimatedObject(propsAnimations, propsAnimations.IsDynamic)
            {
                IsFramed = true,
                BgPriority = bgPriority,
                ObjPriority = 0,
                ScreenPos = position,
                CurrentAnimation = 8 + (index % 3),
                RenderContext = renderContext,
            };
        }
        else
        {
            ExtIcon = new SpriteTextureObject
            {
                BgPriority = bgPriority,
                ObjPriority = 0,
                ScreenPos = position,
                Texture = Engine.FrameContentManager.Load<Texture2D>($"SaveSlotIcon_{index + 1}"),
                RenderContext = renderContext,
            };
        }
        LumIcon = new AnimatedObject(propsAnimations, propsAnimations.IsDynamic)
        {
            IsFramed = true,
            BgPriority = bgPriority,
            ObjPriority = 0,
            ScreenPos = position + new Vector2(23, 1),
            CurrentAnimation = 13,
            RenderContext = renderContext,
        };
        LumText = new SpriteTextObject()
        {
            BgPriority = bgPriority,
            ObjPriority = 0,
            ScreenPos = position + new Vector2(45, 1),
            Text = Slot?.LumsCount.ToString() ?? "0",
            FontSize = FontSize.Font16,
            Color = TextColor.Menu,
            RenderContext = renderContext,
        };
        CageIcon = new AnimatedObject(propsAnimations, propsAnimations.IsDynamic)
        {
            IsFramed = true,
            BgPriority = bgPriority,
            ObjPriority = 0,
            ScreenPos = position + new Vector2(81, -4),
            CurrentAnimation = 11,
            RenderContext = renderContext,
        };
        CageText = new SpriteTextObject()
        {
            BgPriority = bgPriority,
            ObjPriority = 0,
            ScreenPos = position + new Vector2(106, 1),
            Text = Slot?.CagesCount.ToString() ?? "0",
            FontSize = FontSize.Font16,
            Color = TextColor.Menu,
            RenderContext = renderContext,
        };
        TimeIcon = new SpriteTextureObject
        {
            BgPriority = bgPriority,
            ObjPriority = 0,
            ScreenPos = position + new Vector2(131, 0),
            Texture = Engine.FrameContentManager.Load<Texture2D>("SaveSlotTime"),
            RenderContext = renderContext,
        };
        TimeText = new SpriteTextObject()
        {
            BgPriority = bgPriority,
            ObjPriority = 0,
            ScreenPos = position + new Vector2(150, 1),
            Text = "1:54:03", // TODO: Get actual time from save slot
            FontSize = FontSize.Font16,
            Color = TextColor.Menu,
            RenderContext = renderContext,
        };
        EmptyText = new SpriteFontTextObject()
        {
            BgPriority = bgPriority,
            ObjPriority = 0,
            ScreenPos = position + new Vector2(42, 13),
            Text = "EMPTY",
            Font = ReadvancedFonts.MenuYellow,
            RenderContext = renderContext,
        };
    }

    public override void Draw(AnimationPlayer animationPlayer)
    {
        if (Icon != null)
            animationPlayer.Play(Icon);
        else
            animationPlayer.Play(ExtIcon);

        if (Slot == null)
        {
            animationPlayer.Play(EmptyText);
        }
        else
        {
            animationPlayer.Play(LumIcon);
            animationPlayer.Play(LumText);
            animationPlayer.Play(CageIcon);
            animationPlayer.Play(CageText);

            // TODO: Make displaying play time optional
            animationPlayer.Play(TimeIcon);
            animationPlayer.Play(TimeText);
        }
    }
}