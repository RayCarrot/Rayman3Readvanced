using BinarySerializer;
using Microsoft.Xna.Framework.Graphics;

namespace GbaMonoGame.Rayman3.Tests;

public class EngineTests(MockGame game)
{
    [Fact]
    public void AddToCache_Texture_ReturnsSameInstance()
    {
        const int id = 99;

        // Create the cache
        Cache<Texture2D> textureCache = new();

        // Create a texture
        using Texture2D texture = new(Engine.Assets.GraphicsDevice, 1, 1);

        // Get an arbitrary pointer from the ROM to use as a key
        Pointer pointer = Rom.Loader.GameOffsetTable.Offset;

        // Add to the cache
        textureCache.RegisterObject(texture, pointer, id);

        // Try to retrieve the same texture
        Assert.True(textureCache.TryGetObject(pointer, id, out Texture2D retrievedTexture));
        Assert.Equal(texture, retrievedTexture);
    }

    [Fact]
    public void OverrideSettings_ChangeInternalResolution_ResolutionIsDifferent()
    {
        // Get the original resolution
        Vector2? ogRes = Engine.Settings.Active.Tweaks.InternalGameResolution;
        Vector2 newRes = ogRes == null ? new Vector2(99, 99) : ogRes.Value * 2;

        // Override the settings with a new resolution
        Engine.Settings.OverrideActive(new ActiveGameSettings(
            tweaks: Engine.Settings.Local.Tweaks with
            {
                InternalGameResolution = newRes,
            },
            difficulty: Engine.Settings.Local.Difficulty with { },
            debug: Engine.Settings.Local.Debug with { }));

        // Validate the resolution is updated
        Assert.True(Engine.Settings.IsOverriden);
        Assert.Equal(newRes, Engine.Settings.Active.Tweaks.InternalGameResolution);

        // Restore the settings
        Engine.Settings.RestoreActive();

        // Validate the resolution is back to the original
        Assert.False(Engine.Settings.IsOverriden);
        Assert.Equal(ogRes, Engine.Settings.Active.Tweaks.InternalGameResolution);
    }
}