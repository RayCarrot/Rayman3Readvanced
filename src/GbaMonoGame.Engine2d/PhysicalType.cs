using System;

namespace GbaMonoGame.Engine2d;

public readonly struct PhysicalType
{
    public PhysicalType(PhysicalTypeValue type)
    {
        ValueByte = (byte)type;
        Value = type;
    }
    public PhysicalType(byte value)
    {
        ValueByte = value;
        Value = (PhysicalTypeValue)value;
    }

    public byte ValueByte { get; }
    public PhysicalTypeValue Value { get; }

    public bool IsFullySolid => ValueByte < 16;
    public bool IsAngledSolid => ValueByte is >= 16 and < 32;
    public bool IsSolid => ValueByte < 32;

    public float GetBlockTopSolid(float xPos)
    {
        // NOTE: Set comment below about flooring floats. This is added to fix Rayman's sloped collision
        //       when using the below fix.
        xPos = MathF.Floor(xPos);

        float subTileX = MathHelpers.Mod(xPos, Tile.Size);

        // In the game this is done using pre-calculated arrays and rounded down to nearest integer
        float topSolid = Value switch
        {
            PhysicalTypeValue.SolidAngle90Left => subTileX,
            PhysicalTypeValue.SolidAngle90Right => Tile.Size - subTileX - 1,

            PhysicalTypeValue.SolidAngle30Left1 => subTileX / 2,
            PhysicalTypeValue.SlideAngle30Left1 => subTileX / 2,
            PhysicalTypeValue.SolidAngle30Right1 => Tile.Size - subTileX / 2 - 0.5f,
            PhysicalTypeValue.SlideAngle30Right1 => Tile.Size - subTileX / 2 - 0.5f,
            
            PhysicalTypeValue.SolidAngle30Left2 => subTileX / 2 + Tile.Size / 2f,
            PhysicalTypeValue.SlideAngle30Left2 => subTileX / 2 + Tile.Size / 2f,
            PhysicalTypeValue.SolidAngle30Right2 => Tile.Size - (subTileX / 2 + Tile.Size / 2f) - 0.5f,
            PhysicalTypeValue.SlideAngle30Right2 => Tile.Size - (subTileX / 2 + Tile.Size / 2f) - 0.5f,
            _ => throw new Exception($"The physical value {this} is not an angled block")
        };

        // NOTE: Generally we don't want to floor floats so that we can keep sub-pixel positions, however
        //       in this case it causes issues when actors move down on sloped ground as they don't move
        //       down with it fast enough to keep up with the slope, and thus can get de-synced. This is
        //       an issue with the Slapdash enemy in Hoodlum Hideout 2 when charging down the slopes. It
        //       also causes an issue in Mega Havoc 4 where the boulders get stuck on the slopes.
        topSolid = MathF.Floor(topSolid);

        return topSolid;
    }

    public bool IsBlockPointSolid(Vector2 position)
    {
        float subTileY = MathHelpers.Mod(position.Y, Tile.Size);
        float solidHeight = GetBlockTopSolid(position.X);

        // In the game this is done using a pre-calculated table, but since we want float-precision we calculate it dynamically
        return subTileY >= Tile.Size - solidHeight;
    }

    public static implicit operator byte(PhysicalType type) => type.ValueByte;
    public static implicit operator PhysicalTypeValue(PhysicalType type) => type.Value;
    public static implicit operator PhysicalType(PhysicalTypeValue type) => new(type);

    public override string ToString() => Value.ToString();
}