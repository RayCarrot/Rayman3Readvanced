using BinarySerializer.Ubisoft.GbaEngine.Rayman3;

namespace GbaMonoGame.Rayman3;

public class MarshAwakening2 : FrameWaterSkiMode7
{
    public MarshAwakening2(MapId mapId) : base(mapId) { }

    public override void Init()
    {
        base.Init();

        ExtendMap(
        [
            new(1), new(2), new(3),
            new(25), new(26), new(44),
            new(43), new(14), new(15)
        ], 3, 3);

        CameraMode7 cam = (CameraMode7)Scene.Camera;
        cam.IsWaterSki = true;
        cam.MainActorDistance = 85;
    }

    public override void UnInit()
    {
        base.UnInit();
        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Stop__SkiLoop1);
    }
}