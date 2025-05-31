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

        Shader = Engine.FixContentManager.Load<Effect>(GbaMonoGame.Assets.VertexShaderFog);

        TgxRotscaleLayerMode7 layer = playfield.RotScaleLayers[0];

        // Create the textures (use a separate one for the sides since we want it to be smaller)
        Texture2D topTexture = CreateTexture(playfield.GfxTileKitManager, layer.TileMap, layer.Width, wallPoint, wallSize);
        Texture2D sideTexture = Engine.FrameContentManager.Load<Texture2D>(Assets.Mode7WallSideTexture);

        Vector3 wallBoxSize = new Vector3(wallSize.X, wallSize.Y, wallHeight) * Tile.Size;

        MeshFragments =
        [
            CreateMesh(layer.TileMap, layer.Width, layer.Height, wallPoint, wallSize, wallBoxSize, topTexture, true),
            CreateMesh(layer.TileMap, layer.Width, layer.Height, wallPoint, wallSize, wallBoxSize, sideTexture, false)
        ];

        Engine.DisposableResources.Register(Shader);
        Engine.DisposableResources.Register(topTexture);
    }

    public TgxCameraMode7 Camera { get; }
    public Effect Shader { get; }
    public MeshFragment[] MeshFragments { get; }

    private static Texture2D CreateTexture(GfxTileKitManager tileKitManager, MapTile[] tileMap, int tileMapWidth, Point wallPoint, Point wallSize)
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

        return new TiledTexture2D(
            width: wallSize.X,
            height: wallSize.Y,
            tileSet: tileKitManager.TileSet,
            tileMap: wallTiles,
            baseTileIndex: 512,
            palette: tileKitManager.SelectedPalette,
            is8Bit: true,
            ignoreZero: true);
    }

    private static MeshFragment CreateMesh(
        MapTile[] tileMap, 
        int tileMapWidth, 
        int tileMapHeight, 
        Point wallPoint, 
        Point wallSize, 
        Vector3 wallBoxSize,
        Texture2D texture,
        bool isTop)
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
            (new Vector3(1.0f, 0.0f, 0.0f), new Vector2(1, 0.5f)),
            (new Vector3(1.0f, 0.0f, 0.5f), new Vector2(1, 1)),
            (new Vector3(0.0f, 0.0f, 0.5f), new Vector2(0, 1)),
            (new Vector3(0.0f, 0.0f, 0.0f), new Vector2(0, 0.5f)),
        };

        var leftFace = new (Vector3 Position, Vector2 TexCoord)[]
        {
            (new Vector3(0.0f, 0.0f, 0.0f), new Vector2(1, 0.5f)),
            (new Vector3(0.0f, 0.0f, 0.5f), new Vector2(1, 1)),
            (new Vector3(0.0f, 1.0f, 0.5f), new Vector2(0, 1)),
            (new Vector3(0.0f, 1.0f, 0.0f), new Vector2(0, 0.5f)),
        };

        var rightFace = new (Vector3 Position, Vector2 TexCoord)[]
        {
            (new Vector3(1.0f, 0.0f, 0.5f), new Vector2(0, 0)),
            (new Vector3(1.0f, 0.0f, 0.0f), new Vector2(0, 0.5f)),
            (new Vector3(1.0f, 1.0f, 0.0f), new Vector2(1, 0.5f)),
            (new Vector3(1.0f, 1.0f, 0.5f), new Vector2(1, 0)),
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

        bool isWall(int x, int y) => 
            x >= 0 && y >= 0 && 
            x < tileMapWidth && y < tileMapHeight && 
            tileMap[x + y * tileMapWidth].TileIndex == wallTile.TileIndex;

        for (int y = 0; y < tileMapHeight; y++)
        {
            for (int x = 0; x < tileMapWidth; x++)
            {
                if (isWall(x, y))
                {
                    Vector2 pos = new(x * Tile.Size, y * Tile.Size);

                    if (isTop)
                    {
                        addFace(pos, topFace);
                    }
                    else
                    {
                        if (!isWall(x - wallSize.X, y))
                            addFace(pos, leftFace);

                        if (!isWall(x + wallSize.X, y))
                            addFace(pos, rightFace);

                        if (!isWall(x, y - wallSize.Y))
                            addFace(pos, frontFace);

                        if (!isWall(x, y + wallSize.Y))
                            addFace(pos, backFace);
                    }
                }
            }
        }

        // Create the vertex buffer
        VertexBuffer vertexBuffer = new(Engine.GraphicsDevice, VertexPositionTexture.VertexDeclaration, vertices.Count, BufferUsage.WriteOnly);
        vertexBuffer.SetData(vertices.ToArray());

        // Create the index buffer
        IndexBuffer indexBuffer = new(Engine.GraphicsDevice, IndexElementSize.SixteenBits, indices.Count, BufferUsage.WriteOnly);
        indexBuffer.SetData(indices.ToArray());

        Engine.DisposableResources.Register(vertexBuffer);
        Engine.DisposableResources.Register(indexBuffer);

        return new MeshFragment(PrimitiveType.TriangleList, vertexBuffer, indexBuffer, indices.Count / 3, texture);
    }

    public Vector2 GetSize(GfxScreen screen)
    {
        return screen.RenderOptions.RenderContext.Resolution;
    }

    public void Draw(GfxRenderer renderer, GfxScreen screen, Vector2 position, Color color)
    {
        // Begin rendering the mesh, culling clockwise
        renderer.BeginMeshRender(screen.RenderOptions, RasterizerState.CullClockwise);

        Engine.GraphicsDevice.BlendState = BlendState.NonPremultiplied;
        Engine.GraphicsDevice.BlendFactor = Color.White;

        // Update the shader
        Shader.Parameters["WorldViewProj"].SetValue(Camera.ViewProjectionMatrix);
        Shader.Parameters["FarPlane"].SetValue(Camera.CameraFar);

        // Draw each mesh fragment
        foreach (MeshFragment meshFragment in MeshFragments)
        {
            EffectPassCollection passes = Shader.CurrentTechnique.Passes;
            foreach (EffectPass pass in passes)
            {
                pass.Apply();
                meshFragment.Draw(Engine.GraphicsDevice);
            }
        }
    }
}