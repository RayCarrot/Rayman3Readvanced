using BinarySerializer.Ubisoft.GbaEngine;
using GbaMonoGame.AnimEngine;
using Microsoft.Xna.Framework;

namespace GbaMonoGame.Rayman3.Readvanced;

public class LevelsMenuOption : TextMenuOption
{
    public LevelsMenuOption(int levelCurtainId, bool isAvailable, float scale) : base(GetLevelName(levelCurtainId, isAvailable), scale)
    {
        LevelCurtainId = levelCurtainId;
        IsAvailable = isAvailable;
    }

    public int LevelCurtainId { get; }
    public bool IsAvailable { get; }

    public AnimatedObject LumIcon { get; set; }
    public SpriteTextObject LumText { get; set; }
    public AnimatedObject CageIcon { get; set; }
    public SpriteTextObject CageText { get; set; }

    private static string GetLevelName(int levelCurtainId, bool isAvailable)
    {
        // TODO: Fix replacing char (same as in time attack)
        if (isAvailable)
            return Rayman3.Loc.GetText(TextBankId.LevelNames, GameInfo.Levels[(int)GameInfo.LevelMaps[levelCurtainId][0]].NameTextId)[0].ToUpperInvariant().Replace('’', '\'');
        else
            return "??";
    }

    public override void Init(int bgPriority, RenderContext renderContext, int index)
    {
        base.Init(bgPriority, renderContext, index);

        AnimatedObjectResource propsAnimations = Rom.Loader.ReadResource<AnimatedObjectResource>(Rayman3DefinedResource.MenuPropAnimations);

        // Calculate collectibles
        int collectedLums = 0;
        int totalLums = 0;
        int collectedCages = 0;
        int totalCages = 0;
        foreach (MapId mapId in GameInfo.LevelMaps[LevelCurtainId])
        {
            collectedLums += GameInfo.GetDeadLumsForCurrentMap(mapId);
            totalLums += GameInfo.Levels[(int)mapId].LumsCount;
            collectedCages += GameInfo.GetDeadCagesForCurrentMap(mapId);
            totalCages += GameInfo.Levels[(int)mapId].CagesCount;
        }

        Color textColor = new(197, 98, 0);

        if (totalLums != 0)
        {
            LumIcon = new AnimatedObject(propsAnimations, propsAnimations.IsDynamic)
            {
                IsFramed = true,
                BgPriority = bgPriority,
                ObjPriority = 0,
                CurrentAnimation = 13,
                RenderContext = renderContext,
            };
            LumText = new SpriteTextObject()
            {
                BgPriority = bgPriority,
                ObjPriority = 0,
                Text = IsAvailable ? $"{collectedLums:00}/{totalLums:00}" : "??",
                Color = textColor,
                RenderContext = renderContext,
            };
        }
        else
        {
            LumIcon = null;
            LumText = null;
        }

        if (totalCages != 0)
        {
            CageIcon = new AnimatedObject(propsAnimations, propsAnimations.IsDynamic)
            {
                IsFramed = true,
                BgPriority = bgPriority,
                ObjPriority = 0,
                CurrentAnimation = 11,
                RenderContext = renderContext,
            };
            CageText = new SpriteTextObject()
            {
                BgPriority = bgPriority,
                ObjPriority = 0,
                Text = IsAvailable ? $"{collectedCages}/{totalCages}" : "??",
                Color = textColor,
                RenderContext = renderContext,
            };
        }
        else
        {
            CageIcon = null;
            CageText = null;
        }
    }

    public override void SetPosition(Vector2 position)
    {
        base.SetPosition(position);

        LumIcon?.ScreenPos = position + new Vector2(163, 1);
        LumText?.ScreenPos = position + new Vector2(185, 1);
        CageIcon?.ScreenPos = position + new Vector2(226, -3);
        CageText?.ScreenPos = position + new Vector2(251, 1);
    }

    public override void Draw(AnimationPlayer animationPlayer)
    {
        base.Draw(animationPlayer);

        if (LumIcon != null)
            animationPlayer.Play(LumIcon);
        if (LumText != null)
            animationPlayer.Play(LumText);
        if (CageIcon != null)
            animationPlayer.Play(CageIcon);
        if (CageText != null)
            animationPlayer.Play(CageText);
    }
}