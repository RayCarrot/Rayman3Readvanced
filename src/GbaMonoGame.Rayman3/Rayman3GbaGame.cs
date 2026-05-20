using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using BinarySerializer.Ubisoft.GbaEngine;
using GbaMonoGame.Engine2d;
using GbaMonoGame.Rayman3.Readvanced;
using GbaMonoGame.TgxEngine;

namespace GbaMonoGame.Rayman3;

public class Rayman3GbaGame : GbaGame
{
    #region Protected Properties

    protected override string Title => "Rayman 3";

    #endregion

    #region Private Methods

    private static SoundEventsManager CreateSoundEventsManager()
    {
        SoundEventsManager sem;

        // Load sound manager
        if (Rom.Platform == Platform.GBA)
        {
            // Load song table and add custom ones
            Dictionary<int, string> songTable = Rayman3SoundTables.GbaSongTable;
            foreach (var songEntry in ReadvancedSongTables.GbaSongTable)
                songTable.Add(songEntry.Key, songEntry.Value);

            // Load sound events and add custom ones
            List<SoundEvent> soundEvents = Rom.Loader.SoundBank.Events.Select(x => x.Value).ToList();
            foreach (var readvancedSoundEvent in ReadvancedSongTables.GbaSoundEvents)
            {
                if (readvancedSoundEvent.Key >= soundEvents.Count)
                    CollectionsMarshal.SetCount(soundEvents, readvancedSoundEvent.Key + 1);

                soundEvents[readvancedSoundEvent.Key] = readvancedSoundEvent.Value;
            }

            // Load sound resources and add custom ones
            List<SoundResource> soundResources = Rom.Loader.SoundBank.Resources.Select(x => x.Value).ToList();
            foreach (var readvancedSoundResource in ReadvancedSongTables.GbaSoundResources)
            {
                if (readvancedSoundResource.Key >= soundResources.Count)
                    CollectionsMarshal.SetCount(soundResources, readvancedSoundResource.Key + 1);

                soundResources[readvancedSoundResource.Key] = readvancedSoundResource.Value;
            }

            sem = new GbaSoundEventsManager($"{Paths.AssetsDirectoryName}/{Assets.BaseName}", songTable, soundEvents.ToArray(), soundResources.ToArray());
        }
        else if (Rom.Platform == Platform.NGage)
        {
            // Load song tables
            Dictionary<int, string> songTable = Rayman3SoundTables.NGageSongTable;
            Dictionary<int, string> readvancedSongTable = ReadvancedSongTables.NGageSongTable;

            // Load sound events and add custom ones
            List<NGageSoundEvent> soundEvents = Rom.Loader.NGage_SoundEvents.ToList();
            foreach (var readvancedSoundEvent in ReadvancedSongTables.NGageSoundEvents)
            {
                if (readvancedSoundEvent.Key >= soundEvents.Count)
                    soundEvents.AddRange(Enumerable.Repeat(new NGageSoundEvent() { IsValid = false }, (readvancedSoundEvent.Key - soundEvents.Count) + 1));

                soundEvents[readvancedSoundEvent.Key] = readvancedSoundEvent.Value;
            }

            sem = new NGageSoundEventsManager(songTable, $"{Paths.AssetsDirectoryName}/{Assets.BaseName}", readvancedSongTable, soundEvents.ToArray());
        }
        else
        {
            throw new UnsupportedPlatformException();
        }

        // Set sound engine callbacks
        sem.SetCallBacks(new Rayman3CallBackSet());

        return sem;
    }

    #endregion

    #region Protected Methods

    protected override Frame CreateInitialFrame() => new TitleScreen(false);
    protected override Frame CreateFatalErrorFrame(Exception exception) => new FrameFatalError(exception);

    protected override void InitEngine()
    {
        Rayman3.InitEngine();
    }

    protected override void InitGame()
    {
        Engine.InitGame(
            sem: CreateSoundEventsManager(),
            font: new FontManager(Rom.Loader.Font8, Rom.Loader.Font16, Rom.Loader.Font32));
        Rayman3.InitGame(
            save: new SaveGameManager());
    }

    protected override void UnInitEngine()
    {
        Rayman3.UnInitEngine();
    }

    protected override void UnInitGame()
    {
        Engine.UnInitGame();
        Rayman3.UnInitGame();
    }

    protected override void AddDebugWindowsAndMenus(DebugLayout debugLayout)
    {
        debugLayout.AddWindow(new SceneDebugWindow());
        debugLayout.AddWindow(new GameObjectDebugWindow());
        debugLayout.AddWindow(new PlayfieldDebugWindow());
        debugLayout.AddWindow(new Rayman3DebugWindow());
        debugLayout.AddMenu(new FramesDebugMenu());
        debugLayout.AddMenu(new ExportDebugMenu());
    }

    #endregion
}