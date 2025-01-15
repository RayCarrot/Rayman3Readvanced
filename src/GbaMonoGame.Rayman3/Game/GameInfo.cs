using System;
using BinarySerializer.Ubisoft.GbaEngine;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;

namespace GbaMonoGame.Rayman3;

public static class GameInfo
{
    private const int LumsPerWorld = 230;

    public static MapId? NextMapId { get; set; }
    public static MapId MapId { get; set; }
    public static LevelType LevelType { get; set; }
    public static WorldId WorldId { get; set; }
    public static int LoadedYellowLums { get; set; }
    public static int LoadedCages { get; set; }
    public static int YellowLumsCount { get; set; }
    public static int CagesCount { get; set; }
    public static int GameCubeCollectedYellowLumsCount { get; set; } // Unused since GCN levels don't have lums
    public static int GameCubeCollectedCagesCount { get; set; } // Unused since GCN levels don't have cages
    public static int GreenLums { get; set; }
    public static int LastGreenLumAlive { get; set; }
    public static Vector2 CheckpointPosition { get; set; }
    public static int RemainingTime { get; set; }
    public static bool field7_0x7 { get; set; }
    public static bool IsInWorldMap { get; set; }
    public static bool HasCollectedWhiteLum { get; set; }
    public static ushort BlueLumTimer { get; set; }
    public static Power Powers { get; set; }
    public static Cheat Cheats { get; set; }
    public static ActorSoundFlags ActorSoundFlags { get; set; } // Defines if actor type has made sound this frame to avoid repeated sounds

    public static int CurrentSlot { get; set; }
    public static SaveGameSlot PersistentInfo { get; set; } = new()
    {
        Lums = new byte[125],
        Cages = new byte[7],
    };

    public static LevelInfo Level => Levels[(int)MapId];
    public static LevelInfo[] Levels => Rom.Loader.Rayman3_LevelInfo;

    public static void Init()
    {
        NextMapId = null;
        MapId = MapId.WoodLight_M1;
        LoadedYellowLums = 0;
        LoadedCages = 0;
        Powers = Power.None;
        Cheats = Cheat.None;
        HasCollectedWhiteLum = false;
        field7_0x7 = true;
        IsInWorldMap = false;
        ResetPersistentInfo();
    }

    public static void ResetPersistentInfo()
    {
        PersistentInfo.Lums ??= new byte[125];
        Array.Fill(PersistentInfo.Lums, (byte)0xFF);

        PersistentInfo.Cages ??= new byte[7];
        Array.Fill(PersistentInfo.Cages, (byte)0xFF);

        PersistentInfo.LastPlayedLevel = 0;
        PersistentInfo.LastCompletedLevel = 0;
        PersistentInfo.Lives = 3;
        PersistentInfo.FinishedLyChallenge1 = false;
        PersistentInfo.FinishedLyChallenge2 = false;
        PersistentInfo.FinishedLyChallengeGCN = false;
        PersistentInfo.UnlockedBonus1 = false;
        PersistentInfo.UnlockedBonus2 = false;
        PersistentInfo.UnlockedBonus3 = false;
        PersistentInfo.UnlockedBonus4 = false;
        PersistentInfo.UnlockedWorld2 = false;
        PersistentInfo.UnlockedWorld3 = false;
        PersistentInfo.UnlockedWorld4 = false;
        PersistentInfo.PlayedWorld2Unlock = false;
        PersistentInfo.PlayedWorld3Unlock = false;
        PersistentInfo.PlayedWorld4Unlock = false;
        PersistentInfo.PlayedAct4 = false;
        PersistentInfo.PlayedMurfyWorldHelp = false;
        PersistentInfo.UnlockedFinalBoss = false;
        PersistentInfo.UnlockedLyChallengeGCN = false;
        PersistentInfo.CompletedGCNBonusLevels = 0;
    }

    public static void Load(int saveSlot)
    {
        PersistentInfo = SaveGameManager.LoadSlot(saveSlot);
    }

    public static void Save(int saveSlot)
    {
        SaveGameManager.SaveSlot(saveSlot, PersistentInfo);
    }

