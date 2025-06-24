using System;
using System.Linq;
using BinarySerializer.Ubisoft.GbaEngine;
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

    public override void Reset()
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

    public override EditStepResult EditStep()
    {
        if (JoyPad.IsButtonJustPressed(GbaInput.A))
        {
            if (_selectedIndex != _prevSelectedIndex)
            {
                _setData(GetSelectedData());
                _prevSelectedIndex = _selectedIndex;
            }

            return EditStepResult.Confirm;
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

    public class Item(string displayName, T data)
    {
        public string DisplayName { get; } = displayName;
        public T Data { get; } = data;
    }
}