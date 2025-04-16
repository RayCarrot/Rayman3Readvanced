namespace GbaMonoGame;

public abstract class DebugWindow
{
    public abstract string Name { get; }
    public virtual bool CanClose => true;
    public bool IsOpen { get; set; } = true;

    public abstract void Draw(DebugLayout debugLayout, DebugLayoutTextureManager textureManager);

    public virtual void DrawGame(GfxRenderer renderer) { }

    public virtual void OnWindowOpened() { }
    public virtual void OnWindowClosed() { }

    public virtual void Unload() { }
}