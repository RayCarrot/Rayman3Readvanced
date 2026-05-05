namespace GbaMonoGame.Rayman3.Readvanced;

public readonly struct TimeFreezeItemResource(TimeFreezeItem.Action firstActionId, BinarySerializer.Ubisoft.GbaEngine.Vector2 pos)
{
    public TimeFreezeItem.Action FirstActionId { get; } = firstActionId;
    public BinarySerializer.Ubisoft.GbaEngine.Vector2 Pos { get; } = pos;
}