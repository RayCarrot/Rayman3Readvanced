using System.Collections.Generic;
using System.Linq;
using BinarySerializer.Nintendo.GBA;
using GbaMonoGame.TgxEngine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GbaMonoGame.Rayman3;

public class Mode7WallsScreenRenderer : IScreenRenderer
{
    public Mode7WallsScreenRenderer(TgxRotscaleLayerMode7 layer, TgxCameraMode7 camera, Texture2D wallTexture)
    {
        Camera = camera;

        // TODO: Dispose shader
        Shader = new BasicEffect(Engine.GraphicsDevice)
        {
            TextureEnabled = true,
        };
        Shader.Texture = wallTexture;

        MapTile wallTile = layer.TileMap[1 + 22 * layer.Width]; // TODO: magic coordinate (1, 22)

        CreateMesh(layer.TileMap.Select(t => t.TileIndex == wallTile.TileIndex).ToArray(), layer.Width, layer.Height, Tile.Size * 3);
    }

    public TgxCameraMode7 Camera { get; }
    public BasicEffect Shader { get; }
    public VertexBuffer VertexBuffer { get; set; }
    public IndexBuffer IndexBuffer { get; set; }

    private void CreateMesh(bool[] boxMap, int boxMapWidth, int boxMapHeight, float boxSize)
    {
        var topFace = new (Vector3 Position, Vector2 TexCoord)[]
        {
            (new Vector3(0.0f, 0.0f, 0), new Vector2(0, 0)),
            (new Vector3(0.0f, 1.0f, 0), new Vector2(0, 1)),
            (new Vector3(1.0f, 1.0f, 0), new Vector2(1, 1)),
            (new Vector3(1.0f, 0.0f, 0), new Vector2(1, 0)),
        };

        var frontFace = new (Vector3 Position, Vector2 TexCoord)[]
        {
            (new Vector3(1.0f, 0.0f, 0.0f), new Vector2(1, 1)),
            (new Vector3(1.0f, 0.0f, 0.5f), new Vector2(1, 0.5f)),
            (new Vector3(0.0f, 0.0f, 0.5f), new Vector2(0, 0.5f)),
            (new Vector3(0.0f, 0.0f, 0.0f), new Vector2(0, 1)),
        };

        var leftFace = new (Vector3 Position, Vector2 TexCoord)[]
        {
            (new Vector3(0.0f, 0.0f, 0.0f), new Vector2(0, 0)),
            (new Vector3(0.0f, 0.0f, 0.5f), new Vector2(0.5f, 0)),
            (new Vector3(0.0f, 1.0f, 0.5f), new Vector2(0.5f, 1)),
            (new Vector3(0.0f, 1.0f, 0.0f), new Vector2(0, 1)),
        };

        var rightFace = new (Vector3 Position, Vector2 TexCoord)[]
        {
            (new Vector3(1.0f, 0.0f, 0.5f), new Vector2(0, 0)),
            (new Vector3(1.0f, 0.0f, 0.0f), new Vector2(0.5f, 0)),
            (new Vector3(1.0f, 1.0f, 0.0f), new Vector2(0.5f, 1)),
            (new Vector3(1.0f, 1.0f, 0.5f), new Vector2(0, 1)),
        };

        var backFace = new (Vector3 Position, Vector2 TexCoord)[]
        {
            (new Vector3(0.0f, 1.0f, 0.0f), new Vector2(0, 0)),
            (new Vector3(0.0f, 1.0f, 0.5f), new Vector2(0, 0.5f)),
            (new Vector3(1.0f, 1.0f, 0.5f), new Vector2(1, 0.5f)),
            (new Vector3(1.0f, 1.0f, 0.0f), new Vector2(1, 0)),
        };

        int[] faceIndices =
        [
            0, 1, 2,
            0, 2, 3
        ];

        int vertexOffset = 0;

        List<VertexPositionNormalTexture> vertices = [];
        List<ushort> indices = [];

        // Helper method to add a face of the box mesh
        void addFace(Vector2 pos, (Vector3 Position, Vector2 TexCoord)[] face)
        {
            // Add vertices
            foreach ((Vector3 v, Vector2 uv) in face)
            {
                vertices.Add(new VertexPositionNormalTexture(
                    new Vector3(pos.X, pos.Y, -boxSize * 0.5f) + v * boxSize,
                    Vector3.Zero, uv)
                );
            }

            // Add the indices
            indices.AddRange(faceIndices.Select(x => (ushort)(vertexOffset + x)));

            vertexOffset += (ushort)face.Length;
        }

        // Add the boxes
        for (int y = 0; y < boxMapHeight; y++)
        {
            for (int x = 0; x < boxMapWidth; x++)
            {
                if (boxMap[x + y * boxMapWidth])
                {
                    Vector2 pos = new(x * Tile.Size, y * Tile.Size);
                    
                    addFace(pos, topFace);
                    addFace(pos, leftFace);
                    addFace(pos, rightFace);
                    addFace(pos, frontFace);
                    addFace(pos, backFace);
                }
            }
        }

        // Create the vertex buffer
        VertexBuffer = new VertexBuffer(Engine.GraphicsDevice, VertexPositionNormalTexture.VertexDeclaration, vertices.Count, BufferUsage.WriteOnly);
        VertexBuffer.SetData(vertices.ToArray());

        // Create the index buffer
        IndexBuffer = new IndexBuffer(Engine.GraphicsDevice, IndexElementSize.SixteenBits, indices.Count, BufferUsage.WriteOnly);
        IndexBuffer.SetData(indices.ToArray());
    }

    public Vector2 GetSize(GfxScreen screen)
    {
        return screen.RenderOptions.RenderContext.Resolution;
    }

    public void Draw(GfxRenderer renderer, GfxScreen screen, Vector2 position, Color color)
    {
        GraphicsDevice graphicsDevice = Engine.GraphicsDevice;

        // Begin rendering the mesh, culling clockwise
        renderer.BeginMeshRender(screen.RenderOptions, RasterizerState.CullClockwise);

        // Update the shader
        Shader.Projection = Camera.ViewProjectionMatrix;

        // Set the mesh data
        graphicsDevice.SetVertexBuffer(VertexBuffer);
        graphicsDevice.Indices = IndexBuffer;
        graphicsDevice.DepthStencilState = DepthStencilState.Default;

        // Draw the mesh
        EffectPassCollection passes = Shader.CurrentTechnique.Passes;
        foreach (EffectPass pass in passes)
        {
            pass.Apply();
            graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, IndexBuffer.IndexCount / 3);
        }
    }
}