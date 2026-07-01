using System;
using GbaMonoGame.AnimEngine;
using Microsoft.Xna.Framework.Graphics;

namespace GbaMonoGame.Rayman3.Readvanced;

public class BonusActionMenuOption : ActionMenuOption
{
    public BonusActionMenuOption(string text, Collection[] collections, Action action) : base(text, action)
    {
        Collections = collections;
    }

    public Collection[] Collections { get; }
    public AObject[] CollectionIcons { get; set; }
    public SpriteTextObject[] CollectionTextObjects { get; set; }

    public override void Init(int bgPriority, RenderContext renderContext, int index)
    {
        base.Init(bgPriority, renderContext, index);

        CollectionIcons = new AObject[Collections.Length];
        for (int i = 0; i < CollectionIcons.Length; i++)
        {
            if (Collections[i].IconTexture != null)
            {
                CollectionIcons[i] = new SpriteTextureObject()
                {
                    Texture = Engine.Assets.FrameContentManager.Load<Texture2D>(Collections[i].IconTexture),
                };
            }
            else
            {
                CollectionIcons[i] = Collections[i].IconAnimatedObject;
            }

            CollectionIcons[i].BgPriority = bgPriority;
            CollectionIcons[i].ObjPriority = 0;
            CollectionIcons[i].RenderContext = renderContext;
        }

        CollectionTextObjects = new SpriteTextObject[Collections.Length];
        for (int i = 0; i < CollectionTextObjects.Length; i++)
        {
            CollectionTextObjects[i] = new SpriteTextObject()
            {
                BgPriority = bgPriority,
                ObjPriority = 0,
                RenderContext = renderContext,
                Text = Collections[i].Text,
                Color = TextColor.Menu,
            };
        }
    }

    public override void SetPosition(Vector2 position)
    {
        base.SetPosition(position);

        const int startX = 110;
        const int width = 67;

        for (int i = 0; i < CollectionIcons.Length; i++)
            CollectionIcons[i].ScreenPos = position + new Vector2(startX + i * width, 0) + Collections[i].Offset;

        for (int i = 0; i < CollectionTextObjects.Length; i++)
            CollectionTextObjects[i].ScreenPos = position + new Vector2(startX + 19 + i * width, 1);
    }

    public override void Draw(AnimationPlayer animationPlayer)
    {
        base.Draw(animationPlayer);

        foreach (AObject icon in CollectionIcons)
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
        public Collection(AnimatedObject iconAnimatedObject, Vector2 offset, string text)
        {
            IconAnimatedObject = iconAnimatedObject;
            Offset = offset;
            Text = text;
        }

        public string IconTexture { get; }
        public AnimatedObject IconAnimatedObject { get; }
        public Vector2 Offset { get; }
        public string Text { get; }
    }
}