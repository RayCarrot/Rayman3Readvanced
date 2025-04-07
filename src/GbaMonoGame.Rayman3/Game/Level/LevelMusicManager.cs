using System;
using BinarySerializer.Ubisoft.GbaEngine;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;

namespace GbaMonoGame.Rayman3;

public static class LevelMusicManager
{
    public static short OverridenSoundEvent { get; set; }
    public static byte Timer { get; set; }

    // Flags
    public static bool IsPlayingSpecialMusic { get; set; }
    public static bool IsCooldown { get; set; }
    public static bool ShouldPlaySpecialMusic { get; set; }
    public static bool HasOverridenLevelMusic { get; set; }

    public static void Init()
    {
        Timer = 0;
        IsPlayingSpecialMusic = false;
        IsCooldown = false;
        ShouldPlaySpecialMusic = false;
        HasOverridenLevelMusic = false;
    }

    private static void PlaySpecialMusic()
    {
        if (IsPlayingSpecialMusic || IsCooldown)
            return;

        if (GameInfo.MapId is MapId.TombOfTheAncients_M1 or MapId.TombOfTheAncients_M2)
        {
            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Stop__ancients);
            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__spiderchase);
        }
        else
        {
            SoundEventsManager.ReplaceAllSongs(GameInfo.GetSpecialLevelMusicSoundEvent(), 3);
        }

        if (Rom.Platform == Platform.NGage)
        {
            switch (GameInfo.MapId)
            {
                case MapId.WoodLight_M1:
                case MapId.WoodLight_M2:
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__enemy1__After__woodlight);
                    break;

                case MapId.FairyGlade_M1:
                case MapId.FairyGlade_M2:
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__enemy1__After__fairyglades);
                    break;

                case MapId.SanctuaryOfBigTree_M1:
                case MapId.SanctuaryOfBigTree_M2:
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__enemy2__After__bigtrees);
                    break;

                case MapId.EchoingCaves_M2:
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__enemy1__After__echocave);
                    SoundEventsManager.ReplaceAllSongs(Rayman3SoundEvent.Play__enemy1, 3);
                    break;

                case MapId.SanctuaryOfStoneAndFire_M2:
                case MapId.SanctuaryOfStoneAndFire_M3:
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__enemy1__After__firestone);
                    break;

                case MapId.BeneathTheSanctuary_M1:
                case MapId.BeneathTheSanctuary_M2:
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__enemy2__After__helico);
                    break;

                case MapId.ThePrecipice_M1:
                case MapId.ThePrecipice_M2:
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__enemy2__After__precipice);
                    break;

                case MapId.TheCanopy_M1:
                    SoundEventsManager.ReplaceAllSongs(Rayman3SoundEvent.Play__enemy2, 3);
                    break;
                    
                case MapId.TombOfTheAncients_M1:
                case MapId.TombOfTheAncients_M2:
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Stop__ancients);
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__spiderchase);
                    break;

                case MapId.World1:
                case MapId.World2:
                case MapId.World3:
                case MapId.World4:
                    SoundEventsManager.ReplaceAllSongs(Rayman3SoundEvent.Play__tizetre, 3);
                    break;
            }
        }

        IsPlayingSpecialMusic = true;
        IsCooldown = true;
    }

    public static void StopSpecialMusic()
    {
        if (IsPlayingSpecialMusic && !IsCooldown)
        {
            Rayman3SoundEvent soundEvent = GameInfo.GetLevelMusicSoundEvent();
            SoundEventsManager.ReplaceAllSongs(soundEvent, 3);

            if (Rom.Platform == Platform.NGage)
            {
                switch (GameInfo.MapId)
                {
                    case MapId.WoodLight_M1:
                    case MapId.WoodLight_M2:
                        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__woodlight__After__enemy1);
                        break;

                    case MapId.FairyGlade_M1:
                    case MapId.FairyGlade_M2:
                        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__fairyglades__After__enemy1);
                        break;

                    case MapId.SanctuaryOfBigTree_M1:
                    case MapId.SanctuaryOfBigTree_M2:
                        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__bigtrees__After__enemy2);
                        break;

                    case MapId.EchoingCaves_M2:
                        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__echocave__After__enemy1);
                        SoundEventsManager.ReplaceAllSongs(Rayman3SoundEvent.Play__echocave, 3);
                        break;

                    case MapId.SanctuaryOfStoneAndFire_M2:
                    case MapId.SanctuaryOfStoneAndFire_M3:
                        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__firestone__After__enemy1);
                        break;

                    case MapId.BeneathTheSanctuary_M1:
                    case MapId.BeneathTheSanctuary_M2:
                        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__helico__After__enemy2);
                        break;

                    case MapId.ThePrecipice_M1:
                    case MapId.ThePrecipice_M2:
                        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__precipice__After__enemy2);
                        break;

                    case MapId.TheCanopy_M1:
                        SoundEventsManager.ReplaceAllSongs(Rayman3SoundEvent.Play__canopy, 3);
                        break;

                    case MapId.TombOfTheAncients_M1:
                    case MapId.TombOfTheAncients_M2:
                        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__ancients__After__spiderchase);
                        break;

                    case MapId.World1:
                    case MapId.World2:
                    case MapId.World3:
                    case MapId.World4:
                        SoundEventsManager.ReplaceAllSongs(Rayman3SoundEvent.Play__polokus, 3);
                        break;
                }
            }

            IsPlayingSpecialMusic = false;
            IsCooldown = true;
        }
    }

    public static void Step()
    {
        if (!SoundEventsManager.IsSongPlaying(Rayman3SoundEvent.Play__win3))
        {
            if (!IsCooldown)
            {
                if (!IsPlayingSpecialMusic)
                {
                    if (ShouldPlaySpecialMusic)
                        PlaySpecialMusic();
                }
                else
                {
                    if (!ShouldPlaySpecialMusic)
                        StopSpecialMusic();
                }
            }
            else
            {
                Timer--;

                if (Timer == 0)
                {
                    Timer = 120;
                    IsCooldown = false;

                    if (HasOverridenLevelMusic)
                    {
                        SoundEventsManager.ProcessEvent(OverridenSoundEvent);
                        HasOverridenLevelMusic = false;
                    }
                }
            }
        }

        ShouldPlaySpecialMusic = false;
    }

    public static void PlaySpecialMusicIfDetected(GameObject obj)
    {
        Box objBox = new(
            minX: obj.Position.X - Rom.OriginalResolution.X,
            minY: obj.Position.Y - Rom.OriginalResolution.Y / 2,
            maxX: obj.Position.X + Rom.OriginalResolution.X,
            maxY: obj.Position.Y + 5);

        // Extend the box if playing
        if (IsPlayingSpecialMusic)
        {
            objBox = new Box(
                minX: objBox.MinX - 64,
                minY: objBox.MinY - 64,
                maxX: objBox.MaxX + 64,
                maxY: objBox.MaxY + 64);
        }

        Box mainActorDetectionBox = obj.Scene.MainActor.GetDetectionBox();

        if (mainActorDetectionBox.Intersects(objBox))
            ShouldPlaySpecialMusic = true;
    }

    public static void PlaySpecialMusicIfDetectedWith(GameObject obj, Box box)
    {
        Box objBox = box.Offset(obj.Position);

        // Extend the box if playing
        if (IsPlayingSpecialMusic)
        {
            objBox = new Box(
                minX: objBox.MinX - 64,
                minY: objBox.MinY - 64,
                maxX: objBox.MaxX + 64,
                maxY: objBox.MaxY + 64);
        }

        Box mainActorDetectionBox = obj.Scene.MainActor.GetDetectionBox();

        if (mainActorDetectionBox.Intersects(objBox))
            ShouldPlaySpecialMusic = true;
    }

    public static void OverrideLevelMusic(Enum soundEventId) => OverrideLevelMusic((short)(object)soundEventId);
    public static void OverrideLevelMusic(short soundEventId)
    {
        if (HasOverridenLevelMusic) 
            return;
        
        Timer = 180;
        IsPlayingSpecialMusic = false;
        IsCooldown = true;
        HasOverridenLevelMusic = true;
        OverridenSoundEvent = SoundEventsManager.ReplaceAllSongs(soundEventId, 0);
    }
}