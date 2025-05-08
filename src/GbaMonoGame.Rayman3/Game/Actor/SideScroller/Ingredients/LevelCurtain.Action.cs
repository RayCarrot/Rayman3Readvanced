namespace GbaMonoGame.Rayman3;

public partial class LevelCurtain
{
    public new Action ActionId
    {
        get => (Action)base.ActionId;
        set => base.ActionId = (int)value;
    }

    public enum Action
    {
        Level_WoodLight = 0,
        Level_FairyGlade = 1,
        Level_MarshAwakening1 = 2,
        Level_SanctuaryOfBigTree = 3,
        Level_BossMachine = 4,
        Level_Bonus1 = 5,
        Level_MissileRace1 = 6,
        Level_EchoingCaves = 7,
        Level_CavesOfBadDreams = 8,
        Level_MenhirHills = 9,
        Level_MarshAwakening2 = 10,
        Level_BossBadDreams = 11,
        Level_Bonus2 = 12,
        Level_ChallengeLy1 = 13,
        Level_SanctuaryOfRockAndLava = 14,
        Level_BeneathTheSanctuary = 15,
        Level_ThePrecipice = 16,
        Level_TheCanopy = 17,
        Level_SanctuaryOfStoneAndFire = 18,
        Level_BossRockAndLava = 19,
        Level_Bonus3 = 20,
        Level_TombOfTheAncients = 21,
        Level_IronMountains = 22,
        Level_MissileRace2 = 23,
        Level_PirateShip = 24,
        Level_BossScaleMan = 25,
        Level_BossFinal = 26,
        Level_Bonus4 = 27,
        Level_ChallengeLy2 = 28,
        Level_1000Lums = 29,
        Level_ChallengeLyGCN = 30,
        EnterCurtain1 = 31,
        EnterCurtain2 = 32,
        Sparkle = 33,
    }
}