﻿namespace GbaMonoGame.Engine2d;

// TODO: Since messages are implemented across different libraries maybe this should be constants instead of an enum?
public enum Message
{
    None = 0,

    // Object
    WakeUp = 100,
    Sleep = 101,
    Destroy = 102,
    Resurrect = 103,
    ResurrectWakeUp = 104,

    // Captor
    Captor_Trigger = 200,
    Captor_Trigger_Sound = 201,
    Captor_Trigger_None = 202,
    Captor_Trigger_SendMessageWithParam = 203,
    Captor_Trigger_SendMessageWithCaptorParam = 204,
    
    // Rayman 3
    RaymanBody_FinishedAttack = 1002,
    Main_LinkMovement = 1003,
    Main_UnlinkMovement = 1004,
    Main_BeginBounce = 1005,
    Main_Bounce = 1006,
    Gate_Open = 1007,
    Gate_Close = 1008,
    Main_CollectedYellowLum = 1009,
    MissileMode7_CollectedBlueLum = 1009,
    Main_CollectedRedLum = 1010,
    MissileMode7_CollectedRedLum = 1010,
    Main_CollectedGreenLum = 1011,
    Main_CollectedBlueLum = 1012,
    Main_CollectedWhiteLum = 1013,
    MainMode7_CollectedYellowLum = 1014,
    Main_CollectedBigYellowLum = 1015, // Unused
    Main_CollectedBigBlueLum = 1016,
    Main_Victory = 1017, // Unused
    Main_Determined = 1018, // Unused
    Main_LevelEnd = 1019,
    Main_PickUpObject = 1020,
    Main_CatchObject = 1021,
    ThrowObjectUp = 1022,
    ThrowObjectForward = 1023,
    DropObject = 1024,
    Damaged = 1025,
    Cam_CenterPositionX = 1026,
    Cam_ResetPositionX = 1027,
    FallingBridge_Fall = 1028,
    Main_PreventWallJumps = 1028,
    Main_BeginHang = 1029,
    Main_EndHang = 1030,
    Main_LevelExit = 1031,
    MainMode7_LevelStart = 1031,
    Spider_Spawn = 1031,
    Boulder_Start = 1031,

    Main_CollectedCage = 1033,
    LightOnFire_Right = 1034,
    LightOnFire_Left = 1035,
    Main_StartFlyingWithKegRight = 1036,
    Plum_HitRight = 1036,
    Main_StartFlyingWithKegLeft = 1037,
    Plum_HitLeft = 1037,
    HitActorOfSameType = 1038,
    Cam_DoNotFollowPositionY = 1039,
    Cam_FollowPositionY = 1040,
    Cam_FollowPositionYUntilNearby = 1041,
    Main_StopFlyingWithKeg = 1042,
    Hit = 1043,
    Main_BeginSwing = 1044,
    Main_Damaged2 = 1045,
    Main_DetachPlum = 1046,
    Main_AttachPlum = 1047,
    Main_AllowCoyoteJump = 1048,
    Murfy_Spawn = 1049,
    Main_QuickFinishBodyShotAttack = 1050,

    CamMode7_Spin = 1053,
    Cam_Shake = 1054,
    Cam_MoveToTarget = 1055,
    Main_1056 = 1056, // TODO: Name
    Main_Stop = 1057,
    Cam_MoveToLinkedObject = 1058,
    Main_ExitStopOrCutscene = 1059,
    Exploded = 1060,
    CamMode7_Reset = 1061,
    Cam_SetPosition = 1062,
    Lum_ToggleVisibility = 1063,
    Main_MountWalkingShell = 1064,
    Main_UnmountWalkingShell = 1065,
    FlyingShell_RefillAmmo = 1066,
    Main_CollectedMultiItemGlobox = 1067,
    Main_CollectedMultiItemReverse = 1068,
    Main_CollectedMultiItemInvisibility = 1069,
    Main_CollectedMultiItemFist = 1070,

    Main_Hide = 1072, // Unused
    MissileMode7_StartRace = 1073,
    MissileMode7_EndRace = 1074,

