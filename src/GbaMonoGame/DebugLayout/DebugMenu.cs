namespace GbaMonoGame;

public abstract class DebugMenu
{
    public abstract string Name { get; }

    public abstract void Draw(DebugLayout debugLayout, DebugLayoutTextureManager textureManager);

    public virtual void Unload() { }
}