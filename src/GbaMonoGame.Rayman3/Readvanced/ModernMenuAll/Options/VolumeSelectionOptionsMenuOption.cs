using System;
using System.Linq;

namespace GbaMonoGame.Rayman3.Readvanced;

public class VolumeSelectionOptionsMenuOption : MultiSelectionOptionsMenuOption<float>
{
    public VolumeSelectionOptionsMenuOption(string text, string infoText, Func<float> getVolume, Action<float> setVolume) 
        : base(text, infoText, CreateItems(), CreateGetData(getVolume), CreateSetData(setVolume), CreateGetCustomName())
    {

    }

    private const int MaxStep = 10;

    private static Item[] CreateItems() => Enumerable.Range(0, MaxStep + 1).Select(x => new Item(x.ToString(), x)).ToArray();
    private static Func<Item[], float> CreateGetData(Func<float> getVolume) => _ => getVolume() * MaxStep;
    private static Action<float> CreateSetData(Action<float> setVolume) => data => setVolume(data / MaxStep);
    private static Func<float, string> CreateGetCustomName() => data => $"{data:0.00}";
}