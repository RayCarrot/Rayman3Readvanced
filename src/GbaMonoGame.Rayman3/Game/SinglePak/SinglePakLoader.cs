namespace GbaMonoGame.Rayman3;

// Skeleton class to re-implement the structure of the loader for the SinglePak game
public class SinglePakLoader
{
    public bool IsStarted { get; set; }

    public void BeginDownloadLoader()
    {
        // The game initializes the downloading of the loader ROM here
        IsStarted = true;
    }

    public void Step()
    {
        // The game processes the downloading of the loader ROM here
    }

    public bool HasFinishedDownload()
    {
        // The game checks if a value is equal to 233, which seems to indicate it's finished
        return IsStarted;
    }

    public void DecompressAndPlay(int language)
    {
        // The game loads some graphics and then creates an infinite game loop for loading the game, decompressing it into RAM and running it
        FrameManager.SetNextFrame(new SinglePak());
    }
}