using System;
using BinarySerializer;
using BinarySerializer.Nintendo.GBA;
using BinarySerializer.Ubisoft.GbaEngine;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.AnimEngine;

namespace GbaMonoGame.Rayman3.Readvanced;

public class ExportSaveSelectionMenuPage : MenuPage
{
    public ExportSaveSelectionMenuPage(
        ModernMenuAll menu, 
        int slot, 
        SaveGameSlot[] saveSlots, 
        Context saveContext, 
        string saveFileName, 
        BinarySerializable saveData) : base(menu)
    {
        Slot = slot;
        SaveSlots = saveSlots;
        SaveContext = saveContext;
        SaveFileName = saveFileName;
        SaveData = saveData;
    }

    public override bool UsesCursor => true;
    public override int BackgroundPalette => 1;
    public override int LineHeight => 20;

    public int Slot { get; }
    public SaveGameSlot[] SaveSlots { get; }
    public Context SaveContext { get; }
    public string SaveFileName { get; }
    public BinarySerializable SaveData { get; }

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
            try
            {
                GbaEngineSettings settings = SaveContext.GetRequiredSettings<GbaEngineSettings>();

                switch (settings.Platform)
                {
                    case Platform.GBA:
                        using (SaveContext)
                        {
                            // Get the save data
                            EEPROM<SaveGame> saveGame = (EEPROM<SaveGame>)SaveData;

                            // Load the Readvanced save which we want to export
                            GameInfo.Load(Slot);

                            // Replace the save slot
                            saveGame.Obj.Slots[SelectedOption].SaveSlot = GameInfo.PersistentInfo;

                            // Write the save file
                            Engine.BeginLoad();
                            FileFactory.Write(SaveContext, SaveFileName, saveGame);
                        }
                        break;

                    case Platform.NGage:
                        using (SaveContext)
                        {
                            // Get the save data
                            NGageSaveGame saveGame = (NGageSaveGame)SaveData;

                            // Load the Readvanced save which we want to export
                            GameInfo.Load(Slot);

                            // Replace the save slot
                            saveGame.Slots[SelectedOption] = GameInfo.PersistentInfo;
                            saveGame.ValidSlots[SelectedOption] = true;

                            // Write the save file
                            Engine.BeginLoad();
                            FileFactory.Write(SaveContext, SaveFileName, saveGame);
                        }
                        break;

                    default:
                        throw new UnsupportedPlatformException();
                }
            }
            catch (Exception ex)
            {
                Engine.MessageManager.EnqueueExceptionMessage(
                    ex: ex,
                    text: "An error occurred when exporting the save.",
                    header: "Exporting save error");
            }

            Menu.ChangePage(new SinglePlayerMenuPage(Menu), NewPageMode.Next);
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