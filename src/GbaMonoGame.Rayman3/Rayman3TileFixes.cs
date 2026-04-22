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