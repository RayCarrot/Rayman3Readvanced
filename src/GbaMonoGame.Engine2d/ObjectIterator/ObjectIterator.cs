using System.Collections;
using System.Collections.Generic;

namespace GbaMonoGame.Engine2d;

public abstract class ObjectIterator<T> : IEnumerable<T>, IEnumerator<T>
    where T : GameObject
{
    protected ObjectIterator(Scene2D scene, byte[] objectTable)
    {
        KnotManager = scene.KnotManager;
        ObjectTable = objectTable;
    }

    public KnotManager KnotManager { get; }
    public byte[] ObjectTable { get; }

    public int Index { get; protected set; }
    public T Current { get; protected set; }
    object IEnumerator.Current => Current;

    public IEnumerator<T> GetEnumerator() => this;
    IEnumerator IEnumerable.GetEnumerator() => this;

    public virtual bool MoveNext()
    {
        int length = ObjectTable?.Length ?? KnotManager.GameObjectsCount;

        if (Index < length)
        {
            int id = ObjectTable == null ? Index : ObjectTable[Index];
            Current = (T)KnotManager.GetGameObject(id);
            Index++;
            return true;
        }
        else
        {
            Index = length + 1;
            Current = default;
            return false;
        }
    }

    public virtual void Reset()
    {
        Index = 0;
        Current = default;
    }

    public void Dispose() { }
}