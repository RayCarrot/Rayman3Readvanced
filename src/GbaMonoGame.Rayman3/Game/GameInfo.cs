using System;
using System.Diagnostics;
using BinarySerializer.Ubisoft.GbaEngine;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;

namespace GbaMonoGame.Rayman3;

public static class GameInfo
{
    public const int LumsPerWorld = 230;
    public const int OriginalSaveSlotsCount = 3;
    public const int ModernSaveSlotsCount = 5;

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
    public static int LoadedGreenLums { get; set; }
    public static int LastGreenLumAlive { get; set; }
    public static Vector2 CheckpointPosition { get; set; }
    public static int RemainingTime { get; set; }
    public static bool CanShowMurfyHelp { get; set; }
    public static bool IsInWorldMap { get; set; }
    public static bool HasCollectedWhiteLum { get; set; }
    public static ushort BlueLumsTimer { get; set; }
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
        CanShowMurfyHelp = true;
        IsInWorldMap = false;
        ResetPersistentInfo();
    }

    public static void UnInit()
    {
        PersistentInfo = new SaveGameSlot()
        {
            Lums = new byte[125],
            Cages = new byte[7],
        };
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

        if (Rom.Platform == Platform.GBA)
            Engine.LocalConfig.General.LastPlayedGbaSaveSlot = CurrentSlot;
        else if (Rom.Platform == Platform.NGage)
            Engine.LocalConfig.General.LastPlayedNGageSaveSlot = CurrentSlot;
        else
            throw new UnsupportedPlatformException();
    }

    public static void EnablePower(Power power)
    {
        Powers |= power;
    }

    public static void DisablePower(Power power)
    {
        Powers &= ~power;
    }

    public static bool IsPowerEnabled(Power power)
    {
        return (Powers & power) != 0;
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
                EnablePower(Power.All);
                break;

            case Cheat.InfiniteLives:
                ModifyLives(99);
                break;
        }
    }

    public static bool IsCheatEnabled(Cheat cheat)
    {
        return (Cheats & cheat) != 0;
    }

    public static int GetGreenLumsId()
    {
        int id = LoadedGreenLums;
        LoadedGreenLums++;
        return id;
    }

    public static bool IsGreenLumDead(int lumId)
    {
        return lumId < LastGreenLumAlive;
    }

    public static void GreenLumTouchedByRayman(int id, Vector2 pos)
    {
        Debug.Assert(id == LastGreenLumAlive, "Invalid Greens lums id. The lums ids have to be ordered.");

        LastGreenLumAlive++;
        CheckpointPosition = pos;
    }

    public static bool GetLevelHasBlueLum()
    {
        return Level.HasBlueLum;
    }

    public static bool IsBlueLumsNearEnd()
    {
        return BlueLumsTimer < 79;
    }

    public static void ResetBlueLumsTime()
    {
        BlueLumsTimer = 0;
    }

    public static bool IsBlueLumsTimeOver()
    {
        return BlueLumsTimer == 0;
    }

    public static void IncBlueLumsTime()
    {
        if (IsBlueLumsNearEnd())
            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Stop__LumTimer_Mix02);

        BlueLumsTimer += 304;
        if (BlueLumsTimer > 416)
            BlueLumsTimer = 416;
    }

    public static bool GetLumStatus(int lumId)
    {
        return (PersistentInfo.Lums[lumId >> 3] & (1 << (lumId & 7))) == 0;
    }

    public static bool GetCageStatus(int cageId)
    {
        return (PersistentInfo.Cages[cageId >> 3] & (1 << (cageId & 7))) == 0;
    }

    public static void SetLumStatus(int lumId, bool isDead)
    {
        if (isDead)
            PersistentInfo.Lums[lumId >> 3] = (byte)(PersistentInfo.Lums[lumId >> 3] & ~(1 << (lumId & 7)));
        else
            PersistentInfo.Lums[lumId >> 3] = (byte)(PersistentInfo.Lums[lumId >> 3] | (1 << (lumId & 7)));
    }

    public static void SetCageStatus(int cageId, bool isDead)
    {
        if (isDead)
            PersistentInfo.Cages[cageId >> 3] = (byte)(PersistentInfo.Cages[cageId >> 3] & ~(1 << (cageId & 7)));
        else
            PersistentInfo.Cages[cageId >> 3] = (byte)(PersistentInfo.Cages[cageId >> 3] | (1 << (cageId & 7)));
    }

    public static int GetLumsCountForCurrentMap()
    {
        if (LevelType != LevelType.GameCube)
            return Level.LumsCount;
        else
            return YellowLumsCount;
    }

    public static int GetCagesCountForCurrentMap()
    {
        if (LevelType != LevelType.GameCube)
            return Level.CagesCount;
        else
            return CagesCount;
    }

    public static int GetDeadLumsForCurrentMap(MapId mapId)
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
                if (IsLumDead(i, mapId))
                    count++;
            }

            return count;
        }
    }

    public static int GetDeadCagesForCurrentMap(MapId mapId)
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
                if (IsCageDead(i, mapId))
                    count++;
            }

            return count;
        }
    }

    public static int GetLumsId()
    {
        if (LevelType != LevelType.GameCube && LoadedYellowLums >= YellowLumsCount)
            throw new Exception("Too many Yellow lums registered");

        int id = LoadedYellowLums;
        LoadedYellowLums++;
        return id;
    }

    public static int GetCageId()
    {
        int id = LoadedCages;
        LoadedCages++;
        return id;
    }

    public static int GetTotalDeadLums()
    {
        int count = 0;

        for (int i = 0; i < 1000; i++)
        {
            if (GetLumStatus(i))
                count++;
        }

        return count;
    }

    public static int GetTotalDeadCages()
    {
        int count = 0;

        for (int i = 0; i < 50; i++)
        {
            if (GetCageStatus(i))
                count++;
        }

        return count;
    }

    public static bool AreAllLumsDead()
    {
        return GetTotalDeadLums() == 1000;
    }

    public static bool AreAllCagesDead()
    {
        return GetTotalDeadCages() == 50;
    }

    public static bool World1LumsCompleted()
    {
        int count = 0;
        for (MapId mapId = MapId.WoodLight_M1; mapId <= MapId.SanctuaryOfBigTree_M2; mapId++)
            count += GetDeadLumsForCurrentMap(mapId);

        Debug.Assert(count <= LumsPerWorld, "Abnormal dead lums count in world #1");

        return count == LumsPerWorld;
    }

    public static bool World2LumsCompleted()
    {
        int count = 0;
        for (MapId mapId = MapId.MissileRace1; mapId <= MapId.MarshAwakening2; mapId++)
            count += GetDeadLumsForCurrentMap(mapId);

        Debug.Assert(count <= LumsPerWorld, "Abnormal dead lums count in world #2");

        return count == LumsPerWorld;
    }

    public static bool World3LumsCompleted()
    {
        int count = 0;
        for (MapId mapId = MapId.SanctuaryOfStoneAndFire_M1; mapId <= MapId.SanctuaryOfRockAndLava_M3; mapId++)
            count += GetDeadLumsForCurrentMap(mapId);

        Debug.Assert(count <= LumsPerWorld, "Abnormal dead lums count in world #3");

        return count == LumsPerWorld;
    }

    public static bool World4LumsCompleted()
    {
        int count = 0;
        for (MapId mapId = MapId.TombOfTheAncients_M1; mapId <= MapId.BossFinal_M2; mapId++)
            count += GetDeadLumsForCurrentMap(mapId);

        Debug.Assert(count <= LumsPerWorld, "Abnormal dead lums count in world #4");

        return count == LumsPerWorld;
    }

    public static bool IsLumDead(int lumId, MapId mapId)
    {
        return GetLumStatus(Levels[(int)mapId].GlobalLumsIndex + lumId);
    }

    public static bool IsCageDead(int cageId, MapId mapId)
    {
        return GetCageStatus(Levels[(int)mapId].GlobalCagesIndex + cageId);
    }

    public static bool HasCollectedAllLumsInLevel()
    {
        return GetDeadLumsForCurrentMap(MapId) == YellowLumsCount;
    }

    public static bool HasCollectedAllCagesInLevel()
    {
        return GetDeadCagesForCurrentMap(MapId) == CagesCount;
    }

    public static void KillLum(int lumId)
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
            SetLumStatus(Level.GlobalLumsIndex + lumId, true);

            // NOTE: Game also checks to MapId is not 0xFF, but that shouldn't be possible
            if (HasCollectedAllLumsInLevel() && LevelType != LevelType.Race)
            {
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__LumTotal_Mix02);
                LevelMusicManager.OverrideLevelMusic(Rayman3SoundEvent.Play__win2);
            }
        }
    }

    public static void KillCage(int cageId)
    {
        if (LevelType == LevelType.GameCube)
        {
            GameCubeCollectedCagesCount++;
            if (GameCubeCollectedCagesCount == CagesCount)
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__LumTotal_Mix02);
        }
        else
        {
            SetCageStatus(Level.GlobalCagesIndex + cageId, true);

            // NOTE: Game also checks to MapId is not 0xFF, but that shouldn't be possible
            if (HasCollectedAllCagesInLevel())
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__LumTotal_Mix02);
        }
    }

    public static void KillAllLums()
    {
        Array.Clear(PersistentInfo.Lums);
    }

    public static void KillAllCages()
    {
        Array.Clear(PersistentInfo.Cages);
    }

    public static void SetPowerBasedOnMap(MapId mapId)
    {
        if (mapId >= MapId.WoodLight_M2)
            EnablePower(Power.DoubleFist);

        if (mapId >= MapId.BossMachine)
            EnablePower(Power.Grab);

        if (mapId >= MapId.EchoingCaves_M2)
            EnablePower(Power.WallJump);

        if (mapId >= MapId.SanctuaryOfStoneAndFire_M3)
            EnablePower(Power.SuperHelico);

        if (mapId >= MapId.BossRockAndLava)
            EnablePower(Power.BodyShot);

        if (mapId >= MapId.BossScaleMan)
            EnablePower(Power.SuperFist);
    }

    public static MapId GetNextLevelId()
    {
        return (MapId)Level.NextLevelId;
    }

    public static byte GetLevelCurtainActorId()
    {
        if (PersistentInfo.LastPlayedLevel == 0xFF)
            return 0;
        else
            return Levels[PersistentInfo.LastPlayedLevel].LevelCurtainActorId;
    }

    public static void LoadLevel(MapId mapId)
    {
        if (mapId > MapId.WorldMap)
            throw new Exception("Invalid map");

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

    public static void GotoLastSaveGame()
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
                throw new Exception("Invalid last map id");
        }
    }

    public static void SetNextMapId(MapId mapId)
    {
        LastGreenLumAlive = 0;
        NextMapId = mapId;
        LoadedGreenLums = 0;
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
        LoadedGreenLums = 0;
        MapId = NextMapId ?? throw new Exception("No map id set");
        YellowLumsCount = Level.LumsCount;
        CagesCount = Level.CagesCount;
        GameCubeCollectedYellowLumsCount = 0;
        GameCubeCollectedCagesCount = 0;
        LevelType = type;
    }

    public static bool IsFirstTimeCompletingLevel()
    {
        return PersistentInfo.LastCompletedLevel < (int)MapId;
    }

    public static void UpdateLastCompletedLevel()
    {
        if (MapId < MapId.Bonus1 && PersistentInfo.LastCompletedLevel < (int)MapId)
            PersistentInfo.LastCompletedLevel = (byte)MapId;
    }

    public static void ModifyLives(int change)
    {
        if (IsCheatEnabled(Cheat.InfiniteLives))
        {
            PersistentInfo.Lives = 99;
            return;
        }

        // Don't allow losing lives if the infinite lives option is enabled
        if (Engine.ActiveConfig.Difficulty.InfiniteLives && change < 0)
            return;

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