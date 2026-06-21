using System;
using System.Collections.Generic;
using System.Diagnostics;
using BinarySerializer.Ubisoft.GbaEngine;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.Rayman3.Readvanced;

namespace GbaMonoGame.Rayman3;

public class GameInfo
{
    public GameInfo(LevelInfo[] levels)
    {
        LevelMaps =
        [
            [MapId.WoodLight_M1, MapId.WoodLight_M2],
            [MapId.FairyGlade_M1, MapId.FairyGlade_M2],
            [MapId.MarshAwakening1],
            [MapId.SanctuaryOfBigTree_M1, MapId.SanctuaryOfBigTree_M2],
            [MapId.BossMachine],
            [MapId.Bonus1],

            [MapId.MissileRace1],
            [MapId.EchoingCaves_M1, MapId.EchoingCaves_M2],
            [MapId.CavesOfBadDreams_M1, MapId.CavesOfBadDreams_M2],
            [MapId.MenhirHills_M1, MapId.MenhirHills_M2],
            [MapId.MarshAwakening2],
            [MapId.BossBadDreams],
            [MapId.Bonus2],
            [MapId.ChallengeLy1],

            [MapId.SanctuaryOfRockAndLava_M1, MapId.SanctuaryOfRockAndLava_M2, MapId.SanctuaryOfRockAndLava_M3],
            [MapId.BeneathTheSanctuary_M1, MapId.BeneathTheSanctuary_M2],
            [MapId.ThePrecipice_M1, MapId.ThePrecipice_M2],
            [MapId.TheCanopy_M1, MapId.TheCanopy_M2],
            [MapId.SanctuaryOfStoneAndFire_M1, MapId.SanctuaryOfStoneAndFire_M2, MapId.SanctuaryOfStoneAndFire_M3],
            [MapId.BossRockAndLava],
            [MapId.Bonus3],

            [MapId.TombOfTheAncients_M1, MapId.TombOfTheAncients_M2],
            [MapId.IronMountains_M1, MapId.IronMountains_M2],
            [MapId.MissileRace2],
            [MapId.PirateShip_M1, MapId.PirateShip_M2],
            [MapId.BossScaleMan],
            [MapId.BossFinal_M1],
            [MapId.Bonus4],
            [MapId.ChallengeLy2],
            [MapId._1000Lums],
            [MapId.ChallengeLyGCN]
        ];

        CollectedWhiteLums = new List<int>(2);
        SaveSlot = new ReadvancedSlot
        {
            SaveGame = new SaveGameSlot
            {
                Lums = new byte[125],
                Cages = new byte[7],
            },
        };
        PlayTimer = new Stopwatch();
        Levels = levels;
    }

    public const int LumsPerWorld = 230;
    public const int OriginalSaveSlotsCount = 3;
    public const int ModernSaveSlotsCount = 5;

    // NOTE: In the original game this is only in LevelInfoBar, but we want to use it elsewhere too
    public MapId[][] LevelMaps { get; }
    
    public MapId? NextMapId { get; set; }
    public MapId MapId { get; set; }
    public LevelType LevelType { get; set; }
    public WorldId WorldId { get; set; }
    public int LoadedYellowLums { get; set; }
    public int LoadedCages { get; set; }
    public int YellowLumsCount { get; set; }
    public int CagesCount { get; set; }
    public int GameCubeCollectedYellowLumsCount { get; set; } // Unused since GCN levels don't have lums
    public int GameCubeCollectedCagesCount { get; set; } // Unused since GCN levels don't have cages
    public int LoadedGreenLums { get; set; }
    public int LastGreenLumAlive { get; set; }
    public Vector2 CheckpointPosition { get; set; }
    public int RemainingTime { get; set; }
    public bool CanShowMurfyHelp { get; set; }
    public bool IsInWorldMap { get; set; }
    public bool HasCollectedWhiteLum { get; set; }
    public List<int> CollectedWhiteLums { get; } // Custom to allow multiple white lums in a level, such as in The Precipice 2
    public ushort BlueLumsTimer { get; set; }
    public Power Powers { get; set; }
    public Cheat Cheats { get; set; }
    public ActorSoundFlags ActorSoundFlags { get; set; } // Defines if actor type has made sound this frame to avoid repeated sounds

