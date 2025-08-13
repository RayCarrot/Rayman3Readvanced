using System;
using GbaMonoGame.AnimEngine;
using GbaMonoGame.Engine2d;
using GbaMonoGame.Rayman3.Readvanced;

namespace GbaMonoGame.Rayman3;

public class UserInfoSideScroller : Dialog
{
    public UserInfoSideScroller(Scene2D scene, bool hasBlueLum) : base(scene)
    {
        LifeBar = new LifeBar(scene);

        if (GameInfo.MapId == MapId._1000Lums)
            Lums1000Bar = new Lums1000Bar(scene);
        else
            LumsBar = new LumsBar(scene);

        CagesBar = new CagesBar(scene);

        if (hasBlueLum)
            BlueLumBar = new BlueLumBar(scene);

        if (GameInfo.MapId == MapId.EchoingCaves_M1)
            SwitchBar = new SwitchBar(scene);

        if (GameInfo.MapId == MapId.BossMachine)
            BossBar = new BossMachineBar(scene);
        else if (GameInfo.MapId == MapId.BossRockAndLava)
            BossBar = new BossRockAndLavaBar(scene);
        else if (GameInfo.MapId == MapId.BossScaleMan)
            BossBar = new BossScalemanBar(scene);
        else if (GameInfo.MapId == MapId.BossFinal_M1)
            BossBar = new BossFinalBar(scene, 0);
        else if (GameInfo.MapId == MapId.BossFinal_M2)
            BossBar = new BossFinalBar(scene, 1);

        // Disable lums and cages bars in bosses
        if (GameInfo.MapId is
            MapId.BossMachine or
            MapId.BossBadDreams or
            MapId.BossRockAndLava or
            MapId.BossScaleMan or
            MapId.BossFinal_M1 or
            MapId.BossFinal_M2)
        {
            GetLumsBar().Disable();
            CagesBar.Disable();
        }

        // Disable lums and cages bars in time attack
        if (TimeAttackInfo.IsActive)
        {
            if (LumsBar != null)
            {
                LumsBar.Disable();
            }
            else
            {
                Lums1000Bar.Disable();
                Lums1000Bar.EnableTransitions = true;
            }

            CagesBar.Disable();
        }
    }

    public LifeBar LifeBar { get; }
    public LumsBar LumsBar { get; }
    public Lums1000Bar Lums1000Bar { get; }
    public SwitchBar SwitchBar { get; }
    public Bar BossBar { get; }
    public CagesBar CagesBar { get; }
    public BlueLumBar BlueLumBar { get; }

    public Bar GetLumsBar() => LumsBar != null ? LumsBar : Lums1000Bar;

    public void UpdateLife()
    {
        LifeBar.UpdateLife();
    }

    public void AddLums(int count)
    {
        if (LumsBar != null)
            LumsBar.AddLums(count);
        else
            Lums1000Bar.AddLastLums();
    }

    public void AddCages(int count)
    {
        CagesBar.AddCages(count);
    }

    public void SwitchActivated()
    {
        if (SwitchBar == null)
            throw new Exception("SwitchActivated can only be called in selected maps");

        SwitchBar.ActivateSwitch();
    }

    public void BossHit()
    {
        if (BossBar == null)
            throw new Exception("BossHit can only be called in selected maps");

        BossBar.Set();
    }

    public void HideBars()
    {
        LifeBar.SetToStayHidden();
        GetLumsBar().SetToStayHidden();
        CagesBar.SetToStayHidden();
    }

    public void ForceHideLevelBars()
    {
        LifeBar.OffsetY = 0;
        LifeBar.DrawStep = BarDrawStep.Hide;

        LumsBar.OffsetY = 0;
        LumsBar.DrawStep = BarDrawStep.Hide;

        CagesBar.OffsetX = 0;
        CagesBar.DrawStep = BarDrawStep.Hide;
    }

    public void MoveInBars()
    {
        LifeBar.SetToStayVisible();
        LifeBar.MoveIn();

        GetLumsBar().SetToStayVisible();
        GetLumsBar().MoveIn();

        CagesBar.SetToStayVisible();
        CagesBar.MoveIn();
    }

    public void MoveOutBars()
    {
        LifeBar.MoveOut();
        GetLumsBar().MoveOut();
        CagesBar.MoveOut();
    }

    protected override bool ProcessMessageImpl(object sender, Message message, object param)
    {
        // Handle messages
        switch (message)
        {
            case Message.UserInfo_Pause:
                if (GameInfo.MapId is not (MapId.ChallengeLy1 or MapId.ChallengeLy2 or MapId.ChallengeLyGCN))
                    MoveInBars();

                BlueLumBar?.SetToStayHidden();
                SwitchBar?.SetToStayHidden();
                BossBar?.SetToStayHidden();
                return true;

            case Message.UserInfo_Unpause:
                if (GameInfo.MapId is not (MapId.ChallengeLy1 or MapId.ChallengeLy2 or MapId.ChallengeLyGCN))
                {
                    LifeBar.SetToDefault();
                    GetLumsBar().SetToDefault();
                    CagesBar.SetToDefault();
                }

                BlueLumBar?.SetToDefault();
                SwitchBar?.SetToDefault();
                BossBar?.SetToDefault();
                return true;

            case Message.UserInfo_GameCubeLevel:
                CagesBar.SetToStayHidden();
                LumsBar.SetToStayHidden();
                return true;

            default:
                return false;
        }
    }

    public override void Load()
    {
        LifeBar.Load();
        GetLumsBar().Load();
        CagesBar.Load();
        BlueLumBar?.Load();
        SwitchBar?.Load();
        BossBar?.Load();

        LifeBar.Set();
        GetLumsBar().Set();
        CagesBar.Set();
        BlueLumBar?.Set();
        SwitchBar?.Set();
    }

    public override void Init() { }

    public override void Draw(AnimationPlayer animationPlayer)
    {
        LifeBar.Draw(animationPlayer);
        GetLumsBar().Draw(animationPlayer);
        CagesBar.Draw(animationPlayer);
        BlueLumBar?.Draw(animationPlayer);
        SwitchBar?.Draw(animationPlayer);
        BossBar?.Draw(animationPlayer);
    }
}