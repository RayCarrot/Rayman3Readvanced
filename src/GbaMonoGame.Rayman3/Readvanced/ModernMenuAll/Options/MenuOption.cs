using GbaMonoGame.AnimEngine;

namespace GbaMonoGame.Rayman3.Readvanced;

public abstract class MenuOption
{
    public bool IsInitialized { get; set; }

    public virtual void Init(int bgPriority, RenderContext renderContext, int index) { }
    public virtual void SetPosition(Vector2 position) { }
    public virtual void ChangeIsSelected(bool isSelected) { }
    public virtual void Draw(AnimationPlayer animationPlayer) { }
}