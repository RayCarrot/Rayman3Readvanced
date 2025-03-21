using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GbaMonoGame.Rayman3;

public class MeshScreenRenderer : IScreenRenderer
{
    public MeshScreenRenderer()
    {
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
        // End previous render batch
        renderer.EndRender();

        // Get the render context
        RenderContext renderContext = screen.RenderOptions.RenderContext;

        // Set the viewport
        Engine.GraphicsDevice.Viewport = renderContext.Viewport;
        
        // Set the culling to clockwise
        Engine.GraphicsDevice.RasterizerState = RasterizerState.CullClockwise;
        
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