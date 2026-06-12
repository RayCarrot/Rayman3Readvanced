namespace GbaMonoGame.Rayman3.Readvanced;

public interface ISaveGameManager
{
    void Step();

    void ShowPopup();

    bool SlotExists(int index);
    ReadvancedSlot LoadSlot(int index);
    void SaveSlot(int index, ReadvancedSlot save);
    void DeleteSlot(int index);

    AchievementsSave LoadAchievementsSave();
    void SaveAchievementsSave(AchievementsSave save);

    TimeAttackSave LoadTimeAttackSave();
    void SaveTimeAttackSave(TimeAttackSave save);

    TimeAttackGhostSave LoadTimeAttackGhost(MapId mapId);
    void SaveTimeAttackGhost(TimeAttackGhostSave save, MapId mapId);
}