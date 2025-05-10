using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GbaMonoGame.Rayman3;

public class MeshScreenRenderer : IScreenRenderer
{
    public MeshScreenRenderer()
    {
        // TODO: Dispose shader
        Shader = new BasicEffect(Engine.GraphicsDevice)
        {
            TextureEnabled = true,
        };
    }

    public BasicEffect Shader { get; }
    public Vector3 Scale { get; set; }
    public Vector3 Rotation { get; set; }
    public Vector3 Position { get; set; }
    public MeshFragment[] MeshFragments { get; set; }

    public Vector2 GetSize(GfxScreen screen)
    {
        return screen.RenderOptions.RenderContext.Resolution;
    }

    public void Draw(GfxRenderer renderer, GfxScreen screen, Vector2 position, Color color)
    {
        // Begin rendering the mesh, culling clockwise
        renderer.BeginMeshRender(screen.RenderOptions, RasterizerState.CullClockwise);

        RenderContext renderContext = screen.RenderOptions.RenderContext;

        // Update the shader
        Shader.Projection = Matrix.CreateOrthographicOffCenter(0, renderContext.Viewport.Width, renderContext.Viewport.Height, 0, -1000, 1000);
        Shader.View = Matrix.CreateScale(renderContext.Scale);
        Shader.World = Matrix.CreateScale(Scale) *
                       Matrix.CreateRotationZ(Rotation.Z) *
                       Matrix.CreateRotationY(Rotation.Y) *
                       Matrix.CreateRotationX(Rotation.X) *
                       Matrix.CreateTranslation(Position);

        // Draw each mesh fragment
        foreach (MeshFragment meshFragment in MeshFragments)
        {
            EffectPassCollection passes = Shader.CurrentTechnique.Passes;
            foreach (EffectPass pass in passes)
            {
                pass.Apply();

                Engine.GraphicsDevice.Textures[0] = meshFragment.Texture;

                Engine.GraphicsDevice.DrawUserPrimitives(
                    meshFragment.PrimitiveType,
                    meshFragment.VertexData,
                    0,
                    meshFragment.PrimitivesCount);
            }
        }
    }
}