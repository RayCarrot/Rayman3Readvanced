#if WINDOWSDX
using System.Windows.Forms;
#else
using NativeFileDialogSharp;
#endif

namespace GbaMonoGame;

public static class FileDialog
{
    public static string OpenFile(string title, FileFilter filter)
    {
        // Force windowed mode for this
        DisplayMode prevDisplayMode = Engine.GameWindow.DisplayMode;
        if (Engine.GameWindow.DisplayMode != DisplayMode.Windowed)
            Engine.GameWindow.DisplayMode = DisplayMode.Windowed;

        try
        {
#if WINDOWSDX
            using OpenFileDialog fileDialog = new();
            fileDialog.Title = title;
            fileDialog.Filter = $"{filter.Description} (*.{filter.FileExtension})|*.{filter.FileExtension}|All files (*.*)|*.*";

            DialogResult result = fileDialog.ShowDialog();

            if (result == DialogResult.OK)
                return fileDialog.FileName;
            else
                return null;
#else
            DialogResult result = Dialog.FileOpen(filterList: filter.FileExtension);

            if (result.IsError)
            {
                Logger.Error("Open file error {0}", result.ErrorMessage);
                return null;
            }

            if (result.IsOk)
                return result.Path;
            else
                return null;
#endif
        }
        finally
        {
            // Restore the display mode
            if (prevDisplayMode != DisplayMode.Windowed)
                Engine.GameWindow.DisplayMode = prevDisplayMode;
        }
    }

    public static string OpenFolder(string title)
    {
        // Force windowed mode for this
        DisplayMode prevDisplayMode = Engine.GameWindow.DisplayMode;
        if (Engine.GameWindow.DisplayMode != DisplayMode.Windowed)
            Engine.GameWindow.DisplayMode = DisplayMode.Windowed;

        try
        {
#if WINDOWSDX
            using FolderBrowserDialog folderDialog = new();
            folderDialog.Description = title;

            DialogResult result = folderDialog.ShowDialog();

            if (result == DialogResult.OK)
                return folderDialog.SelectedPath;
            else
                return null;
#else
            DialogResult result = Dialog.FolderPicker();

            if (result.IsError)
            {
                Logger.Error("Open folder error {0}", result.ErrorMessage);
                return null;
            }

            if (result.IsOk)
                return result.Path;
            else
                return null;
#endif
        }
        finally
        {
            // Restore the display mode
            if (prevDisplayMode != DisplayMode.Windowed)
                Engine.GameWindow.DisplayMode = prevDisplayMode;
        }
    }

    public readonly struct FileFilter(string fileExtension, string description)
    {
        public string FileExtension { get; } = fileExtension;
        public string Description { get; } = description;
    }
}