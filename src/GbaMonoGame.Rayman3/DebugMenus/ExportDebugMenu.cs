using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using BinarySerializer.Ubisoft.GbaEngine;
using ImGuiNET;
using Microsoft.Xna.Framework.Graphics;

namespace GbaMonoGame.Rayman3;

public class ExportDebugMenu : DebugMenu
{
    public override string Name => "Export";

    private void GenerateCreditsWheelMesh()
    {
        const string exportDir = "Models";
        const string name = "CreditsWheel";
        const float textureSize = 64;

        Directory.CreateDirectory(exportDir);

        // Load resources
        AnimActorResource animActorResource = Rom.LoadResource<AnimActorResource>(Rayman3DefinedResource.CreditsWheelAnimActor);
        TextureTable textureTable = Rom.LoadResource<TextureTable>(Rayman3DefinedResource.CreditsWheelTextureTable);
        PaletteTable paletteTable = Rom.LoadResource<PaletteTable>(Rayman3DefinedResource.CreditsWheelPaletteTable);

        GeometryObject geometryObject = animActorResource.GeometryTable.GeometryObjects[0];

        using StreamWriter objWriter = new(Path.Combine(exportDir, $"{name}.obj"));
        using StreamWriter mtlWriter = new(Path.Combine(exportDir, $"{name}.mtl"));

        objWriter.WriteLine($"mtllib {name}.mtl");

        // Vertices
        foreach (Vector3 vertex in geometryObject.Vertices)
            objWriter.WriteLine(String.Format("v {0} {1} {2}", 
                vertex.X.AsFloat.ToString(CultureInfo.InvariantCulture),
                vertex.Y.AsFloat.ToString(CultureInfo.InvariantCulture),
                vertex.Z.AsFloat.ToString(CultureInfo.InvariantCulture)));

        objWriter.WriteLine();

        // UVs
        foreach (UV uv in geometryObject.TriangleUVs.SelectMany(x => x.UVs))
            objWriter.WriteLine(String.Format("vt {0} {1}",
                (uv.U / (textureSize - 1)).ToString(CultureInfo.InvariantCulture),
                (uv.V / (textureSize - 1)).ToString(CultureInfo.InvariantCulture)));

        objWriter.WriteLine();

        HashSet<string> writtenMaterials = new();

        // Triangles
        writeGroup("Side1", 0, 8, -1);
        for (int i = 0; i < 16; i += 2)
            writeGroup($"Icon{i / 2}", 8 + i, 2, i / 2);
        writeGroup("Side2", 24, 8, -1);

        void writeGroup(string groupName, int triangleStartIndex, int trianglesCount, int textureIndex)
        {
            string materialName = textureIndex == -1 ? "Untextured" : $"Texture{textureIndex}";

            objWriter.WriteLine($"g group_{groupName}");
            objWriter.WriteLine($"usemtl mtl_{materialName}");

            for (int i = triangleStartIndex; i < triangleStartIndex + trianglesCount; i++)
            {
                Triangle triangle = geometryObject.Triangles[i];

                objWriter.WriteLine("f {2}/{5} {1}/{4} {0}/{3}",
                    triangle.Vertices[0] + 1,
                    triangle.Vertices[1] + 1,
                    triangle.Vertices[2] + 1,
                    triangle.UVsOffset / 2 + 0 + 1,
                    triangle.UVsOffset / 2 + 1 + 1,
                    triangle.UVsOffset / 2 + 2 + 1);
            }

            objWriter.WriteLine();

            if (!writtenMaterials.Add(materialName))
                return;

            mtlWriter.WriteLine($"newmtl mtl_{materialName}");
            mtlWriter.WriteLine("Ka 0.00000 0.00000 0.00000");
            mtlWriter.WriteLine("Kd 0.50000 0.50000 0.50000");
            mtlWriter.WriteLine("Ks 0.00000 0.00000 0.00000");
            mtlWriter.WriteLine("d 1.00000");
            mtlWriter.WriteLine("illum 0");

            if (textureIndex != -1)
            {
                Texture2D tex = Engine.TextureCache.GetOrCreateObject(
                    pointer: textureTable.Offset,
                    id: textureIndex,
                    data: (Texture: textureTable.Textures[textureIndex].Value, Palette: paletteTable.Palettes[0].Value),
                    createObjFunc: static data =>
                    {
                        Palette palette = Engine.PaletteCache.GetOrCreateObject(
                            pointer: data.Palette.Offset,
                            id: 0,
                            data: data.Palette,
                            createObjFunc: static paletteData => new Palette(paletteData));

                        return new BitmapTexture2D(data.Texture.Width, data.Texture.Height, data.Texture.ImgData, palette);
                    });

                string texFilePath = $"{name}_{materialName}.png";

                using Stream texStream = File.Create(Path.Combine(exportDir, texFilePath));
                tex.SaveAsPng(texStream, tex.Width, tex.Height);

                mtlWriter.WriteLine($"map_Kd {texFilePath}");
            }

            mtlWriter.WriteLine();
        }
    }

    public override void Draw(DebugLayout debugLayout, DebugLayoutTextureManager textureManager)
    {
        if (ImGui.MenuItem("Credits wheel mesh"))
            GenerateCreditsWheelMesh();
    }
}