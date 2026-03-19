using System.Collections.Generic;

namespace GbaMonoGame.Rayman3;

public class Text
{
    public Text(IReadOnlyList<string> lines)
    {
        Lines = lines;
    }

    public IReadOnlyList<string> Lines { get; }
}