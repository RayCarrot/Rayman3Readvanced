using System;

namespace GbaMonoGame.Engine2d;

[Flags]
public enum IteratorFlags
{
    None = 0,

    AlwaysActor = 1 << 0,
    Actor = 1 << 1,
    Captor = 1 << 2,

    Enabled = 1 << 3,
    Disabled = 1 << 4,

    ActorTypeMask = AlwaysActor | Actor | Captor,
    EnabledStateMask = Enabled | Disabled,
}