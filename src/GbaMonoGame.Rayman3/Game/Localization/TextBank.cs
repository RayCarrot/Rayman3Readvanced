using System.Collections.Generic;

namespace GbaMonoGame.Rayman3;

public class TextBank
{
    public TextBank(IReadOnlyList<Text> texts)
    {
        Texts = texts;
    }

    public IReadOnlyList<Text> Texts { get; }
}