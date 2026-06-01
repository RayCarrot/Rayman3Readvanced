namespace GbaMonoGame.Rayman3.J2ME;

public class AnimData
{
    public sbyte resID { get; set; }
    public sbyte nbModule { get; set; }
    public sbyte nbFrame { get; set; }
    public sbyte nbAction { get; set; }
    public ANIM_DATA_FLAGS flag { get; set; }
    public AnimModule[] modules { get; set; }
    public AnimFrame[] frames { get; set; }
    public Action[] actions { get; set; }
    public MechModelParams[] mmParam { get; set; }
}