    public static void EnableCheat(Scene2D scene, Cheat cheat)
    {
        Cheats |= cheat;

        switch (cheat)
        {
            case Cheat.Invulnerable:
                scene.MainActor.IsInvulnerable = true;
                break;

            case Cheat.AllPowers:
                Powers = Power.All;
                break;

            case Cheat.InfiniteLives:
                ModifyLives(99);
                break;
        }
    }

    public static void SetCheckpoint(Vector2 pos)
    {
        LastGreenLumAlive++;
        CheckpointPosition = pos;
    }

    public static void AddBlueLumTime()
    {
        if (BlueLumTimer < 79)
            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Stop__LumTimer_Mix02);

        BlueLumTimer += 304;
        if (BlueLumTimer > 416)
            BlueLumTimer = 416;
    }

    public static bool GetLumStatus(int lumId)
    {
        return (PersistentInfo.Lums[lumId >> 3] & (1 << (lumId & 7))) == 0;
    }

    public static bool GetCageStatus(int cageId)
    {
        return (PersistentInfo.Cages[cageId >> 3] & (1 << (cageId & 7))) == 0;
    }

    public static void KillLum(int lumId)
    {
        PersistentInfo.Lums[lumId >> 3] = (byte)(PersistentInfo.Lums[lumId >> 3] & ~(1 << (lumId & 7)));
    }

    public static void KillCage(int cageId)
    {
        PersistentInfo.Cages[cageId >> 3] = (byte)(PersistentInfo.Cages[cageId >> 3] & ~(1 << (cageId & 7)));
    }

    public static int GetCollectedYellowLumsInLevel(MapId mapId)
    {
        if (LevelType == LevelType.GameCube)
        {
            return GameCubeCollectedYellowLumsCount;
        }
        else
        {
            int count = 0;

            for (int i = 0; i < Levels[(int)mapId].LumsCount; i++)
            {
                if (HasCollectedYellowLum(i, mapId))
                    count++;
            }

            return count;
        }
    }

    public static int GetCollectedCagesInLevel(MapId mapId)
    {
        if (LevelType == LevelType.GameCube)
        {
            return GameCubeCollectedCagesCount;
        }
        else
        {
            int count = 0;

            for (int i = 0; i < Levels[(int)mapId].CagesCount; i++)
            {
                if (HasCollectedCage(i, mapId))
                    count++;
            }

            return count;
        }
    }

    public static int GetTotalCollectedYellowLums()
    {
        int count = 0;

        for (int i = 0; i < 1000; i++)
        {
            if (GetLumStatus(i))
                count++;
        }

        return count;
    }

    public static int GetTotalCollectedCages()
    {
        int count = 0;

        for (int i = 0; i < 50; i++)
        {
            if (GetCageStatus(i))
                count++;
        }

        return count;
    }

    public static bool HasCollectedAllYellowLums()
    {
        return GetTotalCollectedYellowLums() == 1000;
    }

    public static bool HasCollectedAllCages()
    {
        return GetTotalCollectedCages() == 50;
    }

    public static bool World1LumsCompleted()
    {
        int count = 0;
        for (MapId mapId = MapId.WoodLight_M1; mapId <= MapId.SanctuaryOfBigTree_M2; mapId++)
            count += GetCollectedYellowLumsInLevel(mapId);
        return count == LumsPerWorld;
    }

    public static bool World2LumsCompleted()
    {
        int count = 0;
        for (MapId mapId = MapId.MissileRace1; mapId <= MapId.MarshAwakening2; mapId++)
            count += GetCollectedYellowLumsInLevel(mapId);
        return count == LumsPerWorld;
    }

    public static bool World3LumsCompleted()
    {
        int count = 0;
        for (MapId mapId = MapId.SanctuaryOfStoneAndFire_M1; mapId <= MapId.SanctuaryOfRockAndLava_M3; mapId++)
            count += GetCollectedYellowLumsInLevel(mapId);
        return count == LumsPerWorld;
    }

    public static bool World4LumsCompleted()
    {
        int count = 0;
        for (MapId mapId = MapId.TombOfTheAncients_M1; mapId <= MapId.BossFinal_M2; mapId++)
            count += GetCollectedYellowLumsInLevel(mapId);
        return count == LumsPerWorld;
    }

    public static bool HasCollectedYellowLum(int lumId, MapId mapId)
    {
        return GetLumStatus(Levels[(int)mapId].GlobalLumsIndex + lumId);
    }

    public static bool HasCollectedCage(int cageId, MapId mapId)
    {
        return GetCageStatus(Levels[(int)mapId].GlobalCagesIndex + cageId);
    }

    public static void SetYellowLumAsCollected(int lumId)
    {
        if (LevelType == LevelType.GameCube)
        {
            GameCubeCollectedYellowLumsCount++;
            
            if (GameCubeCollectedYellowLumsCount == YellowLumsCount)
            {
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__LumTotal_Mix02);
                LevelMusicManager.OverrideLevelMusic(Rayman3SoundEvent.Play__win2);
            }
        }
        else
        {
            KillLum(Level.GlobalLumsIndex + lumId);

            // NOTE: Game also checks to MapId is not 0xFF, but that shouldn't be possible
            if (GetCollectedYellowLumsInLevel(MapId) == YellowLumsCount && LevelType != LevelType.Race)
            {
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__LumTotal_Mix02);
                LevelMusicManager.OverrideLevelMusic(Rayman3SoundEvent.Play__win2);
            }
        }
    }

    public static void SetCageAsCollected(int cageId)
    {
        if (LevelType == LevelType.GameCube)
        {
            GameCubeCollectedCagesCount++;
            if (GameCubeCollectedCagesCount == CagesCount)
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__LumTotal_Mix02);
        }
        else
        {
            KillCage(Level.GlobalCagesIndex + cageId);

            // NOTE: Game also checks to MapId is not 0xFF, but that shouldn't be possible
            if (GetCollectedCagesInLevel(MapId) == CagesCount)
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__LumTotal_Mix02);
        }
    }

    public static void SetAllYellowLumsAsCollected()
    {
        Array.Clear(PersistentInfo.Lums);
    }

    public static void SetAllCagesAsCollected()
    {
        Array.Clear(PersistentInfo.Cages);
    }

    public static void SetPowerBasedOnMap(MapId mapId)
    {
        if (mapId >= MapId.WoodLight_M2)
            Powers |= Power.DoubleFist;

        if (mapId >= MapId.BossMachine)
            Powers |= Power.Grab;

        if (mapId >= MapId.EchoingCaves_M2)
            Powers |= Power.WallJump;

        if (mapId >= MapId.SanctuaryOfStoneAndFire_M3)
            Powers |= Power.SuperHelico;

        if (mapId >= MapId.BossRockAndLava)
            Powers |= Power.BodyShot;

        if (mapId >= MapId.BossScaleMan)
            Powers |= Power.SuperFist;
    }

    public static MapId GetNextLevelId()
    {
        return (MapId)Level.NextLevelId;
    }

    public static void LoadLevel(MapId mapId)
    {
        if (mapId == MapId.MarshAwakening1 && PersistentInfo.LastCompletedLevel < (int)MapId.MarshAwakening1)
        {
            FrameManager.SetNextFrame(new Act2());
        }
        else if (mapId == MapId.PirateShip_M1 && PersistentInfo.LastCompletedLevel < (int)MapId.PirateShip_M1)
        {
            FrameManager.SetNextFrame(new Act5());
        }
        else
        {
            FrameManager.SetNextFrame(LevelFactory.Create(mapId));
        }
    }

    public static void LoadLastWorld()
    {
        switch ((MapId)PersistentInfo.LastPlayedLevel)
        {
            case MapId.WoodLight_M1:
            case MapId.WoodLight_M2:
            case MapId.FairyGlade_M1:
            case MapId.FairyGlade_M2:
            case MapId.MarshAwakening1:
            case MapId.BossMachine:
            case MapId.SanctuaryOfBigTree_M1:
            case MapId.SanctuaryOfBigTree_M2:
            case MapId.Bonus1:
                LoadLevel(MapId.World1);
                break;
            
            case MapId.MissileRace1:
            case MapId.EchoingCaves_M1:
            case MapId.EchoingCaves_M2:
            case MapId.CavesOfBadDreams_M1:
            case MapId.CavesOfBadDreams_M2:
            case MapId.BossBadDreams:
            case MapId.MenhirHills_M1:
            case MapId.MenhirHills_M2:
            case MapId.MarshAwakening2:
            case MapId.Bonus2:
            case MapId.ChallengeLy1:
                LoadLevel(MapId.World2);
                break;
            
            case MapId.SanctuaryOfStoneAndFire_M1:
            case MapId.SanctuaryOfStoneAndFire_M2:
            case MapId.SanctuaryOfStoneAndFire_M3:
            case MapId.BeneathTheSanctuary_M1:
            case MapId.BeneathTheSanctuary_M2:
            case MapId.ThePrecipice_M1:
            case MapId.ThePrecipice_M2:
            case MapId.BossRockAndLava:
            case MapId.TheCanopy_M1:
            case MapId.TheCanopy_M2:
            case MapId.SanctuaryOfRockAndLava_M1:
            case MapId.SanctuaryOfRockAndLava_M2:
            case MapId.SanctuaryOfRockAndLava_M3:
            case MapId.Bonus3:
                LoadLevel(MapId.World3);
                break;

            case MapId.TombOfTheAncients_M1:
            case MapId.TombOfTheAncients_M2:
            case MapId.BossScaleMan:
            case MapId.IronMountains_M1:
            case MapId.IronMountains_M2:
            case MapId.MissileRace2:
            case MapId.PirateShip_M1:
            case MapId.PirateShip_M2:
            case MapId.BossFinal_M1:
            case MapId.BossFinal_M2:
            case MapId.Bonus4:
            case MapId._1000Lums:
            case MapId.ChallengeLy2:
            case MapId.ChallengeLyGCN:
                LoadLevel(MapId.World4);
                break;

            case MapId.Power1:
            case MapId.Power2:
            case MapId.Power3:
            case MapId.Power4:
            case MapId.Power5:
            case MapId.Power6:
            case MapId.World1:
            case MapId.World2:
            case MapId.World3:
            case MapId.World4:
            case MapId.WorldMap:
            default:
                throw new Exception("Invalid level to load world from");
        }
    }

    public static void SetNextMapId(MapId mapId)
    {
        LastGreenLumAlive = 0;
        NextMapId = mapId;
        GreenLums = 0;
        HasCollectedWhiteLum = false;
        SetPowerBasedOnMap((MapId)PersistentInfo.LastCompletedLevel);

        switch (mapId)
        {
            case MapId.WoodLight_M1:
            case MapId.WoodLight_M2:
            case MapId.FairyGlade_M1:
            case MapId.FairyGlade_M2:
            case MapId.MarshAwakening1:
            case MapId.BossMachine:
            case MapId.SanctuaryOfBigTree_M1:
            case MapId.SanctuaryOfBigTree_M2:
            case MapId.Bonus1:
            case MapId.World1:
                WorldId = WorldId.World1;
                break;

            case MapId.MissileRace1:
            case MapId.EchoingCaves_M1:
            case MapId.EchoingCaves_M2:
            case MapId.CavesOfBadDreams_M1:
            case MapId.CavesOfBadDreams_M2:
            case MapId.BossBadDreams:
            case MapId.MenhirHills_M1:
            case MapId.MenhirHills_M2:
            case MapId.MarshAwakening2:
            case MapId.Bonus2:
            case MapId.ChallengeLy1:
            case MapId.World2:
                WorldId = WorldId.World2;
                break;

            case MapId.SanctuaryOfStoneAndFire_M1:
            case MapId.SanctuaryOfStoneAndFire_M2:
            case MapId.SanctuaryOfStoneAndFire_M3:
            case MapId.BeneathTheSanctuary_M1:
            case MapId.BeneathTheSanctuary_M2:
            case MapId.ThePrecipice_M1:
            case MapId.ThePrecipice_M2:
            case MapId.BossRockAndLava:
            case MapId.TheCanopy_M1:
            case MapId.TheCanopy_M2:
            case MapId.SanctuaryOfRockAndLava_M1:
            case MapId.SanctuaryOfRockAndLava_M2:
            case MapId.SanctuaryOfRockAndLava_M3:
            case MapId.Bonus3:
            case MapId.World3:
                WorldId = WorldId.World3;
                break;

            case MapId.TombOfTheAncients_M1:
            case MapId.TombOfTheAncients_M2:
            case MapId.BossScaleMan:
            case MapId.IronMountains_M1:
            case MapId.IronMountains_M2:
            case MapId.MissileRace2:
            case MapId.PirateShip_M1:
            case MapId.PirateShip_M2:
            case MapId.BossFinal_M1:
            case MapId.BossFinal_M2:
            case MapId.Bonus4:
            case MapId._1000Lums:
            case MapId.ChallengeLy2:
            case MapId.ChallengeLyGCN:
            case MapId.World4:
                WorldId = WorldId.World4;
                break;

            case MapId.Power1:
            case MapId.Power2:
            case MapId.Power3:
            case MapId.Power4:
            case MapId.Power5:
            case MapId.Power6:
                WorldId = WorldId.Power;
                break;

            case MapId.GbaMulti_MissileRace:
            case MapId.GbaMulti_MissileArena:
            case MapId.GbaMulti_TagWeb:
            case MapId.GbaMulti_TagSlide:
            case MapId.GbaMulti_CatAndMouseSlide:
            case MapId.GbaMulti_CatAndMouseSpider:
            case MapId.NGageMulti_TagWeb:
            case MapId.NGageMulti_TagSlide:
            case MapId.NGageMulti_CatAndMouseSlide:
            case MapId.NGageMulti_CatAndMouseSpider:
                WorldId = WorldId.Special;
                break;

            case MapId.WorldMap:
            case MapId.GameCube_Bonus1:
            case MapId.GameCube_Bonus2:
            case MapId.GameCube_Bonus3:
            case MapId.GameCube_Bonus4:
            case MapId.GameCube_Bonus5:
            case MapId.GameCube_Bonus6:
            case MapId.GameCube_Bonus7:
            case MapId.GameCube_Bonus8:
            case MapId.GameCube_Bonus9:
            case MapId.GameCube_Bonus10:
            default:
                // Do nothing
                break;
        }
    }

    public static void InitLevel(LevelType type)
    {
        LoadedYellowLums = 0;
        LoadedCages = 0;
        GreenLums = 0;
        MapId = NextMapId ?? throw new Exception("No map id set");
        YellowLumsCount = Level.LumsCount;
        CagesCount = Level.CagesCount;
        GameCubeCollectedYellowLumsCount = 0;
        GameCubeCollectedCagesCount = 0;
        LevelType = type;
    }

    public static void UpdateLastCompletedLevel()
    {
        if (MapId < MapId.Bonus1 && MapId > (MapId)PersistentInfo.LastCompletedLevel)
            PersistentInfo.LastCompletedLevel = (byte)MapId;
    }

    public static void ModifyLives(int change)
    {
        if ((Cheats & Cheat.InfiniteLives) != 0)
        {
            PersistentInfo.Lives = 99;
            return;
        }

        int newCount = PersistentInfo.Lives + change;

        if (newCount < 0)
            PersistentInfo.Lives = 0;
        else if (newCount < 100)
            PersistentInfo.Lives = (byte)newCount;
    }

    public static void PlayLevelMusic()
    {
        SoundEventsManager.ProcessEvent(Level.StartMusicSoundEvent);
    }

    public static void StopLevelMusic()
    {
        if (LevelType != LevelType.GameCube)
            SoundEventsManager.ProcessEvent(Level.StopMusicSoundEvent);
    }

    public static Rayman3SoundEvent GetLevelMusicSoundEvent()
    {
        if (LevelType == LevelType.GameCube)
            return ((FrameSideScrollerGCN)Frame.Current).MapInfo.StartMusicSoundEvent;
        else
            return Level.StartMusicSoundEvent;
    }

    public static Rayman3SoundEvent GetSpecialLevelMusicSoundEvent()
    {
        if (LevelType == LevelType.GameCube)
            return ((FrameSideScrollerGCN)Frame.Current).MapInfo.StartSpecialMusicSoundEvent;
        else
            return Level.StartSpecialMusicSoundEvent;
    }
}