using GbaMonoGame.AnimEngine;

namespace GbaMonoGame.Rayman3;

public abstract class MenuOption
{
    public bool IsInitialized { get; set; }

    public virtual void Init(ModernMenuAll menu, RenderContext renderContext, Vector2 position, int index) { }
    public virtual void ChangeIsSelected(bool isSelected) { }
    public virtual void Draw(AnimationPlayer animationPlayer) { }
}