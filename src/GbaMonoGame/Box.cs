using BinarySerializer.Ubisoft.GbaEngine;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace GbaMonoGame;

public struct Box
{
    public Box(float left, float top, float right, float bottom)
    {
        Left = left;
        Top = top;
        Right = right;
        Bottom = bottom;
    }

    public Box(Vector2 position, Vector2 size)
    {
        Left = position.X;
        Top = position.Y;
        Right = position.X + size.X;
        Bottom = position.Y + size.Y;
    }

    public Box(EngineBox engineBox)
    {
        Left = engineBox.Left;
        Top = engineBox.Top;
        Right = engineBox.Right;
        Bottom = engineBox.Bottom;
    }

    public Box(ChannelBox channelBox)
    {
        Left = channelBox.Left;
        Top = channelBox.Top;
        Right = channelBox.Right;
        Bottom = channelBox.Bottom;
    }

    public static Box Empty { get; } = new(0, 0, 0, 0);

    public float Left;
    public float Top;
    public float Right;
    public float Bottom;

    [JsonIgnore]
    public float Width => Right - Left;
    [JsonIgnore]
    public float Height => Bottom - Top;

    [JsonIgnore]
    public float CenterX => Width / 2 + Left;
    [JsonIgnore]
    public float CenterY => Height / 2 + Top;

    [JsonIgnore]
    public Vector2 TopLeft => new(Left, Top);
    [JsonIgnore]
    public Vector2 TopCenter => new(CenterX, Top);
    [JsonIgnore]
    public Vector2 TopRight => new(Right, Top);

    [JsonIgnore]
    public Vector2 MiddleLeft => new(Left, CenterY);
    [JsonIgnore]
    public Vector2 Center => new(CenterX, CenterY);
    [JsonIgnore]
    public Vector2 MiddleRight => new(Right, CenterY);

    [JsonIgnore]
    public Vector2 BottomLeft => new(Left, Bottom);
    [JsonIgnore]
    public Vector2 BottomCenter => new(CenterX, Bottom);
    [JsonIgnore]
    public Vector2 BottomRight => new(Right, Bottom);

    [JsonIgnore]
    public Vector2 Position => new(Left, Top);
    [JsonIgnore]
    public Vector2 Size => new(Width, Height);

    public static Box Offset(Box box, Vector2 offset)
    {
        box.Left += offset.X;
        box.Top += offset.Y;
        box.Right += offset.X;
        box.Bottom += offset.Y;
        return box;
    }

    public static Box FlipX(Box box)
    {
        (box.Left, box.Right) = (-box.Right, -box.Left);
        return box;
    }

    public static Box FlipY(Box box)
    {
        (box.Top, box.Bottom) = (-box.Bottom, -box.Top);
        return box;
    }

    public static Box Intersect(Box box1, Box box2)
    {
        float maxLeft = box2.Left;
        if (maxLeft < box1.Left)
            maxLeft = box1.Left;

        float maxTop = box2.Top;
        if (maxTop < box1.Top)
            maxTop = box1.Top;

        float minRight = box2.Right;
        if (box1.Right < minRight)
            minRight = box1.Right;

        float minBottom = box2.Bottom;
        if (box1.Bottom < minBottom)
            minBottom = box1.Bottom;

        if (maxLeft < minRight && maxTop < minBottom)
            return new Box(maxLeft, maxTop, minRight, minBottom);
        else
            return Empty;
    }

    public static bool operator ==(Box a, Box b)
    {
        return a.Left == b.Left && a.Top == b.Top && a.Right == b.Right && a.Bottom == b.Bottom;
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
        return (((17 * 23 + Left.GetHashCode()) * 23 + Top.GetHashCode()) * 23 + Right.GetHashCode()) * 23 + Bottom.GetHashCode();
    }

    public bool Intersects(Box otherBox)
    {
        float largestXMin = otherBox.Left;
        if (largestXMin < Left)
            largestXMin = Left;

        float largestYMin = otherBox.Top;
        if (largestYMin < Top)
            largestYMin = Top;

        float smallestXMax = otherBox.Right;
        if (Right < smallestXMax)
            smallestXMax = Right;

        float smallestYMax = otherBox.Bottom;
        if (Bottom < smallestYMax)
            smallestYMax = Bottom;

        return largestXMin < smallestXMax && largestYMin < smallestYMax;
    }

    public bool Contains(Vector2 position) => Left <= position.X && position.X < Right && Top <= position.Y && position.Y < Bottom;

    public Rectangle ToRectangle() => new((int)Left, (int)Top, (int)Width, (int)Height);
}