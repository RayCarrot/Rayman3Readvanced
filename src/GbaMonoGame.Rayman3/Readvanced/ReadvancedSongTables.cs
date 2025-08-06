using System.Collections.Generic;
using BinarySerializer.Ubisoft.GbaEngine;

namespace GbaMonoGame.Rayman3.Readvanced;

// For custom sounds. Start all IDs at 1000 to avoid conflicts with the original game.
public static class ReadvancedSongTables
{
    // Samples are played through agbplay
    public static Dictionary<int, string> GbaSongTable => new()
    {
        [1000] = "NewSfx/PadStamp01_Mix01_Gba",
        [1001] = "NewSfx/PadStamp02_Mix01_Gba",
    };

    // Samples are higher volume
    public static Dictionary<int, string> NGageSongTable => new()
    {
        [1000] = "NewSfx/PadStamp01_Mix01_NGage",
        [1001] = "NewSfx/PadStamp02_Mix01_NGage",
    };

    public static Dictionary<int, SoundEvent> GbaSoundEvents => new()
    {
        // Play__PadStamp01_Mix01
        [1000] = new()
        {
            Type = SoundEvent.SoundEventType.Play,
            Priority = 50,
            ResourceId = 1000,
            SoundType = SoundType.Sfx,
            EnablePan = false,
            EnableRollOff = false,
        },
        // Play__PadStamp02_Mix01
        [1001] = new()
        {
            Type = SoundEvent.SoundEventType.Play,
            Priority = 50,
            ResourceId = 1001,
            SoundType = SoundType.Sfx,
            EnablePan = false,
            EnableRollOff = false,
        },
    };

    public static Dictionary<int, NGageSoundEvent> NGageSoundEvents => new()
    {
        // Play__PadStamp01_Mix01
        [1000] = new()
        {
            IsValid = true,
            SoundResourceId = 1000,
            InstrumentsResourceId = -1,
            Volume = 7,
            Loop = false,
            PlaySong = true,
            IsMusic = false
        },
        // Play__PadStamp02_Mix01
        [1001] = new()
        {
            IsValid = true,
            SoundResourceId = 1001,
            InstrumentsResourceId = -1,
            Volume = 7,
            Loop = false,
            PlaySong = true,
            IsMusic = false
        },
    };

    public static Dictionary<int, SoundResource> GbaSoundResources => new()
    {
        // PadStamp01_Mix01GEN
        [1000] = new()
        {
            Id = 1000,
            Type = SoundResource.ResourceType.Song,
            SongTableIndex = 1000,
            Loop = false,
            IsMusic = false,
        },
        // PadStamp02_Mix01GEN
        [1001] = new()
        {
            Id = 1001,
            Type = SoundResource.ResourceType.Song,
            SongTableIndex = 1001,
            Loop = false,
            IsMusic = false,
        },
        // Originally had it set up to randomize through the resource, but
        // changed to do it through code to support the N-Gage version
        //[1002] = new()
        //{
        //    Id = 1002,
        //    Type = SoundResource.ResourceType.Random,
        //    ResourceIdsCount = 2,
        //    ResourceIds = [1000, 1001],
        //    ResourceIdConditions = [60, 100]
        //},
    };
}