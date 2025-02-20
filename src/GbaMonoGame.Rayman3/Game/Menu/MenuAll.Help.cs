using BinarySerializer.Ubisoft.GbaEngine;
using GbaMonoGame.TgxEngine;

namespace GbaMonoGame.Rayman3;

// N-Gage exclusive
public partial class MenuAll
{
    #region Steps

    private void Step_InitializeTransitionToHelp()
    {
        Anims.ArrowLeft.CurrentAnimation = 1;
        Anims.ArrowRight.CurrentAnimation = 0;

        CurrentStepAction = Step_TransitionToHelp;
        NGageSetMenuText(36, false, null, 0);
        SetBackgroundPalette(2);
        SelectedOption = 0;
    }

    private void Step_TransitionToHelp()
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
            CurrentStepAction = Step_Help;
        }

        DrawText(false);

        if (SelectedOption == 0)
            Anims.ArrowLeft.ScreenPos = Anims.ArrowLeft.ScreenPos with { X = 300 };
        else
            Anims.ArrowLeft.ScreenPos = new Vector2(68, 136);

        if (SelectedOption >= 3)
            Anims.ArrowRight.ScreenPos = Anims.ArrowRight.ScreenPos with { X = 300 };
        else
            Anims.ArrowRight.ScreenPos = new Vector2(152, 136);

        AnimationPlayer.Play(Anims.ArrowLeft);
        AnimationPlayer.Play(Anims.ArrowRight);
    }

    private void Step_Help()
    {
        int prevSelectedOption = SelectedOption;

        if (JoyPad.IsButtonJustPressed(GbaInput.Start) || JoyPad.IsButtonJustPressed(GbaInput.A))
        {
            SelectedOption++;

            if (SelectedOption == 4)
            {
                SelectedOption = 0;
                prevSelectedOption = 0;
                CurrentStepAction = Step_TransitionOutOfHelp;
            }
        }
        else if (JoyPad.IsButtonJustPressed(GbaInput.Right))
        {
            SelectedOption++;

            if (SelectedOption == 4)
                SelectedOption = 3;
        }
        else if (JoyPad.IsButtonJustPressed(GbaInput.Left))
        {
            SelectedOption--;

            if (SelectedOption == -1)
                SelectedOption = 0;
        }
        else if (JoyPad.IsButtonJustPressed(GbaInput.Select) || JoyPad.IsButtonJustPressed(GbaInput.B))
        {
            SelectedOption = 0;
            prevSelectedOption = 0;
            CurrentStepAction = Step_TransitionOutOfHelp;
        }

        if (prevSelectedOption != SelectedOption)
            NGageSetMenuText(36 + SelectedOption, false, null, 0);

        DrawText(true);

        if (SelectedOption == 0)
            Anims.ArrowLeft.ScreenPos = Anims.ArrowLeft.ScreenPos with { X = 300 };
        else
            Anims.ArrowLeft.ScreenPos = new Vector2(68, 136);

        if (SelectedOption >= 3)
            Anims.ArrowRight.ScreenPos = Anims.ArrowRight.ScreenPos with { X = 300 };
        else
            Anims.ArrowRight.ScreenPos = new Vector2(152, 136);

        AnimationPlayer.Play(Anims.ArrowLeft);
        AnimationPlayer.Play(Anims.ArrowRight);
    }

    private void Step_TransitionOutOfHelp()
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