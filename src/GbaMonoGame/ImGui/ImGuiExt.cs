using System;
using ImGuiNET;

namespace GbaMonoGame;

public static class ImGuiExt
{
    public static bool Checkbox(string label, bool isChecked)
    {
        ImGui.Checkbox(label, ref isChecked);
        return isChecked;
    }

    public static T EnumCombo<T>(string label, T currentValue)
        where T : struct, Enum
    {
        if (ImGui.BeginCombo(label, currentValue.ToString()))
        {
            foreach (T value in Enum.GetValues<T>())
            {
                bool isSelected = currentValue.Equals(value);

                if (ImGui.Selectable(value.ToString(), isSelected))
                    currentValue = value;
            }

            ImGui.EndCombo();
        }

        return currentValue;
    }
}