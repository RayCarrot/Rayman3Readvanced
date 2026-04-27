using BinarySerializer.Nintendo.GBA;
using BinarySerializer.Ubisoft.GbaEngine;
using GbaMonoGame.TgxEngine;

namespace GbaMonoGame.Rayman3;

public static class Rayman3TileFixes
{
    public static void DefineTileFixes(Platform platform)
    {
        TgxTileLayer.ClearTileFixes();

        // Out-of-place tile with a pink pixel near Murfy
        TgxTileLayer.DefineTileFix(MapId.WoodLight_M1, layerId: 2, tileX: 153, tileY: 13, newTile: new MapTile(0, 0));

        // Empty tile on the ground
        TgxTileLayer.DefineTileFix(MapId.WoodLight_M1, layerId: 2, tileX: 242, tileY: 36, newTile: new MapTile(1417, 3));

        // Out-of-place tile with a pink pixel at the start
        TgxTileLayer.DefineTileFix(MapId.FairyGlade_M2, layerId: 2, tileX: 101, tileY: 43, newTile: new MapTile(0, 0));

        // Out-of-place rope tile between two climbable nets
        TgxTileLayer.DefineTileFix(MapId.SanctuaryOfBigTree_M2, layerId: 2, tileX: 521, tileY: 0, newTile: new MapTile(0, 0));

        // Out-of-place vertical row of rock tiles in the background
        TgxTileLayer.DefineTileFix(MapId.EchoingCaves_M2, layerId: 1, tileX: 129, tileY: 5, newTile: new MapTile(0, 0));
        TgxTileLayer.DefineTileFix(MapId.EchoingCaves_M2, layerId: 1, tileX: 129, tileY: 6, newTile: new MapTile(0, 0));
        TgxTileLayer.DefineTileFix(MapId.EchoingCaves_M2, layerId: 1, tileX: 129, tileY: 7, newTile: new MapTile(0, 0));
        TgxTileLayer.DefineTileFix(MapId.EchoingCaves_M2, layerId: 1, tileX: 129, tileY: 8, newTile: new MapTile(0, 0));

        // Out-of-place tile next to bones with single white pixel
        TgxTileLayer.DefineTileFix(MapId.CavesOfBadDreams_M1, layerId: 2, tileX: 62, tileY: 592, newTile: new MapTile(0, 0));
        TgxTileLayer.DefineTileFix(MapId.CavesOfBadDreams_M1, layerId: 2, tileX: 63, tileY: 440, newTile: new MapTile(0, 0));
        TgxTileLayer.DefineTileFix(MapId.CavesOfBadDreams_M1, layerId: 2, tileX: 3, tileY: 409, newTile: new MapTile(0, 0));
        TgxTileLayer.DefineTileFix(MapId.CavesOfBadDreams_M1, layerId: 2, tileX: 62, tileY: 381, newTile: new MapTile(0, 0));
        TgxTileLayer.DefineTileFix(MapId.CavesOfBadDreams_M1, layerId: 2, tileX: 62, tileY: 234, newTile: new MapTile(0, 0));
        TgxTileLayer.DefineTileFix(MapId.CavesOfBadDreams_M1, layerId: 2, tileX: 62, tileY: 195, newTile: new MapTile(0, 0));
        TgxTileLayer.DefineTileFix(MapId.CavesOfBadDreams_M1, layerId: 2, tileX: 66, tileY: 755, newTile: new MapTile(0, 0));
        TgxTileLayer.DefineTileFix(MapId.CavesOfBadDreams_M1, layerId: 2, tileX: 9, tileY: 425, newTile: new MapTile(0, 0));

        // Incomplete spike next to other spikes
        TgxTileLayer.DefineTileFix(MapId.CavesOfBadDreams_M1, layerId: 2, tileX: 33, tileY: 817, newTile: new MapTile(0, 0));
        TgxTileLayer.DefineTileFix(MapId.CavesOfBadDreams_M1, layerId: 2, tileX: 33, tileY: 818, newTile: new MapTile(0, 0));
        TgxTileLayer.DefineTileFix(MapId.CavesOfBadDreams_M1, layerId: 2, tileX: 33, tileY: 819, newTile: new MapTile(0, 0));
        TgxTileLayer.DefineTileFix(MapId.CavesOfBadDreams_M1, layerId: 2, tileX: 33, tileY: 820, newTile: new MapTile(0, 0));

        // Missing tile for bone ladder
        TgxTileLayer.DefineTileFix(MapId.CavesOfBadDreams_M1, layerId: 2, tileX: 66, tileY: 783, newTile: new MapTile(875, 2));

        // The following fixes are already present in the N-Gage version, so only add them for GBA
        if (platform == Platform.GBA)
        {
            // Missing tiles on the edge of a pillar
            TgxTileLayer.DefineTileFix(MapId.CavesOfBadDreams_M2, layerId: 2, tileX: 187, tileY: 29, newTile: new MapTile(6, 3, true));
            TgxTileLayer.DefineTileFix(MapId.CavesOfBadDreams_M2, layerId: 2, tileX: 187, tileY: 30, newTile: new MapTile(30, 3, true));
            TgxTileLayer.DefineTileFix(MapId.CavesOfBadDreams_M2, layerId: 2, tileX: 187, tileY: 31, newTile: new MapTile(59, 3, true));
            TgxTileLayer.DefineTileFix(MapId.CavesOfBadDreams_M2, layerId: 2, tileX: 187, tileY: 32, newTile: new MapTile(92, 3, true));
            TgxTileLayer.DefineTileFix(MapId.CavesOfBadDreams_M2, layerId: 2, tileX: 187, tileY: 33, newTile: new MapTile(132, 3, true));
            TgxTileLayer.DefineTileFix(MapId.CavesOfBadDreams_M2, layerId: 2, tileX: 187, tileY: 34, newTile: new MapTile(174, 3, true));

            // Bottom part of a pillar incorrectly shifted to the left
            TgxTileLayer.DefineTileFix(MapId.CavesOfBadDreams_M2, layerId: 2, tileX: 1250, tileY: 52, newTile: new MapTile(0, 0));
            TgxTileLayer.DefineTileFix(MapId.CavesOfBadDreams_M2, layerId: 2, tileX: 1251, tileY: 52, newTile: new MapTile(1291, 3, true));
            TgxTileLayer.DefineTileFix(MapId.CavesOfBadDreams_M2, layerId: 2, tileX: 1252, tileY: 52, newTile: new MapTile(1290, 3, true));
            TgxTileLayer.DefineTileFix(MapId.CavesOfBadDreams_M2, layerId: 2, tileX: 1253, tileY: 52, newTile: new MapTile(1289, 3, true));
            TgxTileLayer.DefineTileFix(MapId.CavesOfBadDreams_M2, layerId: 2, tileX: 1254, tileY: 52, newTile: new MapTile(1288, 3, true));
            TgxTileLayer.DefineTileFix(MapId.CavesOfBadDreams_M2, layerId: 2, tileX: 1255, tileY: 52, newTile: new MapTile(1287, 3, true));
            TgxTileLayer.DefineTileFix(MapId.CavesOfBadDreams_M2, layerId: 2, tileX: 1256, tileY: 52, newTile: new MapTile(1286, 3, true));
            TgxTileLayer.DefineTileFix(MapId.CavesOfBadDreams_M2, layerId: 2, tileX: 1257, tileY: 52, newTile: new MapTile(1285, 3, true));
            TgxTileLayer.DefineTileFix(MapId.CavesOfBadDreams_M2, layerId: 2, tileX: 1258, tileY: 52, newTile: new MapTile(1284, 3, true));
            TgxTileLayer.DefineTileFix(MapId.CavesOfBadDreams_M2, layerId: 2, tileX: 1251, tileY: 53, newTile: new MapTile(804, 3, true));
            TgxTileLayer.DefineTileFix(MapId.CavesOfBadDreams_M2, layerId: 2, tileX: 1252, tileY: 53, newTile: new MapTile(803, 3, true));
            TgxTileLayer.DefineTileFix(MapId.CavesOfBadDreams_M2, layerId: 2, tileX: 1253, tileY: 53, newTile: new MapTile(802, 3, true));
            TgxTileLayer.DefineTileFix(MapId.CavesOfBadDreams_M2, layerId: 2, tileX: 1254, tileY: 53, newTile: new MapTile(801, 3, true));
            TgxTileLayer.DefineTileFix(MapId.CavesOfBadDreams_M2, layerId: 2, tileX: 1255, tileY: 53, newTile: new MapTile(800, 3, true));
            TgxTileLayer.DefineTileFix(MapId.CavesOfBadDreams_M2, layerId: 2, tileX: 1256, tileY: 53, newTile: new MapTile(799, 3, true));
            TgxTileLayer.DefineTileFix(MapId.CavesOfBadDreams_M2, layerId: 2, tileX: 1257, tileY: 53, newTile: new MapTile(798, 3, true));
            TgxTileLayer.DefineTileFix(MapId.CavesOfBadDreams_M2, layerId: 2, tileX: 1258, tileY: 53, newTile: new MapTile(797, 3, true));
            TgxTileLayer.DefineTileFix(MapId.CavesOfBadDreams_M2, layerId: 2, tileX: 1259, tileY: 53, newTile: new MapTile(796, 3, true));
            TgxTileLayer.DefineTileFix(MapId.CavesOfBadDreams_M2, layerId: 2, tileX: 1251, tileY: 54, newTile: new MapTile(834, 3, true));
            TgxTileLayer.DefineTileFix(MapId.CavesOfBadDreams_M2, layerId: 2, tileX: 1252, tileY: 54, newTile: new MapTile(833, 3, true));
            TgxTileLayer.DefineTileFix(MapId.CavesOfBadDreams_M2, layerId: 2, tileX: 1253, tileY: 54, newTile: new MapTile(832, 3, true));
            TgxTileLayer.DefineTileFix(MapId.CavesOfBadDreams_M2, layerId: 2, tileX: 1254, tileY: 54, newTile: new MapTile(831, 3, true));
            TgxTileLayer.DefineTileFix(MapId.CavesOfBadDreams_M2, layerId: 2, tileX: 1255, tileY: 54, newTile: new MapTile(830, 3, true));
            TgxTileLayer.DefineTileFix(MapId.CavesOfBadDreams_M2, layerId: 2, tileX: 1256, tileY: 54, newTile: new MapTile(829, 3, true));
            TgxTileLayer.DefineTileFix(MapId.CavesOfBadDreams_M2, layerId: 2, tileX: 1257, tileY: 54, newTile: new MapTile(828, 3, true));
            TgxTileLayer.DefineTileFix(MapId.CavesOfBadDreams_M2, layerId: 2, tileX: 1258, tileY: 54, newTile: new MapTile(827, 3, true));
            TgxTileLayer.DefineTileFix(MapId.CavesOfBadDreams_M2, layerId: 2, tileX: 1259, tileY: 54, newTile: new MapTile(826, 3, true));
        }

        // Out-of-place tile with just a few pixels
        TgxTileLayer.DefineTileFix(MapId.CavesOfBadDreams_M2, layerId: 2, tileX: 264, tileY: 17, newTile: new MapTile(0, 0));
        TgxTileLayer.DefineTileFix(MapId.CavesOfBadDreams_M2, layerId: 2, tileX: 260, tileY: 52, newTile: new MapTile(0, 0));

        // Tree branches that don't extend all the way to the top of the screen
        DefineTreeBranchTileFix(MapId.MenhirHills_M1, layerId: 3, tileX: 8, offY: 1, height: 2, flipX: true);
        DefineTreeBranchTileFix(MapId.MenhirHills_M1, layerId: 3, tileX: 15, offY: 0, height: 5, flipX: false);
        DefineTreeBranchTileFix(MapId.MenhirHills_M1, layerId: 3, tileX: 38, offY: 0, height: 3, flipX: true);
        DefineTreeBranchTileFix(MapId.MenhirHills_M1, layerId: 3, tileX: 42, offY: 0, height: 3, flipX: false);
        DefineTreeBranchTileFix(MapId.MenhirHills_M1, layerId: 3, tileX: 65, offY: 1, height: 2, flipX: true);
        DefineTreeBranchTileFix(MapId.MenhirHills_M1, layerId: 3, tileX: 114, offY: 0, height: 3, flipX: true);
        DefineTreeBranchTileFix(MapId.MenhirHills_M1, layerId: 3, tileX: 124, offY: 1, height: 2, flipX: true);
        DefineTreeBranchTileFix(MapId.MenhirHills_M1, layerId: 3, tileX: 141, offY: 2, height: 4, flipX: false);
        DefineTreeBranchTileFix(MapId.MenhirHills_M1, layerId: 3, tileX: 174, offY: 1, height: 2, flipX: true);
        DefineTreeBranchTileFix(MapId.MenhirHills_M1, layerId: 3, tileX: 206, offY: 1, height: 2, flipX: false);
        DefineTreeBranchTileFix(MapId.MenhirHills_M1, layerId: 3, tileX: 230, offY: 1, height: 2, flipX: true);

        // The following branches were moved in the N-Gage version
        if (platform == Platform.GBA)
        {
            DefineTreeBranchTileFix(MapId.MenhirHills_M1, layerId: 3, tileX: 322, offY: 0, height: 6, flipX: false);
            DefineTreeBranchTileFix(MapId.MenhirHills_M1, layerId: 3, tileX: 369, offY: 1, height: 5, flipX: true);
            DefineTreeBranchTileFix(MapId.MenhirHills_M1, layerId: 3, tileX: 559, offY: 0, height: 3, flipX: true);
            DefineTreeBranchTileFix(MapId.MenhirHills_M1, layerId: 3, tileX: 571, offY: 2, height: 4, flipX: false);
            DefineTreeBranchTileFix(MapId.MenhirHills_M1, layerId: 3, tileX: 583, offY: 1, height: 2, flipX: false);
            DefineTreeBranchTileFix(MapId.MenhirHills_M1, layerId: 3, tileX: 676, offY: 1, height: 5, flipX: true);
            DefineTreeBranchTileFix(MapId.MenhirHills_M1, layerId: 3, tileX: 684, offY: 0, height: 3, flipX: false);
        }
        else
        {
            DefineTreeBranchTileFix(MapId.MenhirHills_M1, layerId: 3, tileX: 322, offY: 1, height: 2, flipX: false);
            DefineTreeBranchTileFix(MapId.MenhirHills_M1, layerId: 3, tileX: 369, offY: 3, height: 0, flipX: true);
            DefineTreeBranchTileFix(MapId.MenhirHills_M1, layerId: 3, tileX: 559, offY: 1, height: 2, flipX: true);
            DefineTreeBranchTileFix(MapId.MenhirHills_M1, layerId: 3, tileX: 571, offY: 0, height: 3, flipX: false);
            DefineTreeBranchTileFix(MapId.MenhirHills_M1, layerId: 3, tileX: 583, offY: 2, height: 1, flipX: false);
            DefineTreeBranchTileFix(MapId.MenhirHills_M1, layerId: 3, tileX: 675, offY: 1, height: 2, flipX: true);
            DefineTreeBranchTileFix(MapId.MenhirHills_M1, layerId: 3, tileX: 683, offY: 1, height: 2, flipX: false);
        }

        // The following fixes are unnecessary for the N-Gage version since it moves these leaves up to the top
        if (platform == Platform.GBA)
        {
            // Incomplete leaves
            DefineLeafTileFix(MapId.MenhirHills_M1, layerId: 3, tileX: 377, tileY: 4, offY: 0, height: 1, flipX: false);
            DefineLeafTileFix(MapId.MenhirHills_M1, layerId: 3, tileX: 382, tileY: 2, offY: 0, height: 3, flipX: false);
        }

        // Tree branches that don't extend all the way to the top of the screen
        DefineTreeBranchTileFix(MapId.MenhirHills_M2, layerId: 1, tileX: 176, offY: 1, height: 4, flipX: true);
        DefineTreeBranchTileFix(MapId.MenhirHills_M2, layerId: 1, tileX: 187, offY: 2, height: 4, flipX: false);
        DefineTreeBranchTileFix(MapId.MenhirHills_M2, layerId: 1, tileX: 279, offY: 0, height: 3, flipX: true);
        DefineTreeBranchTileFix(MapId.MenhirHills_M2, layerId: 1, tileX: 290, offY: 0, height: 3, flipX: false);
        DefineTreeBranchTileFix(MapId.MenhirHills_M2, layerId: 1, tileX: 1070, offY: 2, height: 1, flipX: false);

        // Incomplete hanging green grass
        DefineGreenHangingGrassTileFix(MapId.MenhirHills_M2, layerId: 1, tileX: 158, tileY: 3, offY: 0, height: 1, flipX: false);
        DefineGreenHangingGrassTileFix(MapId.MenhirHills_M2, layerId: 1, tileX: 195, tileY: 2, offY: 0, height: 2, flipX: false);
        DefineGreenHangingGrassTileFix(MapId.MenhirHills_M2, layerId: 1, tileX: 216, tileY: 3, offY: 0, height: 1, flipX: false);
        DefineGreenHangingGrassTileFix(MapId.MenhirHills_M2, layerId: 1, tileX: 261, tileY: 2, offY: 0, height: 1, flipX: false);
        DefineGreenHangingGrassTileFix(MapId.MenhirHills_M2, layerId: 1, tileX: 298, tileY: 1, offY: 0, height: 2, flipX: false);
        DefineGreenHangingGrassTileFix(MapId.MenhirHills_M2, layerId: 1, tileX: 319, tileY: 2, offY: 0, height: 1, flipX: false);
        DefineGreenHangingGrassTileFix(MapId.MenhirHills_M2, layerId: 1, tileX: 332, tileY: 2, offY: 0, height: 1, flipX: true);

        // Incomplete leaves
        DefineLeafTileFix(MapId.MenhirHills_M2, layerId: 3, tileX: 172, tileY: 1, offY: 0, height: 3, flipX: true);
        DefineLeafTileFix(MapId.MenhirHills_M2, layerId: 3, tileX: 201, tileY: 3, offY: 0, height: 1, flipX: false);
        DefineLeafTileFix(MapId.MenhirHills_M2, layerId: 3, tileX: 275, tileY: 0, offY: 0, height: 3, flipX: true);
        DefineLeafTileFix(MapId.MenhirHills_M2, layerId: 3, tileX: 304, tileY: 2, offY: 0, height: 1, flipX: false);

        // NOTE: Marshes of Awakening 2 is rather messy as it has leftover content from the prev. map, but
        //       it's hard to fix and not really noticeable in-game, so we skip it...

        // N-Gage exclusive level
        if (platform == Platform.NGage)
        {
            // Out-of-place leftover tiles and a missing tile for spikes
            TgxTileLayer.DefineTileFix(MapId.MarshAwakening2, layerId: 2, tileX: 18, tileY: 278, newTile: new MapTile(0, 0));
            TgxTileLayer.DefineTileFix(MapId.MarshAwakening2, layerId: 2, tileX: 19, tileY: 278, newTile: new MapTile(0, 0));
            TgxTileLayer.DefineTileFix(MapId.MarshAwakening2, layerId: 2, tileX: 19, tileY: 281, newTile: new MapTile(1189, 2));
            
            // Out-of-place leftover tiles
            TgxTileLayer.DefineTileFix(MapId.MarshAwakening2, layerId: 2, tileX: 16, tileY: 638, newTile: new MapTile(0, 0));
            TgxTileLayer.DefineTileFix(MapId.MarshAwakening2, layerId: 2, tileX: 17, tileY: 638, newTile: new MapTile(0, 0));
            TgxTileLayer.DefineTileFix(MapId.MarshAwakening2, layerId: 2, tileX: 17, tileY: 639, newTile: new MapTile(0, 0));
            TgxTileLayer.DefineTileFix(MapId.MarshAwakening2, layerId: 2, tileX: 19, tileY: 640, newTile: new MapTile(0, 0));
            TgxTileLayer.DefineTileFix(MapId.MarshAwakening2, layerId: 2, tileX: 20, tileY: 638, newTile: new MapTile(0, 0));
        }

        // Out-of-place tile with a yellow pixel
        TgxTileLayer.DefineTileFix(MapId.TheCanopy_M2, layerId: 3, tileX: 100, tileY: 153, newTile: new MapTile(0, 0));

        // Out-of-place tile with some yellow pixels
        TgxTileLayer.DefineTileFix(MapId.TheCanopy_M2, layerId: 2, tileX: 175, tileY: 210, newTile: new MapTile(0, 0));

        // The following fix is already present in the N-Gage version, so only add it for GBA
        if (platform == Platform.GBA)
        {
            // Out-of-place tile with a yellow pixel
            TgxTileLayer.DefineTileFix(MapId.SanctuaryOfRockAndLava_M3, layerId: 2, tileX: 198, tileY: 10, newTile: new MapTile(0, 0));
        }

        // Ground shifted downwards
        DefineRockGroundTileFix(MapId.IronMountains_M2, layerId: 1, tileX: 1065, tileY: 30);
        TgxTileLayer.DefineTileFix(MapId.IronMountains_M2, layerId: 1, tileX: 1073, tileY: 31, newTile: new MapTile(993, 3, true));
        TgxTileLayer.DefineTileFix(MapId.IronMountains_M2, layerId: 1, tileX: 1073, tileY: 32, newTile: new MapTile(1002, 3, true));
        TgxTileLayer.DefineTileFix(MapId.IronMountains_M2, layerId: 1, tileX: 1073, tileY: 33, newTile: new MapTile(1021, 3, true));
        TgxTileLayer.DefineTileFix(MapId.IronMountains_M2, layerId: 1, tileX: 1073, tileY: 34, newTile: new MapTile(1040, 3, true));
        TgxTileLayer.DefineTileFix(MapId.IronMountains_M2, layerId: 1, tileX: 1073, tileY: 35, newTile: new MapTile(1059, 3, true));
        TgxTileLayer.DefineTileFix(MapId.IronMountains_M2, layerId: 1, tileX: 1073, tileY: 36, newTile: new MapTile(1078, 3, true));
        TgxTileLayer.DefineTileFix(MapId.IronMountains_M2, layerId: 1, tileX: 1073, tileY: 37, newTile: new MapTile(0, 0));

        // The following fixes are already present in the N-Gage version, so only add them for GBA
        if (platform == Platform.GBA)
        {
            // Ground shifted downwards at the end
            DefineRockGroundTileFix(MapId.IronMountains_M2, layerId: 1, tileX: 1212, tileY: 20);

            // Leftover pot tiles at the end
            TgxTileLayer.DefineTileFix(MapId.IronMountains_M2, layerId: 1, tileX: 1212, tileY: 12, newTile: new MapTile(0, 0));
            TgxTileLayer.DefineTileFix(MapId.IronMountains_M2, layerId: 1, tileX: 1212, tileY: 13, newTile: new MapTile(0, 0));
            TgxTileLayer.DefineTileFix(MapId.IronMountains_M2, layerId: 1, tileX: 1212, tileY: 14, newTile: new MapTile(0, 0));
            TgxTileLayer.DefineTileFix(MapId.IronMountains_M2, layerId: 1, tileX: 1212, tileY: 15, newTile: new MapTile(0, 0));
            TgxTileLayer.DefineTileFix(MapId.IronMountains_M2, layerId: 1, tileX: 1213, tileY: 14, newTile: new MapTile(0, 0));
            TgxTileLayer.DefineTileFix(MapId.IronMountains_M2, layerId: 1, tileX: 1213, tileY: 15, newTile: new MapTile(0, 0));
        }

        // N-Gage exclusive level
        if (platform == Platform.NGage)
        {
            // Out-of-place tile with some gray pixels
            TgxTileLayer.DefineTileFix(MapId.MissileRace2, layerId: 3, tileX: 0, tileY: 96, newTile: new MapTile(0, 0));

            // The bottom of a lava tub has the wrong tiles (others do too, but only noticeable here)
            TgxTileLayer.DefineTileFix(MapId.MissileRace2, layerId: 1, tileX: 0, tileY: 200, newTile: new MapTile(0, 0));
            TgxTileLayer.DefineTileFix(MapId.MissileRace2, layerId: 1, tileX: 1, tileY: 200, newTile: new MapTile(0, 0));
            TgxTileLayer.DefineTileFix(MapId.MissileRace2, layerId: 1, tileX: 2, tileY: 200, newTile: new MapTile(1311, 4));
            TgxTileLayer.DefineTileFix(MapId.MissileRace2, layerId: 1, tileX: 3, tileY: 200, newTile: new MapTile(1312, 4));
            TgxTileLayer.DefineTileFix(MapId.MissileRace2, layerId: 1, tileX: 4, tileY: 200, newTile: new MapTile(1313, 4));
            TgxTileLayer.DefineTileFix(MapId.MissileRace2, layerId: 1, tileX: 5, tileY: 200, newTile: new MapTile(1315, 4));
        }

        // Incomplete warning sign
        TgxTileLayer.DefineTileFix(MapId.PirateShip_M1, layerId: 3, tileX: 193, tileY: 50, newTile: new MapTile(1894, 6));
        TgxTileLayer.DefineTileFix(MapId.PirateShip_M1, layerId: 3, tileX: 194, tileY: 50, newTile: new MapTile(1895, 6));
        TgxTileLayer.DefineTileFix(MapId.PirateShip_M1, layerId: 3, tileX: 195, tileY: 50, newTile: new MapTile(1896, 6));
        TgxTileLayer.DefineTileFix(MapId.PirateShip_M1, layerId: 3, tileX: 199, tileY: 50, newTile: new MapTile(1900, 6));
        TgxTileLayer.DefineTileFix(MapId.PirateShip_M1, layerId: 3, tileX: 193, tileY: 51, newTile: new MapTile(1901, 6));
        TgxTileLayer.DefineTileFix(MapId.PirateShip_M1, layerId: 3, tileX: 194, tileY: 51, newTile: new MapTile(1902, 6));
        TgxTileLayer.DefineTileFix(MapId.PirateShip_M1, layerId: 3, tileX: 199, tileY: 51, newTile: new MapTile(1903, 6));

        // Out-of-place fence tiles
        TgxTileLayer.DefineTileFix(MapId.PirateShip_M1, layerId: 1, tileX: 296, tileY: 64, newTile: new MapTile(0, 0));
        TgxTileLayer.DefineTileFix(MapId.PirateShip_M1, layerId: 1, tileX: 296, tileY: 65, newTile: new MapTile(0, 0));
        TgxTileLayer.DefineTileFix(MapId.PirateShip_M1, layerId: 1, tileX: 296, tileY: 66, newTile: new MapTile(0, 0));

        // Out-of-place tiles with a pink pixel
        TgxTileLayer.DefineTileFix(MapId.World1, layerId: 2, tileX: 115, tileY: 52, newTile: new MapTile(0, 0));
        TgxTileLayer.DefineTileFix(MapId.World1, layerId: 2, tileX: 123, tileY: 17, newTile: new MapTile(0, 0));

        // Out-of-place tiles with a brown pixel
        TgxTileLayer.DefineTileFix(MapId.World4, layerId: 1, tileX: 170, tileY: 34, newTile: new MapTile(0, 0));
        TgxTileLayer.DefineTileFix(MapId.World4, layerId: 1, tileX: 193, tileY: 49, newTile: new MapTile(0, 0));

        // N-Gage exclusive level
        if (platform == Platform.NGage)
        {
            // Out-of-place tile with a purple pixel
            TgxTileLayer.DefineTileFix(MapId.NGageMulti_CaptureTheFlagTeamWork, layerId: 1, tileX: 95, tileY: 3, newTile: new MapTile(0, 0));
        }

        // N-Gage exclusive level
        if (platform == Platform.NGage)
        {
            // Out-of-place tiles with a pink pixel
            TgxTileLayer.DefineTileFix(MapId.NGageMulti_CaptureTheFlagTeamPlayer, layerId: 2, tileX: 23, tileY: 6, newTile: new MapTile(0, 0));
            TgxTileLayer.DefineTileFix(MapId.NGageMulti_CaptureTheFlagTeamPlayer, layerId: 2, tileX: 138, tileY: 6, newTile: new MapTile(0, 0));
        }

        // GBA exclusive levels
        if (platform == Platform.GBA)
        {
            // Out-of-place tile with a red pixel
            TgxTileLayer.DefineTileFix(MapId.GameCube_Bonus3, layerId: 3, tileX: 141, tileY: 31, newTile: new MapTile(0, 0));

            // Missing and incorrect tiles for the right side of spikes
            TgxTileLayer.DefineTileFix(MapId.GameCube_Bonus4, layerId: 3, tileX: 38, tileY: 788, newTile: new MapTile(1017, 2, true));
            TgxTileLayer.DefineTileFix(MapId.GameCube_Bonus4, layerId: 3, tileX: 38, tileY: 789, newTile: new MapTile(1041, 2, true));
            TgxTileLayer.DefineTileFix(MapId.GameCube_Bonus4, layerId: 3, tileX: 38, tileY: 790, newTile: new MapTile(1071, 2, true));
            TgxTileLayer.DefineTileFix(MapId.GameCube_Bonus4, layerId: 3, tileX: 38, tileY: 791, newTile: new MapTile(1101, 2, true));
            TgxTileLayer.DefineTileFix(MapId.GameCube_Bonus4, layerId: 3, tileX: 38, tileY: 792, newTile: new MapTile(1131, 2, true));
            TgxTileLayer.DefineTileFix(MapId.GameCube_Bonus4, layerId: 3, tileX: 38, tileY: 793, newTile: new MapTile(1159, 2, true));
            TgxTileLayer.DefineTileFix(MapId.GameCube_Bonus4, layerId: 3, tileX: 38, tileY: 794, newTile: new MapTile(1186, 2, true));

            // Out-of-place tile with an orange pixel
            TgxTileLayer.DefineTileFix(MapId.GameCube_Bonus4, layerId: 3, tileX: 34, tileY: 536, newTile: new MapTile(0, 0));

            // Out-of-place tiles with purple pixels
            TgxTileLayer.DefineTileFix(MapId.GameCube_Bonus7, layerId: 2, tileX: 689, tileY: 19, newTile: new MapTile(0, 0));
            TgxTileLayer.DefineTileFix(MapId.GameCube_Bonus7, layerId: 2, tileX: 689, tileY: 27, newTile: new MapTile(0, 0));
            TgxTileLayer.DefineTileFix(MapId.GameCube_Bonus7, layerId: 2, tileX: 858, tileY: 19, newTile: new MapTile(0, 0));
            TgxTileLayer.DefineTileFix(MapId.GameCube_Bonus7, layerId: 2, tileX: 858, tileY: 27, newTile: new MapTile(0, 0));
        }
    }

