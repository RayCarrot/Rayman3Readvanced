using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BinarySerializer;
using BinarySerializer.Nintendo.GCN;
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

    #region Properties

    private const string MapInfosFileName = "gba.nfo";

    public AnimationPlayer AnimationPlayer { get; set; }
    public GameCubeMenuAnimations Anims { get; set; }
    public FiniteStateMachine State { get; } = new();

    public GameCubeMenuTransitionInScreenEffect TransitionInScreenEffect { get; set; }
    public GameCubeMenuTransitionOutScreenEffect TransitionOutScreenEffect { get; set; }

    public bool UseJoyBus { get; set; } // Custom

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
    public uint Timer { get; set; }
    public int MapInfoFileSize { get; set; }

    // Downloaded
    public GameCubeMapInfos MapInfos { get; set; }
    public GameCubeMap Map { get; set; }

    #endregion

    #region Private Methods

    private bool ExtractGameCubeFiles(string isoFilePath)
    {
        Engine.BeginLoad();

        // Start by verifying the file
        string gameId;
        using (FileStream stream = File.OpenRead(isoFilePath))
        using (Reader reader = new(stream))
            gameId = reader.ReadString(6, Encoding.ASCII);

        // Rayman 3 EU and US IDs
        if (gameId is not ("GRHE41" or "GRHP41"))
            return false;

        string isoDirPath = Path.GetDirectoryName(isoFilePath) ?? String.Empty;
        string isoFileName = Path.GetFileName(isoFilePath);

        // Create serializer settings for the GameCube disc
        SerializerSettings serializerSettings = new()
        {
            DefaultStringEncoding = Encoding.GetEncoding("shift_jis"),
            DefaultEndianness = Endian.Big,
            IgnoreCacheOnRead = true,
        };

        // Create a new context for reading the disc.
        using Context context = new(isoDirPath,
            settings: serializerSettings,
            systemLogger: new BinarySerializerSystemLogger());

        context.AddFile(new LinearFile(context, isoFileName));
        GCM gcm = FileFactory.Read<GCM>(context, isoFileName);

        // List the files to extract. Since the names are unique we don't have to care about the full directory path.
        string[] fileNames =
        [
            "gba.nfo",
            "map.000",
            "map.001",
            "map.002",
            "map.003",
            "map.004",
            "map.005",
            "map.006",
            "map.007",
            "map.008",
            "map.009",
        ];

        // Find the matching file entries.
        List<GCMFileEntry> fileEntries = new();
        foreach (string fileName in fileNames)
        {
            GCMFileEntry fileEntry = gcm.FileEntries.FirstOrDefault(x => x.Name == fileName);

            if (fileEntry == null)
                return false;
            
            fileEntries.Add(fileEntry);
        }

        // Extract the files.
        foreach (GCMFileEntry fileEntry in fileEntries)
        {
            // Read file data
            byte[] fileData = fileEntry.ReadFile();

            // Extract
            File.WriteAllBytes(Path.Combine(Rom.GameDirectory, fileEntry.Name), fileData);
        }

        return true;
    }

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
        if (!UseJoyBus)
            return;

        string[] text = Localization.GetText(TextBankId.Connectivity, 6);

        for (int i = 0; i < text.Length; i++)
        {
            SpriteTextObject textObj = i == 0 ? Anims.StatusText : Anims.ReusableTexts[i - 1];

            textObj.Color = TextColor.GameCubeMenu;
            textObj.Text = text[i];
            textObj.ScreenPos = new Vector2(140 - textObj.GetStringWidth() / 2f, 34 + i * 14);
        }
    }

    private void ShowCustomText(string text)
    {
        Anims.StatusText.Text = FontManager.WrapText(Anims.StatusText.FontSize, text, 120);
        Anims.StatusText.ScreenPos = new Vector2(80, 34);

        foreach (SpriteTextObject textObj in Anims.ReusableTexts)
            textObj.Text = "";
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

        if (Rom.Region == Region.Europe)
            JoyBus.SetRegion(0x41595a50, 0x47524850); // AYZP GRHP
        else if (Rom.Region == Region.Usa)
            JoyBus.SetRegion(0x41595a45, 0x47524845); // AYZE GRHE

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

        if (WaitingForConnection || 
            State == Fsm_DownloadMap || 
            State == Fsm_SelectMap || 
            State == Fsm_DownloadMapAck ||
            (State == Fsm_WaitForConnection && !UseJoyBus))
            AnimationPlayer.Play(Anims.StatusText);

        TransitionsFX.StepAll();
        AnimationPlayer.Execute();
    }

    #endregion
}