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

        // Play the recording
        game.StepRecording("Map0_RaymanWalkDownSlopes_DoNotEnterFallState", () =>
        {
            // Validate not in the fall state
            Assert.NotEqual(rayman._Fsm_Fall, rayman.State.CurrentState);
        });

        // Validate Rayman's final position
        Assert.Equal(155.600f, rayman.Position.X, 0.1f);
        Assert.Equal(270.000f, rayman.Position.Y, 0);
    }
}