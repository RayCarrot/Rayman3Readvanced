using System.Collections.Generic;
using System.Linq;
using BinarySerializer.Nintendo.GBA;
using GbaMonoGame.TgxEngine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GbaMonoGame.Rayman3;

public class Mode7WallsScreenRenderer : IScreenRenderer
{
    public MapTile[] TileMap { get; }
    public Texture2D Texture { get; }
    public TgxPlayfieldMode7 Playfield { get; }

    private VertexBuffer vertexBuffer;
    private IndexBuffer indexBuffer;
    private BasicEffect effect;

    struct VertexPositionNormal
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector2 UV;

        public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration(
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
            new VertexElement(24, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0)

        );

        public VertexPositionNormal(Vector3 position, Vector3 normal, Vector2 uv)
        {
            Position = position;
            Normal = normal;
            UV = uv;
        }
    }

    public Mode7WallsScreenRenderer(MapTile[] tileMap, Texture2D texture, TgxPlayfieldMode7 playfield)
    {
        TileMap = tileMap;
        Texture = texture;
        Playfield = playfield;

        var tileLayer = playfield.RotScaleLayers[0];
        var wallTile = tileLayer.TileMap[1 + 22 * tileLayer.Width]; // TODO: magic coordinate (1, 22)

        CreateMesh(tileLayer.TileMap.Select(t=>t.TileIndex == wallTile.TileIndex).ToArray(), tileLayer.Width, tileLayer.Height, Constants.TileSize*3);
    }

    public Vector2 GetSize(GfxScreen screen)
    {
        return screen.RenderOptions.RenderContext.Resolution;
    }

    private void CreateMesh(bool[] boxMap, int boxMapWidth, int boxMapHeight, float boxSize)
    {
        int boxCount = boxMap.Count(b => b);

        var topFace = new (Vector3 Position, Vector2 TexCoord)[]
        {
            (new Vector3(0.0f, 0.0f, 0), new Vector2(0, 0)),
            (new Vector3(0.0f, 1.0f, 0), new Vector2(0, 1)),
            (new Vector3(1.0f, 1.0f, 0), new Vector2(1, 1)),
            (new Vector3(1.0f, 0.0f, 0), new Vector2(1, 0)),
        };

        var frontFace = new (Vector3 Position, Vector2 TexCoord)[]
         {
            (new Vector3(0.0f, 0.0f, 0.0f), new Vector2(0, 1)),
            (new Vector3(0.0f, 0.0f, 0.5f), new Vector2(0, 0.5f)),
            (new Vector3(1.0f, 0.0f, 0.5f), new Vector2(1, 0.5f)),
            (new Vector3(1.0f, 0.0f, 0.0f), new Vector2(1, 1)),
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


        var faceIndices = new ushort[]
        {
            0, 1, 2, 0, 2, 3,
        };

        ushort vertexOffset = 0;

        var vertices = new List<VertexPositionNormal>();
        var indices = new List<ushort>();

        void addFace(Vector2 pos, (Vector3 Position, Vector2 TexCoord)[] face)
        {
            foreach (var (v, uv) in face)
            {
                vertices.Add(new VertexPositionNormal(
                    new Vector3(pos.X, pos.Y, -boxSize*0.5f) + v * boxSize,
                    Vector3.Zero, uv)
                );
            }

            foreach (var idx in faceIndices)
            {
                indices.Add((ushort)(vertexOffset + idx));
            }

            vertexOffset += (ushort)face.Length;
        }

        for (int i = 0; i < boxMapWidth; i++)
        {
            for (int j = 0; j < boxMapHeight; j++)
            {
                if (boxMap[i + j * boxMapWidth])
                {
                    Vector2 pos = new Vector2(i * Constants.TileSize, j * Constants.TileSize);
                    addFace(pos, topFace);
                    addFace(pos, leftFace);
                    addFace(pos, rightFace);
                    addFace(pos, frontFace);
                    addFace(pos, backFace);
                }
            }
        }

        GraphicsDevice graphicsDevice = Engine.GraphicsDevice;

        vertexBuffer = new VertexBuffer(graphicsDevice, VertexPositionNormal.VertexDeclaration, vertices.Count, BufferUsage.WriteOnly);
        vertexBuffer.SetData(vertices.ToArray());

        indexBuffer = new IndexBuffer(graphicsDevice, IndexElementSize.SixteenBits, indices.Count, BufferUsage.WriteOnly);
        indexBuffer.SetData(indices.ToArray());

        effect = new BasicEffect(graphicsDevice)
        {
            TextureEnabled = true,
            LightingEnabled = false,
            VertexColorEnabled = false
        };
        effect.Texture = Texture;
    }

    public void Draw(GfxRenderer renderer, GfxScreen screen, Vector2 position, Color color)
    {
        GraphicsDevice graphicsDevice = Engine.GraphicsDevice;

        graphicsDevice.SetVertexBuffer(vertexBuffer);
        graphicsDevice.Indices = indexBuffer;
        graphicsDevice.DepthStencilState = DepthStencilState.Default;

        effect.World = Matrix.Identity;
        effect.View = Matrix.Identity; // If viewProjection baked
        effect.Projection = Playfield.Camera.ViewProjectionMatrix;

        foreach (var pass in effect.CurrentTechnique.Passes)
        {
            pass.Apply();
            graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, indexBuffer.IndexCount / 3);
        }

        /*        screen.RenderOptions.WorldViewProj = Playfield.Camera.ViewProjectionMatrix;
        renderer.BeginRender(screen.RenderOptions);

        for (int i = 0; i < Playfield.PhysicalLayer.Width; i++) {
            for (int j = 0; j < Playfield.PhysicalLayer.Height; j++) {
                if (i==0||i==Playfield.PhysicalLayer.Width-1 || j==0||j==Playfield.PhysicalLayer.Height-1) {
                    renderer.Draw(Texture, new Vector2(i*Constants.TileSize*3,j*Constants.TileSize*3));
                }
            }
        }
        renderer.EndRender();*/
    }
}