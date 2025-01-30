namespace GbaMonoGame.Rayman3;

public class MissileRace1 : FrameMissileSingleMode7
{
    public MissileRace1(MapId mapId) : base(mapId) { }

    public override void Init()
    {
        base.Init();

        // NOTE: Temp code for testing. This is the start position of the map.
        Scene.Playfield.Camera.Position = new Vector2(334.5f, 121);

        // TODO: Implement
    }

    public override void UnInit()
    {
        base.UnInit();

        // TODO: Implement
    }

    public override void Step()
    {
        base.Step();

        // TODO: Implement
    }
}