namespace GbaMonoGame.Rayman3;

public partial class Rayman
{
    public readonly object CAM_TARGET_Y_DEFAULT = 120;
    public readonly object CAM_TARGET_Y_DOWN = 70;
    public readonly object CAM_TARGET_Y_CENTER = 112;
    public readonly object CAM_TARGET_Y_UP = 160;
    public readonly object CAM_TARGET_Y_JUMP = 130;
    public readonly object CAM_TARGET_Y_SWING = 65;

    public const int BODY_PARTS_FISTS_INDEX = (int)RaymanBody.RaymanBodyPartType.Fist;
    public const int BODY_PARTS_FISTS_COUNT = 2;
    public const int BODY_PARTS_FOOT_INDEX = (int)RaymanBody.RaymanBodyPartType.Foot;
    public const int BODY_PARTS_TORSO_INDEX = (int)RaymanBody.RaymanBodyPartType.Torso;
}