using System;
using System.Collections.Immutable;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.AnimEngine;
using Microsoft.Xna.Framework.Graphics;

namespace GbaMonoGame.Rayman3.Readvanced;

public class AchievementsMenuPage : MenuPage
{
    public AchievementsMenuPage(ModernMenuAll menu) : base(menu) { }

    private const int AchievementsPerRow = 3;
    private const int HorizontalMargin = 8;
    private const int SelectedAchievementTitleTopMargin = 20;
    private const int SelectedAchievementTitleMaxLines = 2;
    private const int SelectedAchievementIconSize = 64;
    private const int SelectedAchievementIconTopMargin = 36;
    private const int SelectedAchievementDescriptionTopMargin = -2;

    public override bool UsesCursor => false;
    public override int BackgroundPalette => 2;
    public override int LineHeight => 36;
    public override int MaxOptions => 3;
    public override bool HasScrollBar => true;
    public override MenuScrollBarSize ScrollBarSize => MenuScrollBarSize.Big;

    public SpriteTextureObject Cloth { get; set; }
    public SpriteFontTextObject[] SelectedAchievementTitleLines { get; set; }
    public SpriteTextureObject SelectedAchievementIcon { get; set; }
    public SpriteTextObject SelectedAchievementDescription { get; set; }

    private void UpdateSelectedAchievement()
    {
        AchievementMenuOption option = (AchievementMenuOption)Options[SelectedOption];
        AchievementInfo achievement = option.Achievements[option.SelectedIndex];

        // Get the available width
        int availableWidth = Cloth.Texture.Width - HorizontalMargin - HorizontalMargin;

        // Set the title
        Font titleFont = SelectedAchievementTitleLines[0].Font;
        string wrappedTitle = titleFont.WrapText(achievement.Title, availableWidth);
        string[] lines = wrappedTitle.Split(['\n', '\r'], StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length > SelectedAchievementTitleLines.Length)
            throw new Exception($"Achievement title \"{achievement.Title}\" is too long to fit in the menu");

        float textHeight = lines.Length * titleFont.LineHeight;
        Vector2 textPos = Cloth.ScreenPos + 
                          new Vector2(HorizontalMargin, SelectedAchievementTitleTopMargin) + 
                          new Vector2(0, ((SelectedAchievementTitleMaxLines * titleFont.LineHeight) - textHeight) / 2f);

        // Set each title text line
        for (int i = 0; i < SelectedAchievementTitleLines.Length; i++)
        {
            if (i >= lines.Length)
            {
                SelectedAchievementTitleLines[i].Text = String.Empty;
                continue;
            }

            string line = lines[i];
            Vector2 posOffset = new((availableWidth - titleFont.GetWidth(line)) / 2f, titleFont.LineHeight * i);
            SelectedAchievementTitleLines[i].ScreenPos = textPos + posOffset;
            SelectedAchievementTitleLines[i].Text = line;
        }

        // Set the icon
        SelectedAchievementIcon.Texture = Engine.FixContentManager.Load<Texture2D>(achievement.BigIconTexturePath);

        // Set the text and wrap
        SelectedAchievementDescription.Text = FontManager.WrapText(SelectedAchievementDescription.FontSize, achievement.Description, availableWidth);
    }

    protected override bool SetSelectedOption(int selectedOption, bool playSound = true, bool forceUpdate = false)
    {
        int prevSelectedIndex = ((AchievementMenuOption)Options[SelectedOption]).SelectedIndex;

        if (!base.SetSelectedOption(selectedOption, playSound, forceUpdate))
            return false;

        ((AchievementMenuOption)Options[SelectedOption]).SetSelectedIndex(prevSelectedIndex);
        UpdateSelectedAchievement();

        return true;
    }

    protected override void Init()
    {
        Cloth = new SpriteTextureObject
        {
            BgPriority = 3,
            ObjPriority = 0,
            ScreenPos = new Vector2(190, 38),
            RenderContext = RenderContext,
            Texture = Engine.FrameContentManager.Load<Texture2D>(Assets.ClothTexture)
        };

        SelectedAchievementTitleLines = new SpriteFontTextObject[SelectedAchievementTitleMaxLines];
        for (int i = 0; i < SelectedAchievementTitleLines.Length; i++)
        {
            SelectedAchievementTitleLines[i] = new SpriteFontTextObject
            {
                BgPriority = 3,
                ObjPriority = 0,
                RenderContext = RenderContext,
                Font = ReadvancedFonts.MenuYellow,
            };
        }

        SelectedAchievementIcon = new SpriteTextureObject()
        {
            BgPriority = 3,
            ObjPriority = 0,
            ScreenPos = Cloth.ScreenPos + new Vector2((Cloth.Texture.Width - SelectedAchievementIconSize) / 2f, SelectedAchievementIconTopMargin),
            RenderContext = RenderContext,
        };

        SelectedAchievementDescription = new SpriteTextObject
        {
            BgPriority = 3,
            ObjPriority = 0,
            ScreenPos = Cloth.ScreenPos + new Vector2(HorizontalMargin, SelectedAchievementIconTopMargin + SelectedAchievementIconSize + SelectedAchievementDescriptionTopMargin),
            RenderContext = RenderContext,
            Color = TextColor.TextBox,
            FontSize = FontSize.Font16,
        };

        // Add achievements, 3 per row
        ImmutableArray<AchievementInfo> achievements = AchievementsManager.Achievements.Values;
        for (int i = 0; i < achievements.Length; i += AchievementsPerRow)
        {
            ImmutableArray<AchievementInfo> slice = achievements.Slice(i, Math.Min(AchievementsPerRow, achievements.Length - i));
            AddOption(new AchievementMenuOption(slice));
        }

        // Set default selected achievement
        ((AchievementMenuOption)Options[SelectedOption]).SetSelectedIndex(0);
        UpdateSelectedAchievement();
    }

    protected override void Step_Active()
    {
        if (JoyPad.IsButtonJustPressed(Rayman3Input.MenuUp))
        {
            SetSelectedOption(SelectedOption - 1);
        }
        else if (JoyPad.IsButtonJustPressed(Rayman3Input.MenuDown))
        {
            SetSelectedOption(SelectedOption + 1);
        }
        else if (JoyPad.IsButtonJustPressed(Rayman3Input.MenuLeft))
        {
            AchievementMenuOption option = (AchievementMenuOption)Options[SelectedOption];
            option.SetSelectedIndex(option.SelectedIndex - 1);
            UpdateSelectedAchievement();
            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__MenuMove);
        }
        else if (JoyPad.IsButtonJustPressed(Rayman3Input.MenuRight))
        {
            AchievementMenuOption option = (AchievementMenuOption)Options[SelectedOption];
            option.SetSelectedIndex(option.SelectedIndex + 1);
            UpdateSelectedAchievement();
            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__MenuMove);
        }
        else if (JoyPad.IsButtonJustPressed(Rayman3Input.MenuBack))
        {
            Menu.ChangePage(new BonusMenuPage(Menu), NewPageMode.Back);
        }
    }

    protected override void Draw(AnimationPlayer animationPlayer)
    {
        DrawOptions(animationPlayer);

        animationPlayer.Play(Cloth);
        foreach (SpriteFontTextObject line in SelectedAchievementTitleLines)
            animationPlayer.Play(line);
        animationPlayer.Play(SelectedAchievementIcon);
        animationPlayer.Play(SelectedAchievementDescription);
    }
}