namespace GbaMonoGame.Engine2d;

// Use for messages which use the param as a ref to a value that gets set
public class MessageRefParam<T>
{
    public T Value { get; set; }
}