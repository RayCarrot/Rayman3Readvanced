using BinarySerializer.Gameloft.J2me;
using Microsoft.Xna.Framework.Graphics;

namespace GbaMonoGame.Rayman3.J2me;

public class AnimData
{
    public sbyte resID { get; set; }
    public byte nbModule { get; set; }
    public byte nbFrame { get; set; }
    public byte nbAction { get; set; }
    public ANIM_DATA_FLAGS flag { get; set; }
    public AnimationModule[] modules { get; set; }
    public AnimationFrame[] frames { get; set; }
    public Action[] actions { get; set; }
    public MechModelParams[] mmParam { get; set; }

    // Custom
    public Texture2D[] ModuleTextures { get; set; }
}