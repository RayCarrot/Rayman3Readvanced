using System;
using BinarySerializer.Ubisoft.GbaEngine;

namespace GbaMonoGame;

public class GeneralGameConfig : IniSectionObject
{
    public GeneralGameConfig()
    {
        LastPlayedGbaSaveSlot = null;
        LastPlayedNGageSaveSlot = null;
        LastPlayedPlatform = Platform.GBA;
    }

    public override string SectionKey => "General";

    public int? LastPlayedGbaSaveSlot { get; set; }
    public int? LastPlayedNGageSaveSlot { get; set; }
    public Platform LastPlayedPlatform { get; set; }

    public override void Serialize(BaseIniSerializer serializer)
    {
        LastPlayedGbaSaveSlot = serializer.Serialize<int?>(LastPlayedGbaSaveSlot, "LastPlayedGbaSaveSlot");
        LastPlayedNGageSaveSlot = serializer.Serialize<int?>(LastPlayedNGageSaveSlot, "LastPlayedNGageSaveSlot");
        LastPlayedPlatform = serializer.Serialize<Platform>(LastPlayedPlatform, "LastPlayedPlatform");

        if (!Enum.IsDefined(LastPlayedPlatform))
            LastPlayedPlatform = Platform.GBA;
    }
}