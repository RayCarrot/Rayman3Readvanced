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
            // Read the sound bank
            SoundBank soundBank = Rom.Loader.ReadSoundBank();

            // Load song table and add custom ones
            Dictionary<int, string> songTable = Rayman3SoundTables.GbaSongTable;
            foreach (var songEntry in ReadvancedSongTables.GbaSongTable)
                songTable.Add(songEntry.Key, songEntry.Value);

            // Load sound events and add custom ones
            List<SoundEvent> events = soundBank.Events.Select(x => x.Value).ToList();
            foreach (var readvancedSoundEvent in ReadvancedSongTables.GbaSoundEvents)
            {
                if (readvancedSoundEvent.Key >= events.Count)
                    CollectionsMarshal.SetCount(events, readvancedSoundEvent.Key + 1);

                events[readvancedSoundEvent.Key] = readvancedSoundEvent.Value;
            }

            // Load sound resources and add custom ones
            List<SoundResource> resources = soundBank.Resources.Select(x => x.Value).ToList();
            foreach (var readvancedSoundResource in ReadvancedSongTables.GbaSoundResources)
            {
                if (readvancedSoundResource.Key >= resources.Count)
                    CollectionsMarshal.SetCount(resources, readvancedSoundResource.Key + 1);

                resources[readvancedSoundResource.Key] = readvancedSoundResource.Value;
            }

            sem = new GbaSoundEventsManager($"{Paths.AssetsDirectoryName}/{Assets.BaseName}", songTable, events.ToArray(), resources.ToArray());
        }
        else if (Rom.Platform == Platform.NGage)
        {
            // Read the sound events
            NGageSoundEvent[] soundEvents = Rom.Loader.ReadNGageSoundEvents();

            // Load song tables
            Dictionary<int, string> songTable = Rayman3SoundTables.NGageSongTable;
            Dictionary<int, string> readvancedSongTable = ReadvancedSongTables.NGageSongTable;

            // Load sound events and add custom ones
            List<NGageSoundEvent> events = soundEvents.ToList();
            foreach (var readvancedSoundEvent in ReadvancedSongTables.NGageSoundEvents)
            {
                if (readvancedSoundEvent.Key >= events.Count)
                    events.AddRange(Enumerable.Repeat(new NGageSoundEvent() { IsValid = false }, (readvancedSoundEvent.Key - events.Count) + 1));

                events[readvancedSoundEvent.Key] = readvancedSoundEvent.Value;
            }

            sem = new NGageSoundEventsManager(songTable, $"{Paths.AssetsDirectoryName}/{Assets.BaseName}", readvancedSongTable, events.ToArray());
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
            font: new FontManager());
        Rayman3.InitGame(
            save: new SaveGameManager(),
            localizationManager: new LocalizationManager(Rom.Loader.ReadTextBanks()));
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