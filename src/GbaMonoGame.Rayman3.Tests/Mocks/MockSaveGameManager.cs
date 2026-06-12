using GbaMonoGame.Rayman3.Readvanced;

namespace GbaMonoGame.Rayman3.Tests;

public class MockSaveGameManager : ISaveGameManager
{
    public void Step() { }

    public void ShowPopup() { }

    public bool SlotExists(int index) => false;
    public ReadvancedSlot LoadSlot(int index) => null;
    public void SaveSlot(int index, ReadvancedSlot save) { }
    public void DeleteSlot(int index) { }

    public AchievementsSave LoadAchievementsSave() => null;
    public void SaveAchievementsSave(AchievementsSave save) { }

    public TimeAttackSave LoadTimeAttackSave() => null;
    public void SaveTimeAttackSave(TimeAttackSave save) { }

    public TimeAttackGhostSave LoadTimeAttackGhost(MapId mapId) => null;
    public void SaveTimeAttackGhost(TimeAttackGhostSave save, MapId mapId) { }
}