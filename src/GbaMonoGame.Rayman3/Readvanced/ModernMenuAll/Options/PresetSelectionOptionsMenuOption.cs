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
        List<Enum> validPresets = [];
        foreach (OptionsMenuOption option in options)
        {
            Enum[] usedPresets = option.GetUsedPresets();

            // If the option has no presets...
            if (usedPresets.Length == 0)
            {
                // If we have valid presets and the option has at least one of them defined
                if (validPresets.Count != 0 && validPresets.Any(option.HasPresetDefined))
                {
                    // No valid presets, break
                    validPresets.Clear();
                    break;
                }
            }
            else
            {
                // If we have no valid presets yet
                if (validPresets.Count == 0)
                {
                    // Add all the presets
                    validPresets.AddRange(usedPresets);
                }
                else
                {
                    // Enumerate every valid preset
                    foreach (Enum validPreset in validPresets.ToArray())
                    {
                        // Remove if not used by this option
                        if (!usedPresets.Contains(validPreset) && option.HasPresetDefined(validPreset))
                            validPresets.Remove(validPreset);
                    }

                    // Break if we've removed all valid presets
                    if (validPresets.Count == 0)
                        break;
                }
            }
        }

        Enum currentPreset = validPresets.FirstOrDefault();
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
        if (Engine.LocalConfig.Controls.UseModernButtonMapping switch
            {
                true => JoyPad.IsButtonJustPressed(GbaInput.A) || 
                        JoyPad.IsButtonJustPressed(GbaInput.Start),
                false when Rom.Platform is Platform.GBA => JoyPad.IsButtonJustPressed(GbaInput.A),
                false when Rom.Platform is Platform.NGage => NGageJoyPadHelpers.IsConfirmButtonJustPressed(),
                _ => throw new UnsupportedPlatformException()
            })
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
        else if (Engine.LocalConfig.Controls.UseModernButtonMapping switch
                 {
                     true => JoyPad.IsButtonJustPressed(GbaInput.B) ||
                             JoyPad.IsButtonJustPressed(GbaInput.Select),
                     false when Rom.Platform is Platform.GBA => JoyPad.IsButtonJustPressed(GbaInput.B),
                     false when Rom.Platform is Platform.NGage => NGageJoyPadHelpers.IsBackButtonJustPressed(),
                     _ => throw new UnsupportedPlatformException()
                 })
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