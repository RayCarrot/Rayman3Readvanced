using System;
using GbaMonoGame.AnimEngine;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public sealed partial class LevelCurtain : ActionActor
{
    public LevelCurtain(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        InitialActionId = ActionId;
        MapId = ActionId switch
        {
            Action.Level_WoodLight => MapId.WoodLight_M1,
            Action.Level_FairyGlade => MapId.FairyGlade_M1,
            Action.Level_MarshAwakening1 => MapId.MarshAwakening1,
            Action.Level_SanctuaryOfBigTree => MapId.SanctuaryOfBigTree_M1,
            Action.Level_BossMachine => MapId.BossMachine,
            Action.Level_Bonus1 => MapId.Bonus1,
            Action.Level_MissileRace1 => MapId.MissileRace1,
            Action.Level_EchoingCaves => MapId.EchoingCaves_M1,
            Action.Level_CavesOfBadDreams => MapId.CavesOfBadDreams_M1,
            Action.Level_MenhirHills => MapId.MenhirHills_M1,
            Action.Level_MarshAwakening2 => MapId.MarshAwakening2,
            Action.Level_BossBadDreams => MapId.BossBadDreams,
            Action.Level_Bonus2 => MapId.Bonus2,
            Action.Level_ChallengeLy1 => MapId.ChallengeLy1,
            Action.Level_SanctuaryOfRockAndLava => MapId.SanctuaryOfRockAndLava_M1,
            Action.Level_BeneathTheSanctuary => MapId.BeneathTheSanctuary_M1,
            Action.Level_ThePrecipice => MapId.ThePrecipice_M1,
            Action.Level_TheCanopy => MapId.TheCanopy_M1,
            Action.Level_SanctuaryOfStoneAndFire => MapId.SanctuaryOfStoneAndFire_M1,
            Action.Level_BossRockAndLava => MapId.BossRockAndLava,
            Action.Level_Bonus3 => MapId.Bonus3,
            Action.Level_TombOfTheAncients => MapId.TombOfTheAncients_M1,
            Action.Level_IronMountains => MapId.IronMountains_M1,
            Action.Level_MissileRace2 => MapId.MissileRace2,
            Action.Level_PirateShip => MapId.PirateShip_M1,
            Action.Level_BossScaleMan => MapId.BossScaleMan,
            Action.Level_BossFinal => MapId.BossFinal_M1,
            Action.Level_Bonus4 => MapId.Bonus4,
            Action.Level_ChallengeLy2 => MapId.ChallengeLy2,
            Action.Level_1000Lums => MapId._1000Lums,
            Action.Level_ChallengeLyGCN => MapId.ChallengeLyGCN,
            _ => throw new ArgumentOutOfRangeException(nameof(ActionId), ActionId, "Invalid action id"),
        };

        IsLocked = false;

        if (MapId == MapId.Bonus1)
        {
            if (GameInfo.World1LumsCompleted())
            {
                State.SetTo(Fsm_Unlocked);

                if (!GameInfo.PersistentInfo.UnlockedBonus1)
                    IsLocked = true;
            }
            else
            {
                IsLocked = true;
                State.SetTo(Fsm_Locked);
            }
        }
        else if (MapId == MapId.Bonus2)
        {
            if (GameInfo.World2LumsCompleted())
            {
                State.SetTo(Fsm_Unlocked);

                if (!GameInfo.PersistentInfo.UnlockedBonus2)
                    IsLocked = true;
            }
            else
            {
                IsLocked = true;
                State.SetTo(Fsm_Locked);
            }
        }
        else if (MapId == MapId.Bonus3)
        {
            if (GameInfo.World3LumsCompleted())
            {
                State.SetTo(Fsm_Unlocked);

                if (!GameInfo.PersistentInfo.UnlockedBonus3)
                    IsLocked = true;
            }
            else
            {
                IsLocked = true;
                State.SetTo(Fsm_Locked);
            }
        }
        else if (MapId == MapId.Bonus4)
        {
            if (GameInfo.World4LumsCompleted())
            {
                State.SetTo(Fsm_Unlocked);

                if (!GameInfo.PersistentInfo.UnlockedBonus4)
                    IsLocked = true;
            }
            else
            {
                IsLocked = true;
                State.SetTo(Fsm_Locked);
            }
        }
        else
        {
            if (MapId <= (MapId)(GameInfo.PersistentInfo.LastCompletedLevel + 1) ||
                MapId is MapId.ChallengeLy1 or MapId.ChallengeLy2 or MapId.ChallengeLyGCN ||
                (MapId == MapId._1000Lums && GameInfo.GetTotalDeadLums() >= 999))
            {
                State.SetTo(Fsm_Unlocked);
            }
            else
            {
                IsLocked = true;
                State.SetTo(Fsm_Locked);
            }
        }

        if (MapId == MapId.ChallengeLyGCN && !GameInfo.PersistentInfo.UnlockedLyChallengeGCN)
            ProcessMessage(this, Message.Destroy);

        AnimatedObject.BasePaletteIndex = IsLocked ? 1 : 0;
    }

    public Action InitialActionId { get; }
    public MapId MapId { get; }
    public bool IsLocked { get; set; }

    // Custom property to fix issue in high res
    public bool IsRaymanInFront { get; set; }

    protected override bool ProcessMessageImpl(object sender, Message message, object param)
    {
        if (base.ProcessMessageImpl(sender, message, param))
            return false;

        // Handle messages
        switch (message)
        {
            case Message.Actor_ReloadAnimation:
                // Don't need to do anything. The original game sets the palette index again, but we're using local indexes, so it never changes.
                return false;

            default:
                return false;
        }
    }

    public override void Draw(AnimationPlayer animationPlayer, bool forceDraw)
    {
        DrawLarge(animationPlayer, forceDraw);
    }
}