using BinarySerializer.Ubisoft.GbaEngine;
using GbaMonoGame.AnimEngine;

namespace GbaMonoGame.Rayman3;

public class GameCubeMenuData
{
    public GameCubeMenuData()
    {
        AnimatedObjectResource animations = Rom.LoadResource<AnimatedObjectResource>(GameResource.GameCubeMenuAnimations);
        AnimatedObjectResource levelCheckAnimations = Rom.LoadResource<AnimatedObjectResource>(GameResource.GameCubeMenuLevelCheckAnimations);

        ReusableTexts = new SpriteTextObject[4];
        for (int i = 0; i < ReusableTexts.Length; i++)
        {
            ReusableTexts[i] = new SpriteTextObject()
            {
                Text = "",
                FontSize = FontSize.Font16,
                Color = TextColor.GameCubeMenu,
            };
        }

        LumRequirementTexts = new SpriteTextObject[3];
        for (int i = 0; i < LumRequirementTexts.Length; i++)
        {
            LumRequirementTexts[i] = new SpriteTextObject()
            {
                Text = "",
                ScreenPos = new Vector2(192, 36 + i * 24),
                FontSize = FontSize.Font16,
                Color = TextColor.GameCubeMenu,
            };
        }

        int collectedYellowLums = GameInfo.GetTotalCollectedYellowLums();
        TotalLumsText = new SpriteTextObject()
        {
            Text = collectedYellowLums.ToString(),
            ScreenPos = new Vector2(36, 16),
            FontSize = FontSize.Font16,
            Color = TextColor.GameCubeMenu,
        };

        StatusText = new SpriteTextObject()
        {
            Text = "",
            FontSize = FontSize.Font16,
            Color = TextColor.GameCubeMenu,
        };

        Wheel1 = new AnimatedObject(animations, animations.IsDynamic)
        {
            IsFramed = true,
            BgPriority = 0,
            ObjPriority = 0,
            ScreenPos = new Vector2(6, 106),
            CurrentAnimation = 0,
            AffineMatrix = AffineMatrix.Identity
        };

        Wheel2 = new AnimatedObject(animations, animations.IsDynamic)
        {
            IsFramed = true,
            BgPriority = 0,
            ObjPriority = 0,
            ScreenPos = new Vector2(132, 105),
            CurrentAnimation = 1,
            AffineMatrix = AffineMatrix.Identity
        };

        Wheel3 = new AnimatedObject(animations, animations.IsDynamic)
        {
            IsFramed = true,
            BgPriority = 0,
            ObjPriority = 32,
            ScreenPos = new Vector2(172, 108),
            CurrentAnimation = 2,
            AffineMatrix = AffineMatrix.Identity
        };

        Wheel4 = new AnimatedObject(animations, animations.IsDynamic)
        {
            IsFramed = true,
            BgPriority = 0,
            ObjPriority = 0,
            ScreenPos = new Vector2(66, 138),
            CurrentAnimation = 1,
            AffineMatrix = AffineMatrix.Identity
        };

        LumIcons = new AnimatedObject[3];
        LevelChecks = new AnimatedObject[3];
        for (int i = 0; i < 3; i++)
        {
            LumIcons[i] = new AnimatedObject(animations, animations.IsDynamic)
            {
                IsFramed = true,
                BgPriority = 0,
                ObjPriority = 0,
                ScreenPos = new Vector2(170, 36 + i * 24),
                CurrentAnimation = 3,
            };
            LevelChecks[i] = new AnimatedObject(levelCheckAnimations, levelCheckAnimations.IsDynamic)
            {
                IsFramed = true,
                BgPriority = 0,
                ObjPriority = 0,
                ScreenPos = new Vector2(69, 50 + i * 24),
            };
        }
    }

    public SpriteTextObject[] ReusableTexts { get; }
    public SpriteTextObject[] LumRequirementTexts { get; }
    public SpriteTextObject TotalLumsText { get; }
    public SpriteTextObject StatusText { get; }
    public AnimatedObject Wheel1 { get; }
    public AnimatedObject Wheel2 { get; }
    public AnimatedObject Wheel3 { get; }
    public AnimatedObject Wheel4 { get; }
    public AnimatedObject[] LumIcons { get; }
    public AnimatedObject[] LevelChecks { get; }
}