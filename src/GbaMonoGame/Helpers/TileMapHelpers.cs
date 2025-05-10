using BinarySerializer.Nintendo.GBA;
using Microsoft.Xna.Framework;

namespace GbaMonoGame;

public static class TileMapHelpers
{
    public static void CopyRegion(
        MapTile[] sourceMap, 
        int sourceWidth, 
        Point sourcePoint, 
        MapTile[] destMap, 
        int destWidth, 
        Point destPoint, 
        Point regionSize)
    {
        for (int y = 0; y < regionSize.Y; y++)
        {
            for (int x = 0; x < regionSize.X; x++)
            {
                int sourceX = sourcePoint.X + x;
                int sourceY = sourcePoint.Y + y;
                MapTile sourceTile = sourceMap[sourceX + sourceY * sourceWidth];

                int destX = destPoint.X + x;
                int destY = destPoint.Y + y;
                destMap[destX + destY * destWidth] = sourceTile;
            }
        }
    }
}