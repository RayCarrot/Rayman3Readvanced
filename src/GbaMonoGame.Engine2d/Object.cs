using System;
using System.Diagnostics;

namespace GbaMonoGame.Engine2d;

public abstract class Object
{
    protected abstract bool ProcessMessageImpl(object sender, Message message, object param);

    public void ProcessMessage(object sender, Message message) => ProcessMessage(sender, message, null);
    public void ProcessMessage(object sender, Message message, object param)
    {
        Debug.Assert(Enum.IsDefined(message));

        ProcessMessageImpl(sender, message, param);
    }

    public virtual void DrawDebugLayout(DebugLayout debugLayout, DebugLayoutTextureManager textureManager) { }
}