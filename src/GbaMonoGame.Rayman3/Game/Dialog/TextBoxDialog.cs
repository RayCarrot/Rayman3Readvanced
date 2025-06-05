using System;
using BinarySerializer.Ubisoft.GbaEngine;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.AnimEngine;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

// Original name: InGameDlg
public partial class TextBoxDialog : Dialog
{
    #region Constructor

    public TextBoxDialog(Scene2D scene) : base(scene) { }

    #endregion

    #region Public Properties

    public AnimatedObject Canvas { get; set; }
    public AnimatedObject RaymanIcon { get; set; }
    public SpriteTextObject[] TextObjects { get; set; } // One for every line of text
    public AnimatedObject MurfyIcon { get; set; }
    public AnimatedObject LyIcon { get; set; }
    public AnimatedObject TeensiesIcon { get; set; }

    public bool ShouldPlayedLySound { get; set; } = true;
    public bool ShouldPlayRaymanSound { get; set; } = true;
    public bool ShouldPlayedMurfySound { get; set; } = true;
    public int OffsetY { get; set; } = 45;
    public int IconAnimationTimer { get; set; } = 60;
    public byte Timer { get; set; }
    public Rayman3SoundEvent NextSoundEvent { get; set; } = Rayman3SoundEvent.None;
    public float TextTransitionValue { get; set; } = 1;
    public bool IsShowingCutsceneCharacter { get; set; }
    public bool NextText { get; set; }

    public TextBankId TextBankId { get; set; }
    public int TextId { get; set; }
    public TextBoxCutsceneCharacter CutsceneCharacter { get; set; }
    public int CurrentTextLine { get; set; }
    public string[] CurrentText { get; set; }

    public bool IsFinished { get; set; }

    #endregion

    #region Private Methods

    private void UpdateText(int textObjectIndex)
    {
        CurrentText = Localization.GetText(TextBankId, TextId);

        if (textObjectIndex != 0)
            IsFinished = false;

        if (CurrentTextLine + textObjectIndex >= CurrentText.Length)
        {
            TextObjects[textObjectIndex].Text = "";
            return;
        }

        if (textObjectIndex == 0)
        {
            // Rayman speaking
            if (CurrentText[CurrentTextLine][0] == '1')
            {
                IsShowingCutsceneCharacter = false;
                if (ShouldPlayRaymanSound && CutsceneCharacter == TextBoxCutsceneCharacter.Ly)
                {
                    ShouldPlayRaymanSound = false;
                    if (OffsetY > 44)
                        NextSoundEvent = Rayman3SoundEvent.Play__RyVOLy_Mix01;
                    else
                        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__RyVOLy_Mix01);
                }
                else
                {
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__RyVO1_Mix01__or__RyVO2_Mix01__or__RyVO3_Mix01);
                }
            }
            // Character speaking
            else
            {
                IsShowingCutsceneCharacter = true;

                switch (CutsceneCharacter)
                {
                    case TextBoxCutsceneCharacter.Murfy:
                        if (ShouldPlayedMurfySound)
                        {
                            ShouldPlayedMurfySound = false;
                            if (OffsetY > 44)
                                NextSoundEvent = Rayman3SoundEvent.Play__MurfyVO4A_Mix01;
                            else
                                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__MurfyVO4A_Mix01);
                        }
                        else
                        {
                            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__MurfyVO1A_Mix01__or__MurfyVO1B_Mix01);
                        }

                        break;

