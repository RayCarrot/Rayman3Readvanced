namespace GbaMonoGame.Rayman3.J2ME;

public class AnimData
{
    public sbyte resID { get; set; }
    public sbyte nbModule { get; set; }
    public sbyte nbFrame { get; set; }
    public sbyte nbAction { get; set; }
    public sbyte flag { get; set; } // TODO: Flags enum
    public sbyte[][] modules { get; set; } // TODO: Structs
    public sbyte[][] frames { get; set; } // TODO: Structs
    public sbyte[][] actions { get; set; } // TODO: Structs
    public sbyte[][] mmParam { get; set; }
}