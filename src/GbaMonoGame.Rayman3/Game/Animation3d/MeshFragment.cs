using Microsoft.Xna.Framework.Graphics;

namespace GbaMonoGame.Rayman3;

public class MeshFragment
{
    public MeshFragment(PrimitiveType primitiveType, VertexBuffer vertexBuffer, IndexBuffer indexBuffer, int primitivesCount, Texture2D texture)
    {
        PrimitiveType = primitiveType;
        VertexBuffer = vertexBuffer;
        IndexBuffer = indexBuffer;
        PrimitivesCount = primitivesCount;
        Texture = texture;
    }

    public PrimitiveType PrimitiveType { get; }
    public VertexBuffer VertexBuffer { get; } // TODO: Dispose
    public IndexBuffer IndexBuffer { get; } // TODO: Dispose
    public int PrimitivesCount { get; }
    public Texture2D Texture { get; }

    public void Draw(GraphicsDevice graphicsDevice)
    {
        graphicsDevice.Textures[0] = Texture;
        graphicsDevice.SetVertexBuffer(VertexBuffer);

        if (IndexBuffer == null)
        {
            graphicsDevice.DrawPrimitives(PrimitiveType, 0, PrimitivesCount);
        }
        else
        {
            graphicsDevice.Indices = IndexBuffer;
            graphicsDevice.DrawIndexedPrimitives(PrimitiveType, 0, 0, PrimitivesCount);
        }
    }
}