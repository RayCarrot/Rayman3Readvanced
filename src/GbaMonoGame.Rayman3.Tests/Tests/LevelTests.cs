using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3.Tests;

public class LevelTests(MockGame game)
{
    [Fact]
    public void LoadLevelFrames_Run10Steps()
    {
        foreach (MapId[] levelMaps in GameInfo.LevelMaps)
        {
            foreach (MapId mapId in levelMaps)
            {
                // Load the frame
                GameInfo.MapId = mapId;
                Frame frame = LevelFactory.Create(mapId);
                Engine.FrameMngr.SetNextFrame(frame);
                GameInfo.SetPowerBasedOnMap(mapId);
                game.Step();

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
}