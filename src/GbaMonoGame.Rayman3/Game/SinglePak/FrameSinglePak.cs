using BinarySerializer.Ubisoft.GbaEngine;
using GbaMonoGame.AnimEngine;

namespace GbaMonoGame.Rayman3.SinglePak;

// TODO: Implement loading screen?
// The game treats the SinglePak game as its own game since it's in a separate ROM, but here it's easier creating a new Frame for it
public class FrameSinglePak : Frame
{
    public const int PlayersCount = 2;

    public RenderContext RenderContext { get; set; }
    public AnimationPlayer AnimationPlayer { get; set; }

    public ushort Timer { get; set; }
    public byte field_0x18 { get; set; } // TODO: Name
    public bool field_0x3f7 { get; set; } // TODO: Name
    public byte field_0x3fd { get; set; } // TODO: Name
    public byte field_0x3f6 { get; set; } // TODO: Name
    public ushort field_0x3f8 { get; set; } // TODO: Name

    public Player[] Players { get; set; }
    public Missile[] Missiles { get; set; }
    public Missile UnkMissile { get; set; } // TODO: Name
    public Items[] Items { get; set; }
    public byte[] field_0x1c { get; set; } // TODO: Name
    public byte[] field_0x3fb { get; set; } // TODO: Name

    public AnimatedObject Target { get; set; }
    public AnimatedObject Arrow { get; set; }
    public SpriteTextObject UnknownSpriteTextObject1 { get; set; } // TODO: Name
    public SpriteTextObject UnknownSpriteTextObject2 { get; set; } // TODO: Name
    public SpriteTextObject UnknownSpriteTextObject3 { get; set; } // TODO: Name
    public SpriteTextObject UnknownSpriteTextObject4 { get; set; } // TODO: Name

    public T LoadResource<T>(int index)
        where T : Resource, new()
    {
        // TODO: Root table is wrong...
        return Rom.Loader.Rayman3_SinglePakOffsetTable.ReadResource<T>(Rom.Context, index);
    }

    public string GetString(int id)
    {
        // TODO: Implement
        return "";
    }

    public override void Init()
    {
        RenderContext = Engine.GameRenderContext;

        // TODO: Load tileset
        // TODO: Load map palette
        // TODO: Load map 1
        // TODO: Load map 2
        // TODO: Set BG0CNT
        // TODO: Set DISPCNT

        Timer = 0;
        field_0x3fd = 100;
        GameTime.Reset(); // TODO: Restore old gametime when exiting? Or don't reset at all?
        RSMultiplayer.UnInit();
        field_0x18 = 0;
        field_0x3f7 = false;

        AnimationPlayer = new AnimationPlayer(false, null);

        Players = new Player[PlayersCount];
        Missiles = new Missile[PlayersCount];
        field_0x1c = new byte[PlayersCount];
        field_0x3fb = new byte[PlayersCount];
        for (int i = 0; i < PlayersCount; i++)
        {
            Players[i] = new Player(this, i);
            Missiles[i] = new Missile(this, i);
            field_0x1c[i] = 0;
            field_0x3fb[i] = 0;
        }

        UnkMissile = new Missile(this, 2);

        Items = new Items[3];
        for (int i = 0; i < Items.Length; i++)
            Items[i] = new Items(this, i);

        Target = new AnimatedObject(LoadResource<AnimatedObjectResource>(3), false)
        {
            ScreenPos = new Vector2(240, 20),
            CurrentAnimation = 12,
            RenderContext = RenderContext,
        };
        Arrow = new AnimatedObject(LoadResource<AnimatedObjectResource>(3), false)
        {
            ScreenPos = new Vector2(250, 23),
            CurrentAnimation = 13,
            RenderContext = RenderContext,
        };

        UnknownSpriteTextObject1 = new SpriteTextObject
        {
            FontSize = FontSize.Font16,
            Color = TextColor.SinglePak,
            Text = GetString(0), // PRESS A BUTTON WHEN READY
            HorizontalAnchor = HorizontalAnchorMode.Center,
            VerticalAnchor = VerticalAnchorMode.Scale,
            RenderContext = RenderContext,
        };
        UnknownSpriteTextObject1.ScreenPos = new Vector2(-UnknownSpriteTextObject1.GetStringWidth() / 2f, 120);
        
        UnknownSpriteTextObject3 = new SpriteTextObject
        {
            FontSize = FontSize.Font16,
            Color = TextColor.SinglePak,
            Text = GetString(7), // Empty
            HorizontalAnchor = HorizontalAnchorMode.Center,
            VerticalAnchor = VerticalAnchorMode.Scale,
            RenderContext = RenderContext,
        };
        UnknownSpriteTextObject3.ScreenPos = new Vector2(-UnknownSpriteTextObject3.GetStringWidth() / 2f, 140);
        
        UnknownSpriteTextObject2 = new SpriteTextObject
        {
            FontSize = FontSize.Font16,
            Color = TextColor.SinglePak,
            Text = "",
            HorizontalAnchor = HorizontalAnchorMode.Center,
            VerticalAnchor = VerticalAnchorMode.Scale,
            RenderContext = RenderContext,
        };

        UnknownSpriteTextObject4 = new SpriteTextObject
        {
            FontSize = FontSize.Font16,
            Color = TextColor.SinglePak,
            Text = GetString(5), // Game Boy Advance
            HorizontalAnchor = HorizontalAnchorMode.Center,
            VerticalAnchor = VerticalAnchorMode.Scale,
            RenderContext = RenderContext,
        };
        UnknownSpriteTextObject4.ScreenPos = new Vector2(-UnknownSpriteTextObject4.GetStringWidth() / 2f, 80);

        field_0x3f6 = 1;
        field_0x3f8 = 0;
    }

    public override void UnInit()
    {
        // TODO: Implement
    }

    public override void Step()
    {
        bool bVar1 = true;

        // TODO: Clean up
        if (field_0x3f6 != 0)
        {
            int uVar19 = 0;
            if (field_0x1c[0] == 0)
            {
                bVar1 = false;
            }
            else
            {
                while (uVar19 + 1 < field_0x3f6)
                {
                    int iVar9 = uVar19 + 1;
                    uVar19++;

                    if (field_0x1c[iVar9] == 0)
                    {
                        bVar1 = false;
                        break;
                    }
                }
            }
        }

        if (bVar1)
        {
            if (MultiJoyPad.IsButtonJustPressed(0, GbaInput.Start) || 
                MultiJoyPad.IsButtonJustPressed(1, GbaInput.Start))
            {
                field_0x3f7 = !field_0x3f7;
            }

            if (!field_0x3f7)
            {
                foreach (Items item in Items)
                    item.Step();
            }
        }

        for (int i = 0; i < field_0x3f6; i++)
        {
            // TODO: Implement            
        }

        UnkMissile.Step();

        UnkMissile.Draw(AnimationPlayer);
        foreach (Items item in Items)
            item.Draw(AnimationPlayer);

        AnimationPlayer.Execute();

        Timer++;
        Timer %= 2048;

        // TODO: Set background scroll
    }
}