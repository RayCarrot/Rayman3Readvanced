using BinarySerializer.Ubisoft.GbaEngine;
using GbaMonoGame.AnimEngine;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public class UserInfoWorld : Dialog
{
    public UserInfoWorld(Scene2D scene, bool hasBlueLum) : base(scene)
    {
        LifeBar = new LifeBar(scene);
        LevelInfoBar = new LevelInfoBar(scene);
        Lums1000Bar = new Lums1000Bar(scene);
        Cages50Bar = new Cages50Bar(scene);

        if (hasBlueLum)
            BlueLumBar = new BlueLumBar(scene);

        if (GameInfo.MapId == MapId.WorldMap)
            WorldNameBar = new WorldNameBar(scene);

        LifeBar.SetToStayVisible();

        Hide = false;
        ShouldPlayCurtainAnimation = true;
    }

    public LifeBar LifeBar { get; }
    public LevelInfoBar LevelInfoBar { get; }
    public Lums1000Bar Lums1000Bar { get; }
    public Cages50Bar Cages50Bar { get; }
    public BlueLumBar BlueLumBar { get; }
    public WorldNameBar WorldNameBar { get; }

    public bool ShouldPlayCurtainAnimation { get; set; }

    // The game only has a single animated object, but we split it into two in order
    // to support different screen resolutions (so it fills the width of the screen)
    public AnimatedObject CurtainsLeft { get; set; }
    public AnimatedObject CurtainsRight { get; set; }

    public bool Hide { get; set; }

    protected override bool ProcessMessageImpl(object sender, Message message, object param)
    {
        // Handle messages
        switch (message)
        {
            case Message.UserInfo_Pause:
                BlueLumBar?.SetToStayHidden();
                WorldNameBar?.MoveOutWorldNameBar();
                return true;

            case Message.UserInfo_Unpause:
                BlueLumBar?.SetToDefault();
                WorldNameBar?.MoveInWorldNameBar();
                return true;

            default:
                return false;
        }
    }

    public void SetLevelInfoBar(int levelCurtainId)
    {
        LevelInfoBar.SetLevel(levelCurtainId);
    }

    public bool HasFinishedMovingInCurtains()
    {
        if (CurtainsLeft == null)
            return true;

        if (CurtainsLeft.EndOfAnimation)
        {
            CurtainsLeft.CurrentAnimation = 0;
            CurtainsRight.CurrentAnimation = 0;
            return true;
        }

        return false;
    }

    public void MoveOutCurtains()
    {
        if (CurtainsLeft == null)
            return;

        CurtainsLeft.CurrentAnimation = 2;
        CurtainsRight.CurrentAnimation = 2;
    }

    public bool HasFinishedMovingOutCurtains()
    {
        if (CurtainsLeft == null)
            return true;

        if (CurtainsLeft.EndOfAnimation)
            return true;

        return false;
    }

    public override void Load()
    {
        LifeBar.Load();
        LevelInfoBar.Load();
        Lums1000Bar.Load();
        Cages50Bar.Load();
        BlueLumBar?.Load();
        WorldNameBar?.Load();

        LifeBar.Set();
        LevelInfoBar.Set();
        Lums1000Bar.Set();
        Cages50Bar.Set();
        BlueLumBar?.Set();

        if (Engine.Settings.Platform == Platform.GBA && GameInfo.MapId != MapId.WorldMap)
        {
            AnimatedObjectResource resource = Storage.LoadResource<AnimatedObjectResource>(GameResource.WorldCurtainAnimations);

            CurtainsLeft = new AnimatedObject(resource, false)
            {
                IsFramed = true,
                CurrentAnimation = ShouldPlayCurtainAnimation ? 1 : 0,
                ScreenPos = new Vector2(120, 56),
                OverrideGfxColor = true, // Should not be effected by the palette fading
                RenderContext = Engine.OriginalGameRenderContext,
            };
            CurtainsRight = new AnimatedObject(resource, false)
            {
                IsFramed = true,
                CurrentAnimation = ShouldPlayCurtainAnimation ? 1 : 0,
                ScreenPos = new Vector2(-120, 56),
                HorizontalAnchor = HorizontalAnchorMode.Right,
                OverrideGfxColor = true, // Should not be effected by the palette fading
                RenderContext = Engine.OriginalGameRenderContext,
            };

            for (int i = 0; i < 6; i++)
            {
                CurtainsLeft.SetChannelInvisible(i);
                CurtainsRight.SetChannelInvisible(i + 6);
            }

            if (ShouldPlayCurtainAnimation)
                ShouldPlayCurtainAnimation = false;
        }
    }

    public override void Init() { }

    public override void Draw(AnimationPlayer animationPlayer)
    {
        if (Engine.Settings.Platform == Platform.GBA && GameInfo.MapId != MapId.WorldMap)
        {
            animationPlayer.PlayFront(CurtainsLeft);
            animationPlayer.PlayFront(CurtainsRight);
        }

        if (!Hide)
        {
            LifeBar.Draw(animationPlayer);
            LevelInfoBar.Draw(animationPlayer);
            Lums1000Bar.Draw(animationPlayer);
            Cages50Bar.Draw(animationPlayer);
            BlueLumBar?.Draw(animationPlayer);
            WorldNameBar?.Draw(animationPlayer);
        }
    }
}