                    case TextBoxCutsceneCharacter.Ly:
                        if (ShouldPlayedLySound)
                        {
                            ShouldPlayedLySound = false;
                            if (OffsetY > 44)
                                NextSoundEvent = Rayman3SoundEvent.Play__LyVO1_Mix01;
                            else
                                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__LyVO1_Mix01);
                        }
                        break;

                    case TextBoxCutsceneCharacter.Teensies:
                        if (CurrentTextLine == 0)
                        {
                            if (OffsetY > 44)
                                NextSoundEvent = Rayman3SoundEvent.Play__TiztrVO1_Mix01;
                            else
                                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__TiztrVO1_Mix01);
                        }
                        else
                        {
                            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__TiztrVO2_Mix01__or__TiztrVO3_Mix01__or__TiztrVO4_Mix01);
                        }
                        break;
                }
            }

            // Skip first 2 characters because it shows who's speaking
            TextObjects[0].Text = CurrentText[CurrentTextLine][2..];
        }
        else
        {
            TextObjects[textObjectIndex].Text = CurrentText[CurrentTextLine + textObjectIndex];
        }
    }

    #endregion

    #region Protected Methods

    protected override bool ProcessMessageImpl(object sender, Message message, object param)
    {
        return false;
    }

    #endregion

    #region Public Methods

    public void SetCutsceneCharacter(TextBoxCutsceneCharacter cutsceneCharacter)
    {
        CutsceneCharacter = cutsceneCharacter;
        TextBankId = cutsceneCharacter switch
        {
            TextBoxCutsceneCharacter.Murfy => TextBankId.TextBoxMurfy,
            TextBoxCutsceneCharacter.Ly => TextBankId.TextBoxLy,
            TextBoxCutsceneCharacter.Teensies => TextBankId.TextBoxTeensies,
            _ => throw new ArgumentOutOfRangeException(nameof(cutsceneCharacter), cutsceneCharacter, null)
        };
    }

    public void SetText(int textId)
    {
        TextId = textId;

        for (int i = 0; i < TextObjects.Length; i++)
            UpdateText(i);
    }

    public void MoveInOurOut(bool moveIn)
    {
        if (moveIn)
            State.MoveTo(Fsm_MoveIn);
        else
            State.MoveTo(Fsm_MoveOut);
    }

    public void MoveToNextText()
    {
        if (State == Fsm_WaitForNextText)
            NextText = true;
    }

    public void Skip()
    {
        if (State == Fsm_WaitForNextText)
        {
            CurrentTextLine = 0;
            ShouldPlayedLySound = true;
            ShouldPlayRaymanSound = true;
            ShouldPlayedMurfySound = true;
            IsFinished = true;
            State.MoveTo(Fsm_TransitionTextIn);
        }
    }

    public bool IsOnScreen() => OffsetY < 45;

    public override void Load()
    {
        // NOTE: Game has it set up so Load can be called multiple times. Dynamic objects don't get recreated after the first time, but instead
        //       reloaded into VRAM. We don't need to do that though due to how the graphics system works here, so just always create everything.

        AnimatedObjectResource canvasResource = Rom.LoadResource<AnimatedObjectResource>(Rayman3DefinedResource.TextBoxCanvasAnimations);
        Canvas = new AnimatedObject(canvasResource, false)
        {
            IsFramed = true,
            CurrentAnimation = 0,
            ScreenPos = new Vector2(0, -OffsetY),
            HorizontalAnchor = HorizontalAnchorMode.Scale,
            RenderContext = Scene.HudRenderContext,
        };

        AnimatedObjectResource raymanIconResource = Rom.LoadResource<AnimatedObjectResource>(Rayman3DefinedResource.TextBoxRaymanIconAnimations);
        RaymanIcon = new AnimatedObject(raymanIconResource, true)
        {
            IsFramed = true,
            CurrentAnimation = 0,
            ScreenPos = new Vector2(8, 8 - OffsetY),
            HorizontalAnchor = HorizontalAnchorMode.Scale,
            RenderContext = Scene.HudRenderContext,
        };

        int textsCount = Rom.Platform switch
        {
            Platform.GBA => 2,
            Platform.NGage => 3,
            _ => throw new UnsupportedPlatformException()
        };
        TextObjects = new SpriteTextObject[textsCount];
        for (int i = 0; i < textsCount; i++)
        {
            TextObjects[i] = new SpriteTextObject()
            {
                Text = i < CurrentText?.Length ? (i == 0 ? CurrentText[i][2..] : CurrentText[i]) : "",
                AffineMatrix = AffineMatrix.Identity,
                ScreenPos = new Vector2(38, 7 + 14 * i - OffsetY),
                HorizontalAnchor = HorizontalAnchorMode.Scale,
                Color = TextColor.TextBox,
                RenderContext = Scene.HudRenderContext,
            };
        }

        AnimatedObjectResource murfyIconResource = Rom.LoadResource<AnimatedObjectResource>(Rayman3DefinedResource.TextBoxMurfyIconAnimations);
        MurfyIcon = new AnimatedObject(murfyIconResource, true)
        {
            IsFramed = true,
            CurrentAnimation = 0,
            ScreenPos = new Vector2(8, 8 - OffsetY),
            HorizontalAnchor = HorizontalAnchorMode.Scale,
            RenderContext = Scene.HudRenderContext,
        };

        if (GameInfo.MapId is not (MapId.FairyGlade_M2 or MapId.World3 or MapId.BossMachine or MapId.MarshAwakening1))
        {
            AnimatedObjectResource lyIconResource = Rom.LoadResource<AnimatedObjectResource>(Rayman3DefinedResource.TextBoxLyIconAnimations);
            LyIcon = new AnimatedObject(lyIconResource, true)
            {
                IsFramed = true,
                CurrentAnimation = 0,
                ScreenPos = new Vector2(8, 8 - OffsetY),
                HorizontalAnchor = HorizontalAnchorMode.Scale,
                RenderContext = Scene.HudRenderContext,
            };
        }

        if (GameInfo.MapId is not (MapId.FairyGlade_M2 or MapId.BossMachine or MapId.MarshAwakening1))
        {
            AnimatedObjectResource teensiesIconResource = Rom.LoadResource<AnimatedObjectResource>(Rayman3DefinedResource.TextBoxTeensiesIconAnimations);
            TeensiesIcon = new AnimatedObject(teensiesIconResource, true)
            {
                IsFramed = true,
                CurrentAnimation = 0,
                ScreenPos = new Vector2(8, 8 - OffsetY),
                HorizontalAnchor = HorizontalAnchorMode.Scale,
                RenderContext = Scene.HudRenderContext,
            };
        }
    }

    public override void Draw(AnimationPlayer animationPlayer)
    {
        if (OffsetY > 44)
            return;

        if (NextSoundEvent != Rayman3SoundEvent.None)
        {
            SoundEventsManager.ProcessEvent(NextSoundEvent);
            NextSoundEvent = Rayman3SoundEvent.None;
        }

        Canvas.ScreenPos = Canvas.ScreenPos with { Y = -OffsetY };

        for (int i = 0; i < TextObjects.Length; i++)
            TextObjects[i].ScreenPos = TextObjects[i].ScreenPos with { Y = 7 + 14 * i - OffsetY };

        RaymanIcon.ScreenPos = RaymanIcon.ScreenPos with { Y = 8 - OffsetY };
        MurfyIcon.ScreenPos = MurfyIcon.ScreenPos with { Y = 8 - OffsetY };
        LyIcon.ScreenPos = LyIcon.ScreenPos with { Y = 8 - OffsetY };
        TeensiesIcon.ScreenPos = TeensiesIcon.ScreenPos with { Y = 8 - OffsetY };

        // Blink next text symbol
        if (CurrentTextLine + 2 < CurrentText.Length && TextTransitionValue == 1 && (GameTime.ElapsedFrames & 0x10) != 0)
            Canvas.ActivateChannel(0);
        else
            Canvas.DeactivateChannel(0);

        animationPlayer.PlayFront(Canvas);

        if (Timer == 0)
        {
            foreach (SpriteTextObject textObj in TextObjects)
                animationPlayer.PlayFront(textObj);
        }

        AnimatedObject icon;
        if (IsShowingCutsceneCharacter)
            icon = CutsceneCharacter switch
            {
                TextBoxCutsceneCharacter.Murfy => MurfyIcon,
                TextBoxCutsceneCharacter.Ly => LyIcon,
                TextBoxCutsceneCharacter.Teensies => TeensiesIcon,
                _ => throw new ArgumentOutOfRangeException()
            };
        else
            icon = RaymanIcon;

        if (icon.CurrentAnimation == 0)
        {
            IconAnimationTimer--;

            if (IconAnimationTimer == 0)
                icon.CurrentAnimation = Random.GetNumber(100) < 50 ? 1 : 2;
        }
        else if (icon.EndOfAnimation)
        {
            IconAnimationTimer = 60 + Random.GetNumber(60);
            icon.CurrentAnimation = 0;
        }

        animationPlayer.PlayFront(icon);
    }

    #endregion
}