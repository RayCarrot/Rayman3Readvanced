using System;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.AnimEngine;
using GbaMonoGame.TgxEngine;
using Microsoft.Xna.Framework;

namespace GbaMonoGame.Rayman3;

public partial class GameCubeMenu : Frame
{
    #region Constructor

    public GameCubeMenu()
    {
        SoundEventsManager.StopAllSongs();

        // Use filesystem for now. In the future we can allow JoyBus mode and perhaps connect to
        // Dolphin through TCP (see https://github.dev/mgba-emu/mgba/tree/master/src/gba/sio).
        UseJoyBus = false;
    }

    #endregion

    #region Private Properties

    public AnimationPlayer AnimationPlayer { get; set; }
    public TransitionsFX TransitionsFX { get; set; }
    public GameCubeMenuAnimations Anims { get; set; }
    public FiniteStateMachine State { get; } = new();

    public GameCubeMenuTransitionInScreenEffect TransitionInScreenEffect { get; set; }
    public GameCubeMenuTransitionOutScreenEffect TransitionOutScreenEffect { get; set; }

    public bool UseJoyBus { get; set; }
    public JoyBus JoyBus { get; set; }
    public bool IsJoyBusActive { get; set; }
    public bool WaitingForConnection { get; set; }
    public int MapScroll { get; set; }
    public int SelectedMap { get; set; }
    public byte GbaUnlockFlags { get; set; }
    public byte GcnUnlockFlags { get; set; }
    public bool IsShowingLyChallengeUnlocked { get; set; }
    public bool IsActive { get; set; }
    public int WheelRotation { get; set; }
    public int Timer { get; set; }
    public int MapInfoFileSize { get; set; }

    // Downloaded
    public GameCubeMapInfos MapInfos { get; set; }
    public GameCubeMap Map { get; set; }

    #endregion

    #region Private Methods

    private bool IsMapUnlocked(int mapId)
    {
        int lums = GameInfo.GetTotalDeadLums();
        return lums >= (mapId + 1) * 100 &&
               GameInfo.PersistentInfo.CompletedGCNBonusLevels >= mapId;
    }

    private bool IsMapCompleted(int mapId)
    {
        return GameInfo.PersistentInfo.CompletedGCNBonusLevels > mapId;
    }

    private void ShowPleaseConnectText()
    {
        string[] text = Localization.GetText(TextBankId.Connectivity, 6);

        for (int i = 0; i < text.Length; i++)
        {
            SpriteTextObject textObj = i == 0 ? Anims.StatusText : Anims.ReusableTexts[i - 1];

            textObj.Color = TextColor.GameCubeMenu;
            textObj.Text = text[i];
            textObj.ScreenPos = new Vector2(140 - textObj.GetStringWidth() / 2f, 34 + i * 14);
        }
    }

    private void MapSelectionUpdateText()
    {
        if (MapInfos.MapsCount < 3)
            throw new Exception("Need at least 3 maps");

        // Set text colors
        int selectedIndex = SelectedMap - MapScroll;
        for (int i = 0; i < 3; i++)
            Anims.ReusableTexts[i].Color = i == selectedIndex ? TextColor.GameCubeMenu : TextColor.GameCubeMenuFaded;

        // Update animations and texts
        for (int i = 0; i < 3; i++)
        {
            MapSelectionUpdateAnimations(MapScroll + i, i);
            Anims.LumRequirementTexts[i].Text = ((MapScroll + i + 1) * 100).ToString();
            Anims.ReusableTexts[i].Text = MapInfos.Maps[MapScroll + i].Name;
        }
    }

    private void MapSelectionUpdateAnimations(int mapId, int index)
    {
        if (!IsMapUnlocked(mapId))
            Anims.LevelChecks[index].CurrentAnimation = 2;
        else if (!IsMapCompleted(mapId))
            Anims.LevelChecks[index].CurrentAnimation = 0;
        else
            Anims.LevelChecks[index].CurrentAnimation = 1;
    }

    private void ResetReusableTexts()
    {
        for (int i = 0; i < 3; i++)
            Anims.ReusableTexts[i].ScreenPos = new Vector2(85, 36 + i * 24);
    }

    #endregion

    #region Public Methods

