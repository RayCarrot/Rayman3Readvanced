using System;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GbaMonoGame;

public class AssetManager : IDisposable
{
    public AssetManager(IServiceProvider serviceProvider)
    {
        GraphicsDevice = ((IGraphicsDeviceService)serviceProvider.GetService(typeof(IGraphicsDeviceService)))!.GraphicsDevice;
        FixContentManager = new ContentManager(serviceProvider, Paths.AssetsDirectoryName);
        FrameContentManager = new ContentManager(serviceProvider, Paths.AssetsDirectoryName);
        TextureCache = new Cache<long, Texture2D>();
        BinaryTextureCache = new BinaryCache<Texture2D>();
        BinaryPaletteCache = new BinaryCache<Palette>();
    }

    /// <summary>
    /// The graphics device to use for creating textures.
    /// </summary>
    public GraphicsDevice GraphicsDevice { get; }

    /// <summary>
    /// The fixed content manager to load contents which should stay loaded through the entire lifecycle of the game.
    /// </summary>
    public ContentManager FixContentManager { get; }

    /// <summary>
    /// The frame content manager to load contents which should be unloaded when changing the current <see cref="Frame"/>.
    /// </summary>
    public ContentManager FrameContentManager { get; }

    public Cache<long, Texture2D> TextureCache { get; }
    public BinaryCache<Texture2D> BinaryTextureCache { get; }
    public BinaryCache<Palette> BinaryPaletteCache { get; }

    public void UnloadFrameCache()
    {
        FrameContentManager.Unload();
        TextureCache.Clear();
        BinaryTextureCache.Clear();
        BinaryPaletteCache.Clear();
    }

    public void UnloadAllCache()
    {
        FixContentManager.Unload();
        FrameContentManager.Unload();
        TextureCache.Clear();
        BinaryTextureCache.Clear();
        BinaryPaletteCache.Clear();
    }

    public void Dispose()
    {
        GraphicsDevice.Dispose();
        FixContentManager.Dispose();
        FrameContentManager.Dispose();
    }
}