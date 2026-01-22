using GbaMonoGame.TgxEngine;

namespace GbaMonoGame.Rayman3;

// N-Gage exclusive
public partial class MenuAll
{
    #region Steps

    private void Step_TransitionToStorageError()
    {
        TransitionValue += 4;

        if (TransitionValue <= 80)
        {
            TgxCluster cluster = Playfield.Camera.GetCluster(1);
            cluster.Position += new Vector2(0, 8);
        }

        if (TransitionValue >= 160)
        {
            TransitionValue = 0;
            CurrentStepAction = Step_StorageError;
        }

        DrawText(false);
    }

    private void Step_StorageError()
    {
        if (JoyPad.IsButtonJustPressed(Rayman3Input.MenuConfirm))
            CurrentStepAction = Step_TransitionOutOfStorageError;

        DrawText(true);
    }

    private void Step_TransitionOutOfStorageError()
    {
        TransitionValue += 4;

        if (TransitionValue <= Playfield.RenderContext.Resolution.Y)
        {
            TgxCluster cluster = Playfield.Camera.GetCluster(1);
            cluster.Position -= new Vector2(0, 4);
        }
        else if (TransitionValue >= Playfield.RenderContext.Resolution.Y + 60)
        {
            TransitionValue = 0;
            CurrentStepAction = Step_InitializeTransitionToGameMode;
        }

        DrawText(false);
    }

    #endregion
}