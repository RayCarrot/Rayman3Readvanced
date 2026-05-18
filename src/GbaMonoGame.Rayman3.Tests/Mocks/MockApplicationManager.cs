namespace GbaMonoGame.Rayman3.Tests;

public class MockApplicationManager : IApplicationManager
{
    public bool IsActive => true;
    
    public void BeginLoad() { }
    public void Exit() { }
}