using System.Collections.Generic;
using BinarySerializer.Ubisoft.GbaEngine;
using GbaMonoGame.Editor;
using GbaMonoGame.Engine2d;
using GbaMonoGame.Rayman3.Readvanced;
using GbaMonoGame.TgxEngine;

namespace GbaMonoGame.Rayman3;

public class Rayman3 : GbaGame
{
    #region Protected Properties

    protected override string Title => "Rayman 3";
    
    #endregion

    #region Protected Methods

    protected override Frame CreateInitialFrame() => new TitleScreen();

    protected override void LoadGame()
    {
        // Load sound manager
        SoundEventsManager.Load(Rom.Platform switch
        {
            Platform.GBA => new GbaSoundEventsManager(Rayman3SoundTables.GbaSongTable, Rom.Loader.SoundBank),
            Platform.NGage => new NGageSoundEventsManager(Rayman3SoundTables.NGageSongTable, Rom.Loader.NGage_SoundEvents),
            _ => throw new UnsupportedPlatformException()
        });
        SoundEventsManager.SetCallBacks(new CallBackSet(
            getObjectPosition: x =>
            {
                if (x is not GameObject obj)
                    return Vector2.Zero;

                return new Vector2(obj.Position.X, 0);
            },
            getMikePosition: x =>
            {
                if (x is not GameObject obj)
                    return Vector2.Zero;

                TgxCamera cam = obj.Scene.Playfield.Camera;
                return new Vector2(cam.Position.X + obj.Scene.Resolution.X / 2, 0);
            },
            getSwitchIndex: () => 0));

        // Load fonts
        FontManager.Load(Rom.Loader.Font8, Rom.Loader.Font16, Rom.Loader.Font32);

        ActorFactory.Init(new Dictionary<ActorType, ActorFactory.CreateActor>()
        {
            { ActorType.Rayman, (instanceId, scene, resource) => new Rayman(instanceId, scene, resource) },
            { ActorType.RaymanBody, (instanceId, scene, resource) => new RaymanBody(instanceId, scene, resource) },
            { ActorType.RedPirate, (instanceId, scene, resource) => new RedPirate(instanceId, scene, resource) },
            { ActorType.Piranha, (instanceId, scene, resource) => new Piranha(instanceId, scene, resource) },
            { ActorType.WaterSplash, (instanceId, scene, resource) => new WaterSplash(instanceId, scene, resource) },
            { ActorType.Explosion, (instanceId, scene, resource) => new Explosion(instanceId, scene, resource) },
            { ActorType.EnergyBall, (instanceId, scene, resource) => new EnergyBall(instanceId, scene, resource) },
            { ActorType.BouncyPlatform, (instanceId, scene, resource) => new BouncyPlatform(instanceId, scene, resource) },
            { ActorType.MovingFlowerPlatform, (instanceId, scene, resource) => new MovingPlatform(instanceId, scene, resource) },
            { ActorType.FallingChainedPlatform, (instanceId, scene, resource) => new FallingPlatform(instanceId, scene, resource) },
            { ActorType.Switch, (instanceId, scene, resource) => new Switch(instanceId, scene, resource) },
            { ActorType.Gate, (instanceId, scene, resource) => new Gate(instanceId, scene, resource) },
            { ActorType.Lums, (instanceId, scene, resource) => new Lums(instanceId, scene, resource) },
            { ActorType.Cage, (instanceId, scene, resource) => new Cage(instanceId, scene, resource) },
            { ActorType.LevelCurtain, (instanceId, scene, resource) => new LevelCurtain(instanceId, scene, resource) },
            { ActorType.FallingWoodenPlatform, (instanceId, scene, resource) => new FallingPlatform(instanceId, scene, resource) },
            { ActorType.UnusedBouncyPlatform, (instanceId, scene, resource) => new UnusedBouncyPlatform(instanceId, scene, resource) },
            { ActorType.BreakableDoor, (instanceId, scene, resource) => new BreakableDoor(instanceId, scene, resource) },
            { ActorType.Keg, (instanceId, scene, resource) => new Keg(instanceId, scene, resource) },
            { ActorType.Barrel, (instanceId, scene, resource) => new Barrel(instanceId, scene, resource) },
            { ActorType.SphereBase, (instanceId, scene, resource) => new SphereBase(instanceId, scene, resource) },
            { ActorType.Sphere, (instanceId, scene, resource) => new Sphere(instanceId, scene, resource) },
            { ActorType.FallingBridge, (instanceId, scene, resource) => new FallingBridge(instanceId, scene, resource) },
            { ActorType.GreenPirate, (instanceId, scene, resource) => new GreenPirate(instanceId, scene, resource) },
            { ActorType.BluePirate, (instanceId, scene, resource) => new BluePirate(instanceId, scene, resource) },
            { ActorType.SilverPirate, (instanceId, scene, resource) => new SilverPirate(instanceId, scene, resource) },
            { ActorType.HelicopterBomb, (instanceId, scene, resource) => new FlyingBomb(instanceId, scene, resource) },
            { ActorType.ZombieChicken, (instanceId, scene, resource) => new ZombieChicken(instanceId, scene, resource) },
            { ActorType.FallingNet, (instanceId, scene, resource) => new FallingNet(instanceId, scene, resource) },
            { ActorType.BarrelSplash, (instanceId, scene, resource) => new BarrelSplash(instanceId, scene, resource) },
            { ActorType.Depart, (instanceId, scene, resource) => new Depart(instanceId, scene, resource) },
            { ActorType.RedShell, (instanceId, scene, resource) => new RedShell(instanceId, scene, resource) },
            { ActorType.KegFire, (instanceId, scene, resource) => new KegFire(instanceId, scene, resource) },
            { ActorType.RaymanMode7, (instanceId, scene, resource) => new RaymanMode7(instanceId, scene, resource) },
            { ActorType.LumsMode7, (instanceId, scene, resource) => new LumsMode7(instanceId, scene, resource) },
            { ActorType.Caterpillar, (instanceId, scene, resource) => new Caterpillar(instanceId, scene, resource) },
            { ActorType.UnusedScenery1, (instanceId, scene, resource) => new Scenery(instanceId, scene, resource) },
            { ActorType.Butterfly, (instanceId, scene, resource) => new Scenery(instanceId, scene, resource) },
            { ActorType.Snail, (instanceId, scene, resource) => new Snail(instanceId, scene, resource) },
            { ActorType.Jano, (instanceId, scene, resource) => new Jano(instanceId, scene, resource) },
            { ActorType.JanoSkullPlatform, (instanceId, scene, resource) => new JanoSkullPlatform(instanceId, scene, resource) },
            { ActorType.Spider, (instanceId, scene, resource) => new Spider(instanceId, scene, resource) },
            { ActorType.SamMode7, (instanceId, scene, resource) => new SamMode7(instanceId, scene, resource) },
            { ActorType.Vines, (instanceId, scene, resource) => new Vines(instanceId, scene, resource) },
            { ActorType.WoodenShieldedHoodboom, (instanceId, scene, resource) => new WoodenShieldedHoodboom(instanceId, scene, resource) },
            { ActorType.Spinneroo, (instanceId, scene, resource) => new Spinneroo(instanceId, scene, resource) },
            { ActorType.Slapdash, (instanceId, scene, resource) => new Slapdash(instanceId, scene, resource) },
            { ActorType.PurpleLum, (instanceId, scene, resource) => new PurpleLum(instanceId, scene, resource) },
            { ActorType.Grenade, (instanceId, scene, resource) => new Grenade(instanceId, scene, resource) },
            { ActorType.SwingSparkle, (instanceId, scene, resource) => new SwingSparkle(instanceId, scene, resource) },
            { ActorType.BreakableGround, (instanceId, scene, resource) => new BreakableGround(instanceId, scene, resource) },
            { ActorType.Boulder, (instanceId, scene, resource) => new Boulder(instanceId, scene, resource) },
            { ActorType.Wall, (instanceId, scene, resource) => new Wall(instanceId, scene, resource) },
            { ActorType.MovingWoodenPlatform, (instanceId, scene, resource) => new MovingPlatform(instanceId, scene, resource) },
            { ActorType.Plum, (instanceId, scene, resource) => new Plum(instanceId, scene, resource) },
            { ActorType.LavaSplash, (instanceId, scene, resource) => new LavaSplash(instanceId, scene, resource) },
            { ActorType.UnusedScenery2, (instanceId, scene, resource) => new Scenery(instanceId, scene, resource) },
            { ActorType.BlackLum, (instanceId, scene, resource) => new BlackLum(instanceId, scene, resource) },
            { ActorType.Electricity, (instanceId, scene, resource) => new Electricity(instanceId, scene, resource) },
            { ActorType.Hoodstormer, (instanceId, scene, resource) => new Hoodstormer(instanceId, scene, resource) },
            { ActorType.SpikyFlyingBomb, (instanceId, scene, resource) => new SpikyFlyingBomb(instanceId, scene, resource) },
            { ActorType.FlowerFire, (instanceId, scene, resource) => new FlowerFire(instanceId, scene, resource) },
            { ActorType.FlyingBombMode7, (instanceId, scene, resource) => new FlyingBombMode7(instanceId, scene, resource) },
            { ActorType.WaterSplashMode7, (instanceId, scene, resource) => new WaterSplashMode7(instanceId, scene, resource) },
            { ActorType.Murfy, (instanceId, scene, resource) => new Murfy(instanceId, scene, resource) },
            { ActorType.LavaFall, (instanceId, scene, resource) => new LavaFall(instanceId, scene, resource) },
            { ActorType.ExplosionMode7, (instanceId, scene, resource) => new ExplosionMode7(instanceId, scene, resource) },
            { ActorType.ChainedSparkles, (instanceId, scene, resource) => new ChainedSparkles(instanceId, scene, resource) },
            { ActorType.PlantMode7, (instanceId, scene, resource) => new SceneryMode7(instanceId, scene, resource) },
            { ActorType.BrokenFenceMode7, (instanceId, scene, resource) => new BrokenFenceMode7(instanceId, scene, resource) },
            { ActorType.DeadTreeMode7, (instanceId, scene, resource) => new SceneryMode7(instanceId, scene, resource) },
            { ActorType.PumpkinMode7, (instanceId, scene, resource) => new PumpkinMode7(instanceId, scene, resource) },
            { ActorType.Bats, (instanceId, scene, resource) => new Bats(instanceId, scene, resource) },
            { ActorType.Sparkle, (instanceId, scene, resource) => new Sparkle(instanceId, scene, resource) },
            { ActorType.MissileMode7, (instanceId, scene, resource) => new MissileMode7(instanceId, scene, resource) },
            { ActorType.Impact, (instanceId, scene, resource) => new Explosion(instanceId, scene, resource) },
            { ActorType.WalkingShell, (instanceId, scene, resource) => new WalkingShell(instanceId, scene, resource) },
            { ActorType.WoodenBar, (instanceId, scene, resource) => new WoodenBar(instanceId, scene, resource) },
            { ActorType.Ly, (instanceId, scene, resource) => new Ly(instanceId, scene, resource) },
            { ActorType.Flag, (instanceId, scene, resource) => new Scenery(instanceId, scene, resource) },
            { ActorType.UnusedScenery3, (instanceId, scene, resource) => new Scenery(instanceId, scene, resource) },
            { ActorType.BreakableWall, (instanceId, scene, resource) => new BreakableWall(instanceId, scene, resource) },
            { ActorType.KegDebris, (instanceId, scene, resource) => new KegDebris(instanceId, scene, resource) },
            { ActorType.Scaleman, (instanceId, scene, resource) => new Scaleman(instanceId, scene, resource) },
            { ActorType.Machine, (instanceId, scene, resource) => new Machine(instanceId, scene, resource) },
            { ActorType.Balloon, (instanceId, scene, resource) => new Balloon(instanceId, scene, resource) },
            { ActorType.ScalemanShadow, (instanceId, scene, resource) => new ScalemanShadow(instanceId, scene, resource) },
            { ActorType.ItemsMulti, (instanceId, scene, resource) => new ItemsMulti(instanceId, scene, resource) },
            { ActorType.FlyingShell, (instanceId, scene, resource) => new FlyingShell(instanceId, scene, resource) },
            { ActorType.Skull, (instanceId, scene, resource) => new Skull(instanceId, scene, resource) },
            { ActorType.UnusedEnemyMode7, (instanceId, scene, resource) => new UnusedEnemyMode7(instanceId, scene, resource) },
            { ActorType.SpikyBag, (instanceId, scene, resource) => new SpikyBag(instanceId, scene, resource) },
            { ActorType.MurfyStone, (instanceId, scene, resource) => new MurfyStone(instanceId, scene, resource) },
            { ActorType.Grolgoth, (instanceId, scene, resource) => new Grolgoth(instanceId, scene, resource) },
            { ActorType.GrolgothProjectile, (instanceId, scene, resource) => new GrolgothProjectile(instanceId, scene, resource) },
            { ActorType.Teensies, (instanceId, scene, resource) => new Teensies(instanceId, scene, resource) },
            { ActorType.Ammo, (instanceId, scene, resource) => new Ammo(instanceId, scene, resource) },
            { ActorType.Rocky, (instanceId, scene, resource) => new Rocky(instanceId, scene, resource) },
            { ActorType.RockyFlame, (instanceId, scene, resource) => new RockyFlame(instanceId, scene, resource) },
            { ActorType.MechanicalPlatform, (instanceId, scene, resource) => new MechanicalPlatform(instanceId, scene, resource) },
            { ActorType.Urchin, (instanceId, scene, resource) => new Urchin(instanceId, scene, resource) },
            { ActorType.Arrive, (instanceId, scene, resource) => new Arrive(instanceId, scene, resource) },
            { ActorType.Mine, (instanceId, scene, resource) => new FlyingBomb(instanceId, scene, resource) },
            { ActorType.RaymanWorldMap, (instanceId, scene, resource) => new RaymanWorldMap(instanceId, scene, resource) },
            { ActorType.BumperMode7, (instanceId, scene, resource) => new SceneryMode7(instanceId, scene, resource) },
            { ActorType.BoulderMode7, (instanceId, scene, resource) => new BoulderMode7(instanceId, scene, resource) },
            { ActorType.UnusedMovingPlatform, (instanceId, scene, resource) => new MovingPlatform(instanceId, scene, resource) },
            { ActorType.RotatedHelicopterBomb, (instanceId, scene, resource) => new FlyingBomb(instanceId, scene, resource) },
            { ActorType.Leaf, (instanceId, scene, resource) => new Leaf(instanceId, scene, resource) },
            { ActorType.JanoShot, (instanceId, scene, resource) => new JanoShot(instanceId, scene, resource) },
            { ActorType.MetalShieldedHoodboom, (instanceId, scene, resource) => new MetalShieldedHoodboom(instanceId, scene, resource) },
            // TODO: CaptureTheFlagFlag
            { ActorType.CaptureTheFlagRaymanSolo, (instanceId, scene, resource) => new Rayman(instanceId, scene, resource) },
            // TODO: CaptureTheFlagFlagBase
            { ActorType.CaptureTheFlagItems, (instanceId, scene, resource) => new CaptureTheFlagItems(instanceId, scene, resource) },
            { ActorType.CaptureTheFlagRaymanTeams, (instanceId, scene, resource) => new Rayman(instanceId, scene, resource) },
        }, x => ((ActorType)x).ToString());
        
        Dictionary<MapId, LevelFactory.CreateLevel> levelCreations = new()
        {
            // World 1
            { MapId.WoodLight_M1, mapId => new WoodLight_M1(mapId) },
            { MapId.WoodLight_M2, mapId => new WoodLight_M2(mapId) },
            { MapId.FairyGlade_M1, mapId => new FairyGlade_M1(mapId) },
            { MapId.FairyGlade_M2, mapId => new FairyGlade_M2(mapId) },
            { MapId.MarshAwakening1, mapId => Rom.Platform != Platform.NGage ? new MarshAwakening1(mapId) : new NGageAscension(mapId) },
            { MapId.BossMachine, mapId => new BossMachine(mapId) },
            { MapId.SanctuaryOfBigTree_M1, mapId => new SanctuaryOfBigTree(mapId) },
            { MapId.SanctuaryOfBigTree_M2, mapId => new SanctuaryOfBigTree(mapId) },

            // World 2
            { MapId.MissileRace1, mapId => Rom.Platform != Platform.NGage ? new MissileRace1(mapId) : new FrameSideScroller(mapId) },
            { MapId.EchoingCaves_M1, mapId => new EchoingCaves_M1(mapId) },
            { MapId.EchoingCaves_M2, mapId => new EchoingCaves_M2(mapId) },
            { MapId.CavesOfBadDreams_M1, mapId => new CavesOfBadDreams(mapId) },
            { MapId.CavesOfBadDreams_M2, mapId => new CavesOfBadDreams(mapId) },
            { MapId.BossBadDreams, mapId => new BossBadDreams(mapId) },
            { MapId.MenhirHills_M1, mapId => new MenhirHills_M1(mapId) },
            { MapId.MenhirHills_M2, mapId => new FrameSideScroller(mapId) },
            { MapId.MarshAwakening2, mapId => Rom.Platform != Platform.NGage ? new MarshAwakening2(mapId) : new FrameSideScroller(mapId) },

            // World 3
            { MapId.SanctuaryOfStoneAndFire_M1, mapId => new SanctuaryOfStoneAndFire_M1(mapId) },
            { MapId.SanctuaryOfStoneAndFire_M2, mapId => new FrameSideScroller(mapId) },
            { MapId.SanctuaryOfStoneAndFire_M3, mapId => new FrameSideScroller(mapId) },
            { MapId.BeneathTheSanctuary_M1, mapId => new BeneathTheSanctuary_M1(mapId) },
            { MapId.BeneathTheSanctuary_M2, mapId => new FrameSideScroller(mapId) },
            { MapId.ThePrecipice_M1, mapId => new ThePrecipice_M1(mapId) },
            { MapId.ThePrecipice_M2, mapId => new ThePrecipice_M2(mapId) },
            { MapId.BossRockAndLava, mapId => new FrameSideScroller(mapId) },
            { MapId.TheCanopy_M1, mapId => new FrameSideScroller(mapId) },
            { MapId.TheCanopy_M2, mapId => new FrameSideScroller(mapId) },
            { MapId.SanctuaryOfRockAndLava_M1, mapId => new SanctuaryOfRockAndLava(mapId) },
            { MapId.SanctuaryOfRockAndLava_M2, mapId => new SanctuaryOfRockAndLava(mapId) },
            { MapId.SanctuaryOfRockAndLava_M3, mapId => new SanctuaryOfRockAndLava(mapId) },

            // World 4
            { MapId.TombOfTheAncients_M1, mapId => new FrameSideScroller(mapId) },
            { MapId.TombOfTheAncients_M2, mapId => new FrameSideScroller(mapId) },
            { MapId.BossScaleMan, mapId => new BossScaleMan(mapId) },
            { MapId.IronMountains_M1, mapId => new FrameSideScroller(mapId) },
            { MapId.IronMountains_M2, mapId => new FrameSideScroller(mapId) },
            { MapId.MissileRace2, mapId => Rom.Platform != Platform.NGage ? new MissileRace2(mapId) : new FrameSideScroller(mapId) },
            { MapId.PirateShip_M1, mapId => new FrameSideScroller(mapId) },
            { MapId.PirateShip_M2, mapId => new FrameSideScroller(mapId) },
            { MapId.BossFinal_M1, mapId => new BossFinal(mapId) },
            { MapId.BossFinal_M2, mapId => new BossFinal(mapId) },

            // Bonus
            { MapId.Bonus1, mapId => new FrameSideScroller(mapId) },
            { MapId.Bonus2, mapId => new FrameSideScroller(mapId) },
            { MapId.Bonus3, mapId => new FrameSideScroller(mapId) },
            { MapId.Bonus4, mapId => new FrameSideScroller(mapId) },
            { MapId._1000Lums, mapId => new FrameSideScroller(mapId) },

            // Ly's Challenge
            { MapId.ChallengeLy1, mapId => new ChallengeLy(mapId) },
            { MapId.ChallengeLy2, mapId => new ChallengeLy(mapId) },
            { MapId.ChallengeLyGCN, mapId => new ChallengeLy(mapId) },

            // Power
            { MapId.Power1, mapId => new FrameNewPower(mapId) },
            { MapId.Power2, mapId => new FrameNewPower(mapId) },
            { MapId.Power3, mapId => new FrameNewPower(mapId) },
            { MapId.Power4, mapId => new FrameNewPower(mapId) },
            { MapId.Power5, mapId => new FrameNewPower(mapId) },
            { MapId.Power6, mapId => new FrameNewPower(mapId) },

            // World
            { MapId.World1, mapId => new World1(mapId) },
            { MapId.World2, mapId => new World(mapId) },
            { MapId.World3, mapId => new World(mapId) },
            { MapId.World4, mapId => new World(mapId) },
            { MapId.WorldMap, mapId => new WorldMap(mapId) },
        };

        switch (Rom.Platform)
        {
            case Platform.GBA:
                levelCreations.Add(MapId.GbaMulti_MissileRace, mapId => new FrameMultiMissileRace());
                levelCreations.Add(MapId.GbaMulti_MissileArena, mapId => new FrameMultiMissileArena());
                levelCreations.Add(MapId.GbaMulti_TagWeb, mapId => new FrameMultiTag(mapId));
                levelCreations.Add(MapId.GbaMulti_TagSlide, mapId => new FrameMultiTag(mapId));
                levelCreations.Add(MapId.GbaMulti_CatAndMouseSlide, mapId => new FrameMultiCatAndMouse(mapId));
                levelCreations.Add(MapId.GbaMulti_CatAndMouseSpider, mapId => new FrameMultiCatAndMouse(mapId));
                break;

            case Platform.NGage:
                levelCreations.Add(MapId.NGageMulti_CaptureTheFlagMiddleGround, mapId => new FrameMultiCaptureTheFlag(mapId));
                levelCreations.Add(MapId.NGageMulti_CaptureTheFlagFloors, mapId => new FrameMultiCaptureTheFlag(mapId));
                levelCreations.Add(MapId.NGageMulti_CaptureTheFlagOneForAll, mapId => new FrameMultiCaptureTheFlag(mapId));
                levelCreations.Add(MapId.NGageMulti_CaptureTheFlagAllForOne, mapId => new FrameMultiCaptureTheFlag(mapId));
                levelCreations.Add(MapId.NGageMulti_CaptureTheFlagTeamWork, mapId => new FrameMultiCaptureTheFlag(mapId));
                levelCreations.Add(MapId.NGageMulti_CaptureTheFlagTeamPlayer, mapId => new FrameMultiCaptureTheFlag(mapId));
                levelCreations.Add(MapId.NGageMulti_TagWeb, mapId => new FrameMultiTag(mapId));
                levelCreations.Add(MapId.NGageMulti_TagSlide, mapId => new FrameMultiTag(mapId));
                levelCreations.Add(MapId.NGageMulti_CatAndMouseSlide, mapId => new FrameMultiCatAndMouse(mapId));
                levelCreations.Add(MapId.NGageMulti_CatAndMouseSpider, mapId => new FrameMultiCatAndMouse(mapId));
                break;

            default:
                throw new UnsupportedPlatformException();
        }

        LevelFactory.Init(levelCreations);

        // TODO: Fill out definitions for every actor so they can be used in the editor
        EditorData.Init(
        [
            new ActorDefinition(ActorType.Rayman, "Rayman",
            [
                new ActorActionDefinition { ActionId = 0, Name = "Default" }
            ])
        ]);
    }

    protected override void UnloadGame()
    {
        SoundEventsManager.Unload();
        FontManager.Unload();
    }

    protected override void Initialize()
    {
        base.Initialize();

        // Load fonts
        ReadvancedFonts.Load();
    }

    protected override void AddDebugWindowsAndMenus(DebugLayout debugLayout)
    {
        debugLayout.AddWindow(new SceneDebugWindow());
        debugLayout.AddWindow(new GameObjectDebugWindow());
        debugLayout.AddWindow(new PlayfieldDebugWindow());
        debugLayout.AddWindow(new GameInfoDebugWindow());
        debugLayout.AddMenu(new FramesDebugMenu());
        debugLayout.AddMenu(new GenerateDebugMenu());

#if WINDOWSDX
        debugLayout.AddMenu(new AnalyzeDebugMenu());
#endif
    }

    #endregion
}