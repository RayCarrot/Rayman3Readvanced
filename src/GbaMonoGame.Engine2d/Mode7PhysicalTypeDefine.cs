namespace GbaMonoGame.Engine2d;

public class Mode7PhysicalTypeDefine
{
    public static Mode7PhysicalTypeDefine[] Defines { get; } =
    [
        new(), // #0
        new() { Solid = true, }, // #1
        new() { Solid = true, Bumper2 = true, }, // #2
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
        new() { Damage = true, Directional = true, Direction = Mode7PhysicalTypeDirection.Left, }, // #35
        new() { Damage = true, Directional = true, Direction = Mode7PhysicalTypeDirection.Right, }, // #36
        new() { Damage = true, Directional = true, Direction = Mode7PhysicalTypeDirection.Up, }, // #37
        new() { Damage = true, Directional = true, Direction = Mode7PhysicalTypeDirection.Down, }, // #38
        new() { Directional = true, Direction = Mode7PhysicalTypeDirection.Left, }, // #39
        new() { Directional = true, Direction = Mode7PhysicalTypeDirection.Right, }, // #40
        new() { Directional = true, Direction = Mode7PhysicalTypeDirection.Up, }, // #41
        new() { Directional = true, Direction = Mode7PhysicalTypeDirection.Down, }, // #42
        new() { Directional = true, Direction = Mode7PhysicalTypeDirection.DownLeft, }, // #43
        new() { Directional = true, Direction = Mode7PhysicalTypeDirection.DownRight, }, // #44
        new() { Directional = true, Direction = Mode7PhysicalTypeDirection.UpRight, }, // #45
        new() { Directional = true, Direction = Mode7PhysicalTypeDirection.UpLeft, }, // #46
        new() { Bounce = true, }, // #47
        new() { Bumper1 = true, }, // #48
        new() { Bumper2 = true, }, // #49
        new() { Damage = true, RaceEnd = true, }, // #50
        new() { RaceEnd = true, }, // #51
        new() { Bumper1 = true, Directional = true, Direction = Mode7PhysicalTypeDirection.Left, }, // #52
        new() { Bumper1 = true, Directional = true, Direction = Mode7PhysicalTypeDirection.Right, }, // #53
        new() { Bumper1 = true, Directional = true, Direction = Mode7PhysicalTypeDirection.Up, }, // #54
        new() { Bumper1 = true, Directional = true, Direction = Mode7PhysicalTypeDirection.Down, }, // #55
        new() { Bumper1 = true, Directional = true, Direction = Mode7PhysicalTypeDirection.UpLeft, }, // #56
        new() { Bumper1 = true, Directional = true, Direction = Mode7PhysicalTypeDirection.DownLeft, }, // #57
        new() { Bumper1 = true, Directional = true, Direction = Mode7PhysicalTypeDirection.DownRight, }, // #58
        new() { Bumper1 = true, Directional = true, Direction = Mode7PhysicalTypeDirection.UpRight, }, // #59
        new() { Damage = true, Directional = true, Direction = Mode7PhysicalTypeDirection.UpLeft, }, // #60
        new() { Damage = true, Directional = true, Direction = Mode7PhysicalTypeDirection.DownLeft, }, // #61
        new() { Damage = true, Directional = true, Direction = Mode7PhysicalTypeDirection.DownRight, }, // #62
        new() { Damage = true, Directional = true, Direction = Mode7PhysicalTypeDirection.UpRight, }, // #63
        new() { Bumper2 = true, Directional = true, Direction = Mode7PhysicalTypeDirection.Up, }, // #64
        new() { Bumper2 = true, Directional = true, Direction = Mode7PhysicalTypeDirection.Down, }, // #65
        new() { Bumper2 = true, Directional = true, Direction = Mode7PhysicalTypeDirection.Left, }, // #66
        new() { Bumper2 = true, Directional = true, Direction = Mode7PhysicalTypeDirection.Right, }, // #67
        new() { Bumper2 = true, Directional = true, Direction = Mode7PhysicalTypeDirection.UpLeft, }, // #68
        new() { Bumper2 = true, Directional = true, Direction = Mode7PhysicalTypeDirection.UpRight, }, // #69
        new() { Bumper2 = true, Directional = true, Direction = Mode7PhysicalTypeDirection.DownLeft, }, // #70
        new() { Bumper2 = true, Directional = true, Direction = Mode7PhysicalTypeDirection.DownRight, }, // #71
    ];

    public static Mode7PhysicalTypeDefine Empty => Defines[0];

    // Bitfield values
    public bool Solid { get; init; } // 0
    public bool Damage { get; init; } // 1
    public bool Bounce { get; init; } // 2
    public bool RaceEnd { get; init; } // 3
    public bool Bumper1 { get; init; } // 4 TODO: What's the difference between these?
    public bool Bumper2 { get; init; } // 5
    public bool Directional { get; init; } // 6
    public Mode7PhysicalTypeDirection Direction { get; init; } // 7-9

    public enum Mode7PhysicalTypeDirection
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