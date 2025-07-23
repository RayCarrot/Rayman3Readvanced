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

            // NOTE: In the original game it's hard-coded to 240, even on N-Gage!
            int maxX = Engine.ActiveConfig.Tweaks.FixBugs ? (int)Scene.Resolution.X : 240;
            const int maxY = 0; // Huh?

            leaf.Position = new Vector2(camPos.X + Random.GetNumber(maxX + 1), camPos.Y + Random.GetNumber(maxY + 1));

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