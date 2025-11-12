using System.Collections.Generic;
using BinarySerializer.Ubisoft.GbaEngine;

namespace GbaMonoGame;

public static partial class InputManager
{
    private static readonly Dictionary<Input, GbaInput> _gbaInputMapping = new()
    {
        [Input.Gba_A] = GbaInput.A,
        [Input.Gba_B] = GbaInput.B,
        [Input.Gba_Select] = GbaInput.Select,
        [Input.Gba_Start] = GbaInput.Start,
        [Input.Gba_Right] = GbaInput.Right,
        [Input.Gba_Left] = GbaInput.Left,
        [Input.Gba_Up] = GbaInput.Up,
        [Input.Gba_Down] = GbaInput.Down,
        [Input.Gba_R] = GbaInput.R,
        [Input.Gba_L] = GbaInput.L,
    };

    public static GbaInput GetGbaInput(Input input) => _gbaInputMapping[input];
    public static bool TryGetGbaInput(Input input, out GbaInput gbaInput) => _gbaInputMapping.TryGetValue(input, out gbaInput);

    public static GbaInput GetPressedGbaInputs()
    {
        GbaInput inputs = GbaInput.Valid;

        foreach (KeyValuePair<Input, GbaInput> input in _gbaInputMapping)
        {
            if (IsInputPressed(input.Key))
                inputs |= input.Value;
        }

        // Cancel out if opposite directions are pressed
        if ((inputs & (GbaInput.Right | GbaInput.Left)) == (GbaInput.Right | GbaInput.Left))
            inputs &= ~(GbaInput.Right | GbaInput.Left);
        if ((inputs & (GbaInput.Up | GbaInput.Down)) == (GbaInput.Up | GbaInput.Down))
            inputs &= ~(GbaInput.Up | GbaInput.Down);

        return inputs;
    }
}