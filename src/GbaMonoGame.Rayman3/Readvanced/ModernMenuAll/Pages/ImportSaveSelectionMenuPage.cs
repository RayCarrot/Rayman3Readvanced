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
                GameInfo.Load(save);

            ModernMenuAll.Slot slot = save == null || GameInfo.PersistentInfo.Lives == 0
                ? null
                : new ModernMenuAll.Slot(GameInfo.GetTotalDeadLums(), GameInfo.GetTotalDeadCages(), GameInfo.PersistentInfo.Lives);

            AddOption(new SlotMenuOption(slot));
        }
    }

    protected override void Step_Active()
    {
        if (JoyPad.IsButtonJustPressed(Rayman3Input.MenuUp))
        {
            SetSelectedOption(SelectedOption - 1);
        }
        else if (JoyPad.IsButtonJustPressed(Rayman3Input.MenuDown))
        {
            SetSelectedOption(SelectedOption + 1);
        }
        else if (JoyPad.IsButtonJustPressed(Rayman3Input.MenuConfirm))
        {
            if (Menu.Slots[SelectedOption] != null)
            {
                CursorClick(() =>
                {
                    SoundEventsManager.ReplaceAllSongs(Rayman3SoundEvent.None, 1);
                    FadeOut(2, () =>
                    {
                        SoundEventsManager.StopAllSongs();

                        // Load the game
                        GameInfo.Load(SaveSlots[SelectedOption]);
                        GameInfo.GotoLastSaveGame();

                        Gfx.FadeControl = new FadeControl(FadeMode.BrightnessDecrease);
                        Gfx.Fade = AlphaCoefficient.Max;

                        GameInfo.CurrentSlot = Slot;

                        // Save so the slot gets created
                        GameInfo.Save(Slot);
                    });
                });
            }
            else
            {
                InvalidCursorClick();
            }
        }
        else if (JoyPad.IsButtonJustPressed(Rayman3Input.MenuBack))
        {
            Menu.ChangePage(new NewGameMenuPage(Menu, Slot), NewPageMode.Back);
        }
    }

    protected override void Draw(AnimationPlayer animationPlayer)
    {
        DrawOptions(animationPlayer);
    }
}