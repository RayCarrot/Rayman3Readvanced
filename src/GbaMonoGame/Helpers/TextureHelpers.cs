using System.IO;
using Microsoft.Xna.Framework.Graphics;

namespace GbaMonoGame;

public static class TextureHelpers
{
    public static void SaveAsPng(Texture2D texture, string filePath)
    {
        using FileStream fileStream = File.Create(filePath);
        texture.SaveAsPng(fileStream, texture.Width, texture.Height);
    }
}