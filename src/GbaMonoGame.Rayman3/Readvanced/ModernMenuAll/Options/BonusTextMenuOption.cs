using GbaMonoGame.AnimEngine;
using GbaMonoGame.Rayman3.Readvanced;
using Microsoft.Xna.Framework.Graphics;

namespace GbaMonoGame.Rayman3;

public class BonusTextMenuOption : TextMenuOption
{
    public BonusTextMenuOption(string text, Collection[] collections) : base(text)
    {
        Collections = collections;
    }

    public Collection[] Collections { get; }
    public SpriteTextureObject[] CollectionIcons { get; set; }
    public SpriteTextObject[] CollectionTextObjects { get; set; }

    public override void Init(ModernMenuAll menu, RenderContext renderContext, Vector2 position, int index)
    {
        base.Init(menu, renderContext, position, index);

        const int startX = 110;
        const int width = 65;

        CollectionIcons = new SpriteTextureObject[Collections.Length];
        for (int i = 0; i < CollectionIcons.Length; i++)
        {
            CollectionIcons[i] = new SpriteTextureObject()
            {
                BgPriority = 3,
                ObjPriority = 0,
                ScreenPos = position + new Vector2(startX + i * width, 0),
                Texture = Engine.FrameContentManager.Load<Texture2D>(Collections[i].IconTexture),
                RenderContext = renderContext,
            };
        }

        CollectionTextObjects = new SpriteTextObject[Collections.Length];
        for (int i = 0; i < CollectionTextObjects.Length; i++)
        {
            CollectionTextObjects[i] = new SpriteTextObject()
            {
                BgPriority = 3,
                ObjPriority = 0,
                ScreenPos = position + new Vector2(startX + 19 + i * width, 1),
                RenderContext = renderContext,
                Text = Collections[i].Text,
                FontSize = FontSize.Font16,
                Color = TextColor.Menu,
            };
        }
    }

    public override void Draw(AnimationPlayer animationPlayer)
    {
        base.Draw(animationPlayer);

        foreach (SpriteTextureObject icon in CollectionIcons)
            animationPlayer.Play(icon);

        foreach (SpriteTextObject textObject in CollectionTextObjects)
            animationPlayer.Play(textObject);
    }

    public class Collection
    {
        public Collection(string iconTexture, string text)
        {
            IconTexture = iconTexture;
            Text = text;
        }

        public string IconTexture { get; }
        public string Text { get; }
    }
}