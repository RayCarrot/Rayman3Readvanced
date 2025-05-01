namespace GbaMonoGame.Rayman3;

// TODO: Implement
public class FrameMultiCaptureTheFlag : FrameMultiSideScroller
{
    public FrameMultiCaptureTheFlag(MapId mapId) : base(mapId) { }

    public ushort Time { get; set; }
    public bool IsMatchFinished { get; set; }

    public void AddFlag(int machineId)
    {
        // TODO: Implement
    }
}