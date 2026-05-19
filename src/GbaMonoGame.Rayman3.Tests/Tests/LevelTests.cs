using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3.Tests;

public class LevelTests(MockGame game)
{
    private Frame LoadMap(MapId mapId)
    {
        GameInfo.MapId = mapId;
        Frame frame = LevelFactory.Create(mapId);
        Engine.FrameMngr.SetNextFrame(frame);
        GameInfo.SetPowerBasedOnMap(mapId);
        game.Step();
        return frame;
    }

    [Fact]
    public void LoadLevelFrames_Run10Steps()
    {
        foreach (MapId[] levelMaps in GameInfo.LevelMaps)
        {
            foreach (MapId mapId in levelMaps)
            {
                // Load the frame
                Frame frame = LoadMap(mapId);

                // Validate the frame is loaded
                Assert.Equal(frame, Engine.FrameMngr.CurrentFrame);

                // Step 10 times
                for (int i = 0; i < 10; i++)
                    game.Step();

                // Run some sanity checks
                Scene2D scene = ((IHasScene)frame).Scene;
                Assert.NotNull(scene.MainActor);
                Assert.NotEmpty(scene.KnotManager.GameObjects);
            }
        }
    }

    [Fact]
    public void Map0_RaymanWalkDownSlopes_DoNotEnterFallState()
    {
        // Load the map
        Frame frame = LoadMap(MapId.WoodLight_M1);

        // Get the scene and main actor
        Scene2D scene = ((IHasScene)frame).Scene;
        Rayman rayman = (Rayman)scene.MainActor;

        // Wait for Rayman to be able to move
        while (rayman.State == rayman._Fsm_LevelStart)
            game.Step();

        // Play the recording. It has Rayman walking down slopes at the start of the map. There was a bug
        // where he would enter the falling state here, which shouldn't happen.
        game.StepRecording("Map0_RaymanWalkDownSlopes_DoNotEnterFallState", () =>
        {
            // Validate not in the fall state
            Assert.NotEqual(rayman._Fsm_Fall, rayman.State.CurrentState);
        });

        // Validate Rayman's final position
        Assert.Equal(155.600f, rayman.Position.X, 0.1f);
        Assert.Equal(270.000f, rayman.Position.Y, 0);
    }

    [Fact]
    public void Map0_RaymanStandingOnSlopeWithFractionalXPosition_MaintainYPosition()
    {
        // Load the map
        Frame frame = LoadMap(MapId.WoodLight_M1);

        // Get the scene and main actor
        Scene2D scene = ((IHasScene)frame).Scene;
        Rayman rayman = (Rayman)scene.MainActor;

        // Wait for Rayman to be able to move
        while (rayman.State == rayman._Fsm_LevelStart)
            game.Step();

        // Place Rayman above the slope with a fractional X position
        rayman.Position = new Vector2(146.001f, 200);
        rayman.State.MoveTo(rayman._Fsm_Fall);

        // Wait for Rayman to land
        while (rayman.State == rayman._Fsm_Fall)
            game.Step();

        // Validate for 20 steps
        Vector2 pos = rayman.Position;
        for (int i = 0; i < 20; i++)
        {
            game.Step();
            Assert.Equal(pos, rayman.Position);
        }
    }

    [Fact]
    public void Map43_RollingBouldersWhenNotFramed_DoNotGetStuckOnSlope()
    {
        // Load the map
        Frame frame = LoadMap(MapId.Bonus4);

        // Get the scene and main actor
        Scene2D scene = ((IHasScene)frame).Scene;
        Rayman rayman = (Rayman)scene.MainActor;

        // Wait for Rayman to be able to move
        while (rayman.State == rayman._Fsm_LevelStart)
            game.Step();

        // Set Rayman to the captor's position, spawning the boulders
        rayman.Position = new Vector2(4978, 92);
        scene.Camera.SetFirstPosition();

        // Step to trigger the captor
        game.Step();
        game.Step();

        // Get one of the boulders
        Boulder boulder = scene.GetGameObject<Boulder>(44);

        // Wait for the boulder to roll to the left
        while (boulder.ActionId != Boulder.Action.Roll_Left)
            game.Step();

        // Wait for the boulder to hit the left wall
        while (boulder.Speed.X < 0)
            game.Step();

        // Wait 10 steps
        for (int i = 0; i < 10; i++)
            game.Step();

        // Validate we're moving to the right now
        Assert.Equal(Boulder.Action.Roll_Right, boulder.ActionId);
    }

    [Fact]
    public void Map43_RollingBouldersWhenFramed_DoNotGetStuckOnSlope()
    {
        // Load the map
        Frame frame = LoadMap(MapId.Bonus4);

        // Get the scene and main actor
        Scene2D scene = ((IHasScene)frame).Scene;
        Rayman rayman = (Rayman)scene.MainActor;

        // Wait for Rayman to be able to move
        while (rayman.State == rayman._Fsm_LevelStart)
            game.Step();

        // Manually trigger the captor, spawning the boulders without them being framed
        scene.GetGameObject<Captor>(108).ProcessMessage(this, Message.Captor_Trigger);

        // Step to trigger the captor
        game.Step();

        // Get one of the boulders
        Boulder boulder = scene.GetGameObject<Boulder>(44);

        // Wait for the boulder to roll to the left
        while (boulder.ActionId != Boulder.Action.Roll_Left)
            game.Step();

        // Wait for the boulder to hit the left wall
        while (boulder.Speed.X < 0)
            game.Step();

        // Wait 10 steps
        for (int i = 0; i < 10; i++)
            game.Step();

        // Validate we're moving to the right now
        Assert.Equal(Boulder.Action.Roll_Right, boulder.ActionId);
    }
}