    Main_MultiplayerGameOver = 1075,
    Main_MultiplayerTagMoved = 1076,
    Main_JumpOffWalkingShell = 1077,
    Main_EndSuperHelico = 1078,
    UserInfo_Pause = 1079,
    UserInfo_Unpause = 1080,
    UserInfo_GameCubeLevel = 1081,
    Main_EnterLevelCurtain = 1081,
    Main_BeginInFrontOfLevelCurtain = 1082,
    Main_EndInFrontOfLevelCurtain = 1083,
    Main_Damaged3 = 1084,
    MainMode7_ShowTextBox = 1085,
    Main_Damaged4 = 1086,
    ReloadAnimation = 1087,
    Main_EnterCutscene = 1088,
    Main_DamagedShock = 1089,
    Cam_Lock = 1090,
    Cam_Unlock = 1091,

    Main_LockedLevelCurtain = 1093,

    // N-Gage exclusive
    Main_1095 = 1095, // TODO: Name

    Main_1102 = 1102, // TODO: Name
    Main_1103 = 1103, // TODO: Name

    Main_1112 = 1112, // TODO: Name
    Main_1113 = 1113, // TODO: Name
    
    Main_1115 = 1115, // TODO: Name
    Main_1116 = 1116, // TODO: Name
}

/*
--- Captor messages ---

{MapId}: {TriggersCount} - {MessageId} ({Param})

02: 0 - 103 (80)
02: 0 - 103 (76)
02: 0 - 103 (82)
03: 0 - 103 (62)
03: 0 - 103 (70)
03: 0 - 103 (60)
03: 0 - 103 (67)
03: 0 - 103 (64)
03: 0 - 103 (68)
04: 0 - 104 (76)
04: 0 - 104 (75)
04: 0 - 104 (74)
04: 0 - 104 (78)
04: 0 - 104 (95)
04: 0 - 104 (77)
04: 0 - 104 (93)
04: 0 - 104 (16)
04: 0 - 104 (79)
04: 0 - 104 (94)
04: 0 - 104 (115)
04: 0 - 104 (91)
04: 0 - 104 (114)
04: 0 - 104 (13)
04: 0 - 104 (17)
04: 0 - 104 (113)
04: 0 - 104 (83)
04: 0 - 104 (84)
04: 0 - 104 (112)
04: 0 - 104 (111)
04: 0 - 104 (109)
04: 0 - 104 (110)
04: 0 - 104 (85)
04: 0 - 1085 (0)
06: 0 - 103 (85)
07: 0 - 204 (88)
07: 0 - 204 (87)
07: 0 - 204 (99)
07: 0 - 104 (109)
09: 0 - 103 (65)
09: 0 - 103 (64)
09: 0 - 103 (56)
09: 0 - 103 (55)
09: 0 - 103 (60)
09: 0 - 103 (52)
09: 0 - 103 (68)
11: 0 - 103 (19)
11: 0 - 103 (22)
11: 0 - 103 (21)
11: 0 - 103 (23)
11: 0 - 103 (25)
11: 0 - 103 (26)
11: 0 - 103 (30)
11: 0 - 103 (29)
11: 0 - 103 (31)
11: 0 - 103 (32)
11: 0 - 103 (36)
11: 0 - 103 (37)
11: 0 - 103 (35)
11: 0 - 103 (43)
11: 0 - 103 (44)
11: 0 - 103 (47)
12: 0 - 103 (20)
12: 0 - 103 (21)
12: 0 - 103 (22)
12: 0 - 103 (23)
12: 0 - 103 (28)
12: 0 - 103 (27)
12: 0 - 103 (29)
12: 0 - 103 (30)
12: 0 - 103 (32)
12: 0 - 103 (33)
12: 0 - 103 (34)
12: 0 - 103 (35)
12: 0 - 103 (37)
12: 0 - 103 (39)
12: 0 - 103 (38)
12: 0 - 103 (42)
12: 0 - 103 (41)
12: 0 - 103 (43)
12: 0 - 103 (44)
12: 0 - 103 (45)
16: 0 - 104 (68)
16: 0 - 104 (67)
16: 0 - 104 (66)
16: 0 - 104 (86)
16: 0 - 104 (69)
16: 0 - 104 (84)
16: 0 - 104 (16)
16: 0 - 104 (70)
16: 0 - 104 (85)
16: 0 - 104 (101)
16: 0 - 104 (82)
16: 0 - 104 (12)
16: 0 - 104 (17)
16: 0 - 104 (100)
16: 0 - 104 (76)
16: 0 - 104 (77)
16: 0 - 104 (99)
16: 0 - 104 (98)
16: 0 - 104 (96)
16: 0 - 104 (97)
16: 0 - 104 (78)
16: 0 - 104 (147)
16: 0 - 104 (147)
16: 0 - 104 (145)
16: 0 - 104 (146)
16: 0 - 104 (149)
16: 0 - 104 (150)
16: 0 - 104 (152)
16: 0 - 104 (170)
16: 0 - 104 (171)
16: 0 - 104 (173)
16: 0 - 104 (172)
20: 30 - 103 (15)
20: 0 - 103 (17)
20: 60 - 103 (16)
20: 0 - 103 (18)
20: 30 - 103 (19)
20: 60 - 103 (20)
20: 0 - 103 (22)
20: 30 - 103 (23)
20: 60 - 103 (24)
20: 0 - 103 (25)
20: 30 - 103 (26)
20: 0 - 103 (29)
20: 0 - 103 (32)
20: 0 - 103 (30)
20: 0 - 103 (31)
20: 0 - 103 (13)
21: 0 - 103 (26)
21: 0 - 103 (10)
21: 0 - 103 (27)
21: 0 - 103 (83)
21: 0 - 103 (11)
22: 0 - 104 (98)
22: 0 - 104 (99)
23: 0 - 103 (58)
25: 0 - 103 (39)
25: 0 - 103 (40)
25: 0 - 103 (42)
25: 0 - 103 (41)
26: 1 - 1031 (65)
26: 0 - 1031 (36)
30: 0 - 1031 (17)
30: 0 - 1031 (18)
30: 0 - 104 (18)
30: 0 - 104 (17)
33: 0 - 103 (23)
33: 0 - 103 (25)
33: 0 - 103 (28)
33: 0 - 103 (30)
33: 0 - 103 (33)
33: 0 - 103 (36)
33: 0 - 103 (40)
33: 0 - 103 (48)
33: 0 - 103 (53)
33: 0 - 103 (54)
33: 0 - 103 (62)
33: 0 - 103 (38)
33: 0 - 103 (24)
33: 0 - 103 (50)
33: 0 - 103 (49)
33: 0 - 103 (117)
34: 0 - 103 (130)
34: 0 - 103 (128)
34: 0 - 103 (129)
34: 0 - 103 (37)
34: 0 - 103 (44)
34: 0 - 103 (46)
34: 0 - 103 (51)
34: 0 - 103 (55)
34: 0 - 103 (78)
34: 0 - 103 (84)
34: 0 - 103 (121)
43: 0 - 1031 (9)
43: 0 - 1031 (22)
43: 0 - 1031 (14)
43: 0 - 1031 (37)
43: 0 - 1031 (38)
43: 0 - 1031 (43)
43: 0 - 1031 (44)
43: 0 - 1031 (56)
43: 0 - 1031 (67)
43: 0 - 1031 (66)
43: 0 - 1031 (97)
45: 0 - 103 (28)
45: 0 - 103 (36)
45: 0 - 103 (13)
45: 0 - 103 (8)
45: 0 - 103 (17)
45: 0 - 103 (20)
45: 0 - 103 (23)
45: 0 - 103 (27)
46: 0 - 103 (16)
46: 0 - 103 (18)
46: 0 - 103 (21)
46: 0 - 103 (26)
46: 0 - 103 (30)
46: 0 - 103 (33)
46: 0 - 103 (34)
46: 0 - 103 (39)
46: 0 - 103 (46)
46: 0 - 103 (48)
46: 0 - 103 (51)
46: 0 - 103 (53)
46: 0 - 103 (55)
56: 0 - 103 (7)
*/