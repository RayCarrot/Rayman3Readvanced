using BinarySerializer.Ubisoft.GbaEngine;
using GbaMonoGame.AnimEngine;
using GbaMonoGame.Rayman3.Readvanced;

namespace GbaMonoGame.Rayman3;

public class SlotMenuOption : MenuOption
{
    public ModernMenuAll.Slot Slot { get; set; }

    public AnimatedObject Icon { get; set; }
    public AnimatedObject LumIcon { get; set; }
    public AnimatedObject CageIcon { get; set; }
    public SpriteTextObject LumText { get; set; }
    public SpriteTextObject CageText { get; set; }
    public SpriteFontTextObject EmptyText { get; set; }

    public override void Init(ModernMenuAll menu, RenderContext renderContext, Vector2 position, int index)
    {
        Slot = menu.Slots[index];

        AnimatedObjectResource propsAnimations = Rom.LoadResource<AnimatedObjectResource>(GameResource.MenuPropAnimations);

        Icon = new AnimatedObject(propsAnimations, propsAnimations.IsDynamic)
        {
            IsFramed = true,
            BgPriority = 3,
            ObjPriority = 0,
            ScreenPos = position,
            CurrentAnimation = 8 + index,
            RenderContext = renderContext,
        };
        LumIcon = new AnimatedObject(propsAnimations, propsAnimations.IsDynamic)
        {
            IsFramed = true,
            BgPriority = 3,
            ObjPriority = 0,
            ScreenPos = position + new Vector2(23, 1),
            CurrentAnimation = 13,
            RenderContext = renderContext,
        };
        CageIcon = new AnimatedObject(propsAnimations, propsAnimations.IsDynamic)
        {
            IsFramed = true,
            BgPriority = 3,
            ObjPriority = 0,
            ScreenPos = position + new Vector2(81, -4),
            CurrentAnimation = 11,
            RenderContext = renderContext,
        };
        LumText = new SpriteTextObject()
        {
            BgPriority = 3,
            ObjPriority = 0,
            ScreenPos = position + new Vector2(45, 1),
            Text = Slot?.LumsCount.ToString() ?? "0",
            FontSize = FontSize.Font16,
            Color = TextColor.Menu,
            RenderContext = renderContext,
        };
        CageText = new SpriteTextObject()
        {
            BgPriority = 3,
            ObjPriority = 0,
            ScreenPos = position + new Vector2(106, 1),
            Text = Slot?.CagesCount.ToString() ?? "0",
            FontSize = FontSize.Font16,
            Color = TextColor.Menu,
            RenderContext = renderContext,
        };
        EmptyText = new SpriteFontTextObject()
        {
            BgPriority = 3,
            ObjPriority = 0,
            ScreenPos = position + new Vector2(42, 13),
            Text = "EMPTY",
            Font = ReadvancedFonts.MenuYellow,
            RenderContext = renderContext,
        };
    }

    public override void Draw(AnimationPlayer animationPlayer)
    {
        animationPlayer.Play(Icon);

        if (Slot == null)
        {
            animationPlayer.Play(EmptyText);
        }
        else
        {
            animationPlayer.Play(LumIcon);
            animationPlayer.Play(CageIcon);
            animationPlayer.Play(LumText);
            animationPlayer.Play(CageText);
        }
    }
}