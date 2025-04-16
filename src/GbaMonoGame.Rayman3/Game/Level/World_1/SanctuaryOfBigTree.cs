namespace GbaMonoGame.Rayman3;

public class SanctuaryOfBigTree : FrameSideScroller
{
    public SanctuaryOfBigTree(MapId mapId) : base(mapId) { }

    public override void Step()
    {
        base.Step();

        Leaf leaf = Scene.CreateProjectile<Leaf>(ActorType.Leaf, allowAddWhenNeeded: false);
        if (leaf != null)
        {
            Vector2 camPos = Scene.Playfield.Camera.Position;

            // TODO: Both GBA and N-Gage use 240, but it should probably be the screen resolution
            leaf.Position = new Vector2(camPos.X + Random.GetNumber(241), camPos.Y + Random.GetNumber(1));

            leaf.AnimationSet = Random.GetNumber(9) / 3;
            leaf.ActionId = (Leaf.Action)(leaf.AnimationSet * 3);
            
            leaf.AnimatedObject.BgPriority = Random.GetNumber(2) + 1;

            int rand = Random.GetNumber(0x2001);
            leaf.MechModel.Speed = new Vector2(
                x: MathHelpers.FromFixedPoint(rand + 0x5000), 
                y: 2 * MathHelpers.FromFixedPoint(rand + 0x5000));

            leaf.Delay = Random.GetNumber(41) + 20;
        }
    }
}