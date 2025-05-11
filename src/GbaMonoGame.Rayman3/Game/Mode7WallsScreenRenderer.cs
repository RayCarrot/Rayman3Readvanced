using System.Collections.Generic;
using System.Linq;
using BinarySerializer.Nintendo.GBA;
using GbaMonoGame.TgxEngine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GbaMonoGame.Rayman3;

public class Mode7WallsScreenRenderer : IScreenRenderer
{
    public Mode7WallsScreenRenderer(TgxPlayfieldMode7 playfield, Point wallPoint, Point wallSize, float wallHeight)
    {
        Camera = playfield.Camera;

        // TODO: Dispose shader
        Shader = new BasicEffect(Engine.GraphicsDevice)
        {
            TextureEnabled = true,
        };

        TgxRotscaleLayerMode7 layer = playfield.RotScaleLayers[0];

        CreateTexture(playfield.GfxTileKitManager, layer.TileMap, layer.Width, wallPoint, wallSize);
        CreateMesh(layer.TileMap, layer.Width, layer.Height, wallPoint, wallSize, new Vector3(wallSize.X, wallSize.Y, wallHeight) * Tile.Size);
    }

    public TgxCameraMode7 Camera { get; }
    public BasicEffect Shader { get; }
    public VertexBuffer VertexBuffer { get; set; }
    public IndexBuffer IndexBuffer { get; set; }

    private void CreateTexture(GfxTileKitManager tileKitManager, MapTile[] tileMap, int tileMapWidth, Point wallPoint, Point wallSize)
    {
        MapTile[] wallTiles = new MapTile[wallSize.X * wallSize.Y];
        TileMapHelpers.CopyRegion(
            sourceMap: tileMap,
            sourceWidth: tileMapWidth,
            sourcePoint: wallPoint,
            destMap: wallTiles,
            destWidth: wallSize.X,
            destPoint: Point.Zero,
            regionSize: wallSize);

        // TODO: Dispose
        Texture2D wallTexture = new TiledTexture2D(
            width: wallSize.X,
            height: wallSize.Y,
            tileSet: tileKitManager.TileSet,
            tileMap: wallTiles,
            baseTileIndex: 512,
            palette: tileKitManager.SelectedPalette,
            is8Bit: true,
            ignoreZero: true);

        Shader.Texture = wallTexture;
    }

    private void CreateMesh(MapTile[] tileMap, int tileMapWidth, int tileMapHeight, Point wallPoint, Point wallSize, Vector3 wallBoxSize)
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

        List<VertexPositionTexture> vertices = [];
        List<ushort> indices = [];

        // Helper method to add a face of the box mesh
        void addFace(Vector2 pos, (Vector3 Position, Vector2 TexCoord)[] face)
        {
            // Add vertices
            foreach ((Vector3 v, Vector2 uv) in face)
            {
                vertices.Add(new VertexPositionTexture(
                    position: new Vector3(pos.X, pos.Y, -wallBoxSize.Z * 0.5f) + v * wallBoxSize,
                    textureCoordinate: uv)
                );
            }

            // Add the indices
            indices.AddRange(faceIndices.Select(x => (ushort)(vertexOffset + x)));

            vertexOffset += (ushort)face.Length;
        }

        // Add the wall boxes
        MapTile wallTile = tileMap[wallPoint.X + wallPoint.Y * tileMapWidth];

        bool isWall(int x, int y) => x >= 0 && y >= 0 && x<tileMapWidth && y<tileMapHeight && tileMap[x + y * tileMapWidth].TileIndex == wallTile.TileIndex;

        for (int y = 0; y < tileMapHeight; y++)
        {
            for (int x = 0; x < tileMapWidth; x++)
            {
                if (isWall(x, y))
                {
                    Vector2 pos = new(x * Tile.Size, y * Tile.Size);

                    addFace(pos, topFace);

                    if (!isWall(x - wallSize.X, y))
                    {
                        addFace(pos, leftFace);
                    }
                    if (!isWall(x + wallSize.X, y))
                    {
                        addFace(pos, rightFace);
                    }
                    if (!isWall(x, y - wallSize.Y))
                    {
                        addFace(pos, frontFace);
                    }
                    if (!isWall(x, y + wallSize.Y))
                    {
                        addFace(pos, backFace);
                    }
                }
            }
        }

        // Create the vertex buffer
        VertexBuffer = new VertexBuffer(Engine.GraphicsDevice, VertexPositionTexture.VertexDeclaration, vertices.Count, BufferUsage.WriteOnly);
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