    public override void Init()
    {
        AnimationPlayer = new AnimationPlayer(false, null);

        Gfx.AddScreen(new GfxScreen(2)
        {
            IsEnabled = true,
            Priority = 1,
            Offset = Vector2.Zero,
            Renderer = new TextureScreenRenderer(Engine.TextureCache.GetOrCreateObject(
                pointer: Rom.Loader.Rayman3_GameCubeMenuBitmap.Offset,
                id: 0,
                createObjFunc: static () => new BitmapTexture2D(
                    width: (int)Rom.OriginalResolution.X,
                    height: (int)Rom.OriginalResolution.Y,
                    bitmap: Rom.Loader.Rayman3_GameCubeMenuBitmap.ImgData,
                    palette: new Palette(Rom.Loader.Rayman3_GameCubeMenuPalette)))),
            RenderOptions = { RenderContext = Rom.OriginalGameRenderContext },
        });

        Anims = new GameCubeMenuAnimations(Rom.OriginalGameRenderContext);
        
        JoyBus = new JoyBus();
        JoyBus.Connect();
        // TODO: If we use this we should allow both PAL and NTSC regions. Currently this is for the PAL region (AYZP & GRHP).
        JoyBus.SetRegion(0x41595a50, 0x47524850);
        IsJoyBusActive = true;
        
        MapScroll = 0;
        SelectedMap = 0;

        GbaUnlockFlags = 0;
        GcnUnlockFlags = 0;
        IsShowingLyChallengeUnlocked = false;

        if (GameInfo.AreAllLumsDead())
            GcnUnlockFlags |= 1;

        if (GameInfo.AreAllCagesDead())
            GcnUnlockFlags |= 2;

        if (GameInfo.PersistentInfo.LastCompletedLevel >= (int)MapId.BossFinal_M2)
            GcnUnlockFlags |= 4;

        if (GameInfo.PersistentInfo.FinishedLyChallenge1 &&
            GameInfo.PersistentInfo.FinishedLyChallenge2 &&
            GameInfo.PersistentInfo.UnlockedBonus1 &&
            GameInfo.PersistentInfo.UnlockedBonus2 &&
            GameInfo.PersistentInfo.UnlockedBonus3 &&
            GameInfo.PersistentInfo.UnlockedBonus4 &&
            GameInfo.PersistentInfo.UnlockedWorld2 &&
            GameInfo.PersistentInfo.UnlockedWorld3 &&
            GameInfo.PersistentInfo.UnlockedWorld4 &&
            GameInfo.PersistentInfo.LastCompletedLevel >= (int)MapId.BossFinal_M2)
        {
            GcnUnlockFlags |= 8;
        }

        WheelRotation = 0;
        Gfx.ClearColor = Color.Black;

        TransitionInScreenEffect = new GameCubeMenuTransitionInScreenEffect()
        {
            RenderOptions = { RenderContext = Rom.OriginalGameRenderContext },
        };
        Gfx.SetScreenEffect(TransitionInScreenEffect);

        WaitingForConnection = false;
        IsActive = true;
        State.MoveTo(Fsm_PreInit);
    }

    public override void UnInit()
    {
        if (IsJoyBusActive)
            JoyBus.Disconnect();

        Gfx.FadeControl = new FadeControl(FadeMode.BrightnessDecrease);
        Gfx.Fade = 1;
    }

    public override void Step()
    {
        State.Step();

        WheelRotation += 4;

        if (WheelRotation >= 2048)
            WheelRotation = 0;

        Anims.Wheel1.AffineMatrix = new AffineMatrix(WheelRotation % 256, 1, 1);
        Anims.Wheel2.AffineMatrix = new AffineMatrix(255 - WheelRotation / 2f % 256, 1, 1);
        Anims.Wheel3.AffineMatrix = new AffineMatrix(WheelRotation / 4f % 256, 1, 1);
        Anims.Wheel4.AffineMatrix = new AffineMatrix(WheelRotation / 8f % 256, 1, 1);

        AnimationPlayer.Play(Anims.Wheel1);
        AnimationPlayer.Play(Anims.Wheel2);
        AnimationPlayer.Play(Anims.Wheel3);
        AnimationPlayer.Play(Anims.Wheel4);

        if (WaitingForConnection)
        {
            foreach (SpriteTextObject text in Anims.ReusableTexts)
                AnimationPlayer.Play(text);
        }
        else if (State == Fsm_DownloadMap)
        {
            AnimationPlayer.Play(Anims.ReusableTexts[0]);
            AnimationPlayer.Play(Anims.ReusableTexts[1]);
        }
        else if (State == Fsm_SelectMap)
        {
            if (IsShowingLyChallengeUnlocked)
            {
                AnimationPlayer.Play(Anims.ReusableTexts[0]);
            }
            else
            {
                for (int i = 0; i < 3; i++)
                {
                    AnimationPlayer.Play(Anims.ReusableTexts[i]);
                    AnimationPlayer.Play(Anims.LumRequirementTexts[i]);
                    AnimationPlayer.Play(Anims.LumIcons[i]);
                    AnimationPlayer.Play(Anims.LevelChecks[i]);
                }
            }
        }

        AnimationPlayer.Play(Anims.TotalLumsText);

        if (WaitingForConnection || State == Fsm_DownloadMap || State == Fsm_SelectMap || State == Fsm_DownloadMapAck)
            AnimationPlayer.Play(Anims.StatusText);

        TransitionsFX.StepAll();
        AnimationPlayer.Execute();
    }

    #endregion
}