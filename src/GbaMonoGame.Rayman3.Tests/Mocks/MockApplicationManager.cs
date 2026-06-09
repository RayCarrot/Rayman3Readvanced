namespace GbaMonoGame.Rayman3.Tests;

public class MockApplicationManager : IApplicationManager
{
    public bool IsActive => true;
    public float Framerate { get; set; }
    
    public void BeginLoad() { }
    public void Exit() { }
}