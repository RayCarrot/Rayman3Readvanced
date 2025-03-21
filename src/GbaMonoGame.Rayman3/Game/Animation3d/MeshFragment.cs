using Microsoft.Xna.Framework.Graphics;

namespace GbaMonoGame.Rayman3;

public class MeshFragment
{
    public MeshFragment(PrimitiveType primitiveType, VertexPositionColorTexture[] vertexData, int primitivesCount, Texture2D texture)
    {
        PrimitiveType = primitiveType;
        VertexData = vertexData;
        PrimitivesCount = primitivesCount;
        Texture = texture;
    }

    public PrimitiveType PrimitiveType { get; }
    public VertexPositionColorTexture[] VertexData { get; }
    public int PrimitivesCount { get; }
    public Texture2D Texture { get; }
}