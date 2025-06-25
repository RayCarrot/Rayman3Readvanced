using System;
using System.Collections.Generic;
using System.Linq;
using BinarySerializer.Ubisoft.GbaEngine;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;

namespace GbaMonoGame.Rayman3.Readvanced;

public class PresetSelectionOptionsMenuOption : OptionsMenuOption
{
    public PresetSelectionOptionsMenuOption(string text, string infoText, PresetItem[] presetItems) : base(text, infoText)
    {
        _presetItems = presetItems;
    }

    private readonly PresetItem[] _presetItems;

    private string[] _displayNames;
    private bool _hasCustom;

    private int _prevSelectedIndex;
    private int _selectedIndex;

    public override bool ShowArrows => true;

    private void UpdateSelection()
    {
        ValueTextObject.Text = _displayNames[_selectedIndex];
        UpdateArrowPositions();
    }

    public Enum GetSelectedPreset()
    {
        if (_hasCustom)
        {
            if (_selectedIndex == 0)
                return null;
            else
                return _presetItems[_selectedIndex - 1].Preset;
        }
        else
        {
            return _presetItems[_selectedIndex].Preset;
        }
    }

    public override void Reset(IReadOnlyList<OptionsMenuOption> options)
    {
        Enum currentPreset = null;
        foreach (OptionsMenuOption option in options)
        {
            if (option is PresetSelectionOptionsMenuOption)
                continue;

            Enum usedPreset = option.GetUsedPreset();

            if (usedPreset == null)
            {
                currentPreset = null;
                break;
            }

            if (currentPreset == null)
            {
                currentPreset = usedPreset;
            }
            else if (!Equals(currentPreset, usedPreset))
            {
                currentPreset = null;
                break;
            }
        }

        int itemIndex = Array.FindIndex(_presetItems, x => (x.Preset == null && currentPreset == null) || x.Preset?.Equals(currentPreset) == true);

        if (itemIndex == -1)
        {
            _selectedIndex = 0;
            _hasCustom = true;
            _displayNames = _presetItems.Select(x => x.DisplayName).Prepend("CUSTOM").ToArray();
        }
        else
        {
            _selectedIndex = itemIndex;
            _hasCustom = false;
            _displayNames = _presetItems.Select(x => x.DisplayName).ToArray();
        }

        _prevSelectedIndex = _selectedIndex;
        UpdateSelection();
    }

    public override EditStepResult EditStep(IReadOnlyList<OptionsMenuOption> options)
    {
        if (JoyPad.IsButtonJustPressed(GbaInput.A))
        {
            if (_selectedIndex != _prevSelectedIndex)
            {
                // Apply preset
                Enum selectedPreset = GetSelectedPreset();
                if (selectedPreset != null)
                    foreach (OptionsMenuOption option in options)
                        option.ApplyFromPreset(options, selectedPreset);

                _prevSelectedIndex = _selectedIndex;
            }

            return EditStepResult.Apply;
        }
        else if (JoyPad.IsButtonJustPressed(GbaInput.B))
        {
            return EditStepResult.Cancel;
        }
        else if (JoyPad.IsButtonJustPressed(GbaInput.Left))
        {
            _selectedIndex--;
            if (_selectedIndex < 0)
                _selectedIndex = _displayNames.Length - 1;

            UpdateSelection();
            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__MenuMove);
        }
        else if (JoyPad.IsButtonJustPressed(GbaInput.Right))
        {
            _selectedIndex++;
            if (_selectedIndex >= _displayNames.Length)
                _selectedIndex = 0;

            UpdateSelection();
            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__MenuMove);
        }

        return EditStepResult.None;
    }

    public class PresetItem(string displayName, Enum preset)
    {
        public string DisplayName { get; } = displayName;
        public Enum Preset { get; } = preset;
    }
}