    public int CurrentSlot { get; set; }
    public ReadvancedSlot SaveSlot { get; set; }
    public SaveGameSlot PersistentInfo => SaveSlot.SaveGame;
    public Stopwatch PlayTimer { get; } // Custom for keeping track of slot play time

    public LevelInfo Level => Levels[(int)MapId];
    public LevelInfo[] Levels { get; }

    public void Init()
    {
        NextMapId = null;
        MapId = MapId.WoodLight_M1;
        LoadedYellowLums = 0;
        LoadedCages = 0;
        Powers = Power.None;
        Cheats = Cheat.None;
        HasCollectedWhiteLum = false;
        CollectedWhiteLums.Clear();
        CanShowMurfyHelp = true;
        IsInWorldMap = false;
        ResetPersistentInfo();
    }

    public void ResetPersistentInfo()
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

        SaveSlot.PlayTime = 0;
        SaveSlot.DefeatedPirateTypes = PirateType.None;
        SaveSlot.CollectedWhiteLums = [];
    }

    public void Load(int saveSlot)
    {
        ReadvancedSlot save = Rayman3.Save.LoadSlot(saveSlot);
        if (save != null)
            SaveSlot = save;
        else
            ResetPersistentInfo();
    }

    public void Load(ReadvancedSlot save)
    {
        if (save != null)
            SaveSlot = save;
        else
            ResetPersistentInfo();
    }

    public void Save(int saveSlot)
    {
        SavePlayTime();
        Rayman3.Save.SaveSlot(saveSlot, SaveSlot);
        StartPlayTime();
    }

    public void EnablePower(Power power)
    {
        Powers |= power;
    }

    public void DisablePower(Power power)
    {
        Powers &= ~power;
    }

    public bool IsPowerEnabled(Power power)
    {
        return (Powers & power) != 0;
    }

    public void EnableCheat(Scene2D scene, Cheat cheat)
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

    // Custom - the original game doesn't allow them to be disabled
    public void DisableCheat(Scene2D scene, Cheat cheat)
    {
        Cheats &= ~cheat;

        switch (cheat)
        {
            case Cheat.Invulnerable:
                scene.MainActor.IsInvulnerable = false;
                break;
        }
    }

    public bool IsCheatEnabled(Cheat cheat)
    {
        return (Cheats & cheat) != 0;
    }

    public int GetGreenLumsId()
    {
        int id = LoadedGreenLums;
        LoadedGreenLums++;
        return id;
    }

    public bool IsGreenLumDead(int lumId)
    {
        return lumId < LastGreenLumAlive;
    }

    public void GreenLumTouchedByRayman(int id, Vector2 pos)
    {
        Debug.Assert(id == LastGreenLumAlive, "Invalid Greens lums id. The lums ids have to be ordered.");

        LastGreenLumAlive++;
        CheckpointPosition = pos;
    }

    public bool GetLevelHasBlueLum()
    {
        return Level.HasBlueLum;
    }

    public bool IsBlueLumsNearEnd()
    {
        return BlueLumsTimer < 79;
    }

    public void ResetBlueLumsTime()
    {
        BlueLumsTimer = 0;
    }

    public bool IsBlueLumsTimeOver()
    {
        return BlueLumsTimer == 0;
    }

    public void IncBlueLumsTime()
    {
        if (IsBlueLumsNearEnd())
            Engine.Sem.ProcessEvent(Rayman3SoundEvent.Stop__LumTimer_Mix02);

        BlueLumsTimer += 304;
        if (BlueLumsTimer > 416)
            BlueLumsTimer = 416;
    }

    public bool GetLumStatus(int lumId)
    {
        return (PersistentInfo.Lums[lumId >> 3] & (1 << (lumId & 7))) == 0;
    }

    public bool GetCageStatus(int cageId)
    {
        return (PersistentInfo.Cages[cageId >> 3] & (1 << (cageId & 7))) == 0;
    }

    public void SetLumStatus(int lumId, bool isDead)
    {
        if (isDead)
            PersistentInfo.Lums[lumId >> 3] = (byte)(PersistentInfo.Lums[lumId >> 3] & ~(1 << (lumId & 7)));
        else
            PersistentInfo.Lums[lumId >> 3] = (byte)(PersistentInfo.Lums[lumId >> 3] | (1 << (lumId & 7)));
    }

    public void SetCageStatus(int cageId, bool isDead)
    {
        if (isDead)
            PersistentInfo.Cages[cageId >> 3] = (byte)(PersistentInfo.Cages[cageId >> 3] & ~(1 << (cageId & 7)));
        else
            PersistentInfo.Cages[cageId >> 3] = (byte)(PersistentInfo.Cages[cageId >> 3] | (1 << (cageId & 7)));
    }

    public int GetLumsCountForCurrentMap()
    {
        if (LevelType != LevelType.GameCube)
            return Level.LumsCount;
        else
            return YellowLumsCount;
    }

    public int GetCagesCountForCurrentMap()
    {
        if (LevelType != LevelType.GameCube)
            return Level.CagesCount;
        else
            return CagesCount;
    }

    public int GetDeadLumsForCurrentMap(MapId mapId)
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

    public int GetDeadCagesForCurrentMap(MapId mapId)
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

    public int GetLumsId()
    {
        if (LevelType != LevelType.GameCube && LoadedYellowLums >= YellowLumsCount)
            throw new Exception("Too many Yellow lums registered");

        int id = LoadedYellowLums;
        LoadedYellowLums++;
        return id;
    }

    public int GetCageId()
    {
        int id = LoadedCages;
        LoadedCages++;
        return id;
    }

    public int GetTotalDeadLums()
    {
        int count = 0;

        for (int i = 0; i < 1000; i++)
        {
            if (GetLumStatus(i))
                count++;
        }

        return count;
    }

    public int GetTotalDeadCages()
    {
        int count = 0;

        for (int i = 0; i < 50; i++)
        {
            if (GetCageStatus(i))
                count++;
        }

        return count;
    }

    public bool AreAllLumsDead()
    {
        return GetTotalDeadLums() == 1000;
    }

    public bool AreAllCagesDead()
    {
        return GetTotalDeadCages() == 50;
    }

    public bool World1LumsCompleted()
    {
        int count = 0;
        for (MapId mapId = MapId.WoodLight_M1; mapId <= MapId.SanctuaryOfBigTree_M2; mapId++)
            count += GetDeadLumsForCurrentMap(mapId);

        Debug.Assert(count <= LumsPerWorld, "Abnormal dead lums count in world #1");

        return count == LumsPerWorld;
    }

    public bool World2LumsCompleted()
    {
        int count = 0;
        for (MapId mapId = MapId.MissileRace1; mapId <= MapId.MarshAwakening2; mapId++)
            count += GetDeadLumsForCurrentMap(mapId);

        Debug.Assert(count <= LumsPerWorld, "Abnormal dead lums count in world #2");

        return count == LumsPerWorld;
    }

    public bool World3LumsCompleted()
    {
        int count = 0;
        for (MapId mapId = MapId.SanctuaryOfStoneAndFire_M1; mapId <= MapId.SanctuaryOfRockAndLava_M3; mapId++)
            count += GetDeadLumsForCurrentMap(mapId);

        Debug.Assert(count <= LumsPerWorld, "Abnormal dead lums count in world #3");

        return count == LumsPerWorld;
    }

    public bool World4LumsCompleted()
    {
        int count = 0;
        for (MapId mapId = MapId.TombOfTheAncients_M1; mapId <= MapId.BossFinal_M2; mapId++)
            count += GetDeadLumsForCurrentMap(mapId);

        Debug.Assert(count <= LumsPerWorld, "Abnormal dead lums count in world #4");

        return count == LumsPerWorld;
    }

    public bool IsLumDead(int lumId, MapId mapId)
    {
        return GetLumStatus(Levels[(int)mapId].GlobalLumsIndex + lumId);
    }

    public bool IsCageDead(int cageId, MapId mapId)
    {
        return GetCageStatus(Levels[(int)mapId].GlobalCagesIndex + cageId);
    }

    public bool HasCollectedAllLumsInLevel()
    {
        return GetDeadLumsForCurrentMap(MapId) == YellowLumsCount;
    }

    public bool HasCollectedAllCagesInLevel()
    {
        return GetDeadCagesForCurrentMap(MapId) == CagesCount;
    }

    public void KillLum(int lumId)
    {
        if (LevelType == LevelType.GameCube)
        {
            GameCubeCollectedYellowLumsCount++;
            
            if (GameCubeCollectedYellowLumsCount == YellowLumsCount)
            {
                Engine.Sem.ProcessEvent(Rayman3SoundEvent.Play__LumTotal_Mix02);
                LevelMusicManager.OverrideLevelMusic(Rayman3SoundEvent.Play__win2);
            }
        }
        else
        {
            SetLumStatus(Level.GlobalLumsIndex + lumId, true);

            // NOTE: Game also checks to MapId is not 0xFF, but that shouldn't be possible
            if (HasCollectedAllLumsInLevel() && LevelType != LevelType.Race)
            {
                Engine.Sem.ProcessEvent(Rayman3SoundEvent.Play__LumTotal_Mix02);
                LevelMusicManager.OverrideLevelMusic(Rayman3SoundEvent.Play__win2);
            }

            Rayman3Achievements.CheckProgressionBasedAchievements();
        }
    }

    public void KillCage(int cageId)
    {
        if (LevelType == LevelType.GameCube)
        {
            GameCubeCollectedCagesCount++;
            if (GameCubeCollectedCagesCount == CagesCount)
                Engine.Sem.ProcessEvent(Rayman3SoundEvent.Play__LumTotal_Mix02);
        }
        else
        {
            SetCageStatus(Level.GlobalCagesIndex + cageId, true);

            // NOTE: Game also checks to MapId is not 0xFF, but that shouldn't be possible
            if (HasCollectedAllCagesInLevel())
                Engine.Sem.ProcessEvent(Rayman3SoundEvent.Play__LumTotal_Mix02);

            Rayman3Achievements.CheckProgressionBasedAchievements();
        }
    }

    public void KillAllLums()
    {
        Array.Clear(PersistentInfo.Lums);
    }

    public void KillAllCages()
    {
        Array.Clear(PersistentInfo.Cages);
    }

    public void SetPowerBasedOnMap(MapId mapId)
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

    public MapId GetNextLevelId()
    {
        return (MapId)Level.NextLevelId;
    }

    public byte GetLevelCurtainActorId()
    {
        if (PersistentInfo.LastPlayedLevel == 0xFF)
            return 0;
        else
            return Levels[PersistentInfo.LastPlayedLevel].LevelCurtainActorId;
    }

    public void LoadLevel(MapId mapId)
    {
        if (mapId > MapId.WorldMap)
            throw new Exception("Invalid map");

        if (mapId == MapId.MarshAwakening1 && PersistentInfo.LastCompletedLevel < (int)MapId.MarshAwakening1)
        {
            Engine.FrameMngr.SetNextFrame(new Act2());
        }
        else if (mapId == MapId.PirateShip_M1 && PersistentInfo.LastCompletedLevel < (int)MapId.PirateShip_M1)
        {
            Engine.FrameMngr.SetNextFrame(new Act5());
        }
        else
        {
            Engine.FrameMngr.SetNextFrame(LevelFactory.Create(mapId));
        }
    }

    public void LevelDeath()
    {
        if (PersistentInfo.Lives == 0)
        {
            // Game over
            Engine.FrameMngr.SetNextFrame(new GameOver());
        }
        else
        {
            // Reload current map
            Engine.FrameMngr.ReloadCurrentFrame();
        }
    }

    public void GotoLastSaveGame()
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

        if (Rom.Platform == Platform.GBA)
            Engine.Settings.Local.General.LastPlayedGbaSaveSlot = CurrentSlot;
        else if (Rom.Platform == Platform.NGage)
            Engine.Settings.Local.General.LastPlayedNGageSaveSlot = CurrentSlot;
        else
            throw new UnsupportedPlatformException();
    }

    public void SetNextMapId(MapId mapId)
    {
        LastGreenLumAlive = 0;
        NextMapId = mapId;
        LoadedGreenLums = 0;
        HasCollectedWhiteLum = false;
        CollectedWhiteLums.Clear();
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

    public void InitLevel(LevelType type)
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

    public void SetLevelRichPresence()
    {
        string presence = GetLevelName(MapId);
        if (RSMultiplayer.IsActive)
            presence += " (Multiplayer)";
        else if (Rayman3.TimeAttack.IsActive)
            presence += " (Time Attack)";
        Engine.RichPresence.SetPresence(presence);
    }

    // Custom method so we can easily get the name of a map
    public string GetLevelName(MapId mapId)
    {
        switch (mapId)
        {
            case <= MapId.ChallengeLyGCN:
                return Rayman3.Loc.GetText(TextBankId.LevelNames, Levels[(int)mapId].NameTextId)[0];

            case MapId.Power1:
                return GetLevelName(MapId.WoodLight_M2);

            case MapId.Power2:
                return GetLevelName(MapId.BossMachine);

            case MapId.Power3:
                return GetLevelName(MapId.EchoingCaves_M2);
            
            case MapId.Power4:
                return GetLevelName(MapId.BossRockAndLava);
            
            case MapId.Power5:
                return GetLevelName(MapId.SanctuaryOfStoneAndFire_M3);
            
            case MapId.Power6:
                return GetLevelName(MapId.BossScaleMan);

            case MapId.World1:
                return Rayman3.Loc.GetText(TextBankId.LevelNames, 31)[0];

            case MapId.World2:
                return Rayman3.Loc.GetText(TextBankId.LevelNames, 32)[0];

            case MapId.World3:
                return Rayman3.Loc.GetText(TextBankId.LevelNames, 33)[0];

            case MapId.World4:
                return Rayman3.Loc.GetText(TextBankId.LevelNames, 34)[0];

            case MapId.WorldMap:
                return "Worldmap";

            case MapId.GbaMulti_MissileRace when Rom.Platform is Platform.GBA:
                return Rayman3.Loc.GetText(TextBankId.Connectivity, 13)[0];

            case MapId.GbaMulti_MissileArena when Rom.Platform is Platform.GBA:
                return Rayman3.Loc.GetText(TextBankId.Connectivity, 14)[0];

            case MapId.GbaMulti_TagWeb when Rom.Platform is Platform.GBA:
                return Rayman3.Loc.GetText(TextBankId.Connectivity, 9)[0];

            case MapId.GbaMulti_TagSlide when Rom.Platform is Platform.GBA:
                return Rayman3.Loc.GetText(TextBankId.Connectivity, 10)[0];

            case MapId.GbaMulti_CatAndMouseSlide when Rom.Platform is Platform.GBA:
                return Rayman3.Loc.GetText(TextBankId.Connectivity, 11)[0];

            case MapId.GbaMulti_CatAndMouseSpider when Rom.Platform is Platform.GBA:
                return Rayman3.Loc.GetText(TextBankId.Connectivity, 12)[0];

            case MapId.NGageMulti_CaptureTheFlagMiddleGround when Rom.Platform is Platform.NGage:
                return Rayman3.Loc.GetText(TextBankId.Connectivity, 10)[0];

            case MapId.NGageMulti_CaptureTheFlagFloors when Rom.Platform is Platform.NGage:
                return Rayman3.Loc.GetText(TextBankId.Connectivity, 11)[0];

            case MapId.NGageMulti_CaptureTheFlagOneForAll when Rom.Platform is Platform.NGage:
                return Rayman3.Loc.GetText(TextBankId.Connectivity, 12)[0];

            case MapId.NGageMulti_CaptureTheFlagAllForOne when Rom.Platform is Platform.NGage:
                return Rayman3.Loc.GetText(TextBankId.Connectivity, 13)[0];

            case MapId.NGageMulti_CaptureTheFlagTeamWork when Rom.Platform is Platform.NGage:
                return Rayman3.Loc.GetText(TextBankId.Connectivity, 14)[0];

            case MapId.NGageMulti_CaptureTheFlagTeamPlayer when Rom.Platform is Platform.NGage:
                return Rayman3.Loc.GetText(TextBankId.Connectivity, 15)[0];

            case MapId.NGageMulti_TagWeb when Rom.Platform is Platform.NGage:
                return Rayman3.Loc.GetText(TextBankId.Connectivity, 16)[0];

            case MapId.NGageMulti_TagSlide when Rom.Platform is Platform.NGage:
                return Rayman3.Loc.GetText(TextBankId.Connectivity, 17)[0];

            case MapId.NGageMulti_CatAndMouseSlide when Rom.Platform is Platform.NGage:
                return Rayman3.Loc.GetText(TextBankId.Connectivity, 18)[0];

            case MapId.NGageMulti_CatAndMouseSpider when Rom.Platform is Platform.NGage:
                return Rayman3.Loc.GetText(TextBankId.Connectivity, 19)[0];

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
                return $"GameCube Bonus {mapId - MapId.GameCube_Bonus1 + 1}";

            default:
                return String.Empty;
        }
    }

    public bool IsFirstTimeCompletingLevel()
    {
        return PersistentInfo.LastCompletedLevel < (int)MapId;
    }

    public void UpdateLastCompletedLevel()
    {
        if (MapId < MapId.Bonus1 && PersistentInfo.LastCompletedLevel < (int)MapId)
            PersistentInfo.LastCompletedLevel = (byte)MapId;

        Rayman3Achievements.CheckProgressionBasedAchievements();
    }

    public void ModifyLives(int change)
    {
        if (IsCheatEnabled(Cheat.InfiniteLives))
        {
            PersistentInfo.Lives = 99;
            return;
        }

        // Don't allow losing lives if the infinite lives option is enabled
        if (Engine.Settings.Active.Difficulty.InfiniteLives && change < 0)
            return;

        int newCount = PersistentInfo.Lives + change;

        if (newCount < 0)
            PersistentInfo.Lives = 0;
        else if (newCount < 100)
            PersistentInfo.Lives = (byte)newCount;
    }

    public void PlayLevelMusic()
    {
        Engine.Sem.ProcessEvent(Level.StartMusicSoundEvent);
    }

    public void StopLevelMusic()
    {
        if (LevelType != LevelType.GameCube)
            Engine.Sem.ProcessEvent(Level.StopMusicSoundEvent);
    }

    public Rayman3SoundEvent GetLevelMusicSoundEvent()
    {
        if (LevelType == LevelType.GameCube)
            return ((FrameSideScrollerGCN)Frame.Current).MapInfo.StartMusicSoundEvent;
        else
            return Level.StartMusicSoundEvent;
    }

    public Rayman3SoundEvent GetSpecialLevelMusicSoundEvent()
    {
        if (LevelType == LevelType.GameCube)
            return ((FrameSideScrollerGCN)Frame.Current).MapInfo.StartSpecialMusicSoundEvent;
        else
            return Level.StartSpecialMusicSoundEvent;
    }

    // Custom for slot play time
    public void StartPlayTime()
    {
        PlayTimer.Restart();
    }

    // Custom for slot play time
    public void SavePlayTime()
    {
        PlayTimer.Stop();
        SaveSlot.PlayTime += PlayTimer.ElapsedMilliseconds;
    }

    // Custom for slot play time
    public TimeSpan GetPlayTime()
    {
        return TimeSpan.FromMilliseconds(SaveSlot.PlayTime);
    }
}