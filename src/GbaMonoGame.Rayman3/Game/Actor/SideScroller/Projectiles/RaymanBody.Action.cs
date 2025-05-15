namespace GbaMonoGame.Rayman3;

public partial class RaymanBody
{
    public new Action ActionId
    {
        get => (Action)base.ActionId;
        set => base.ActionId = (int)value;
    }

    public enum Action
    {
        Idle = 0,

        Fist_AccelerateForward_Right = 1,
        Fist_AccelerateForward_Left = 2,
        Fist_MoveBackwards_Right = 3,
        Fist_MoveBackwards_Left = 4,
        Fist_DecelerateForward_Right = 5,
        Fist_DecelerateForward_Left = 6,

        Foot_AccelerateForward_Right = 7,
        Foot_AccelerateForward_Left = 8,
        Foot_MoveBackwards_Right = 9,
        Foot_MoveBackwards_Left = 10,
        Foot_DecelerateForward_Right = 11,
        Foot_DecelerateForward_Left = 12,

        Torso_AccelerateForward_Right = 13,
        Torso_AccelerateForward_Left = 14,
        Torso_MoveBackwards_Right = 15,
        Torso_MoveBackwards_Left = 16,
        Torso_DecelerateForward_Right = 17,
        Torso_DecelerateForward_Left = 18,

        SuperFist_AccelerateForward_Right = 19,
        SuperFist_AccelerateForward_Left = 20,
        SuperFist_MoveBackwards_Right = 21,
        SuperFist_MoveBackwards_Left = 22,
        SuperFist_DecelerateForward_Right = 23,
        SuperFist_DecelerateForward_Left = 24,

        HitEffect = 25,
    }
}