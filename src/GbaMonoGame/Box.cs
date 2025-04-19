using BinarySerializer.Ubisoft.GbaEngine;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace GbaMonoGame;

public struct Box
{
    public Box(float minX, float minY, float maxX, float maxY)
    {
        MinX = minX;
        MinY = minY;
        MaxX = maxX;
        MaxY = maxY;
    }

    public Box(Vector2 position, Vector2 size)
    {
        MinX = position.X;
        MinY = position.Y;
        MaxX = position.X + size.X;
        MaxY = position.Y + size.Y;
    }

    public Box(EngineBox engineBox)
    {
        MinX = engineBox.MinX;
        MinY = engineBox.MinY;
        MaxX = engineBox.MaxX;
        MaxY = engineBox.MaxY;
    }

    public Box(ChannelBox channelBox)
    {
        MinX = channelBox.MinX;
        MinY = channelBox.MinY;
        MaxX = channelBox.MaxX;
        MaxY = channelBox.MaxY;
    }

    public static Box Empty { get; } = new(0, 0, 0, 0);

    public float MinX;
    public float MinY;
    public float MaxX;
    public float MaxY;

    [JsonIgnore]
    public float Width => MaxX - MinX;
    [JsonIgnore]
    public float Height => MaxY - MinY;

    [JsonIgnore]
    public float CenterX => Width / 2 + MinX;
    [JsonIgnore]
    public float CenterY => Height / 2 + MinY;

    [JsonIgnore]
    public Vector2 TopLeft => new(MinX, MinY);
    [JsonIgnore]
    public Vector2 TopCenter => new(CenterX, MinY);
    [JsonIgnore]
    public Vector2 TopRight => new(MaxX, MinY);

    [JsonIgnore]
    public Vector2 MiddleLeft => new(MinX, CenterY);
    [JsonIgnore]
    public Vector2 Center => new(CenterX, CenterY);
    [JsonIgnore]
    public Vector2 MiddleRight => new(MaxX, CenterY);

    [JsonIgnore]
    public Vector2 BottomLeft => new(MinX, MaxY);
    [JsonIgnore]
    public Vector2 BottomCenter => new(CenterX, MaxY);
    [JsonIgnore]
    public Vector2 BottomRight => new(MaxX, MaxY);

    [JsonIgnore]
    public Vector2 Position => new(MinX, MinY);
    [JsonIgnore]
    public Vector2 Size => new(Width, Height);

    public static Box Offset(Box box, Vector2 offset)
    {
        box.MinX += offset.X;
        box.MinY += offset.Y;
        box.MaxX += offset.X;
        box.MaxY += offset.Y;
        return box;
    }

    public static Box FlipX(Box box)
    {
        (box.MinX, box.MaxX) = (-box.MaxX, -box.MinX);
        return box;
    }

    public static Box FlipY(Box box)
    {
        (box.MinY, box.MaxY) = (-box.MaxY, -box.MinY);
        return box;
    }

    public static Box Intersect(Box box1, Box box2)
    {
        float largestXMin = box2.MinX;
        if (largestXMin < box1.MinX)
            largestXMin = box1.MinX;

        float largestYMin = box2.MinY;
        if (largestYMin < box1.MinY)
            largestYMin = box1.MinY;

        float smallestXMax = box2.MaxX;
        if (box1.MaxX < smallestXMax)
            smallestXMax = box1.MaxX;

        float smallestYMax = box2.MaxY;
        if (box1.MaxY < smallestYMax)
            smallestYMax = box1.MaxY;

        if (largestXMin < smallestXMax && largestYMin < smallestYMax)
            return new Box(largestXMin, largestYMin, smallestXMax, smallestYMax);
        else
            return Empty;
    }

    public static bool operator ==(Box a, Box b)
    {
        return a.MinX == b.MinX && a.MinY == b.MinY && a.MaxX == b.MaxX && a.MaxY == b.MaxY;
    }

    public static bool operator !=(Box a, Box b)
    {
        return !(a == b);
    }

    public override bool Equals(object obj)
    {
        if (obj is Box box)
            return this == box;

        return false;
    }

    public bool Equals(Box other)
    {
        return this == other;
    }

    public override int GetHashCode()
    {
        return (((17 * 23 + MinX.GetHashCode()) * 23 + MinY.GetHashCode()) * 23 + MaxX.GetHashCode()) * 23 + MaxY.GetHashCode();
    }

    public bool Intersects(Box otherBox)
    {
        float largestXMin = otherBox.MinX;
        if (largestXMin < MinX)
            largestXMin = MinX;

        float largestYMin = otherBox.MinY;
        if (largestYMin < MinY)
            largestYMin = MinY;

        float smallestXMax = otherBox.MaxX;
        if (MaxX < smallestXMax)
            smallestXMax = MaxX;

        float smallestYMax = otherBox.MaxY;
        if (MaxY < smallestYMax)
            smallestYMax = MaxY;

        return largestXMin < smallestXMax && largestYMin < smallestYMax;
    }

    public bool Contains(Vector2 position) => MinX <= position.X && position.X < MaxX && MinY <= position.Y && position.Y < MaxY;

    public Rectangle ToRectangle() => new((int)MinX, (int)MinY, (int)Width, (int)Height);
}