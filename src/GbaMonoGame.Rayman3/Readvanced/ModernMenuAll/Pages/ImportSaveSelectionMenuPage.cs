using System;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.AnimEngine;

namespace GbaMonoGame.Rayman3.Readvanced;

public class ImportSaveSelectionMenuPage : MenuPage
{
    public ImportSaveSelectionMenuPage(ModernMenuAll menu, int slot, SaveGameSlot[] saveSlots) : base(menu)
    {
        Slot = slot;
        SaveSlots = saveSlots;
    }

    public override bool UsesCursor => true;
    public override int BackgroundPalette => 1;
    public override int LineHeight => 20;

    public int Slot { get; }
    public SaveGameSlot[] SaveSlots { get; }

    protected override void Init()
    {
        // Add slots
        foreach (SaveGameSlot save in SaveSlots)
        {
            // Load the save slot
            if (save != null)
                Rayman3.GameInfo.Load(ReadvancedSlot.FromSaveGame(save));

            ModernMenuAll.Slot slot = save == null || Rayman3.GameInfo.PersistentInfo.Lives == 0
                ? null
                : new ModernMenuAll.Slot(Rayman3.GameInfo.GetTotalDeadLums(), Rayman3.GameInfo.GetTotalDeadCages(), Rayman3.GameInfo.PersistentInfo.Lives, TimeSpan.Zero);

            AddOption(new SlotMenuOption(slot));
        }
    }

    protected override void Step_Active()
    {
        if (Engine.JoyPad.IsButtonJustPressed(Rayman3Input.MenuUp))
        {
            SetSelectedOption(SelectedOption - 1);
        }
        else if (Engine.JoyPad.IsButtonJustPressed(Rayman3Input.MenuDown))
        {
            SetSelectedOption(SelectedOption + 1);
        }
        else if (Engine.JoyPad.IsButtonJustPressed(Rayman3Input.MenuConfirm))
        {
            if (SaveSlots[SelectedOption] != null)
            {
                CursorClick(() =>
                {
                    Engine.Sem.ReplaceAllSongs(Rayman3SoundEvent.None, 1);
                    FadeOut(2, () =>
                    {
                        Engine.Sem.StopAllSongs();

                        // Load the game
                        Rayman3.GameInfo.Load(ReadvancedSlot.FromSaveGame(SaveSlots[SelectedOption]));
                        Rayman3.GameInfo.GotoLastSaveGame();

                        Rayman3.GameInfo.StartPlayTime();

                        Gfx.FadeControl = new FadeControl(FadeMode.BrightnessDecrease);
                        Gfx.Fade = AlphaCoefficient.Max;

                        Rayman3.GameInfo.CurrentSlot = Slot;

                        // Save so the slot gets created
                        Rayman3.GameInfo.Save(Slot);
                    });
                });
            }
            else
            {
                InvalidCursorClick();
            }
        }
        else if (Engine.JoyPad.IsButtonJustPressed(Rayman3Input.MenuBack))
        {
            Menu.ChangePage(new NewGameMenuPage(Menu, Slot), NewPageMode.Back);
        }
    }

    protected override void Draw(AnimationPlayer animationPlayer)
    {
        DrawOptions(animationPlayer);
    }
}