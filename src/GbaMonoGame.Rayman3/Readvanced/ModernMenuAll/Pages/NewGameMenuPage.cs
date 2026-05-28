using System;
using System.IO;
using System.Linq;
using BinarySerializer;
using BinarySerializer.Nintendo.GBA;
using BinarySerializer.Ubisoft.GbaEngine;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.AnimEngine;

namespace GbaMonoGame.Rayman3.Readvanced;

public class NewGameMenuPage : MenuPage
{
    public NewGameMenuPage(ModernMenuAll menu, int slot) : base(menu)
    {
        Slot = slot;
    }

    public override bool UsesCursor => true;
    public override int BackgroundPalette => 1;
    public override int LineHeight => 20;

    public int Slot { get; }

    protected override void Init()
    {
        AddOption(new ActionMenuOption("START NEW GAME", () =>
        {
            CursorClick(() =>
            {
                Engine.Sem.ReplaceAllSongs(Rayman3SoundEvent.None, 1);
                FadeOut(2, () =>
                {
                    Engine.Sem.StopAllSongs();

                    // Create a new game
                    Engine.FrameMngr.SetNextFrame(new Act1());
                    Rayman3.GameInfo.ResetPersistentInfo();

                    Rayman3.GameInfo.StartPlayTime();

                    Gfx.FadeControl = new FadeControl(FadeMode.BrightnessDecrease);
                    Gfx.Fade = AlphaCoefficient.Max;

                    Rayman3.GameInfo.CurrentSlot = Slot;
                });
            });
        }));
        AddOption(new ActionMenuOption("IMPORT FROM GBA SAVE", () =>
        {
            string saveFilePath = Engine.FileDialog.OpenFile("Select GBA save file", new FileDialogManager.FileFilter("sav", "GBA Save"));

            if (saveFilePath == null)
                return;

            try
            {
                string saveDirPath = Path.GetDirectoryName(saveFilePath) ?? String.Empty;
                string saveFileName = Path.GetFileName(saveFilePath);

                // Create serializer settings for the save file
                SerializerSettings serializerSettings = new() { IgnoreCacheOnRead = true };

                // Create a new context for reading the save
                using Context context = new(saveDirPath,
                    settings: serializerSettings,
                    systemLogger: BinarySerializerSystemLogger.Create());

                context.AddSettings(new GbaEngineSettings
                {
                    Game = Game.Rayman3,
                    Platform = Platform.GBA,
                });

                context.AddFile(new LinearFile(context, saveFileName));

                Engine.App.BeginLoad();
                EEPROM<SaveGame> saveGame = FileFactory.Read<EEPROM<SaveGame>>(context, saveFileName,
                    (_, x) => x.Pre_Size = EEPROM<SaveGame>.EEPROMSize.Kbit_4);

                SaveGameSlot[] slots = saveGame.Obj.Slots.Select(x => x.SaveSlot).ToArray();
                Menu.ChangePage(new ImportSaveSelectionMenuPage(Menu, Slot, slots), NewPageMode.Next);
            }
            catch (Exception ex)
            {
                Engine.Messages.EnqueueExceptionMessage(
                    ex: ex,
                    text: "The selected save file is either not valid or could not be read properly.",
                    header: "Invalid save");
            }
        }));
        AddOption(new ActionMenuOption("IMPORT FROM N-GAGE SAVE", () =>
        {
            string saveFilePath = Engine.FileDialog.OpenFile("Select N-Gage save file", new FileDialogManager.FileFilter("dat", "N-Gage Save"));

            if (saveFilePath == null)
                return;

            try
            {
                string saveDirPath = Path.GetDirectoryName(saveFilePath) ?? String.Empty;
                string saveFileName = Path.GetFileName(saveFilePath);

                // Create serializer settings for the save file
                SerializerSettings serializerSettings = new() { IgnoreCacheOnRead = true };

                // Create a new context for reading the save
                using Context context = new(saveDirPath,
                    settings: serializerSettings,
                    systemLogger: BinarySerializerSystemLogger.Create());

                context.AddSettings(new GbaEngineSettings
                {
                    Game = Game.Rayman3,
                    Platform = Platform.NGage,
                });

                context.AddFile(new LinearFile(context, saveFileName));

                Engine.App.BeginLoad();
                NGageSaveGame saveGame = FileFactory.Read<NGageSaveGame>(context, saveFileName);

                SaveGameSlot[] slots = saveGame.Slots.Select((x, i) => saveGame.ValidSlots[i] ? x : null).ToArray();
                Menu.ChangePage(new ImportSaveSelectionMenuPage(Menu, Slot, slots), NewPageMode.Next);
            }
            catch (Exception ex)
            {
                Engine.Messages.EnqueueExceptionMessage(
                    ex: ex,
                    text: "The selected save file is either not valid or could not be read properly.",
                    header: "Invalid save");
            }
        }));
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
            if (Options[SelectedOption] is ActionMenuOption action)
                action.Invoke();
        }
        else if (Engine.JoyPad.IsButtonJustPressed(Rayman3Input.MenuBack))
        {
            Menu.ChangePage(new SinglePlayerMenuPage(Menu), NewPageMode.Back);
        }
    }

    protected override void Draw(AnimationPlayer animationPlayer)
    {
        DrawOptions(animationPlayer);
    }
}