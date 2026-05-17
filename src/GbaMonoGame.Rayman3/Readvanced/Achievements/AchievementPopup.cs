using System;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.AnimEngine;
using Microsoft.Xna.Framework.Graphics;

namespace GbaMonoGame.Rayman3.Readvanced;

public class AchievementPopup
{
    private const float BoxHeight = 36;
    private const float BoxTopMargin = 14;
    private const float BackgroundTopMargin = -4;
    private const float IconMargin = 8;
    private const float IconSize = 32;
    private const float TextLeftMargin = 2;
    private const float TextRightMargin = 5;
    private const float TextLineHeight = 14;
    private const int MoveSpeed = 2;
    private const int WaitTime = 2 * 60;
    private const int TextMaxLines = 2;
    private const FontSize TextFontSize = FontSize.Font16;

    public AnimationPlayer AnimationPlayer { get; set; }

    public Texture2D BackgroundTexture { get; set; }
    public Texture2D BackgroundGoldTexture { get; set; }

    public SpriteTextureObject Background { get; set; }
    public SpriteTextureObject Icon { get; set; }
    public SpriteTextObject[] TextLines { get; set; }

    public Vector2 BackgroundScreenPos { get; set; }
    public Vector2 IconScreenPos { get; set; }
    public Vector2 TextLinesBaseScreenPos { get; set; }
    public Vector2[] TextLinesScreenPos { get; set; }

    public BarDrawStep DrawStep { get; set; }
    public float MinY { get; set; }
    public float MaxY { get; set; }
    public float OffsetY { get; set; }
    public int WaitTimer { get; set; }
    public bool IsShowingPopup => DrawStep != BarDrawStep.Hide;

    public void SetText(string text)
    {
        // Get the available size for the text
        Vector2 availableSize = new(
            x: BackgroundTexture.Width - (IconSize + IconMargin) - TextLeftMargin - TextRightMargin,
            y: BoxHeight);

        // Wrap the text to fit
        string wrappedText = FontManager.WrapText(TextFontSize, text, availableSize.X);
        string[] lines = wrappedText.Split(['\n', '\r'], StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length > TextLines.Length)
            throw new Exception($"Achievement title \"{text}\" is too long to fit in the popup");

        // Vertically center the text
        float textHeight = lines.Length * TextLineHeight;
        Vector2 textPos = TextLinesBaseScreenPos + new Vector2(0, (availableSize.Y - textHeight) / 2f);

        // Set each text line
        for (int i = 0; i < TextLines.Length; i++)
        {
            if (i >= lines.Length)
            {
                TextLines[i].Text = String.Empty;
                continue;
            }

            string line = lines[i];
            Vector2 posOffset = new((availableSize.X - FontManager.GetStringWidth(TextFontSize, line)) / 2f, TextLineHeight * i);
            TextLinesScreenPos[i] = textPos + posOffset;
            TextLines[i].Text = line;
        }
    }

    public void SetRank(bool isGold)
    {
        Background.Texture = isGold ? BackgroundGoldTexture : BackgroundTexture;
    }

    public void SetIcon(string iconPath)
    {
        Icon.Texture = Engine.Assets.FixContentManager.Load<Texture2D>(iconPath);
    }

    public void MoveIn()
    {
        DrawStep = BarDrawStep.MoveIn;
        OffsetY = MinY;
        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__LumTotal_Mix02);
    }

    public void Init()
    {
        AnimationPlayer = new AnimationPlayer(false, null);
        RenderContext renderContext = Engine.GameRenderContext;

        // Load textures
        BackgroundTexture = Engine.Assets.FixContentManager.Load<Texture2D>(Assets.Achievements.PopupBackground);
        BackgroundGoldTexture = Engine.Assets.FixContentManager.Load<Texture2D>(Assets.Achievements.PopupBackgroundGold);

        // Create objects
        Background = new SpriteTextureObject()
        {
            HorizontalAnchor = HorizontalAnchorMode.Center,
            RenderContext = renderContext,
            SpriteType = SpriteType.Overlay,
        };
        BackgroundScreenPos = new Vector2(-(BackgroundTexture.Width / 2), BackgroundTopMargin);

        Icon = new SpriteTextureObject()
        {
            HorizontalAnchor = HorizontalAnchorMode.Center,
            RenderContext = renderContext,
            SpriteType = SpriteType.Overlay,
        };
        IconScreenPos = BackgroundScreenPos + 
                        new Vector2(IconMargin, BoxTopMargin + (BoxHeight - IconSize) / 2f);

        TextLines = new SpriteTextObject[TextMaxLines];
        TextLinesScreenPos = new Vector2[TextMaxLines];
        for (int i = 0; i < TextLines.Length; i++)
        {
            TextLines[i] = new SpriteTextObject
            {
                HorizontalAnchor = HorizontalAnchorMode.Center,
                RenderContext = renderContext,
                Color = TextColor.TextBox,
                FontSize = TextFontSize,
                SpriteType = SpriteType.Overlay,
            };
        }
        TextLinesBaseScreenPos = BackgroundScreenPos + 
                                 new Vector2(0, BoxTopMargin) +
                                 new Vector2(IconMargin + IconSize, 0) +
                                 new Vector2(TextLeftMargin, 0);

        // Initialize transition values
        DrawStep = BarDrawStep.Hide;
        MinY = -(BackgroundTexture.Height + BackgroundScreenPos.Y);
        MaxY = 0;
        OffsetY = MinY;
    }

    public void Step()
    {
        // Manage transition
        switch (DrawStep)
        {
            case BarDrawStep.MoveIn:
                OffsetY += MoveSpeed;
                if (OffsetY >= MaxY)
                {
                    OffsetY = MaxY;
                    DrawStep = BarDrawStep.Wait;
                    WaitTimer = 0;
                }
                break;

            case BarDrawStep.MoveOut:
                OffsetY -= MoveSpeed;
                if (OffsetY <= MinY)
                {
                    OffsetY = MinY;
                    DrawStep = BarDrawStep.Hide;
                }
                break;

            case BarDrawStep.Wait:
                if (WaitTimer >= WaitTime)
                    DrawStep = BarDrawStep.MoveOut;
                else
                    WaitTimer++;
                break;
        }

        // Update positions
        Vector2 offset = new(0, OffsetY);
        Background.ScreenPos = BackgroundScreenPos + offset;
        Icon.ScreenPos = IconScreenPos + offset;
        for (int i = 0; i < TextLines.Length; i++)
            TextLines[i].ScreenPos = TextLinesScreenPos[i] + offset;
    }

    public void Draw()
    {
        if (DrawStep == BarDrawStep.Hide)
            return;

        AnimationPlayer.Play(Background);
        AnimationPlayer.Play(Icon);
        foreach (SpriteTextObject textLine in TextLines)
            AnimationPlayer.Play(textLine);
        AnimationPlayer.Execute();
    }
}