    private static void DefineTreeBranchTileFix(MapId sceneId, int layerId, int tileX, int offY, int height, bool flipX)
    {
        DefineRepeatingTileShapeTileFix(sceneId, layerId, tileX, 0, offY, height, flipX,
        [
            [1575, 1576, 1577, 1578],
            [1607, 1608, 1609, 1610],
            [1648, 1649, 1650, 1651],
        ], 6);
    }

    private static void DefineLeafTileFix(MapId sceneId, int layerId, int tileX, int tileY, int offY, int height, bool flipX)
    {
        DefineRepeatingTileShapeTileFix(sceneId, layerId, tileX, tileY, offY, height, flipX,
        [
            [1579, 1580, 1581, 0],
            [1611, 1612, 1613, 0],
            [1652, 1653, 1654, 1655],
        ], 6);
    }

    private static void DefineGreenHangingGrassTileFix(MapId sceneId, int layerId, int tileX, int tileY, int offY, int height, bool flipX)
    {
        DefineRepeatingTileShapeTileFix(sceneId, layerId, tileX, tileY, offY, height, flipX,
        [
            [98, 99],
            [118, 119],
        ], 0);
    }

    private static void DefineRockGroundTileFix(MapId sceneId, int layerId, int tileX, int tileY)
    {
        TgxTileLayer.DefineTileFix(sceneId, layerId, tileX + 0, tileY + 0, newTile: new MapTile(1195, 3));
        TgxTileLayer.DefineTileFix(sceneId, layerId, tileX + 1, tileY + 0, newTile: new MapTile(1210, 3));
        TgxTileLayer.DefineTileFix(sceneId, layerId, tileX + 2, tileY + 0, newTile: new MapTile(1211, 3));
        TgxTileLayer.DefineTileFix(sceneId, layerId, tileX + 3, tileY + 0, newTile: new MapTile(1346, 3));
        TgxTileLayer.DefineTileFix(sceneId, layerId, tileX + 4, tileY + 0, newTile: new MapTile(1347, 3));
        TgxTileLayer.DefineTileFix(sceneId, layerId, tileX + 5, tileY + 0, newTile: new MapTile(986, 3, true));
        TgxTileLayer.DefineTileFix(sceneId, layerId, tileX + 6, tileY + 0, newTile: new MapTile(985, 3, true));
        TgxTileLayer.DefineTileFix(sceneId, layerId, tileX + 7, tileY + 0, newTile: new MapTile(984, 3, true));

        TgxTileLayer.DefineTileFix(sceneId, layerId, tileX + 0, tileY + 1, newTile: new MapTile(1220, 3));
        TgxTileLayer.DefineTileFix(sceneId, layerId, tileX + 1, tileY + 1, newTile: new MapTile(1221, 3));
        TgxTileLayer.DefineTileFix(sceneId, layerId, tileX + 2, tileY + 1, newTile: new MapTile(1222, 3));
        TgxTileLayer.DefineTileFix(sceneId, layerId, tileX + 3, tileY + 1, newTile: new MapTile(1355, 3));
        TgxTileLayer.DefineTileFix(sceneId, layerId, tileX + 4, tileY + 1, newTile: new MapTile(1356, 3));
        TgxTileLayer.DefineTileFix(sceneId, layerId, tileX + 5, tileY + 1, newTile: new MapTile(996, 3, true));
        TgxTileLayer.DefineTileFix(sceneId, layerId, tileX + 6, tileY + 1, newTile: new MapTile(995, 3, true));
        TgxTileLayer.DefineTileFix(sceneId, layerId, tileX + 7, tileY + 1, newTile: new MapTile(994, 3, true));

        TgxTileLayer.DefineTileFix(sceneId, layerId, tileX + 0, tileY + 2, newTile: new MapTile(1242, 3));
        TgxTileLayer.DefineTileFix(sceneId, layerId, tileX + 1, tileY + 2, newTile: new MapTile(1243, 3));
        TgxTileLayer.DefineTileFix(sceneId, layerId, tileX + 2, tileY + 2, newTile: new MapTile(1244, 3));
        TgxTileLayer.DefineTileFix(sceneId, layerId, tileX + 3, tileY + 2, newTile: new MapTile(1362, 3));
        TgxTileLayer.DefineTileFix(sceneId, layerId, tileX + 4, tileY + 2, newTile: new MapTile(1363, 3));
        TgxTileLayer.DefineTileFix(sceneId, layerId, tileX + 5, tileY + 2, newTile: new MapTile(1005, 3, true));
        TgxTileLayer.DefineTileFix(sceneId, layerId, tileX + 6, tileY + 2, newTile: new MapTile(1004, 3, true));
        TgxTileLayer.DefineTileFix(sceneId, layerId, tileX + 7, tileY + 2, newTile: new MapTile(1003, 3, true));

        TgxTileLayer.DefineTileFix(sceneId, layerId, tileX + 0, tileY + 3, newTile: new MapTile(0, 0));
        TgxTileLayer.DefineTileFix(sceneId, layerId, tileX + 1, tileY + 3, newTile: new MapTile(1263, 3));
        TgxTileLayer.DefineTileFix(sceneId, layerId, tileX + 2, tileY + 3, newTile: new MapTile(1264, 3));
        TgxTileLayer.DefineTileFix(sceneId, layerId, tileX + 3, tileY + 3, newTile: new MapTile(1367, 3));
        TgxTileLayer.DefineTileFix(sceneId, layerId, tileX + 4, tileY + 3, newTile: new MapTile(1368, 3));
        TgxTileLayer.DefineTileFix(sceneId, layerId, tileX + 5, tileY + 3, newTile: new MapTile(1366, 3));
        TgxTileLayer.DefineTileFix(sceneId, layerId, tileX + 6, tileY + 3, newTile: new MapTile(1367, 3));
        TgxTileLayer.DefineTileFix(sceneId, layerId, tileX + 7, tileY + 3, newTile: new MapTile(1368, 3));

        TgxTileLayer.DefineTileFix(sceneId, layerId, tileX + 0, tileY + 4, newTile: new MapTile(0, 0));
        TgxTileLayer.DefineTileFix(sceneId, layerId, tileX + 1, tileY + 4, newTile: new MapTile(1284, 3));
        TgxTileLayer.DefineTileFix(sceneId, layerId, tileX + 2, tileY + 4, newTile: new MapTile(1285, 3));
        TgxTileLayer.DefineTileFix(sceneId, layerId, tileX + 3, tileY + 4, newTile: new MapTile(1369, 3));
        TgxTileLayer.DefineTileFix(sceneId, layerId, tileX + 4, tileY + 4, newTile: new MapTile(1370, 3));
        TgxTileLayer.DefineTileFix(sceneId, layerId, tileX + 5, tileY + 4, newTile: new MapTile(1371, 3));
        TgxTileLayer.DefineTileFix(sceneId, layerId, tileX + 6, tileY + 4, newTile: new MapTile(1372, 3));
        TgxTileLayer.DefineTileFix(sceneId, layerId, tileX + 7, tileY + 4, newTile: new MapTile(1373, 3));

        TgxTileLayer.DefineTileFix(sceneId, layerId, tileX + 0, tileY + 5, newTile: new MapTile(0, 0));
        TgxTileLayer.DefineTileFix(sceneId, layerId, tileX + 1, tileY + 5, newTile: new MapTile(1309, 3));
        TgxTileLayer.DefineTileFix(sceneId, layerId, tileX + 2, tileY + 5, newTile: new MapTile(1310, 3));
        TgxTileLayer.DefineTileFix(sceneId, layerId, tileX + 3, tileY + 5, newTile: new MapTile(1374, 3));
        TgxTileLayer.DefineTileFix(sceneId, layerId, tileX + 4, tileY + 5, newTile: new MapTile(1375, 3));
        TgxTileLayer.DefineTileFix(sceneId, layerId, tileX + 5, tileY + 5, newTile: new MapTile(1376, 3));
        TgxTileLayer.DefineTileFix(sceneId, layerId, tileX + 6, tileY + 5, newTile: new MapTile(1377, 3));
        TgxTileLayer.DefineTileFix(sceneId, layerId, tileX + 7, tileY + 5, newTile: new MapTile(1378, 3));

        TgxTileLayer.DefineTileFix(sceneId, layerId, tileX + 0, tileY + 6, newTile: new MapTile(0, 0));
        TgxTileLayer.DefineTileFix(sceneId, layerId, tileX + 1, tileY + 6, newTile: new MapTile(0, 0));
        TgxTileLayer.DefineTileFix(sceneId, layerId, tileX + 2, tileY + 6, newTile: new MapTile(1332, 3));
        TgxTileLayer.DefineTileFix(sceneId, layerId, tileX + 3, tileY + 6, newTile: new MapTile(1390, 3));
        TgxTileLayer.DefineTileFix(sceneId, layerId, tileX + 4, tileY + 6, newTile: new MapTile(1391, 3));
        TgxTileLayer.DefineTileFix(sceneId, layerId, tileX + 5, tileY + 6, newTile: new MapTile(1392, 3));
        TgxTileLayer.DefineTileFix(sceneId, layerId, tileX + 6, tileY + 6, newTile: new MapTile(1393, 3));
        TgxTileLayer.DefineTileFix(sceneId, layerId, tileX + 7, tileY + 6, newTile: new MapTile(1394, 3));

        TgxTileLayer.DefineTileFix(sceneId, layerId, tileX + 0, tileY + 7, newTile: new MapTile(0, 0));
        TgxTileLayer.DefineTileFix(sceneId, layerId, tileX + 1, tileY + 7, newTile: new MapTile(0, 0));
        TgxTileLayer.DefineTileFix(sceneId, layerId, tileX + 2, tileY + 7, newTile: new MapTile(0, 0));
        TgxTileLayer.DefineTileFix(sceneId, layerId, tileX + 3, tileY + 7, newTile: new MapTile(0, 0));
        TgxTileLayer.DefineTileFix(sceneId, layerId, tileX + 4, tileY + 7, newTile: new MapTile(0, 0));
        TgxTileLayer.DefineTileFix(sceneId, layerId, tileX + 5, tileY + 7, newTile: new MapTile(0, 0));
        TgxTileLayer.DefineTileFix(sceneId, layerId, tileX + 6, tileY + 7, newTile: new MapTile(0, 0));
        TgxTileLayer.DefineTileFix(sceneId, layerId, tileX + 7, tileY + 7, newTile: new MapTile(0, 0));
    }

    // Helper for repeating tile shapes
    private static void DefineRepeatingTileShapeTileFix(
        MapId sceneId,
        int layerId,
        int tileX, int tileY,
        int offY,
        int height,
        bool flipX,
        int[][] rows,
        byte palette)
    {
        for (int y = 0; y < height; y++)
        {
            int[] row = rows[MathHelpers.Mod(y + offY, rows.Length)];
            int width = row.Length;
            for (int x = 0; x < width; x++)
                TgxTileLayer.DefineTileFix(sceneId, layerId, tileX + x, tileY + y, newTile: new MapTile(row[flipX ? width - x - 1 : x], palette, flipX));
        }
    }
}