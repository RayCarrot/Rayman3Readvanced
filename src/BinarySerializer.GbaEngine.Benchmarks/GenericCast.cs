using BenchmarkDotNet.Attributes;
using BinarySerializer;

// TODO: CastTo.From seems slightly slower actually
[MemoryDiagnoser]
public class GenericCast
{
    private T NormalCastMethod<T>(string stringValue)
    {
        if (typeof(T) == typeof(int))
        {
            return (T)(object)Int32.Parse(stringValue);
        }
        else
        {
            throw new Exception();
        }
    }

    private T CastToCastMethod<T>(string stringValue)
    {
        if (typeof(T) == typeof(int))
        {
            return CastTo<T>.From(Int32.Parse(stringValue));
        }
        else
        {
            throw new Exception();
        }
    }

    [Benchmark]
    public void NormalCast()
    {
        for (int i = 0; i < 1_000_000; i++)
            _ = NormalCastMethod<int>($"{i}");
    }

    [Benchmark]
    public void CastToCast()
    {
        for (int i = 0; i < 1_000_000; i++)
            _ = CastToCastMethod<int>($"{i}");
    }
}