namespace GbaMonoGame.Rayman3;

public partial class Grolgoth
{
    public new Action ActionId
    {
        get => (Action)base.ActionId;
        set => base.ActionId = (int)value;
    }

    public enum Action
    {
        Ground_Idle_Right = 0,
        Ground_Idle_Left = 1,
        Action2 = 2, // Unused
        Action3 = 3, // Unused
        Action4 = 4, // Unused
        Action5 = 5, // Unused
        Ground_BeginDeployBomb_Right = 6,
        Ground_BeginDeployBomb_Left = 7,
        Ground_DeployBomb_Right = 8,
        Ground_DeployBomb_Left = 9,
        Ground_Hit1_Right = 10,
        Ground_Hit1_Left = 11,
        Ground_Hit2_Right = 12,
        Ground_Hit2_Left = 13,
        Ground_Hit3_Right = 14,
        Ground_Hit3_Left = 15,
        Ground_Hit4_Right = 16,
        Ground_Hit4_Left = 17,
        Ground_PrepareFlyUp_Right = 18,
        Ground_PrepareFlyUp_Left = 19,
        Ground_Land_Right = 20,
        Ground_Land_Left = 21,
        Air_Idle_Left = 22,
        Action23 = 23, // Unused
        Action24 = 24, // Unused
        Air_ShootMissile_Left = 25,
        Action26 = 26, // Unused
        Air_IdleDamaged_Left = 27,
        Air_Hit1_Left = 28,
        Air_Hit2_Left = 29,
        Air_Hit3_Left = 30,
        Air_Hit4_Left = 31,
        Air_BeginFlyUp_Left = 32,
        Air_FlyUp_Left = 33,
        Air_FallDown_Left = 34,
        Air_Dying1_Left = 35,
        Air_Dying2_Left = 36,
        Ground_FallDown_Right = 37,
        Ground_FallDown_Left = 38,
        Ground_FlyUp_Right = 39,
        Ground_FlyUp_Left = 40,
        Ground_ShootLasers_Right = 41,
        Ground_ShootLasers_Left = 42,
        Ground_BeginShootLasers_Right = 43,
        Ground_BeginShootLasers_Left = 44,
        Ground_EndShootLasers_Right = 45,
        Ground_EndShootLasers_Left = 46,
        Ground_BeginShootEnergyShotsHigh_Right = 47,
        Ground_BeginShootEnergyShotsHigh_Left = 48,
        Ground_ShootEnergyShotsHigh_Right = 49,
        Ground_ShootEnergyShotsHigh_Left = 50,
        Ground_BeginShootEnergyShotsLow_Right = 51,
        Ground_BeginShootEnergyShotsLow_Left = 52,
        Ground_ShootEnergyShotsLow_Right = 53,
        Ground_ShootEnergyShotsLow_Left = 54,
        Air_Idle_Right = 55,
        Action56 = 56, // Unused
        Action57 = 57, // Unused
        Air_ShootMissile_Right = 58,
        Action59 = 59, // Unused
        Air_IdleDamaged_Right = 60,
        Air_Hit1_Right = 61,
        Air_Hit2_Right = 62,
        Air_Hit3_Right = 63,
        Air_Hit4_Right = 64,
        Air_BeginFlyUp_Right = 65,
        Air_FlyUp_Right = 66,
        Air_FallDown_Right = 67,
    }
}