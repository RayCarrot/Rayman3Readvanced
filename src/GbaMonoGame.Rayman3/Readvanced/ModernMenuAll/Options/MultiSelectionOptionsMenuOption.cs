using System;
using System.Collections.Generic;
using System.Linq;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;

namespace GbaMonoGame.Rayman3.Readvanced;

public class MultiSelectionOptionsMenuOption<T> : OptionsMenuOption
{
    public MultiSelectionOptionsMenuOption(string text, string infoText, Item[] items, Func<Item[], T> getData, Action<T> setData, Func<T, string> getCustomName) : base(text, infoText)
    {
        _items = items;
        _getData = getData;
        _setData = setData;
        _getCustomName = getCustomName;
    }

    private readonly Item[] _items;
    private readonly Func<Item[], T> _getData;
    private readonly Action<T> _setData;
    private readonly Func<T, string> _getCustomName;

    private string[] _displayNames;
    private T _customData;
    private bool _hasCustom;

    private int _prevSelectedIndex;
    private int _selectedIndex;

    public override bool ShowArrows => true;

    private void UpdateSelection()
    {
        ValueTextObject.Text = _displayNames[_selectedIndex];
        UpdateArrowPositions();
    }

    public T GetSelectedData()
    {
        if (_hasCustom)
        {
            if (_selectedIndex == 0)
                return _customData;
            else
                return _items[_selectedIndex - 1].Data;
        }
        else
        {
            return _items[_selectedIndex].Data;
        }
    }

    public override void Reset(IReadOnlyList<OptionsMenuOption> options)
    {
        T selectedData = _getData(_items);
        int itemIndex = Array.FindIndex(_items, x => (x.Data == null && selectedData == null) || x.Data?.Equals(selectedData) == true);

        if (itemIndex == -1)
        {
            _customData = selectedData;
            _selectedIndex = 0;
            _hasCustom = true;
            string name = _getCustomName(selectedData);
            _displayNames = _items.Select(x => x.DisplayName).Prepend(name == null ? "CUSTOM" : $"CUSTOM ({name})").ToArray();
        }
        else
        {
            _selectedIndex = itemIndex;
            _hasCustom = false;
            _displayNames = _items.Select(x => x.DisplayName).ToArray();
        }

        _prevSelectedIndex = _selectedIndex;
        UpdateSelection();
    }

    public override bool HasPresetDefined(Enum preset)
    {
        return _items.Any(x => x.Presets.Contains(preset));
    }

    public override Enum[] GetUsedPresets()
    {
        if (_hasCustom)
        {
            if (_selectedIndex == 0)
                return [];
            else
                return _items[_selectedIndex - 1].Presets;
        }
        else
        {
            return _items[_selectedIndex].Presets;
        }
    }

    public override void ApplyFromPreset(IReadOnlyList<OptionsMenuOption> options, Enum preset)
    {
        int index = Array.FindIndex(_items, x => x.Presets.Contains(preset));
        if (index != -1)
        {
            _setData(_items[index].Data);
            Reset(options);
        }
    }

    public override EditStepResult EditStep(IReadOnlyList<OptionsMenuOption> options)
    {
        if (JoyPad.IsButtonJustPressed(Rayman3Input.MenuConfirm))
        {
            if (_selectedIndex != _prevSelectedIndex)
            {
                _setData(GetSelectedData());
                _prevSelectedIndex = _selectedIndex;
            }

            return EditStepResult.Apply;
        }
        else if (JoyPad.IsButtonJustPressed(Rayman3Input.MenuBack))
        {
            return EditStepResult.Cancel;
        }
        else if (JoyPad.IsButtonJustPressed(Rayman3Input.MenuLeft))
        {
            _selectedIndex--;
            if (_selectedIndex < 0)
                _selectedIndex = _displayNames.Length - 1;

            UpdateSelection();
            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__MenuMove);
        }
        else if (JoyPad.IsButtonJustPressed(Rayman3Input.MenuRight))
        {
            _selectedIndex++;
            if (_selectedIndex >= _displayNames.Length)
                _selectedIndex = 0;

            UpdateSelection();
            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__MenuMove);
        }

        return EditStepResult.None;
    }

    public class Item(string displayName, T data, params Enum[] presets)
    {
        public string DisplayName { get; } = displayName;
        public T Data { get; } = data;
        public Enum[] Presets { get; } = presets;
    }
}