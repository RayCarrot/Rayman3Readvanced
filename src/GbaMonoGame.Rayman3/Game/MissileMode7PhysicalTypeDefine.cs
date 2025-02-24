namespace GbaMonoGame.Engine2d;

public class MissileMode7PhysicalTypeDefine
{
    public static MissileMode7PhysicalTypeDefine[] Defines { get; } =
    [
        new(), // #0
        new() { Solid = true, }, // #1
        new() { Solid = true, BumperLeft = true, }, // #2
        new() { Solid = true, }, // #3
        new() { Solid = true, }, // #4
        new() { Solid = true, }, // #5
        new() { Solid = true, }, // #6
        new() { Solid = true, }, // #7
        new() { Solid = true, }, // #8
        new() { Solid = true, }, // #9
        new() { Solid = true, }, // #10
        new() { Solid = true, }, // #11
        new() { Solid = true, }, // #12
        new() { Solid = true, }, // #13
        new() { Solid = true, }, // #14
        new() { Solid = true, }, // #15
        new() { Solid = true, }, // #16
        new() { Solid = true, }, // #17
        new() { Solid = true, }, // #18
        new() { Solid = true, }, // #19
        new() { Solid = true, }, // #20
        new() { Solid = true, }, // #21
        new() { Solid = true, }, // #22
        new() { Solid = true, }, // #23
        new() { Solid = true, }, // #24
        new() { Solid = true, }, // #25
        new() { Solid = true, }, // #26
        new() { Solid = true, }, // #27
        new() { Solid = true, }, // #28
        new() { Solid = true, }, // #29
        new() { Solid = true, }, // #30
        new() { Solid = true, }, // #31
        new() { Solid = true, }, // #32
        new(), // #33
        new() { Damage = true, }, // #34
        new() { Damage = true, Directional = true, Direction = TypeDirection.Left, }, // #35
        new() { Damage = true, Directional = true, Direction = TypeDirection.Right, }, // #36
        new() { Damage = true, Directional = true, Direction = TypeDirection.Up, }, // #37
        new() { Damage = true, Directional = true, Direction = TypeDirection.Down, }, // #38
        new() { Directional = true, Direction = TypeDirection.Left, }, // #39
        new() { Directional = true, Direction = TypeDirection.Right, }, // #40
        new() { Directional = true, Direction = TypeDirection.Up, }, // #41
        new() { Directional = true, Direction = TypeDirection.Down, }, // #42
        new() { Directional = true, Direction = TypeDirection.DownLeft, }, // #43
        new() { Directional = true, Direction = TypeDirection.DownRight, }, // #44
        new() { Directional = true, Direction = TypeDirection.UpRight, }, // #45
        new() { Directional = true, Direction = TypeDirection.UpLeft, }, // #46
        new() { Bounce = true, }, // #47
        new() { BumperRight = true, }, // #48
        new() { BumperLeft = true, }, // #49
        new() { Damage = true, RaceEnd = true, }, // #50
        new() { RaceEnd = true, }, // #51
        new() { BumperRight = true, Directional = true, Direction = TypeDirection.Left, }, // #52
        new() { BumperRight = true, Directional = true, Direction = TypeDirection.Right, }, // #53
        new() { BumperRight = true, Directional = true, Direction = TypeDirection.Up, }, // #54
        new() { BumperRight = true, Directional = true, Direction = TypeDirection.Down, }, // #55
        new() { BumperRight = true, Directional = true, Direction = TypeDirection.UpLeft, }, // #56
        new() { BumperRight = true, Directional = true, Direction = TypeDirection.DownLeft, }, // #57
        new() { BumperRight = true, Directional = true, Direction = TypeDirection.DownRight, }, // #58
        new() { BumperRight = true, Directional = true, Direction = TypeDirection.UpRight, }, // #59
        new() { Damage = true, Directional = true, Direction = TypeDirection.UpLeft, }, // #60
        new() { Damage = true, Directional = true, Direction = TypeDirection.DownLeft, }, // #61
        new() { Damage = true, Directional = true, Direction = TypeDirection.DownRight, }, // #62
        new() { Damage = true, Directional = true, Direction = TypeDirection.UpRight, }, // #63
        new() { BumperLeft = true, Directional = true, Direction = TypeDirection.Up, }, // #64
        new() { BumperLeft = true, Directional = true, Direction = TypeDirection.Down, }, // #65
        new() { BumperLeft = true, Directional = true, Direction = TypeDirection.Left, }, // #66
        new() { BumperLeft = true, Directional = true, Direction = TypeDirection.Right, }, // #67
        new() { BumperLeft = true, Directional = true, Direction = TypeDirection.UpLeft, }, // #68
        new() { BumperLeft = true, Directional = true, Direction = TypeDirection.UpRight, }, // #69
        new() { BumperLeft = true, Directional = true, Direction = TypeDirection.DownLeft, }, // #70
        new() { BumperLeft = true, Directional = true, Direction = TypeDirection.DownRight, }, // #71
    ];

    public static MissileMode7PhysicalTypeDefine Empty => Defines[0];
    public static MissileMode7PhysicalTypeDefine FromPhysicalType(PhysicalType type) => Defines[(byte)(type.ValueByte + 1)];

    // Bitfield values
    public bool Solid { get; init; } // 0
    public bool Damage { get; init; } // 1
    public bool Bounce { get; init; } // 2
    public bool RaceEnd { get; init; } // 3
    public bool BumperRight { get; init; } // 4
    public bool BumperLeft { get; init; } // 5
    public bool Directional { get; init; } // 6
    public TypeDirection Direction { get; init; } // 7-9

    public enum TypeDirection
    {
        Right = 0,
        UpRight = 1,
        Up = 2,
        UpLeft = 3,
        Left = 4,
        DownLeft = 5,
        Down = 6,
        DownRight = 7,
    }
}