using System;
using GbaMonoGame.AnimEngine;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

// TODO: Create action enum
public sealed partial class LevelCurtain : ActionActor
{
    public LevelCurtain(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        InitialActionId = ActionId;
        MapId = ActionId switch
        {
            0  => MapId.WoodLight_M1,
            1  => MapId.FairyGlade_M1,
            2  => MapId.MarshAwakening1,
            3  => MapId.SanctuaryOfBigTree_M1,
            4  => MapId.BossMachine,
            5  => MapId.Bonus1,
            6  => MapId.MissileRace1,
            7  => MapId.EchoingCaves_M1,
            8  => MapId.CavesOfBadDreams_M1,
            9  => MapId.MenhirHills_M1,
            10 => MapId.MarshAwakening2,
            11 => MapId.BossBadDreams,
            12 => MapId.Bonus2,
            13 => MapId.ChallengeLy1,
            14 => MapId.SanctuaryOfRockAndLava_M1,
            15 => MapId.BeneathTheSanctuary_M1,
            16 => MapId.ThePrecipice_M1,
            17 => MapId.TheCanopy_M1,
            18 => MapId.SanctuaryOfStoneAndFire_M1,
            19 => MapId.BossRockAndLava,
            20 => MapId.Bonus3,
            21 => MapId.TombOfTheAncients_M1,
            22 => MapId.IronMountains_M1,
            23 => MapId.MissileRace2,
            24 => MapId.PirateShip_M1,
            25 => MapId.BossScaleMan,
            26 => MapId.BossFinal_M1,
            27 => MapId.Bonus4,
            28 => MapId.ChallengeLy2,
            29 => MapId._1000Lums,
            30 => MapId.ChallengeLyGCN,
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

    public int InitialActionId { get; }
    public MapId MapId { get; }
    public bool IsLocked { get; set; }

    protected override bool ProcessMessageImpl(object sender, Message message, object param)
    {
        if (base.ProcessMessageImpl(sender, message, param))
            return false;

        // Handle messages
        switch (message)
        {
            case Message.ReloadAnimation:
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