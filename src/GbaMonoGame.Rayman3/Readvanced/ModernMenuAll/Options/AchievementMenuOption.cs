using System.Collections.Generic;
using GbaMonoGame.AnimEngine;
using Microsoft.Xna.Framework.Graphics;

namespace GbaMonoGame.Rayman3.Readvanced;

public class AchievementMenuOption : MenuOption
{
    public AchievementMenuOption(IReadOnlyList<AchievementInfo> achievements)
    {
        Achievements = achievements;
    }

    // Keep static so it stays consistent when changing option
    private static float CurrentScale { get; set; }
    private static bool IsIncreasingScale { get; set; }

    public IReadOnlyList<AchievementInfo> Achievements { get; }
    public SpriteTextureObject[] Icons { get; set; }
    public int SelectedIndex { get; set; }

    private void ResetScale()
    {
        foreach (SpriteTextureObject icon in Icons)
            icon.AffineMatrix = null;
    }

    public override void Init(int bgPriority, RenderContext renderContext, int index)
    {
        CurrentScale = 1;
        IsIncreasingScale = false;

        Icons = new SpriteTextureObject[Achievements.Count];
        for (int i = 0; i < Icons.Length; i++)
        {
            Icons[i] = new SpriteTextureObject
            {
                BgPriority = bgPriority,
                ObjPriority = 0,
                RenderContext = renderContext,
                Texture = Engine.FixContentManager.Load<Texture2D>(Achievements[i].SmallIconTexturePath),
            };
        }
    }

    public override void SetPosition(Vector2 position)
    {
        for (int i = 0; i < Icons.Length; i++)
            Icons[i].ScreenPos = position + new Vector2(36 * i, -4);
    }

    public override void ChangeIsSelected(bool isSelected)
    {
        // Reset regardless if selected or not as we set the actual selection later
        SelectedIndex = -1;
        ResetScale();
    }

    public void SetSelectedIndex(int index)
    {
        if (index >= Achievements.Count)
            index = 0;
        else if (index < 0)
            index = Achievements.Count - 1;
        SelectedIndex = index;
        ResetScale();
    }

    public override void Draw(AnimationPlayer animationPlayer)
    {
        // Animate the selected icon with a scaling effect
        if (SelectedIndex >= 0)
        {
            Icons[SelectedIndex].AffineMatrix = new AffineMatrix(0, new Vector2(CurrentScale));

            // Same code as in the console Rayman 3 cinematic menu
            if (IsIncreasingScale)
            {
                CurrentScale += 0.02f;
                if (CurrentScale >= 1)
                    IsIncreasingScale = false;
            }
            else
            {
                CurrentScale -= 0.02f;
                if (CurrentScale < 0.7f)
                    IsIncreasingScale = true;
            }
        }

        foreach (SpriteTextureObject icon in Icons)
            animationPlayer.Play(icon);
    }
}